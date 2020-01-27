using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Candidates;
using AzureUpskill.Search.Models.Categories;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;

namespace AzureUpskill.Functions.Search
{
    public class OnCategoryIndexingFunction
    {
        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCategoryIndexingFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Names.OnCategoryIndexingFunctionName)]
        public async Task OnCategoryModified_Indexing_Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CategoriesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = Names.OnCategoryIndexingFunctionName,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CategoryDocument> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformationEx("Documents modified " + input.Count);
                await RunIndexingAsync(input, log);
            }
        }

        private async Task RunIndexingAsync(IEnumerable<CategoryDocument> categoryDocuments, ILogger log)
        {
            var collectionName = "categories";
            try
            {
                log.LogInformationEx($"Start indexing {collectionName}");
                var newOrChangedCategories = categoryDocuments.Where(c => c.Status != Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{newOrChangedCategories.Count} new or changed {collectionName}");
                if (newOrChangedCategories.Count > 0)
                {
                    var searchClientIndex = this.searchIndexClientRegistry.GetSearchIndexClient<CategoryIndex>(CandidateIndex.IndexNameConst);
                    var batch = IndexBatch.MergeOrUpload(
                        this.mapper.Map<IEnumerable<CategoryIndex>>(newOrChangedCategories));
                    var indexResult = await searchClientIndex.Documents.IndexAsync(batch);
                    log.LogInformationEx($"New or changed {collectionName} indexed");
                }

                var deletedCategories = categoryDocuments.Where(c => c.Status == Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{newOrChangedCategories.Count} deleted {collectionName}");
                if (deletedCategories.Count > 0)
                {
                    var deletedBatch = IndexBatch.Delete(
                        this.mapper.Map<IEnumerable<CategoryIndex>>(deletedCategories));
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

        public static class Names
        {
            public const string OnCategoryIndexingFunctionName = "OnCategoryIndexingFunction";
        }
    }
}
