using AzureUpskill.Core;
using FluentValidation;

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
    }
}
