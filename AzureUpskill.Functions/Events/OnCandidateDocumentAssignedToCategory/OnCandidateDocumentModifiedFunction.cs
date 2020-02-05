using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureUpskill.Functions.Helpers.CosmosDb.StoredProcedures;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureUpskill.Functions.Events.OnCandidateDocumentAssignedToCategory
{
    public class OnCandidateDocumentModifiedFunction
    {
        public const string Name = nameof(OnCandidateDocumentAssignedToCategory);

        [FunctionName(Name)]
        public async Task OnCandidateDocumentAssignedToCategory([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = nameof(OnCandidateDocumentAssignedToCategory),
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CandidateDocument> candidates,
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
            var candidatesNewInCategory = candidates.Where(c => c.IsNew() || c.Status == DocumentStatus.Moved).ToList();
            if (candidatesNewInCategory.Count == 0)
            {
                log.LogInformationEx($"No {Candidate.TypeName} documents new in categories");
                return;
            }

            log.LogInformationEx($"Processing {candidates.Count} documents of type '{Candidate.TypeName}'");
            foreach (var candidate in candidatesNewInCategory)
            {
                log.LogInformationEx($"{candidate.Type} ({candidate.Id}) created in category {candidate.CategoryId}");
                await ProcessCandidateNewInCategory(candidate, categoriesDocumentClient, log);
            }
        }

        private async Task ProcessCandidateNewInCategory(
            CandidateDocument candidate,
            DocumentClient categoriesDocumentClient,
            ILogger log)
        {
            await CommonProcedures.RunChangeCandidateCountStoredProcedureAsync(
                        categoriesDocumentClient,
                        candidate.CategoryId,
                        1,
                        log);
        }
    }
}
