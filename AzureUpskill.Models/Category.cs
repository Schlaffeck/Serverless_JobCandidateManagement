using Newtonsoft.Json;

namespace AzureUpskill.Models
{
    public class Category
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string CategoryId { get; set; }

        public string Type { get; set; } = nameof(Category);
    }
}
