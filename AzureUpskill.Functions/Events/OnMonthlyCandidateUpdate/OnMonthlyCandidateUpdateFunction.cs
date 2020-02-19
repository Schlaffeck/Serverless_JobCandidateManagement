using AzureUpskill.Core;
using AzureUpskill.Functions.Events.OnMonthlyCandidateUpdate.Orchestration;
using AzureUpskill.Functions.Helpers;
using AzureUpskill.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Events.OnMonthlyCandidateUpdate
{
    public class OnMonthlyCandidateUpdateFunction
    {
        public const string Name = "OnMonthlyCandidateUpdate";

        public async Task OnMonthlyCandidateUpdate([TimerTrigger("0 0 3 1 * *")]TimerInfo info,
            [DurableClient] IDurableOrchestrationClient durableOrchestrationClient,
            ILogger log)
        {
            var orchestrationId = await durableOrchestrationClient.StartNewAsync(UpdateCandidatesEmploymentTimeOrchestratedFunction.Name);
            var result = await DurableFunctionsHelper.WaitForOrchestratedFunctionResultAsync<Result<int>>(durableOrchestrationClient, orchestrationId);

            log.LogInformationEx($"Updated {result.Value} candidates in databse");
        }
    }
}
