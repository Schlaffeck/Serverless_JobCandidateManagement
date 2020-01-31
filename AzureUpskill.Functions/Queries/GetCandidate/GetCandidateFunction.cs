using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Helpers;
using AutoMapper;
using AzureUpskill.Models.Data;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Filters;

namespace AzureUpskill.Functions
{
    [ExecutionLogging]
    [ErrorHandler]
    public class GetCandidateFunction
    {
        public const string Name = "Candidate_Get";

        private readonly IMapper _mapper;

        public GetCandidateFunction(IMapper mapper)
        {
            this._mapper = mapper;
        }

        [FunctionName(Name)]
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
    }
}
