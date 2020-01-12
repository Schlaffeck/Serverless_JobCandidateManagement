using FluentValidation;
using System;

namespace AzureUpskill.Models.UpdateCandidate.Validation
{
    public class UpdateCandidateInputValidator : AbstractValidator<UpdateCandidateInput>
    {
        public UpdateCandidateInputValidator()
        {
            RuleFor(c => c).NotNull().OverridePropertyName("self");
            RuleFor(c => c.CategoryId).NotEmpty().NotNull();
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
