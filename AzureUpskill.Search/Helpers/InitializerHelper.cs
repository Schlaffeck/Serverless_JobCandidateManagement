using AzureUpskill.Search.Models;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

namespace AzureUpskill.Search.Helpers
{
    public static class InitializerHelper
    {
        public static void InitializeIndexIfNotExists<TIndexType>(
            this ISearchServiceClient searchServiceClient,
            string indexName)
            where TIndexType : class
        {
            if(!(searchServiceClient.Indexes.Exists(indexName)))
            {
                var indexDefinition = new Index
                {
                    Name = indexName,
                    Fields = FieldBuilder.BuildForType<TIndexType>(),
                };
                var index = searchServiceClient.Indexes.Create(indexDefinition);
            }
        }
    }
}
