using AzureUpskill.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureUpskill.Functions.Helpers
{
    public static class ResultHelper
    {
        public static IActionResult ToActionResult<T>(Result<T> result)
        {
            if(result.IsValid)
            {
                return new OkObjectResult(result);
            }

            return result.ToBadRequest();
        }

        public static IActionResult ToBadRequest<T>(this Result<T> result)
        {
            return new BadRequestObjectResult(result);
        }
    }
}
