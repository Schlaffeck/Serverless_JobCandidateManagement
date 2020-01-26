using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Models
{
    public interface IIndexData
    {
        [JsonIgnore]
        string IndexName { get; }
    }
}
