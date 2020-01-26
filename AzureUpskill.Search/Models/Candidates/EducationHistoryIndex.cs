using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Models.Candidates
{
    public class EducationHistoryIndex
    {
        [IsSearchable]
        public string Institution { get; set; }

        [IsSearchable]
        public string Certificate { get; set; }
    }
}
