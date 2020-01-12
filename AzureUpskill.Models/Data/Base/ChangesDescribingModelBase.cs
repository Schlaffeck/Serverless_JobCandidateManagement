using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AzureUpskill.Models.Data.Base
{
    public abstract class ChangesDescribingModelBase
    {
        [JsonProperty("__createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("__updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("__changedPropertiesOldValues")]
        public Dictionary<string, object> ChangedPropertiesOldValues { get; set; } = new Dictionary<string, object>();

        [JsonProperty("__status")]
        public DocumentStatus Status { get; set; } = DocumentStatus.New;
    }
}
