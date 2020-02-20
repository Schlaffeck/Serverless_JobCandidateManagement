using AzureFunctions.Extensions.Swashbuckle.Attribute;
using AzureUpskill.Functions.Commands.SubscribeToNewCandidateAvailable.Models;
using AzureUpskill.Functions.Commands.SubscribeToNewCandidateInCategory.Models;
using AzureUpskill.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureUpskill.Functions.Commands.SubscribeToCandidateInCategoryCreated
{
    public class SubscribeToNewCandidateInCategoryFunction
    {
        public const string Name = "SubscribeToNewCandidateInCategory";

        [FunctionName(Name)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> SubscribeToNewCandidateInCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscriptions/onNewCandidateInCategory")]
            [RequestBodyType(typeof(SubscribeToNewCandidateAvailableInput), "Subscribtion input")]
                SubscribeToNewCandidateInCategoryInput input,
            [SignalR(
                HubName = Consts.Notifications.CandidateCreatedNotificationHubName,
                ConnectionStringSetting = Consts.Notifications.SignalRConnectionStringName)]
            [SwaggerIgnore]
                    IAsyncCollector<SignalRGroupAction> signalRGroupActions,
            ILogger log)
        {
            log.LogInformationEx($"User '{input.UserId}' subscribing to new candidate in category '{input.CategoryId}' event");
            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = input.UserId,
                    GroupName = $"{Consts.Notifications.OnNewCandidateInCategoryGroupNamePrefix}{input.CategoryId}",
                    Action = GroupAction.Add
                });

            return new AcceptedResult();
        }
    }
}
