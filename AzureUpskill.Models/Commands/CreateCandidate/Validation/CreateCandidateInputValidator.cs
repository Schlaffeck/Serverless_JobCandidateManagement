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
            RuleFor(c => c).NotNull().OverridePropertyName("self");
            RuleFor(c => c.FirstName).NotEmpty().NotNull();
            RuleFor(c => c.LastName).NotEmpty().NotNull();
            RuleFor(c => c.EducationLevel).NotEmpty().NotNull();

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
