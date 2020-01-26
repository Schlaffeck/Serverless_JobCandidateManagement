using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Candidates;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureUpskill.Functions.Search
{
    public class OnCandidateIndexingFunction
    {
        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCandidateIndexingFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Names.OnCandidateIndexingFunctionName)]
        public async Task OnCandidateAdded_Indexing_Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName)]IReadOnlyList<CandidateDocument> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformationEx("Documents modified " + input.Count);
                await RunIndexingAsync(input, log);
            }
        }

        private async Task RunIndexingAsync(IEnumerable<CandidateDocument> candidateDocuments, ILogger log)
        {
            try
            {
                log.LogInformationEx("Start indexing");
                var newOrChangedCandidates = candidateDocuments.Where(c => c.Status != Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{newOrChangedCandidates.Count} new or changed candidates");
                var searchClientIndex = this.searchIndexClientRegistry.GetSearchIndexClient<CandidateIndex>(CandidateIndex.Name);
                var batch = IndexBatch.MergeOrUpload(newOrChangedCandidates);
                var indexResult = await searchClientIndex.Documents.IndexAsync(batch);
                log.LogInformationEx("new or changed candiates indexed");

                var deletedCandidates = candidateDocuments.Where(c => c.Status == Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{newOrChangedCandidates.Count} deleted candidates");
                var deletedBatch = IndexBatch.Delete(deletedCandidates);
                log.LogInformation("Deleted candidates removed from index");

                log.LogInformationEx($"Indexing finished successfully");
            }
            catch (IndexBatchException batchException)
            {
                log.LogErrorEx(
                    batchException,
                    string.Format(
                        "Indexing faile for some documents: {0}",
                        string.Join(", ", batchException.IndexingResults.Where(ir => !ir.Succeeded).Select(ir => $"{ir.Key} - {ir.StatusCode}: {ir.ErrorMessage}"));
            }
        }

        public static class Names
        {
            public const string OnCandidateIndexingFunctionName = "OnCandidateIndexingFunction";
        }
    }
}
