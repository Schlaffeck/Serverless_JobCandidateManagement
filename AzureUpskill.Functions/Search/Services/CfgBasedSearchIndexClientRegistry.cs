using AzureUpskill.Helpers;
using AzureUpskill.Search.Helpers;
using AzureUpskill.Search.Models;
using AzureUpskill.Search.Services;
using AzureUpskill.Search.Services.Interfaces;
using Microsoft.Azure.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace AzureUpskill.Functions.Search.Services
{
    public class CfgBasedSearchIndexClientRegistry : ISearchIndexClientRegistry, ISearchServiceClientProvider
    {
        private readonly ILogger log;
        private string searchServiceName;
        private string searchServiceApiKey;
        private IDictionary<string, ISearchIndexClient> clients = new Dictionary<string, ISearchIndexClient>();

        public CfgBasedSearchIndexClientRegistry(
            IConfiguration configuration,
            ILogger log)
        {
            searchServiceName = configuration[AzureUpskill.Search.Constants.SearchServiceNameConfigKey];
            searchServiceApiKey = configuration[AzureUpskill.Search.Constants.SearchServiceQueryApiKeyConfigKey];
            this.Client = new SearchServiceClient(searchServiceName, new SearchCredentials(searchServiceApiKey));
            this.log = log;
        }

        public ISearchServiceClient Client { get; }

        public ISearchIndexClient GetSearchIndexClient<TIndexed, TIndexType>(string indexName)
            where TIndexed : class
            where TIndexType : class, ISearchIndexDescriptor
        {
            if(!clients.ContainsKey(indexName))
            {
                log?.LogInformationEx($"Creating search index client of name '{indexName}' and type {typeof(TIndexType).FullName}");
                this.Client.InitializeIndexIfNotExists<TIndexType>(indexName);
                var indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(searchServiceApiKey));
                clients.Add(indexName, indexClient);
                log?.LogInformationEx($"Search index client of name '{indexName}' created properly");
            }

            return clients[indexName];
        }

        public void Invalidate(string indexName)
        {
            if(clients.ContainsKey(indexName))
            {
                clients.Remove(indexName);
                log?.LogInformationEx($"Search index client of name '{indexName}' invalidated");
            }
        }

        public void InvalidateAll()
        {
            clients.Clear();
            log?.LogInformationEx($"All search index clients invalidated");
        }
    }
}
