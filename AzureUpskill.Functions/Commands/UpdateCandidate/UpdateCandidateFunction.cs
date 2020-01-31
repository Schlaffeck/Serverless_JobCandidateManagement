using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Helpers;
using AutoMapper;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Models.Data;
using Microsoft.Azure.Documents.Client;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Filters;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using AzureUpskill.Core;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Functions.Commands.UpdateCandidate.Models;
using AzureUpskill.Functions.Commands.UpdateCandidate.Validation;
using AzureUpskill.Functions.Commands.UpdateCandidate.Actions;

namespace AzureUpskill.Functions.Commands.UpdateCandidate
{
    [ExecutionLogging]
    [ErrorHandler]
    public class UpdateCandidateFunction
    {
        public const string Name = "Candidate_Update";

        private readonly IMapper _mapper;

        public UpdateCandidateFunction(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Result<Candidate>), (int)HttpStatusCode.OK)]
        [ProducesErrorResponseType(typeof(Result<Candidate>))]
        public async Task<IActionResult> UpdateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "categories/{categoryId}/candidates/{candidateId}")]
            [RequestBodyType(typeof(UpdateCandidateInput), "Update candidate model")]
                UpdateCandidateInput req,
            [DurableClient, SwaggerIgnore] IDurableOrchestrationClient durableOrchestrationClient,
            ExecutionContext functionExecutionContext,
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

                var validated = req.Validate<UpdateCandidateInput, UpdateCandidateInputValidator>();
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
    }
}
