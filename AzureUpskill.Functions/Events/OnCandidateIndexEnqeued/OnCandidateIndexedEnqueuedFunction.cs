using AzureUpskill.Functions.Events.OnCandidateIndexEnqeued.Models;
using AzureUpskill.Helpers;
using AzureUpskill.Models.Data.Base;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
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
            [SignalR(
                HubName = Consts.Notifications.CandidateCreatedNotificationHubName,
                ConnectionStringSetting = Consts.Notifications.SignalRConnectionStringName)] 
                IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
            log.LogInformationEx($"Queue message processing: {queueItem}");

            var message = JsonConvert.DeserializeObject<IndexedCandidateQueueItem>(queueItem);
            await NotifySubscribersAsync(message, signalRMessages, log);
            log.LogInformationEx("Queue message processed");
        }

        private async Task NotifySubscribersAsync(IndexedCandidateQueueItem message, IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            if (message.Status == DocumentStatus.New || message.Status == DocumentStatus.Moved)
            {
                log.LogInformationEx("Send new candidate available notification");
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        GroupName = Consts.Notifications.OnNewCandidateAvailableGroupName,
                        Target = "onNewCandidateAvailable",
                        Arguments = new object[]{ message }
                    });

                log.LogInformationEx("Send new candidate in category notification");
                await signalRMessages.AddAsync(
                    new SignalRMessage
                    {
                        GroupName = $"{Consts.Notifications.OnNewCandidateInCategoryGroupNamePrefix}{message.CategoryId}",
                        Target = "onNewCandidateInCategory",
                        Arguments = new object[] { message }
                    });
            }

        }
    }
}
