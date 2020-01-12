using AzureUpskill.Models.Data.Base;
using Newtonsoft.Json;

namespace AzureUpskill.Models.Data
{
    public class CategoryDocument : Category, IDocumentData
    {
        [JsonProperty("_self")]
        public string SelfLink { get; set; }

        public string PartitionKey => this.CategoryId;
    }
}
