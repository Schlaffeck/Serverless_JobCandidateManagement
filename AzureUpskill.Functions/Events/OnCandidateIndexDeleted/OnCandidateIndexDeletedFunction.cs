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
using Microsoft.Rest.Azure;

namespace AzureUpskill.Functions.Events.OnCandidateIndexDeleted
{
    public class OnCandidateIndexDeletedFunction
    {
        public const string Name = "OnCandidateIndexDeleted";

        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCandidateIndexDeletedFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Name)]
        public async Task OnCandidateDeleted_Indexing_Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = Name,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CandidateDocument> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                await RunIndexingAsync(input, log);
            }
        }

        private async Task RunIndexingAsync(IEnumerable<CandidateDocument> candidateDocuments, ILogger log)
        {
            var collectionName = "candidates";
            try
            {
                log.LogInformationEx($"Start indexing {collectionName}");
                var searchClientIndex = searchIndexClientRegistry.GetSearchIndexClient<CandidateIndex>(CandidateIndex.IndexNameConst);

                var deletedCandidates = candidateDocuments.Where(c => c.Status == Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{deletedCandidates.Count} deleted {collectionName}");
                if (deletedCandidates.Count > 0)
                {
                    var deletedBatch = IndexBatch.Delete(
                        mapper.Map<IEnumerable<CandidateIndex>>(deletedCandidates));
                    await searchClientIndex.Documents.IndexAsync(deletedBatch);
                    log.LogInformation($"Deleted {collectionName} removed from index");
                }

                log.LogInformationEx($"Indexing {collectionName} finished successfully");
            }
            catch (IndexBatchException batchException)
            {
                log.LogErrorEx(
                    batchException,
                    string.Format(
                        $"Indexing failed for some {collectionName} documents: {0}",
                        string.Join(", ", batchException.IndexingResults.Where(ir => !ir.Succeeded).Select(ir => $"{ir.Key} - {ir.StatusCode}: {ir.ErrorMessage}"))));
            }
            catch (CloudException cloudException)
            {
                log.LogErrorEx(cloudException, "Indexing failed");
            }
        }
    }
}
