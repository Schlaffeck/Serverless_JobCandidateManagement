using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureUpskill.Search.Models.Candidates
{
    public class CandidateIndex : ISearchIndexDescriptor
    {
        public const string IndexNameConst = "azups001-search-candidates-index";

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

        public ContactDetailsIndex ContactDetails { get; set; }

        [IsFilterable, IsSortable]
        public int EmploymentFullMonths { get; set; }

        [IsFilterable, IsSortable]
        public int EmploymentFullYears { get; set; }

        [IsFilterable]
        public string EducationLevel { get; set; }

        public IEnumerable<EmploymentHistoryIndex> EmploymentHistory { get; set; }

        public IEnumerable<EducationHistoryIndex> EducationHistory { get; set; }

        [JsonIgnore]
        public string IndexName => IndexNameConst;

        public static IEnumerable<Field> GetIndexedFields()
        {
            return FieldBuilder.BuildForType<CandidateIndex>();
        }
    }
}
