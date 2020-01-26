using Microsoft.Azure.Search;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Models.Candidates
{
    public class CandidateIndex : IIndexData
    {
        public static string Name = "Candidates_AZSearch_Index";

        [System.ComponentModel.DataAnnotations.Key]
        [IsFilterable]
        public string Id { get; set; }

        [IsFilterable]
        public string CategoryId { get; set; }

        [IsFilterable, IsSearchable]
        public string CategoryName { get; set; }

        [IsSearchable, IsSortable]
        public string FirstName { get; set; }

        [IsSearchable, IsSortable]
        public string LastName { get; set; }

        public AddressIndex Address { get; set; }

        [IsFilterable, IsSortable]
        public int EmploymentFullMonths { get; set; }

        [IsFilterable, IsSortable]
        public int EmploymentFullYears { get; set; }

        [IsFilterable]
        public string EducationLevel { get; set; }

        public IEnumerable<EmploymentHistoryIndex> EmploymentHistory { get; set; }

        public IEnumerable<EducationHistoryIndex> EducationHistory { get; set; }

        [JsonIgnore]
        public string IndexName => Name;
    }
}
