﻿using System;
using System.Collections.Generic;
using System.Text;
using AzureUpskill.Models.Data;
using AzureUpskill.Models.UpdateCandidate;

namespace AzureUpskill.Models.Commands.UpdateCandidate
{
    public class CreateCandidateInNewCategoryCommand
    {
        public UpdateCandidateInput UpdateCandidateInput { get; set; }

        public CandidateDocument ExistingCandidate { get; set; }

        public CategoryDocument NewCategory { get; set; }
    }
}
