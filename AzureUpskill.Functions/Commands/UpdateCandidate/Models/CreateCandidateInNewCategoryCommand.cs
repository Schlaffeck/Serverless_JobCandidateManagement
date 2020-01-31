using System;
using System.Collections.Generic;
using System.Text;
using AzureUpskill.Models.Data;

namespace AzureUpskill.Functions.Commands.UpdateCandidate.Models
{
    public class CreateCandidateInNewCategoryCommand
    {
        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public CandidateDocument ExistingCandidate { get; set; }

        public CategoryDocument NewCategory { get; set; }
    }
}
