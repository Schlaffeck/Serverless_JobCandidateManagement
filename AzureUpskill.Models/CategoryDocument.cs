using Newtonsoft.Json;

namespace AzureUpskill.Models
{
    public class CategoryDocument : Category
    {
        [JsonProperty("_self")]
        public string SelfLink { get; set; }
    }
}
