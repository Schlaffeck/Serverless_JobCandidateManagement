using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Helpers;
using AutoMapper;
using AzureUpskill.Models.Data;
using Microsoft.Azure.Documents.Client;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Filters;
using AzureUpskill.Functions.Helpers.CosmosDb;

namespace AzureUpskill.Functions.Commands.DeleteCandidate
{
    [ExecutionLogging]
    [ErrorHandler]
    public class DeleteCandidateFunction
    {
        public const string Name = "Candidate_Delete";

        private readonly IMapper _mapper;

        public DeleteCandidateFunction(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Name)]
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
    }
}
