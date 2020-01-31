using AzureUpskill.Core.Validation;
using AzureUpskill.Functions.Commands.CreateCategory.Models;
using FluentValidation;

namespace AzureUpskill.Functions.Commands.CreateCategory.Validation
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
