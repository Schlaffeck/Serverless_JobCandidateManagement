using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace AzureUpskill.Helpers
{
    public static class LogMessageHelper
    {
        public static void LogInfo(this ILogger log, string message, [CallerMemberName] string caller = null)
        {
            log.LogInformation($"{caller}:\t{message}");
        }
    }
}
