using AzureUpskill.Models.Data.Base;
using Newtonsoft.Json;

namespace AzureUpskill.Models.Data
{
    public class CandidateDocument : Candidate, IDocumentData
    {
        [JsonProperty("_self")]
        public string SelfLink { get; set; }

        public string PartitionKey => this.CategoryId;
    }
}
