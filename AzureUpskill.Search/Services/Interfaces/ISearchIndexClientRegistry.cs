using AzureUpskill.Search.Models;
using Microsoft.Azure.Search;

namespace AzureUpskill.Search.Services.Interfaces
{
    public interface ISearchIndexClientRegistry
    {
        ISearchIndexClient GetSearchIndexClient<TIndexType>(string indexName)
            where TIndexType : class, IIndexData;

        void InvalidateAll();

        void Invalidate(string indexName);
    }
}
