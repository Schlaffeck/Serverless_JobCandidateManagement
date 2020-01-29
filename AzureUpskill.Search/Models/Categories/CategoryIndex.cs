using System.Collections.Generic;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;

namespace AzureUpskill.Search.Models.Categories
{
    public class CategoryIndex : ISearchIndexDescriptor
    {
        public const string IndexNameConst = "azups001-search-categories-index";

        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFilterable, IsSearchable]
        public string Name { get; set; }

        public string CategoryId { get; set; }

        [IsFilterable, IsSortable]
        public int NumberOfCandidates { get; set; }

        [JsonIgnore] public string IndexName => IndexNameConst;

        public IEnumerable<Field> GetIndexedFields()
        {
            return FieldBuilder.BuildForType<CategoryIndex>();
        }
    }
}
