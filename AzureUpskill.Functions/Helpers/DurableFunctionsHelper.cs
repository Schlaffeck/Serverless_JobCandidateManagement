using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Helpers
{
    public static class DurableFunctionsHelper
    {
        public static async Task<DurableOrchestrationStatus> WaitForOrchestratedFunctionAsync(
            this IDurableOrchestrationClient durableOrchestrationClient,
            string orchestrationId)
        {
            var status = await durableOrchestrationClient.GetStatusAsync(orchestrationId);
            while (status.RuntimeStatus == OrchestrationRuntimeStatus.Running
                || status.RuntimeStatus == OrchestrationRuntimeStatus.Pending)
            {
                await Task.Delay(300);
                status = await durableOrchestrationClient.GetStatusAsync(orchestrationId);
            }
            return status;
        }

        public static async Task<TResult> WaitForOrchestratedFunctionResultAsync<TResult>(
            this IDurableOrchestrationClient durableOrchestrationClient,
            string orchestrationId)
        {
            var status = await WaitForOrchestratedFunctionAsync(durableOrchestrationClient, orchestrationId);
            return status.Output.ToObject<TResult>();
        }
    }
}
