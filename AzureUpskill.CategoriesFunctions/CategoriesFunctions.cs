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
using static AzureUpskill.Helpers.LogMessageHelper;
using static AzureUpskill.Helpers.HttpHelper;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System;

namespace AzureUpskill.CategoriesFunctions
{
    public static class CategoriesFunctions
    {
        [FunctionName("CreateCategory")]
        public static async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories")] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                ConnectionStringSetting = "CosmosDbConnection",
                CreateIfNotExists = true)] IAsyncCollector<Category> categories,
            ILogger log)
        {
            try
            {
                log.LogInformationEx("START");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                CreateCategoryInput data = JsonConvert.DeserializeObject<CreateCategoryInput>(requestBody);
                if (string.IsNullOrWhiteSpace(data?.Name))
                {
                    log.LogInformationEx($"No name for category provided");
                    return new BadRequestObjectResult("Input object in wrong format");
                }

                var id = Guid.NewGuid().ToString(); 
                var newCategory = new Category
                {
                    Id = id,
                    CategoryId = id,
                    Name = data.Name
                };

                await categories.AddAsync(newCategory);

                log.LogInformationEx($"Creating category with name: {data.Name}");

                return new OkObjectResult(newCategory);
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        [FunctionName("DeleteCategory")]
        public static async Task<IActionResult> DeleteCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = "CosmosDbConnection")] Document category,
             [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                ConnectionStringSetting = "CosmosDbConnection")] DocumentClient documentClient,
            string categoryId,
            ILogger log)
        {
            try
            {
                log.LogInformationEx("START");

                if(category is null)
                {
                    log.LogInformationEx($"Document with id: {categoryId} was not found");
                    return new NotFoundResult();
                }

                log.LogInformationEx($"Deleting category with id: {categoryId}");

                var result = await documentClient.DeleteDocumentAsync(
                    category.SelfLink,
                    new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(result.Resource ?? category);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }
            catch(DocumentClientException dce)
            {
                var message = "Deleting category failed";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }
    }
}
