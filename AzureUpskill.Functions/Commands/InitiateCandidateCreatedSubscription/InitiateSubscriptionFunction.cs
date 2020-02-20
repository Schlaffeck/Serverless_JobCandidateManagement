using AzureUpskill.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace AzureUpskill.Functions.Commands.InitiateSubscription
{
    public class InitiateCandidateCreatedSubscriptionFunction
    {
        public const string Name = "InitiateCandidateCreatedSubscription";
        private const string UserIdHeader = "{headers.x-ms-client-principal-id}";

        [FunctionName(Name)]
        [ProducesResponseType(typeof(SignalRConnectionInfo), StatusCodes.Status200OK)]
        public static IActionResult InitiateSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscriptions/initiate/{userId}")] 
                HttpRequest request,
            [SignalRConnectionInfo(
                HubName = Consts.Notifications.CandidateCreatedNotificationHubName,
                UserId = "{userId}",
                ConnectionStringSetting = Consts.Notifications.SignalRConnectionStringName)]
                SignalRConnectionInfo connectionInfo,
            string userId,
            ILogger log)
        {
            log.LogInformationEx($"User: {string.Join(", ", userId)} initiated subscription");

            return new OkObjectResult(connectionInfo);
        }
    }
}
