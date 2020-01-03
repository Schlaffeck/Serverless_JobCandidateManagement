using Newtonsoft.Json;

namespace AzureUpskill.Models
{
    public class CandidateDocument : Candidate
    {
        [JsonProperty("_self")]
        public string SelfLink { get; set; }
    }
}
