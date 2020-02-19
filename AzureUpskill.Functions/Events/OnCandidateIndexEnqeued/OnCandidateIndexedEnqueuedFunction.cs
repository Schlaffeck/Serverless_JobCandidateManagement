using AzureUpskill.Functions.Events.OnCandidateIndexEnqeued.Models;
using AzureUpskill.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Events.OnCandidateIndexEnqeued
{
    public class OnCandidateIndexedEnqueuedFunction
    {
        public const string Name = "OnCandidateIndexEnqeued";

        [FunctionName(Name)]
        public async Task OnCandidateIndexEnqeued(
            [QueueTrigger(
                Consts.Queues.CandidatesIndexedQueueName,
                Connection = Consts.Queues.ConnectionStringName)] string queueItem,
            ILogger log)
        {
            log.LogInformationEx($"Queue message processing: {queueItem}");

            var message = JsonConvert.DeserializeObject<IndexedCandidateQueueItem>(queueItem);

            // TODO: send to signalr hub

            log.LogInformationEx("Queue message processed");
        }
    }
}
