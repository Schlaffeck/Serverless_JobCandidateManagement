using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Functions.Events.OnCandidateIndexModified.Models;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
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
using Newtonsoft.Json;

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
            [Queue(Consts.Queues.CandidatesIndexedQueueName,
                Connection = Consts.Queues.ConnectionStringName)] IAsyncCollector<string> outputQueueMessages,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                await RunIndexingAsync(input, categoriesDocumentClient, outputQueueMessages, log);
            }
        }

        private async Task RunIndexingAsync(
            IEnumerable<CandidateDocument> candidateDocuments,
            DocumentClient categoriesDocumentClient,
            IAsyncCollector<string> outputQueueMessages,
            ILogger log)
        {
            var collectionName = "candidates";
            try
            {
                log.LogInformationEx($"Start indexing {collectionName}");
                var newOrChangedCandidates = candidateDocuments.Where(c => c.Status != DocumentStatus.Deleted)
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

                    await EnqueueIndexingResultsAsync(candidateDocuments, indexResult, outputQueueMessages, log);
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

        private async Task EnqueueIndexingResultsAsync(
            IEnumerable<Candidate> candidatesProcessed,
            DocumentIndexResult indexResult,
            IAsyncCollector<string> oututQueueMessages,
            ILogger log)
        {
            log.LogInformationEx($"Post new candidates to queue");

            var successfullIndexes = candidatesProcessed.AsQueryable()
                .Where(cp => cp.IsNewOrMoved())
                .Join(
                indexResult.Results.Where(ir => ir.Succeeded),
                cp => cp.Id,
                ir => ir.Key,
                (ci, ir) => ci)
                    .ToList();

            log.LogInformationEx($"Found {successfullIndexes.Count} candidate indexes to enqueue");
            foreach(var successfullIndex in successfullIndexes)
            {
                var message = this.mapper.Map<CandidateIndexedQueueItem>(successfullIndex);
                var messageJson = JsonConvert.SerializeObject(message);
                await oututQueueMessages.AddAsync(messageJson);
            }

            await oututQueueMessages.FlushAsync();
            log.LogInformationEx($"New candidates documents posted to queue");
        }
    }
}
