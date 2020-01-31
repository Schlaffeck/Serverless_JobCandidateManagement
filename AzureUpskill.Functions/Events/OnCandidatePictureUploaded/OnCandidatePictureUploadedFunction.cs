using System.Threading.Tasks;
using AzureUpskill.Models.Data;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUpskill.Functions.Events.OnCandidatePictureUploaded
{
    class OnCandidatePictureUploadedFunction
    {
        [FunctionName(nameof(OnCandidatePictureUploaded))]
        public async Task OnCandidatePictureUploaded(
            [BlobTrigger(Consts.Storage.CandidatesPicturesBlobContainerName
                         + "/categories/{categoryId}/candidates/{candidateId}/{blobName}.{blobExtension}",
                Connection = Consts.Storage.ConnectionStringName)]
            ICloudBlob cloudBlob,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                PartitionKey = "{categoryId}",
                Id = "{candidateId}",
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)]
            CandidateDocument candidate,
            [CosmosDB(
                databaseName: Consts.CosmosDb.DbName,
                collectionName: Consts.CosmosDb.CandidatesContainerName,
                ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName)]
            DocumentClient documentClient,
            string categoryId,
            ILogger logger)
        {
            if (cloudBlob is null || candidate is null)
            {
                return;
            }

            candidate.PictureUri = cloudBlob.Uri.AbsoluteUri;
            await documentClient.ReplaceDocumentAsync(candidate.SelfLink, candidate, new RequestOptions
            {
                PartitionKey = new PartitionKey(categoryId)
            });
        }
    }
}
