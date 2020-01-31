using AzureUpskill.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using AzureUpskill.Helpers;
using AzureFunctions.Extensions.Swashbuckle.Attribute;

namespace AzureUpskill.Functions.Queries.GetCategory
{
    public class GetCategoryFunction
    {
        public const string Name = "Category_Get";

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore]Category category,
            string categoryId,
            ILogger log)
        {
            if (category is null)
            {
                log.LogInformationEx($"Document with id: {categoryId} was not found");
                return new NotFoundResult();
            }

            return new OkObjectResult(category);
        }
    }
}
