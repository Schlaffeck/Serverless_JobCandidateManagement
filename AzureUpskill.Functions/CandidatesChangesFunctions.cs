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
using Newtonsoft.Json;

namespace AzureUpskill.Functions
{
    public class CandidatesChangesFunctions
    {
        [FunctionName(nameof(OnCandidateChanged))]
        public async Task OnCandidateChanged([CosmosDBTrigger(
            databaseName: Consts.CosmosDb.DbName,
            collectionName: Consts.CosmosDb.CandidatesContainerName,
            ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName,
            LeaseCollectionName = Consts.CosmosDb.LeasesContainerName,
            LeaseCollectionPrefix = nameof(OnCandidateChanged),
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<CandidateDocument> documents,
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
            log.LogInformationEx($"{documents.Count()} documents in change log");
            await ProcessingCandidateDocumentsAsync(documents, categoriesDocumentClient, candidatesDocumentClient, log);
        }

        private async Task ProcessingCandidateDocumentsAsync(IReadOnlyList<CandidateDocument> candidates, DocumentClient categoriesDocumentClient, DocumentClient candidatesDocumentClient, ILogger log)
        {
            log.LogInformationEx($"Processing {candidates.Count} documents of type '{Candidate.TypeName}'");
            foreach (var candidate in candidates)
            {
                if (candidate.IsNew() || candidate.Status == DocumentStatus.Moved)
                {
                    log.LogInformationEx($"{candidate.Type} ({candidate.Id}) created in category {candidate.CategoryId}");
                    await ProcessNewCandidate(candidate, categoriesDocumentClient, log);
                }
                else if(candidate.Status == DocumentStatus.Deleted)
                {
                    log.LogInformationEx($"{candidate.Type} ({candidate.Id}) marked for deletion from category {candidate.CategoryId}");
                    await ProcessCandidateMarkedForDeletion(candidate, categoriesDocumentClient, candidatesDocumentClient, log);
                }
            }
        }

        private async Task ProcessNewCandidate(
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
