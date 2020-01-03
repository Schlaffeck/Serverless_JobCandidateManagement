using AzureUpskill.Core;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AzureUpskill.Functions.Validation
{
    public static class HttpRequestValidationExtensions
    {
        public static IActionResult ToBadRequest<T>(this Result<T> validatedHttpRequest)
        {
            return new BadRequestObjectResult(validatedHttpRequest.Errors.Select(e => new {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            }));
        }
    }
}
