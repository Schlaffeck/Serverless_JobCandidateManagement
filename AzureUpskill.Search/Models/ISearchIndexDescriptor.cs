using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Models
{
    public interface ISearchIndexDescriptor
    {
        [JsonIgnore]
        string IndexName { get; }
    }
}
