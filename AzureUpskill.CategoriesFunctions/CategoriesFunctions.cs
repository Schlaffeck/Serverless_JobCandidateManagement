using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureUpskill.Models;
using AzureUpskill.Models.CreateCategory;
using static AzureUpskill.Helpers.LogMessageHelper;
using static AzureUpskill.Helpers.HttpHelper;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using System;
using AzureUpskill.Models.UpdateCategory;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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

                if(category.GetPropertyValue<int>(nameof(Category.NumberOfCandidates)) > 0)
                {
                    var message = $"Can not delete category with candidates assigned to it";
                    log.LogWarningEx(message);
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
                var message = $"Deleting category failed ${dce.Message}";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        [FunctionName("UpdateCategory")]
        public static async Task<IActionResult> UpdateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "patch", Route = "categories/{categoryId}")] HttpRequest req,
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

                if (category is null)
                {
                    log.LogInformationEx($"Document with id: {categoryId} was not found");
                    return new NotFoundResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                UpdateCategoryInput data = JsonConvert.DeserializeObject<UpdateCategoryInput>(requestBody);
                if (data is null)
                {
                    log.LogWarningEx($"Wrong format of request body: {requestBody}");
                    return new BadRequestObjectResult("Input object in wrong format");
                }

                log.LogInformationEx($"Updating category with id: {categoryId}");

                category.SetPropertyValue(nameof(Category.Name), data.Name);
                var result = await documentClient.UpsertDocumentAsync(
                    category.SelfLink, 
                    category,
                    new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(result.Resource ?? category);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }
            catch (DocumentClientException dce)
            {
                var message = $"Category update failed: {dce.Message}";
                log.LogErrorEx(dce, message);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }

        [FunctionName("GetCategory")]
        public static async Task<IActionResult> GetCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: "CvDatabase",
                collectionName: "Categories",
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = "CosmosDbConnection")] Category category,
            string categoryId,
            ILogger log)
        {
            try
            {
                log.LogInformationEx("START");

                if (category is null)
                {
                    log.LogInformationEx($"Document with id: {categoryId} was not found");
                    return new NotFoundResult();
                }

                return new OkObjectResult(category);
            }
            finally
            {
                log.LogInformationEx("STOP");
            }
        }
    }
}
