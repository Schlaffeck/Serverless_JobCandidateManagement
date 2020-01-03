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
using AzureUpskill.Models;
using System;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using AzureUpskill.Models.UpdateCandidate;
using AzureUpskill.Models.UpdateCandidate.Validation;

namespace AzureUpskill.Functions
{
    public class CandidatesFunctions
    {
        private readonly IMapper mapper;

        public CandidatesFunctions(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [FunctionName("CreateCandidate")]
        public async Task<IActionResult> CreateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories/{categoryId}/candidates")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName,
                CreateIfNotExists = true)] IAsyncCollector<Candidate> candidates,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] Category category,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient categoriesDocumentClient,
            string categoryId,
            ILogger log)
        {
            log.LogInformationEx("START");
            try
            {
                if (category is null)
                {
                    var msg = $"Category for candidate not found by id: {req.Path}";
                    log.LogWarningEx(msg);
                    return new NotFoundResult();
                }

                var createCandidateInput = await req.GetJsonBodyValidatedAsync<CreateCandidateInput, CreateCandidateInputValidator>();
                if (!createCandidateInput.IsValid)
                {
                    log.LogWarningEx($"Can not create candidate: {createCandidateInput.ToErrorString()}");
                    return createCandidateInput.ToBadRequest();
                }

                var candidate = this.mapper.Map<Candidate>(createCandidateInput.Value);
                this.mapper.Map(category, candidate);

                await candidates.AddAsync(candidate);
                await RunChangeCandidateCountStoredProcedureAsync(categoriesDocumentClient, categoryId, 1);

                log.LogInformationEx($"Candidate {candidate.Id} added to category: {candidate.CategoryId}");
                return new OkObjectResult(candidate);
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }



        [FunctionName("DeleteCandidate")]
        public async Task<IActionResult> DeleteCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{categoryId}/candidates/{candidateId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] Document candidateDocument,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient documentClient,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient categoriesDocumentClient,
            string categoryId,
            ILogger log)
        {
            log.LogInformationEx("START");
            try
            {
                if (candidateDocument is null)
                {
                    var msg = $"Candidate not found by id: {req.Path}";
                    log.LogWarningEx(msg);
                    return new NotFoundResult();
                }

                var result = await documentClient.DeleteDocumentAsync(candidateDocument.SelfLink, new RequestOptions
                {
                    PartitionKey = new PartitionKey(categoryId)
                });

                if (result.StatusCode.IsSuccess())
                {

                    log.LogInformationEx($"Candidate {candidateDocument.Id} deleted from category: {categoryId}");

                    await RunChangeCandidateCountStoredProcedureAsync(categoriesDocumentClient, categoryId, -1);
                    return new OkObjectResult(candidateDocument);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }
            catch (DocumentClientException dce)
            {
                var message = $"Deleting category failed ${dce.Message}";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        [FunctionName("UpdateCandidate")]
        public async Task<IActionResult> UpdateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "categories/{categoryId}/candidates/{candidateId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] CandidateDocument candidateDocument,
             [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient documentClient,
            string candidateId,
            ILogger log)
        {
            try
            {
                log.LogInformationEx("START");

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

                log.LogInformationEx($"Updating candidate with id: {candidateId}");

                var candidate = this.mapper.Map(validated.Value, (Candidate)candidateDocument);

                var result = await documentClient.UpsertDocumentAsync(
                    candidateDocument.SelfLink,
                    candidate,
                    new RequestOptions { PartitionKey = new PartitionKey(candidateId) });

                if (result.StatusCode.IsSuccess())
                {
                    // TODO: if candidate category changes - run SP to swap categories count (modify SP)
                    return new OkObjectResult(candidate);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }
            catch (DocumentClientException dce)
            {
                var message = $"Candidate update failed: {dce.Message}";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        [FunctionName("GetCandidate")]
        public static async Task<IActionResult> GetCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}/candidates/{candidateId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] Candidate candidate,
            string candidateId,
            ILogger log)
        {
            try
            {
                log.LogInformationEx("START");

                if (candidate is null)
                {
                    log.LogInformationEx($"Candidate with id: {candidateId} was not found");
                    return new NotFoundResult();
                }

                return new OkObjectResult(candidate);
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        private static async Task RunChangeCandidateCountStoredProcedureAsync(DocumentClient categoriesClient, string categoryId, int changeCountValue)
        {
            var spUri = UriFactory.CreateStoredProcedureUri(Consts.DbName, Consts.CategoriesContainerName,
                "changeCandidateCount");
            await categoriesClient.ExecuteStoredProcedureAsync<dynamic>(spUri, new RequestOptions { PartitionKey = new PartitionKey(categoryId)}, new { categoryId, changeCountValue });
        }
    }
}
