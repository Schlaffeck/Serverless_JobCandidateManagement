using AzureUpskill.Search.Models;
using Microsoft.Azure.Search;

namespace AzureUpskill.Search.Services.Interfaces
{
    public interface ISearchIndexClientRegistry
    {
        ISearchIndexClient GetSearchIndexClient<TIndexed, TIndexType>(string indexName)
            where TIndexed : class
            where TIndexType : class, ISearchIndexDescriptor;

        void InvalidateAll();

        void Invalidate(string indexName);
    }
}
