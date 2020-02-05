using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AzureUpskill.Functions.Queries.GetCategories.Models;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Search.Services.Interfaces;
using AzureUpskill.Functions.Filters;
using System.Collections.Generic;
using System.Net;
using AzureUpskill.Functions.Queries.GetCategories.Validation;
using AzureUpskill.Helpers;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Search.Models.Categories;
using Microsoft.Azure.Search;
using AutoMapper;
using Microsoft.Azure.Search.Models;
using AzureUpskill.Core;
using System.Linq;

namespace AzureUpskill.Functions.Queries.GetCategories
{
    [ExecutionLogging]
    [ErrorHandler]
    public class GetCategoriesFunction
    {
        public const string Name = "Category_Search";
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;
        private readonly IMapper mapper;

        public GetCategoriesFunction(
            ISearchIndexClientRegistry searchIndexClientRegistry,
            IMapper mapper)
        {
            this.searchIndexClientRegistry = searchIndexClientRegistry;
            this.mapper = mapper;
        }

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Result<IEnumerable<Document>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCategories(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "categories/search")] 
            [RequestBodyType(typeof(GetCategoriesQuery), "Search for categories model")]
            GetCategoriesQuery req,
            ILogger log)
        {
            var validated = req.Validate<GetCategoriesQuery, GetCategoriesQueryValidator>();
            if (!validated.IsValid)
            {
                return validated.ToBadRequest();
            }

            var searchIndexClient = this.searchIndexClientRegistry.GetSearchIndexClient<CategoryIndex>(CategoryIndex.IndexNameConst);
            var searchParams = this.mapper.Map<SearchParameters>(req);
            var result = await searchIndexClient.Documents.SearchAsync(req.SearchText, searchParams);

            return new OkObjectResult(new Result<IEnumerable<Document>>(result.Results.Select(sr => sr.Document).ToList()));
        }
    }
}
