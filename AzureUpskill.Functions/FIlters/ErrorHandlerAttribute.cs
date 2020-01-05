using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Filters
{
    public class ErrorHandlerAttribute : FunctionExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            exceptionContext.Logger.Log(LogLevel.Error, $"{exceptionContext.FunctionName} - Error: {exceptionContext.Exception.GetType().Name},message:{exceptionContext.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}
