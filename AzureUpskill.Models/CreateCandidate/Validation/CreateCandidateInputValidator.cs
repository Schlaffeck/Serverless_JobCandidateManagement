using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Models.CreateCandidate.Validation
{
    public class CreateCandidateInputValidator : AbstractValidator<CreateCandidateInput>
    {
        public CreateCandidateInputValidator()
        {
            RuleFor(c => c).NotNull();
            RuleForEach(c => new[] { c.EducationLevel, c.FirstName, c.LastName });

            RuleForEach(c => c.EmploymentHistory)
                .ChildRules(eh => 
                eh.RuleFor(e => e.StartDate)
                .LessThan(e => e.EndDate ?? DateTime.Now));

            RuleForEach(c => c.EducationHistory)
                .ChildRules(eh =>
                eh.RuleFor(e => e.StartDate)
                .LessThan(e => e.EndDate ?? DateTime.Now));
        }
    }
}
