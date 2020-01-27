using Microsoft.Azure.Search;
using System;

namespace AzureUpskill.Search.Models.Candidates
{
    public class EducationHistoryIndex
    {
        [IsSearchable]
        public string Description { get; set; }

        [IsSearchable]
        public string Institution { get; set; }

        [IsSearchable]
        public string Certificate { get; set; }

        public string Degree { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
