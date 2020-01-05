using AzureUpskill.Core;
using AzureUpskill.Core.Validation;
using FluentValidation;
using System;

namespace AzureUpskill.Helpers
{
    public static class ValidationHelper
    {
        public static Result<T> Validate<T,TValidator>(this T input)
            where TValidator : AbstractValidator<T>, new()
        {
            var validator = new TValidator();

            return new Result<T>(input, validator.Validate(input).Errors);
        }

        public static Result<object> Validate(this object input, Type validatorType)
        {
            var validator = Activator.CreateInstance(validatorType) as IModelValidator;
            if(validator is null)
            {
                throw new ArgumentException($"Improper validator type: {validatorType.FullName}");
            }

            return new Result<object>(input, validator.Validate(input).Errors);
        }
    }
}
