using AzureUpskill.Models.Data;
using FluentValidation;

namespace AzureUpskill.Functions.Commands.DeleteCategory.Validation
{
    public class CanDeleteCategoryValidator : AbstractValidator<Category>
    {
        public CanDeleteCategoryValidator()
        {
            RuleFor(c => c.NumberOfCandidates).Equal(0);
        }
    }
}
