using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Models.Data
{
    public class Address
    {
        public string Country { get; set; }

        public string City { get; set; }

        public string AddressLine { get; set; }

        public string ZipCode { get; set; }
    }
}
