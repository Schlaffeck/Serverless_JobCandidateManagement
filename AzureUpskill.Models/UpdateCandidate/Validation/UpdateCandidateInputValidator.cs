using FluentValidation;
using System;

namespace AzureUpskill.Models.UpdateCandidate.Validation
{
    public class UpdateCandidateInputValidator : AbstractValidator<UpdateCandidateInput>
    {
        public UpdateCandidateInputValidator()
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
