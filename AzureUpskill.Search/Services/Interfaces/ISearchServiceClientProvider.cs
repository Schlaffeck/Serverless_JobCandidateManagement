using Microsoft.Azure.Search;

namespace AzureUpskill.Search.Services.Interfaces
{
    public interface ISearchServiceClientProvider
    {
        ISearchServiceClient Client { get; }
    }
}
