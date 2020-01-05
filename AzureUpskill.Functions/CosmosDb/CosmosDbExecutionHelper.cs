using AzureUpskill.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.CosmosDb
{
    public static class CosmosDbExecutionHelper
    {
        public static async Task<IActionResult> RunInCosmosDbContext(Func<Task<IActionResult>> func, ILogger logger, [CallerMemberName] string callerMemberName = null)
        {
            return await RunInCosmosDbContext<IActionResult>(func, logger, callerMemberName);
        }

        public static async Task<IActionResult> RunInCosmosDbContext<TResult>(Func<Task<TResult>> func, ILogger logger, [CallerMemberName] string callerMemberName = null)
        {
            try
            {
                var result = await func();
                return result as IActionResult ?? new OkObjectResult(result);
            }
            catch (DocumentClientException dce)
            {
                var message = $"{callerMemberName} failed: {dce.Message}";
                logger.LogErrorEx(dce, message, callerMemberName);
                return new BadRequestObjectResult(new { Message = message, ErrorCode = dce.Error });
            }
        }
    }
}
