using Newtonsoft.Json;

namespace AzureUpskill.Models.Data.Base
{
    public interface IDocumentData
    {
        [JsonProperty("_self")]
        string SelfLink { get; }

        [JsonIgnore]
        string PartitionKey { get; }
        
        [JsonProperty("id")]
        string Id { get; }
    }
}
