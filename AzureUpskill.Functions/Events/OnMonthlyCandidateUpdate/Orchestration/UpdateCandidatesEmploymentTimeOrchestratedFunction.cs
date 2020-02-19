using AzureUpskill.Core;
using AzureUpskill.Helpers;
using AzureUpskill.Search.Models.Categories;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Events.OnMonthlyCandidateUpdate.Orchestration
{
    class UpdateCandidatesEmploymentTimeOrchestratedFunction
    {
        public const string Name = "UpdateCandidatesEmploymentTimeOrchestrated";

        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public UpdateCandidatesEmploymentTimeOrchestratedFunction(ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Name)]
        public async Task<Result<int>> UpdateCandidatesEmploymentTime([OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            log.LogInformationEx($"Candidates monthly employment time update started");
            log.LogInformationEx("Get all categories");
            var searchIndexClient = this.searchIndexClientRegistry.GetSearchIndexClient<CategoryIndex>(CategoryIndex.IndexNameConst);
            var categoriesSearchResult = await searchIndexClient.Documents.SearchAsync(null, new SearchParameters
            {
                Select = new[] { nameof(CategoryIndex.Id) }
            });
            var tasksList = new List<Task<Result<int>>>();
            log.LogInformationEx($"{categoriesSearchResult.Count} categories found");

            log.LogInformationEx("Updating candidates data per category");
            foreach (var category in categoriesSearchResult.Results)
            {
                object categoryId;
                if (category.Document.TryGetValue(nameof(CategoryIndex.Id), out categoryId))
                {
                    var runInCategory = context.CallActivityAsync<Result<int>>(
                        RecalculateEmploymentTimeInCategoryFunction.Name, 
                        categoryId);
                    tasksList.Add(runInCategory);
                }
            }

            await Task.WhenAll(tasksList);
            log.LogInformationEx($"Candidates monthly employment time update finished");
            var result = new Result<int>();

            foreach(var partResultTask in tasksList)
            {
                var partResult = await partResultTask;
                result.Value += partResult.Value;
                result.AddErrors(partResult.Errors);
            }

            return result;
        }
    }
}
