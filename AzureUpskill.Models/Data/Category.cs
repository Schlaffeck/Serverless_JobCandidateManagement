using AzureUpskill.Models.Data.Base;
using Newtonsoft.Json;

namespace AzureUpskill.Models.Data
{
    public class Category : ChangesDescribingModelBase
    {
        public const string TypeName = nameof(Category);

        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }

        public string CategoryId { get; set; }

        public string Type { get; set; } = TypeName;

        public int NumberOfCandidates { get; set; }
    }
}
