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
using AzureUpskill.Functions.Helpers;
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
using AzureUpskill.Models.Commands.UpdateCandidate;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading;
using Newtonsoft.Json;
using AzureUpskill.Core;
using static AzureUpskill.Functions.Helpers.DurableFunctionsHelper;

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

        [FunctionName(Names.CandidateCreateFunctionName)]
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

        [FunctionName(Names.CandidateDeleteFunctionName)]
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

                var result = await candidateDocument.MarkCandidateForDeletionAsync(candidatesDocumentClient);

                if (result.StatusCode.IsSuccess())
                {
                    log.LogInformationEx($"Candidate {candidateDocument.Id} deleted from category: {categoryId}");
                    return new OkObjectResult(candidateDocument);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }, log);
        }

        [FunctionName(Names.CandidateUpdateFunctionName)]
        [ProducesResponseType(typeof(Result<Candidate>), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(Result<Candidate>))]
        public async Task<IActionResult> UpdateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "categories/{categoryId}/candidates/{candidateId}")]
            [RequestBodyType(typeof(UpdateCandidateInput), "Update candidate model")]
                HttpRequest req,
            [DurableClient, SwaggerIgnore] IDurableOrchestrationClient durableOrchestrationClient,
            Microsoft.Azure.WebJobs.ExecutionContext functionExecutionContext,
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
            return await CosmosDbExecutionHelper.RunInCosmosDbContext(async () =>
            {
                if (candidateDocument is null)
                {
                    var msg = $"Candidate with id: {candidateId} was not found";
                    log.LogInformationEx(msg);
                    return new NotFoundObjectResult(msg);
                }

                var validated = await req.GetJsonBodyValidatedAsync<UpdateCandidateInput, UpdateCandidateInputValidator>();
                if (!validated.IsValid)
                {
                    return validated.ToBadRequest();
                }

                var updateCandidateCommand = new UpdateCandidateOrchestratedCommand
                {
                    CurrentCandidate = candidateDocument,
                    UpdateCandidateInput = validated.Value
                };
                var orchestrationId = await durableOrchestrationClient.StartNewAsync(
                    CandidatesActivityFunctions.Names.CandidateUpdateOrchestratedFunctionName,
                    updateCandidateCommand);
                log.LogInformationEx($"Started '{CandidatesActivityFunctions.Names.CandidateMoveOrchestratedFunctionName}' orchestration with " +
                                      $"ID = '{orchestrationId}'");
                var candidateUpdateResult = await durableOrchestrationClient
                .WaitForOrchestratedFunctionResultAsync<Result<Candidate>>(orchestrationId);
                log.LogInformationEx($"Finished '{CandidatesActivityFunctions.Names.CandidateMoveOrchestratedFunctionName}' orchestration with " +
                                   $"ID = '{orchestrationId}'");

                return ResultHelper.ToActionResult(candidateUpdateResult);
            },
            log);
        }

        [FunctionName(Names.CandidateGetFunctionName)]
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

        public static class Names
        {
            public const string CandidateCreateFunctionName = "Candidate_Create";
            public const string CandidateUpdateFunctionName = "Candidate_Update";
            public const string CandidateGetFunctionName = "Candidate_Get";
            public const string CandidateDeleteFunctionName = "Candidate_Delete";
        }
    }
}
