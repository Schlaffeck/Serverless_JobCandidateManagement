using AzureUpskill.Functions.Commands.UpdateCategory.Models;
using FluentValidation;

namespace AzureUpskill.Functions.Commands.UpdateCategory.Validation
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
