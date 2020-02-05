using AutoMapper;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Core;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Functions.Queries.GetCandidates.Models;
using AzureUpskill.Functions.Queries.GetCandidates.Validation;
using AzureUpskill.Helpers;
using AzureUpskill.Search.Models.Candidates;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Queries.GetCandidates
{
    public class GetCandidatesFunction
    {
        public const string Name = "Candidate_Search";
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;
        private readonly IMapper mapper;

        public GetCandidatesFunction(
            ISearchIndexClientRegistry searchIndexClientRegistry,
            IMapper mapper)
        {
            this.searchIndexClientRegistry = searchIndexClientRegistry;
            this.mapper = mapper;
        }

        [FunctionName(Name)]
        [ProducesResponseType(typeof(Result<IEnumerable<Document>>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetCandidates(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Candidates/search")]
            [RequestBodyType(typeof(GetCandidatesQuery), "Search for Candidates model")]
            GetCandidatesQuery req,
            ILogger log)
        {
            var validated = req.Validate<GetCandidatesQuery, GetCandidatesQueryValidator>();
            if (!validated.IsValid)
            {
                return validated.ToBadRequest();
            }

            var searchIndexClient = this.searchIndexClientRegistry.GetSearchIndexClient<CandidateIndex>(CandidateIndex.IndexNameConst);
            var searchParams = this.mapper.Map<SearchParameters>(req);
            var result = await searchIndexClient.Documents.SearchAsync(req.SearchText, searchParams);

            return new OkObjectResult(new Result<IEnumerable<Document>>(result.Results.Select(sr => sr.Document).ToList()));
        }
    }
}
