using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Models.Data;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Helpers;
using AzureUpskill.Functions.Filters;
using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Functions.Commands.UpdateCategory.Models;
using AzureUpskill.Functions.Commands.UpdateCategory.Validation;

namespace AzureUpskill.Functions.Commands.UpdateCategory
{
    [ExecutionLogging]
    [ErrorHandler]
    public class UpdateCategoryFunction
    {
        public const string Name = "Category_Update";
        private readonly IMapper _mapper;

        public UpdateCategoryFunction(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "patch", Route = "categories/{categoryId}")]
            [RequestBodyType(typeof(UpdateCategoryInput), "update category model")]
                UpdateCategoryInput req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] CategoryDocument categoryDocument,
             [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore] DocumentClient documentClient,
            string categoryId,
            ILogger log)
        {
            return await CosmosDbExecutionHelper.RunInCosmosDbContext(async () =>
            {
                if (categoryDocument is null)
                {
                    log.LogInformationEx($"Document with id: {categoryId} was not found");
                    return new NotFoundResult();
                }

                var validated = req.Validate<UpdateCategoryInput, UpdateCategoryInputValidator>();
                if (!validated.IsValid)
                {
                    return validated.ToBadRequest();
                }

                log.LogInformationEx($"Updating category with id: {categoryId}");

                var category = _mapper.Map(validated.Value, (Category)categoryDocument);
                var result = await documentClient.ReplaceDocumentAsync(
                    categoryDocument.SelfLink,
                    category,
                    new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(category);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }, log);
        }
    }
}
