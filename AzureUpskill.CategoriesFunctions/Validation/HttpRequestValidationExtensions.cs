﻿using AzureUpskill.Helpers.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AzureUpskill.CategoriesFunctions.Validation
{
    public static class HttpRequestValidationExtensions
    {
        public static IActionResult ToBadRequest<T>(this ValidatedHttpRequest<T> validatedHttpRequest)
        {
            return new BadRequestObjectResult(validatedHttpRequest.Errors.Select(e => new {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            }));
        }
    }
}