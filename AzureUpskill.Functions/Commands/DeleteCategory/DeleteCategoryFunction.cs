using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Models.Data;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Helpers;
using AzureUpskill.Functions.Filters;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Functions.Commands.DeleteCategory.Validation;

namespace AzureUpskill.Functions.Commands.DeleteCategory
{
    [ExecutionLogging]
    [ErrorHandler]
    public class DeleteCategoryFunction
    {
        public const string Name = "Category_Delete";

        [FunctionName(Name)]
        [ProducesResponseType(typeof(CategoryDocument), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{categoryId}")] HttpRequest req,
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

                var validationResult = categoryDocument.Validate<Category, CanDeleteCategoryValidator>();
                if (!validationResult.IsValid)
                {
                    var message = $"Can not delete category: {validationResult.ToErrorString()}";
                    log.LogWarningEx(message);
                    return validationResult.ToBadRequest();
                }

                log.LogInformationEx($"Deleting category with id: {categoryId}");

                var result = await documentClient.DeleteDocumentAsync(
                    categoryDocument.SelfLink,
                    new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(categoryDocument);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }, log);
        }
    }
}
