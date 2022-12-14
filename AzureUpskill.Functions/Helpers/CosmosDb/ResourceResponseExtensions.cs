using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.IO;

namespace AzureUpskill.Functions.Helpers.CosmosDb
{
    public static class ResourceResponseExtensions
    {
        public static string ToErrorString<TResource>(this ResourceResponse<TResource> response)
            where TResource : Document, new()
        {
            using (var streamReader = new StreamReader(response.ResponseStream))
            {
                return $"{response.StatusCode} - {streamReader.ReadToEnd()}";
            }
        }

        public static string ToErrorString<TResource>(this DocumentResponse<TResource> response)
            where TResource : new()
        {
            using (var streamReader = new StreamReader(response.ResponseStream))
            {
                return $"{response.StatusCode} - {streamReader.ReadToEnd()}";
            }
        }
    }
}
