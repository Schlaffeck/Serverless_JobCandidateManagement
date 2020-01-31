using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Functions.Helpers.Storage;
using AzureUpskill.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace AzureUpskill.Functions.Queries.GetCandidateCvDownloadLink
{
    public class GetCandidateCvDownloadLinkFunction
    {
        public const string Name = "Candidate_GetCvDownloadLink";

        [FunctionName(Name)]
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
    }
}
