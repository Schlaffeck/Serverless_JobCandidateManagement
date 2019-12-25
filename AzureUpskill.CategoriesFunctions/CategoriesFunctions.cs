using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureUpskill.Models;
using System.Net;
using AzureUpskill.Models.CreateCategory;

namespace AzureUpskill.CategoriesFunctions
{
    public static class CategoriesFunctions
    {
        [FunctionName("Category")]
        public static async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                ConnectionStringSetting = "CosmosDbConnection",
                CreateIfNotExists = true)] IAsyncCollector<Category> categories,
            ILogger log)
        {
            try
            {
                log.LogInformation("START");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                CreateCategoryInput data = JsonConvert.DeserializeObject<CreateCategoryInput>(requestBody);
                if (string.IsNullOrWhiteSpace(data?.Name))
                {
                    log.LogInformation($"No name for category provided");
                    return new BadRequestObjectResult("Input object in wrong format");
                }

                var newCategory = new Category
                {
                    Name = data.Name
                };

                await categories.AddAsync(newCategory);

                log.LogInformation($"Creating category with name: {data.Name}");

                return new AcceptedResult();
            }
            finally
            {
                log.LogInformation("STOP");
            }
        }

        [FunctionName("Category")]
        public static async Task<IActionResult> DeleteCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Category/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = "CosmosDbConnection",
                CreateIfNotExists = true)] Category category,
            string categoryId,
            ILogger log)
        {
            try
            {
                log.LogInformation("START");

                if(category is null)
                {
                    log.LogInformation($"Document with key: {categoryId} was not found");
                    return new NotFoundResult();
                }

                log.LogInformation($"Deleting category with key: {categoryId}");

                // TODO: deletig from document db

                return new OkObjectResult(categoryId);
            }
            finally
            {
                log.LogInformation("STOP");
            }
        }
    }
}