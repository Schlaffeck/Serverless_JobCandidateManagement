using AzureUpskill.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.CosmosDb.StoredProcedures
{
    public static class CommonProcedures
    {
        public static async Task RunChangeCandidateCountStoredProcedureAsync(DocumentClient categoriesClient, string categoryId, int changeCountValue, ILogger logger = null)
        {
            logger?.LogInformationEx($"Run SP increment category {categoryId} count");
            try
            {
                var spUri = UriFactory.CreateStoredProcedureUri(Consts.CosmosDb.DbName, Consts.CosmosDb.CategoriesContainerName,
                    "changeCandidateCount");
                var result = await categoriesClient.ExecuteStoredProcedureAsync<dynamic>(
                    spUri, new RequestOptions { PartitionKey = new PartitionKey(categoryId) },
                    categoryId,
                    changeCountValue );
                if (result.StatusCode.IsSuccess())
                {
                    logger?.LogInformationEx("SP run succesfully");
                }
                else
                {
                    logger?.LogError($"SP failed with code {result.StatusCode}: {result.Response}");
                }
            }
            catch (DocumentClientException dce)
            {
                logger.LogErrorEx(dce, "SP failed");
            }
        }
    }
}
