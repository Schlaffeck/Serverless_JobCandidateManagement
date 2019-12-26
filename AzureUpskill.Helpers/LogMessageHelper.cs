using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace AzureUpskill.Helpers
{
    public static class LogMessageHelper
    {
        public static void LogInformationEx(this ILogger log, string message, [CallerMemberName] string caller = null)
        {
            log.LogInformation($"{caller}:\t{message}");
        }
        public static void LogWarningEx(this ILogger log, string message, [CallerMemberName] string caller = null)
        {
            log.LogWarning($"{caller}:\t{message}");
        }

        public static void LogErrorEx(this ILogger log, Exception ex, string message, [CallerMemberName] string caller = null)
        {
            log.LogError(ex, $"{caller}:\t{message}");
        }
    }
}
