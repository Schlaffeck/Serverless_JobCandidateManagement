using Newtonsoft.Json;
using System;

namespace AzureUpskill.Models.Data.Base
{
    public interface IDocumentStatusInfo
    {
        [JsonProperty("__createdAt")]
        DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("__updatedAt")]
        DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("__status")]
        DocumentStatus Status { get; set; }
    }
}
