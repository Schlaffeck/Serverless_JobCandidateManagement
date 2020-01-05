using FluentValidation;
using FluentValidation.Results;

namespace AzureUpskill.Core.Validation
{
    public abstract class AbstractModelValidator<TModel> : AbstractValidator<TModel>, IModelValidator
    {
        public ValidationResult Validate(object model)
        {
            return base.Validate((TModel)model);
        }
    }
}
