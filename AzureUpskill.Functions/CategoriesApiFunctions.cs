using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureUpskill.Models.Data;
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
using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;

namespace AzureUpskill.Functions
{
    [ExecutionLogging]
    [ErrorHandler]
    public class CategoriesApiFunctions
    {
        private readonly IMapper _mapper;

        public CategoriesApiFunctions(IMapper mapper)
        {
            this._mapper = mapper;
        }

        [FunctionName("Category_Create")]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories")]
            [RequestBodyType(typeof(CreateCategoryInput), "Create category model")]
                CreateCategoryInput req,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CategoriesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
                CreateIfNotExists = true), SwaggerIgnore] IAsyncCollector<Category> categories,
            ILogger log)
        {
            var validated = req.Validate<CreateCategoryInput, CreateCategoryInputValidator>();
            if (!validated.IsValid)
            {
                return validated.ToBadRequest();
            }

            var newCategory = _mapper.Map<Category>(validated.Value);

            await categories.AddAsync(newCategory);

            log.LogInformationEx($"Creating category with name: {validated.Value.Name}");

            return new OkObjectResult(newCategory);
        }

        [FunctionName("Category_Delete")]
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

        [FunctionName("Category_Update")]
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
                var result = await documentClient.UpsertDocumentAsync(
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

        [FunctionName("Category_Get")]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        public static async Task<IActionResult> GetCategory(
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
