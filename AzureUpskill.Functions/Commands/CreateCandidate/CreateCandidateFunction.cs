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
using AzureUpskill.Functions.Commands.CreateCandidate.Models;
using AzureUpskill.Functions.Commands.CreateCandidate.Validation;

namespace AzureUpskill.Functions.Commands.CreateCandidate
{
    [ExecutionLogging]
    [ErrorHandler]
    public class CreateCandidateFunction
    {
        public const string Name = "Candidate_Create";

        private readonly IMapper _mapper;

        public CreateCandidateFunction(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Candidate), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCandidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories/{categoryId}/candidates")]
            [RequestBodyType(typeof(CreateCandidateInput), "Create candidate model")]
                CreateCandidateInput req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] IAsyncCollector<CandidateDocument> candidates,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] CategoryDocument category,
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

            var createCandidateInput = req.Validate<CreateCandidateInput, CreateCandidateInputValidator>();
            if (!createCandidateInput.IsValid)
            {
                log.LogWarningEx($"Can not create candidate: {createCandidateInput.ToErrorString()}");
                return createCandidateInput.ToBadRequest();
            }

            var candidate = _mapper.Map<CandidateDocument>(createCandidateInput.Value);
            _mapper.Map(category, candidate);

            await candidates.AddAsync(candidate);

            log.LogInformationEx($"Candidate {candidate.Id} added to category: {candidate.CategoryId}");
            return new OkObjectResult(candidate);
        }
    }
}
