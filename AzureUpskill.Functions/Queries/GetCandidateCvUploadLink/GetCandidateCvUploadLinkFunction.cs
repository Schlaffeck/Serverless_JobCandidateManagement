using System.Threading.Tasks;
using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Functions.Helpers.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace AzureUpskill.Functions.Queries.GetCandidateCvUploadLink
{
    public class GetCandidateCvUploadLinkFunction
    {
        public const string Name = "Candidate_GetCvUploadLink";

        [FunctionName(Name)]
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
    }
}
