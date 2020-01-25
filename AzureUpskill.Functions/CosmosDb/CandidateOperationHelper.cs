using AzureUpskill.Models.Data;
using AzureUpskill.Models.Data.Base;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.CosmosDb
{
    public static class CandidateOperationHelper
    {
        internal static async Task<ResourceResponse<Document>> MarkCandidateForDeletionAsync(this CandidateDocument document, DocumentClient documentClient)
        {
            document.Status = DocumentStatus.Deleted;
            document.UpdatedAt = DateTime.Now;
            return await documentClient.ReplaceDocumentAsync(document.SelfLink, document, new RequestOptions
            {
                PartitionKey = new PartitionKey(document.PartitionKey)
            });
        }
    }
}
