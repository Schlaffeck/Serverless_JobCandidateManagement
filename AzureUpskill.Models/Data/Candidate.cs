using AzureUpskill.Models.Data.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AzureUpskill.Models.Data
{
    public class Candidate : DocumentStatusInfoBase
    {
        public const string TypeName = nameof(Candidate);

        [JsonProperty("id")]
        public string  Id { get; set; }

        public string CategoryId { get; set; }

        public string Type { get; set; } = TypeName;

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Address Address { get; set; } = new Address();

        public ContactDetails ContactDetails { get; set; } = new ContactDetails();

        public string CvDocumentUri { get; set; }

        public string PictureUri { get; set; }

        public string EducationLevel { get; set; }

        public List<EducationHistory> EducationHistory { get; set; } = new List<EducationHistory>();

        public int EmploymentFullMonths { get; set; }

        public List<EmploymentHistory> EmploymentHistory { get; set; } = new List<EmploymentHistory>();

        public List<Skill> Skills { get; set; } = new List<Skill>();
        public DateTimeOffset CreatedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DateTimeOffset? UpdatedAt { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DocumentStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
