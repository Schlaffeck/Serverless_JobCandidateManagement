using System;

namespace AzureUpskill.Models.Data
{
    public class EducationHistory
    {
        public string Description { get; set; }

        public string Institution { get; set; }

        public string Degree { get; set; }

        public string Certificate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
