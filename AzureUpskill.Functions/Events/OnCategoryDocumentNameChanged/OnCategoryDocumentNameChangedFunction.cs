using AutoMapper;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Helpers;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Search.Services.Interfaces;
using AzureUpskill.Search.Models.Candidates;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using Microsoft.Azure.Documents;

namespace AzureUpskill.Functions.Events.OnCategoryDocumentNameChanged
{
    public class OnCategoryDocumentNameChangedFunction
    {
        public const string Name = "OnCategoryDocumentNameChangedFunction";

        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCategoryDocumentNameChangedFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Name)]
        public async Task Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CategoriesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = Name,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CategoryDocument> categories,
            [CosmosDB(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CategoriesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient categoriesDocumentClient,
            [CosmosDB(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)] DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            var categoriesWithChangedNames = categories.Where(c => c.HasChangedProperty(nameof(c.Name))).ToList();
            if (categoriesWithChangedNames.Count == 0)
            {
                log.LogInformationEx($"No {Category.TypeName} documents with changed name");
                return;
            }

            var searchIndexClient = this.searchIndexClientRegistry.GetSearchIndexClient<CandidateIndex>(CandidateIndex.IndexNameConst);
            foreach (var category in categoriesWithChangedNames)
            {
                await ProcessCategoryWithChangedName(categoriesDocumentClient, candidatesDocumentClient, searchIndexClient, category, log);
            }
        }

        private async Task ProcessCategoryWithChangedName(DocumentClient categoriesDocumentClient, DocumentClient candidatesDocumentClient, ISearchIndexClient candidatesSearchIndexClient, CategoryDocument category, ILogger log)
        {
            var collectionName = "candidates";
            try
            {
                log.LogInformationEx($"Reading candidates collection from category {category.Name} ({category.Id})");
                var candidatesInCategoryCollectionUrl = UriFactory.CreateDocumentCollectionUri(Consts.CosmosDb.DbName, Consts.CosmosDb.CandidatesContainerName);
                var candidatesInCategoryQuery = candidatesDocumentClient.CreateDocumentQuery<CandidateDocument>(
                    candidatesInCategoryCollectionUrl);

                var changedCandidateIndexes = new List<CandidateIndex>();
                foreach (var candidate in candidatesInCategoryQuery)
                {
                    var candidateIndex = this.mapper.Map<CandidateIndex>(candidate);
                    this.mapper.Map(category, candidateIndex);
                    changedCandidateIndexes.Add(candidateIndex);
                }
                var indexBatch = IndexBatch.Merge(changedCandidateIndexes);
                await candidatesSearchIndexClient.Documents.IndexAsync(indexBatch);
                log.LogInformation($"Category name for {collectionName} assigned to {category.Name} updated in search");
            }
            catch (DocumentClientException dce)
            {
                log.LogErrorEx(dce, "Reading documents from category failed");
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
