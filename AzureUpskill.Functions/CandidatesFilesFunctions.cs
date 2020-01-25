using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Functions.Filters;
using AzureUpskill.Functions.Storage;
using AzureUpskill.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace AzureUpskill.Functions
{
    [ExecutionLogging]
    [ErrorHandler]
    public class CandidatesFilesFunctions
    {
        [FunctionName(Names.CandidateGetCvUploadLinkFunctionName)]
        public async Task<IActionResult> GetCandidateCvUploadLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route =
                "categories/{categoryId}/candidates/{candidateId}/documents/upload-link")]
            HttpRequest req,
            [StorageAccount(Consts.Storage.ConnectionStringName), SwaggerIgnore]
            CloudStorageAccount cloudStorageAccount,
            string categoryId,
            string candidateId,
            ILogger logger)
        {
            var blobRef =
                CloudBlobHelper.GetCandidateDocumentBlobReference(cloudStorageAccount, categoryId, candidateId);
            return new OkObjectResult(blobRef.GetUploadLink());
        }

        [FunctionName(Names.CandidateGetPictureUploadLinkFunctionName)]
        public async Task<IActionResult> GetCandidatePictureUploadLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route =
                "categories/{categoryId}/candidates/{candidateId}/pictures/upload-link")]
            HttpRequest req,
            [StorageAccount(Consts.Storage.ConnectionStringName), SwaggerIgnore]
            CloudStorageAccount cloudStorageAccount,
            string categoryId,
            string candidateId,
            ILogger logger)
        {
            var blobRef =
                CloudBlobHelper.GetCandidatePictureBlobReference(cloudStorageAccount, categoryId, candidateId);
            return new OkObjectResult(blobRef.GetUploadLink());
        }

        [FunctionName(Names.CandidateGetCvDownloadLinkFunctionName)]
        public async Task<IActionResult> GetCandidateCvDownloadLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route =
                "categories/{categoryId}/candidates/{candidateId}/documents/download-link")]
            HttpRequest req,
            [StorageAccount(Consts.Storage.ConnectionStringName), SwaggerIgnore]
            CloudStorageAccount cloudStorageAccount,
            [CosmosDB(
                 databaseName: Consts.CosmosDb.DbName,
                 collectionName: Consts.CosmosDb.CandidatesContainerName,
                 PartitionKey = "{categoryId}",
                 Id = "{candidateId}",
                 ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore]
            CandidateDocument candidate,
            string categoryId,
            string candidateId,
            ILogger logger)
        {
            if (candidate is null)
            {
                return new NotFoundObjectResult("Candidate not found");
            }

            var blobRef = CloudBlobHelper.GetFileBlobReference(cloudStorageAccount, candidate.CvDocumentUri);
            if (blobRef is null || !await blobRef.ExistsAsync())
            {
                return new NotFoundObjectResult($"File {candidate.PictureUri} not found in storage");
            }

            return new OkObjectResult(blobRef.GetDownloadLink());
        }

        [FunctionName(Names.CandidateGetPictureDownloadLinkFunctionName)]
        public async Task<IActionResult> GetCandidatePictureDownloadLink(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route =
                "categories/{categoryId}/candidates/{candidateId}/pictures/download-link")]
            HttpRequest req,
            [StorageAccount(Consts.Storage.ConnectionStringName), SwaggerIgnore]
            CloudStorageAccount cloudStorageAccount,
            [CosmosDB(
                 databaseName: Consts.CosmosDb.DbName,
                 collectionName: Consts.CosmosDb.CandidatesContainerName,
                 PartitionKey = "{categoryId}",
                 Id = "{candidateId}",
                 ConnectionStringSetting = Consts.CosmosDb.ConnectionStringName), SwaggerIgnore]
            CandidateDocument candidate,
            string categoryId,
            string candidateId,
            ILogger logger)
        {
            if (candidate is null)
            {
                return new NotFoundObjectResult("Candidate not found");
            }

            var blobRef = CloudBlobHelper.GetFileBlobReference(cloudStorageAccount, candidate.PictureUri);
            if (blobRef is null || !await blobRef.ExistsAsync())
            {
                return new NotFoundObjectResult($"File {candidate.PictureUri} not found in storage");
            }

            return new OkObjectResult(blobRef.GetDownloadLink());
        }

        [FunctionName(nameof(OnCandidateDocumentUploaded))]
        public async Task OnCandidateDocumentUploaded(
            [BlobTrigger(Consts.Storage.CandidatesDocumentsBlobContainerName
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

            candidate.CvDocumentUri = cloudBlob.Uri.AbsoluteUri;
            await documentClient.ReplaceDocumentAsync(candidate.SelfLink, candidate, new RequestOptions
            {
                PartitionKey = new PartitionKey(categoryId)
            });
        }

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

        public static class Names
        {
            public const string CandidateGetPictureDownloadLinkFunctionName = "Candidate_GetPictureDownloadLink";
            public const string CandidateGetCvDownloadLinkFunctionName = "Candidate_GetCvDownloadLink";
            public const string CandidateGetPictureUploadLinkFunctionName = "Candidate_GetPictureUploadLink";
            public const string CandidateGetCvUploadLinkFunctionName = "Candidate_GetCvUploadLink";
        }
    }
}
