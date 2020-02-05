using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureUpskill.Functions.Helpers.CosmosDb;
using AzureUpskill.Functions.Helpers.CosmosDb.StoredProcedures;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using AzureUpskill.Models.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureUpskill.Functions.Events.OnCandidateDocumentDeleted
{
    public class OnCandidateDocumentDeletedFunction
    {
        public const string Name = nameof(OnCandidateDocumentDeleted);

        [FunctionName(Name)]
        public async Task OnCandidateDocumentDeleted([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = nameof(OnCandidateDocumentDeleted),
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
            var deletedCandidates = candidates.Where(c => c.Status == DocumentStatus.Deleted).ToList();
            if (deletedCandidates.Count == 0)
            {
                log.LogInformationEx($"No deleted docuuments of type {Candidate.TypeName}");
                await Task.CompletedTask;
            }

            log.LogInformationEx($"Processing {deletedCandidates.Count} deleted documents of type '{Candidate.TypeName}'");
            foreach (var candidate in deletedCandidates)
            {
                log.LogInformationEx($"{candidate.Type} ({candidate.Id}) marked for deletion from category {candidate.CategoryId}");
                await ProcessCandidateMarkedForDeletion(candidate, categoriesDocumentClient, candidatesDocumentClient, log);
            }
        }

        private async Task ProcessCandidateMarkedForDeletion(
            CandidateDocument candidate,
            DocumentClient categoriesDocumentClient,
            DocumentClient candidatesDocumentClient,
            ILogger log)
        {
            var deletionResult = await candidatesDocumentClient.DeleteDocumentAsync(
                        candidate.SelfLink,
                        new RequestOptions { PartitionKey = new PartitionKey(candidate.CategoryId) });
            if (deletionResult.StatusCode.IsSuccess())
            {
                await CommonProcedures.RunChangeCandidateCountStoredProcedureAsync(
                           categoriesDocumentClient,
                           candidate.CategoryId,
                           -1,
                           log);
            }
            else
            {
                log.LogErrorEx($"Deleting {candidate.Type} failed: {deletionResult.ToErrorString()}");
            }
        }
    }
}
