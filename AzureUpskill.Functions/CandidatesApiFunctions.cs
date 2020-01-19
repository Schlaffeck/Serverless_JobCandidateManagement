using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Models.CreateCandidate.Validation;
using AzureUpskill.Models.CreateCandidate;
using AzureUpskill.Helpers;
using AutoMapper;
using AzureUpskill.Functions.Validation;
using AzureUpskill.Models.Data;
using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using AzureUpskill.Models.UpdateCandidate;
using AzureUpskill.Models.UpdateCandidate.Validation;
using AzureUpskill.Functions.CosmosDb;
using AzureUpskill.Models.Data.Base;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Filters;

namespace AzureUpskill.Functions
{
    [ExecutionLogging]
    [ErrorHandler]
    public class CandidatesApiFunctions
    {
        private readonly IMapper _mapper;

        public CandidatesApiFunctions(IMapper mapper)
        {
            this._mapper = mapper;
        }

        [FunctionName("Candidate_Create")]
        [ProducesResponseType(typeof(Candidate), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories/{categoryId}/candidates")]
            [RequestBodyType(typeof(CreateCandidateInput), "Create candidate model")]
                HttpRequest req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] IAsyncCollector<Candidate> candidates,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] Category category,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] DocumentClient categoriesDocumentClient,
            string categoryId,
            ILogger log)
        {
            if (category is null)
            {
                var msg = $"Category for candidate not found by id: {categoryId}";
                log.LogWarningEx(msg);
                return new NotFoundResult();
            }

            var createCandidateInput = await req.GetJsonBodyValidatedAsync<CreateCandidateInput, CreateCandidateInputValidator>();
            if (!createCandidateInput.IsValid)
            {
                log.LogWarningEx($"Can not create candidate: {createCandidateInput.ToErrorString()}");
                return createCandidateInput.ToBadRequest();
            }

            var candidate = _mapper.Map<Candidate>(createCandidateInput.Value);
            this._mapper.Map(category, candidate);

            await candidates.AddAsync(candidate);

            log.LogInformationEx($"Candidate {candidate.Id} added to category: {candidate.CategoryId}");
            return new OkObjectResult(candidate);
        }

        [FunctionName("Candidate_Delete")]
        [ProducesResponseType(typeof(CandidateDocument), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{categoryId}/candidates/{candidateId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] CandidateDocument candidateDocument,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] DocumentClient candidatesDocumentClient,
            string categoryId,
            string candidateId,
            ILogger log)
        {
            return await CosmosDbExecutionHelper.RunInCosmosDbContext(async () =>
            {
                if (candidateDocument is null)
                {
                    var msg = $"Candidate not found by id: {req.Path}";
                    log.LogWarningEx(msg);
                    return new NotFoundResult();
                }

                var result = await MarkCandidateForDeletionAsync(candidateDocument, candidatesDocumentClient);

                if (result.StatusCode.IsSuccess())
                {
                    log.LogInformationEx($"Candidate {candidateDocument.Id} deleted from category: {categoryId}");
                    return new OkObjectResult(candidateDocument);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }, log);
        }

        [FunctionName("Candidate_Update")]
        [ProducesResponseType(typeof(Candidate), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "categories/{categoryId}/candidates/{candidateId}")]
            [RequestBodyType(typeof(UpdateCandidateInput), "Update candidate model")]
                HttpRequest req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] CandidateDocument candidateDocument,
             [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] DocumentClient candidatesDocumentClient,
             [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] DocumentClient categoriesDocumentClient,
            string categoryId,
            string candidateId,
            ILogger log)
        {
            try
            {
                if (candidateDocument is null)
                {
                    log.LogInformationEx($"Candidate with id: {candidateId} was not found");
                    return new NotFoundResult();
                }

                var validated = await req.GetJsonBodyValidatedAsync<UpdateCandidateInput, UpdateCandidateInputValidator>();
                if (!validated.IsValid)
                {
                    return validated.ToBadRequest();
                }

                var candidate = default(Candidate);
                var result = default(ResourceResponse<Document>);
                var newCategoryId = validated.Value.CategoryId;
                if (newCategoryId != categoryId)
                {
                    var newCategoryUri = UriFactory.CreateDocumentUri(
                        Consts.CosmosDb.DbName,
                        Consts.CosmosDb.CategoriesContainerName,
                        newCategoryId);
                    var newCategory = await categoriesDocumentClient.ReadDocumentAsync<Category>(newCategoryUri, new RequestOptions
                    {
                        PartitionKey = new PartitionKey(newCategoryId)
                    });

                    if (newCategory.Document == null)
                    {
                        var msg = $"New category with id '{newCategoryId}' for candidate not found";
                        log.LogErrorEx(msg);
                        return new BadRequestObjectResult(msg);
                    }

                    candidate = _mapper.Map<Candidate>(candidateDocument);
                    _mapper.Map(validated.Value, candidate as Candidate);
                    _mapper.Map(newCategory.Document, candidate);
                    candidate.Status = DocumentStatus.Moved;
                    candidate.UpdatedAt = DateTime.Now;
                    var docCollectionUri = UriFactory.CreateDocumentCollectionUri(
                        Consts.CosmosDb.DbName,
                        Consts.CosmosDb.CandidatesContainerName);
                    result = await candidatesDocumentClient.CreateDocumentAsync(
                        docCollectionUri,
                        candidate,
                        new RequestOptions { PartitionKey = new PartitionKey(newCategoryId) });
                    if (result.StatusCode.IsSuccess())
                    {
                        await MarkCandidateForDeletionAsync(candidateDocument, candidatesDocumentClient);
                    }
                }
                else
                {
                    log.LogInformationEx($"Updating candidate with id: {candidateId}");
                    candidate = _mapper.Map(validated.Value, (Candidate)candidateDocument);
                    result = await candidatesDocumentClient.ReplaceDocumentAsync(
                        candidateDocument.SelfLink,
                        candidate,
                        new RequestOptions { PartitionKey = new PartitionKey(candidate.CategoryId) });
                }

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(candidate);
                }

                return new ObjectResult(result.ToErrorString()) { StatusCode = (int)result.StatusCode };
            }
            catch (DocumentClientException dce)
            {
                var message = $"Candidate update failed: {dce.Message}";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
        }

        [FunctionName("Candidate_Get")]
        [ProducesResponseType(typeof(Candidate), (int)HttpStatusCode.OK)]
        public static async Task<IActionResult> GetCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}/candidates/{candidateId}")] 
                HttpRequest req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] Candidate candidate,
            string categoryId,
            string candidateId,
            ILogger log)
        {
            if (candidate is null)
            {
                log.LogInformationEx($"Candidate with id: {candidateId} was not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(candidate);
        }

        private async Task<ResourceResponse<Document>> MarkCandidateForDeletionAsync(CandidateDocument document, DocumentClient documentClient)
        {
            document.Status = DocumentStatus.Deleted;
            document.UpdatedAt = DateTime.Now;
            return await documentClient.UpsertDocumentAsync(document.SelfLink, document, new RequestOptions
            {
                PartitionKey = new PartitionKey(document.PartitionKey)
            });
        }
    }
}
