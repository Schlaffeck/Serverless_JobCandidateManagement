using Microsoft.Azure.Search.Models;
using System.Collections.Generic;

namespace AzureUpskill.Functions.Queries.GetCategories.Models
{
    public class GetCategoriesQuery
    {
        public string SearchText { get; set; }

        public IList<string> Select { get; set; }

        public string Filter { get; set; }

        public IList<string> OrderBy { get; set; }

        public int? Skip { get; set; }

        public int? Top { get; set; }

        public bool IncludeTotalResultCount { get; set; }
    }
}
