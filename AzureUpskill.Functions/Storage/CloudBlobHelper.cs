using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace AzureUpskill.Functions.Storage
{
    public static class CloudBlobHelper
    {
        public static CloudBlockBlob GetCandidateDocumentBlobReference(this CloudStorageAccount storageAccount, string categoryId, string candidateId)
        {
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var containerReference = cloudBlobClient.GetContainerReference(Consts.Storage.CandidatesDocumentsBlobContainerName);
            return containerReference.GetBlockBlobReference(ConstructBlobName(categoryId, candidateId));
        }

        public static CloudBlockBlob GetCandidatePictureBlobReference(this CloudStorageAccount storageAccount, string categoryId, string candidateId)
        {
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var containerReference = cloudBlobClient.GetContainerReference(Consts.Storage.CandidatesPicturesBlobContainerName);
            return containerReference.GetBlockBlobReference(ConstructBlobName(categoryId, candidateId));
        }

        public static CloudBlockBlob GetFileBlobReference(CloudStorageAccount storageAccount, string fileUri)
        {
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            return new CloudBlockBlob(new Uri(fileUri), cloudBlobClient);
        }

        public static string ConstructBlobName(string categoryId, string candidateId)
        {
            return $"categories/{categoryId}/candidates/{candidateId}";
        }

        public static string GetUploadLink(this CloudBlockBlob cloudBlockBlob, int validityInMinutes = 15)
        {
            var policy = new SharedAccessBlobPolicy();
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(validityInMinutes);
            policy.Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create;
            return $"{cloudBlockBlob.Uri}{cloudBlockBlob.GetSharedAccessSignature(policy)}";
        }

        public static string GetDownloadLink(this CloudBlockBlob cloudBlockBlob, int validityInMinutes = 15)
        {
            var policy = new SharedAccessBlobPolicy();
            policy.SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(validityInMinutes);
            policy.Permissions = SharedAccessBlobPermissions.Read;
            return $"{cloudBlockBlob.Uri}{cloudBlockBlob.GetSharedAccessSignature(policy)}";
        }
    }
}
