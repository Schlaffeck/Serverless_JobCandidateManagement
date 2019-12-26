using AzureUpskill.Models.CreateCategory;
using FluentValidation;

namespace AzureUpskill.Models.CreateCategory.Validation
{
    public class CreateCategoryInputValidator : AbstractValidator<CreateCategoryInput>
    {
        public CreateCategoryInputValidator()
        {
            RuleFor(_ => _).NotNull();
            RuleFor(_ => _.Name).NotEmpty();
        }
    }
}
