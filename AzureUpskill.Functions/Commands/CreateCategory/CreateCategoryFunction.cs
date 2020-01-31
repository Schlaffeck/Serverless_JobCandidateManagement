using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Models.Data;
using static AzureUpskill.Helpers.LogMessageHelper;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Helpers;
using AzureUpskill.Functions.Filters;
using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using System.Net;
using AzureUpskill.Functions.Commands.CreateCategory.Models;
using AzureUpskill.Functions.Commands.CreateCategory.Validation;

namespace AzureUpskill.Functions.Commands.CreateCategory
{
    [ExecutionLogging]
    [ErrorHandler]
    public class CreateCategoryFunction
    {
        public const string Name = "Category_Create";

        private readonly IMapper _mapper;

        public CreateCategoryFunction(IMapper mapper)
        {
            _mapper = mapper;
        }

        [FunctionName(Name)]
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
    }
}
