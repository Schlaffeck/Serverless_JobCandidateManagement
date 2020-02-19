using AutoMapper;
using AzureUpskill.Core;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Helpers;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Events.OnMonthlyCandidateUpdate.Orchestration
{
    public class RecalculateEmploymentTimeInCategoryFunction
    {
        public const string Name = "RecalculateEmploymentTimeInCategory";
        private readonly IMapper mapper;

        public RecalculateEmploymentTimeInCategoryFunction(IMapper mapper)
        {
            this.mapper = mapper;
        }

        [FunctionName(Name)]
        public async Task<Result<int>> RecalculateEmploymentTimeInCategory([ActivityTrigger] string categoryId,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            log.LogInformationEx($"Run for category '{categoryId}'");
            var query = candidatesDocumentClient.QueryCandidatesInCategory<CandidateDocument>(categoryId);

            var candidateCount = 0;
            var failedCandidateCount = 0;
            foreach(var candidateDocument in query)
            {
                candidateDocument.EmploymentFullMonths = EmploymentCalculator.CalculateEmploymentPeriodFullMonths(candidateDocument.EmploymentHistory);
                var updateResult = await candidatesDocumentClient.UpsertDocumentAsync(candidateDocument.SelfLink, candidateDocument,
                    new RequestOptions
                    {
                        PartitionKey = new Microsoft.Azure.Documents.PartitionKey(categoryId)
                    });

                if(!updateResult.StatusCode.IsSuccess())
                {
                    failedCandidateCount++;
                    log.LogErrorEx($"Failed update of candidate '{candidateDocument.Id}': {updateResult.StatusCode} - {updateResult.ResponseStream.ReadStringToEnd()}");
                }
                else
                {
                    candidateCount++;
                }
            }

            return failedCandidateCount > 0
                ? new Result<int>($"Update of {failedCandidateCount} candidates failed in category '{categoryId}'") 
                : new Result<int>(candidateCount);
        }
    }
}
