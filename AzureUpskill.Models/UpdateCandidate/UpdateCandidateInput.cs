using System.Collections.Generic;

namespace AzureUpskill.Models.UpdateCandidate
{
    public class UpdateCandidateInput
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Address Address { get; set; } = new Address();

        public ContactDetails ContactDetails { get; set; } = new ContactDetails();

        public string EducationLevel { get; set; }

        public List<EducationHistory> EducationHistory { get; set; } = new List<EducationHistory>();

        public int EmploymentFullMonths { get; set; }

        public List<EmploymentHistory> EmploymentHistory { get; set; } = new List<EmploymentHistory>();

        public List<Skill> Skills { get; set; } = new List<Skill>();
    }
}
