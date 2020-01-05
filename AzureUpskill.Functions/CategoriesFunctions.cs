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
using AzureUpskill.Models.CreateCategory.Validation;
using AzureUpskill.Functions.Validation;
using AzureUpskill.Models.UpdateCategory.Validation;
using AzureUpskill.Models.DeleteCategory.Validation;
using AzureUpskill.Helpers;
using AzureUpskill.Functions.Filters;
using AzureUpskill.Functions.CosmosDb;

namespace AzureUpskill.Functions
{
    [ExecutionLogging]
    [ErrorHandler]
    public static class CategoriesFunctions
    {
        [FunctionName("CreateCategory")]
        public static async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories")] CreateCategoryInput req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName,
                CreateIfNotExists = true)] IAsyncCollector<Category> categories,
            ILogger log)
        {
            var validated = req.Validate<CreateCategoryInput, CreateCategoryInputValidator>();
            if (!validated.IsValid)
            {
                return validated.ToBadRequest();
            }

            var id = Guid.NewGuid().ToString();
            var newCategory = new Category
            {
                Id = id,
                CategoryId = id,
                Name = validated.Value.Name
            };

            await categories.AddAsync(newCategory);

            log.LogInformationEx($"Creating category with name: {validated.Value.Name}");

            return new OkObjectResult(newCategory);
        }

        [FunctionName("DeleteCategory")]
        public static async Task<IActionResult> DeleteCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "categories/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] CategoryDocument categoryDocument,
             [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient documentClient,
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

        [FunctionName("UpdateCategory")]
        public static async Task<IActionResult> UpdateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", "patch", Route = "categories/{categoryId}")] UpdateCategoryInput req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] Document category,
             [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] DocumentClient documentClient,
            string categoryId,
            ILogger log)
        {
            return await CosmosDbExecutionHelper.RunInCosmosDbContext(async () =>
            {
                if (category is null)
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

                category.SetPropertyValue(nameof(Category.Name), validated.Value.Name);
                var result = await documentClient.UpsertDocumentAsync(
                    category.SelfLink,
                    category,
                    new RequestOptions { PartitionKey = new PartitionKey(categoryId) });

                if (result.StatusCode.IsSuccess())
                {
                    return new OkObjectResult(result.Resource ?? category);
                }

                return new StatusCodeResult((int)result.StatusCode);
            }, log);
        }

        [FunctionName("GetCategory")]
        public static async Task<IActionResult> GetCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "categories/{categoryId}")] HttpRequest req,
            [CosmosDB(
                databaseName: Consts.DbName,
                collectionName: Consts.CategoriesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{categoryId}",
                ConnectionStringSetting = Consts.CosmosDbConnectionStringName)] Category category,
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
