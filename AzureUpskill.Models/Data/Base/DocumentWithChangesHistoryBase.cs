using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AzureUpskill.Models.Data.Base
{
    public abstract class DocumentWithChangesHistoryBase : DocumentStatusInfoBase, IDocumentWithChangesHistory
    {
        [JsonProperty("__changedPropertiesOldValues")]
        public Dictionary<string, object> ChangedPropertiesOldValues { get; set; } = new Dictionary<string, object>();
    }
}
