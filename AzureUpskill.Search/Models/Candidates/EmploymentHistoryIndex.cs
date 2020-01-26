using Microsoft.Azure.Search;

namespace AzureUpskill.Search.Models.Candidates
{
    public class EmploymentHistoryIndex
    {
        [IsFilterable]
        public string Company { get; set; }
    }
}
