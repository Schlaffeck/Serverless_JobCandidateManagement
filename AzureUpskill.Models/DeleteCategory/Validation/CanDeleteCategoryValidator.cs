using FluentValidation;

namespace AzureUpskill.Models.DeleteCategory.Validation
{
    public class CanDeleteCategoryValidator : AbstractValidator<Category>
    {
        public CanDeleteCategoryValidator()
        {
            RuleFor(c => c.NumberOfCandidates).Equal(0);
        }
    }
}
