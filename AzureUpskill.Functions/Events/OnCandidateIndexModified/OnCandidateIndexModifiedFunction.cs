using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Helpers;
using AzureUpskill.Search.Models.Candidates;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;

namespace AzureUpskill.Functions.Events.OnCandidateIndexModified
{
    public class OnCandidateIndexModifiedFunction
    {
        public const string Name = "OnCandidateIndexModified";

        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCandidateIndexModifiedFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Name)]
        public async Task OnCandidateModified_Indexing_Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = Name,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CandidateDocument> input,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient categoriesDocumentClient,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                await RunIndexingAsync(input, categoriesDocumentClient, log);
            }
        }

        private async Task RunIndexingAsync(
            IEnumerable<CandidateDocument> candidateDocuments,
            DocumentClient categoriesDocumentClient,
            ILogger log)
        {
            var collectionName = "candidates";
            try
            {
                log.LogInformationEx($"Start indexing {collectionName}");
                var newOrChangedCandidates = candidateDocuments.Where(c => c.Status != Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{newOrChangedCandidates.Count} new or changed {collectionName}");
                if (newOrChangedCandidates.Count > 0)
                {
                    var searchClientIndex = searchIndexClientRegistry.GetSearchIndexClient<CandidateIndex>(CandidateIndex.IndexNameConst);
                    var indexesList = new List<CandidateIndex>();
                    foreach (var candidateDocument in newOrChangedCandidates)
                    {
                        var candidateIndex = this.mapper.Map<CandidateIndex>(candidateDocument);
                        if(candidateDocument.IsNewOrMoved())
                        {
                            var categoryReadResult = await categoriesDocumentClient.ReadDocumentAsync<Category>(
                                UriFactory.CreateDocumentUri(Consts.CosmosDb.DbName, Consts.CosmosDb.CategoriesContainerName, candidateDocument.CategoryId)
                                , new RequestOptions
                                {
                                    PartitionKey = new PartitionKey(candidateDocument.PartitionKey)
                                });
                            this.mapper.Map(categoryReadResult.Document, candidateIndex);
                        }
                        indexesList.Add(candidateIndex);
                    }
                    var batch = IndexBatch.MergeOrUpload(indexesList);
                    var indexResult = await searchClientIndex.Documents.IndexAsync(batch);
                    log.LogInformationEx($"New or changed {collectionName} indexed");
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
