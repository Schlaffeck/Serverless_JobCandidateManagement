using System.Collections.Generic;

namespace AzureUpskill.Models.Data.Base
{
    public interface IDocumentWithChangesHistory : IDocumentStatusInfo
    {
        Dictionary<string, object> ChangedPropertiesOldValues { get; set; }
    }
}
