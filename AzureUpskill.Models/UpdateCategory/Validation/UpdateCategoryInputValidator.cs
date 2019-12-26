using AzureUpskill.Models.UpdateCategory;
using FluentValidation;

namespace AzureUpskill.Models.UpdateCategory.Validation
{
    public class UpdateCategoryInputValidator : AbstractValidator<UpdateCategoryInput>
    {
        public UpdateCategoryInputValidator()
        {
            RuleFor(_ => _).NotNull();
            RuleFor(_ => _.Name).NotEmpty();
        }
    }
}
