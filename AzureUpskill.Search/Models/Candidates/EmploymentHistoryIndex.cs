using System;
using Microsoft.Azure.Search;

namespace AzureUpskill.Search.Models.Candidates
{
    public class EmploymentHistoryIndex
    {
        [IsFilterable]
        public string Company { get; set; }

        public string Position { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
