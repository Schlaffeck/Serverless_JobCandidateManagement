using Microsoft.Azure.Search;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Search.Models.Candidates
{
    public class AddressIndex
    {
        [IsFilterable, IsSearchable]
        public string Country { get; set; }

        [IsFilterable, IsSearchable]
        public string City { get; set; }

        public string AddressLine { get; set; }

        public string ZipCode { get; set; }
    }
}
