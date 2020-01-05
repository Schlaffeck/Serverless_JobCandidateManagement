using AzureUpskill.Core.Validation;
using FluentValidation;

namespace AzureUpskill.Models.CreateCategory.Validation
{
    public class CreateCategoryInputValidator : AbstractModelValidator<CreateCategoryInput>
    {
        public CreateCategoryInputValidator()
        {
            RuleFor(_ => _).NotNull();
            RuleFor(_ => _.Name).NotEmpty();
        }
    }
}
