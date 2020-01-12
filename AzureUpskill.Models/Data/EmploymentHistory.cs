using System;

namespace AzureUpskill.Models.Data
{
    public class EmploymentHistory
    {
        public string Company { get; set; }

        public string Position { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
