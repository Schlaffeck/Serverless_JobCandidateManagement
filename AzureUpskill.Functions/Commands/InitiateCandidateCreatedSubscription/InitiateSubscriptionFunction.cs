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
        public static IActionResult InitiateSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest request,
            [SignalRConnectionInfo(
                HubName = Consts.Notifications.CandidateCreatedNotificationHubName,
                UserId = UserIdHeader,
                ConnectionStringSetting = Consts.Notifications.SignalRConnectionStringName)]
                SignalRConnectionInfo connectionInfo,
            ILogger log)
        {
            if(!request.Headers.TryGetValue(UserIdHeader, out StringValues userId))
            {
                return new UnauthorizedResult();
            }

            log.LogInformationEx($"User: {string.Join(", ", userId)} initiated subscription");

            return new OkObjectResult(connectionInfo);
        }
    }
}
