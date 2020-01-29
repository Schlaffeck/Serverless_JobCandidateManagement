using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Search.Models.Categories;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Azure;

namespace AzureUpskill.Functions.Events.OnCategoryIndexDeleted
{
    public class OnCategoryIndexDeletedFunction
    {
        public const string Name = "OnCategoryIndexDeleted";

        private readonly IMapper mapper;
        private readonly ISearchIndexClientRegistry searchIndexClientRegistry;

        public OnCategoryIndexDeletedFunction(
            IMapper mapper,
            ISearchIndexClientRegistry searchIndexClientRegistry)
        {
            this.mapper = mapper;
            this.searchIndexClientRegistry = searchIndexClientRegistry;
        }

        [FunctionName(Name)]
        public async Task OnCategoryDeleted_Indexing_Run([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CategoriesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = Name,
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CategoryDocument> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                await RunIndexingAsync(input, log);
            }
        }

        private async Task RunIndexingAsync(IEnumerable<CategoryDocument> categoryDocuments, ILogger log)
        {
            var collectionName = "categories";
            try
            {
                log.LogInformationEx($"Start indexing {collectionName}");

                var searchClientIndex = searchIndexClientRegistry.GetSearchIndexClient<CategoryIndex>(CategoryIndex.IndexNameConst);

                var deletedCategories = categoryDocuments.Where(c => c.Status == Models.Data.Base.DocumentStatus.Deleted)
                    .ToList();
                log.LogInformationEx($"{deletedCategories.Count} deleted {collectionName}");
                if (deletedCategories.Count > 0)
                {
                    var deletedBatch = IndexBatch.Delete(
                        mapper.Map<IEnumerable<CategoryIndex>>(deletedCategories));
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
