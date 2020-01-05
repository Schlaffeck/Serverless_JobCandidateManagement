using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Filters
{
    [Obsolete]
    public class ExecutionLoggingAttribute : FunctionInvocationFilterAttribute
    {
        public override Task OnExecutingAsync(FunctionExecutingContext context, CancellationToken cancellationToken)
        {
            context.Logger.Log(LogLevel.Information, $"{context.FunctionName} - START");
            return base.OnExecutingAsync(context, cancellationToken);
        }

        public override Task OnExecutedAsync(FunctionExecutedContext context, CancellationToken cancellationToken)
        {
            context.Logger.Log(LogLevel.Information, $"{context.FunctionName} - END");
            return base.OnExecutedAsync(context, cancellationToken);
        }
    }
}
