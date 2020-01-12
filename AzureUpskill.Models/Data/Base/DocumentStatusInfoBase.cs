using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AzureUpskill.Models.Data.Base
{
    public abstract class DocumentStatusInfoBase : IDocumentStatusInfo
    {
        [JsonProperty("__createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("__updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("__status")]
        public DocumentStatus Status { get; set; } = DocumentStatus.New;
    }
}
