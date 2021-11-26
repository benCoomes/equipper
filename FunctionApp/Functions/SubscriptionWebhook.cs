using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Operations;
using Microsoft.AspNetCore.Mvc;
using Coomes.Equipper.StravaApi.Models;
using Coomes.Equipper.StravaApi;
using Coomes.Equipper.CosmosStorage;
using System.Net;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public static class SubscriptionWebhook
    {
        [FunctionName("SubscriptionWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // todo: anonymous endpoint - rate limiting? don't trust anything
            // make sure no activity or athlete information is returned in response.
            // do not make any changes to athlete data that would allow for abuse by malicious callers
            switch(req.Method)
            {
                case "GET":
                {
                    return await  ConfirmSubscription(req, log);
                }
                case "POST":
                {
                    return await ProcessEvent(req, log);
                }
                default:
                {
                    log.LogWarning($"The SubscriptionWebhook function was called with the unsupported HTTP method {req.Method}");
                    return new StatusCodeResult((int)HttpStatusCode.MethodNotAllowed);
                }
            }
        }

        private static Task<IActionResult> ConfirmSubscription(HttpRequest req, ILogger logger)
        {
            var correlationID = Guid.NewGuid();
            logger.LogInformation("{function} {status} {cid}", "SubscriptionWebhook - ConfirmationSubscription", "Starting", correlationID.ToString());

            var challenge = req.Query["hub.challenge"];
            if(challenge == String.Empty)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Subscription confirmations must contain a 'hub.challenge' query parameter."));
            }

            var verifyToken = req.Query["hub.verify_token"];
            if(verifyToken == String.Empty)
            {
                return Task.FromResult<IActionResult>(new BadRequestObjectResult("Subscription confirmations must contain a 'hub.verify_token' query parameter."));
            }

            // todo: better way to build dependencies?
            var expectedToken = Settings.SubscriptionVerificationToken;
            var getConfirmation = new GetSubscriptionConfirmation(logger);

            var confirmation = getConfirmation.Execute(challenge, verifyToken, expectedToken);

            logger.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ConfirmationSubscription", "Success", correlationID.ToString());
            return Task.FromResult<IActionResult>(new OkObjectResult(confirmation));
        }

        private static async Task<IActionResult> ProcessEvent(HttpRequest req, ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook - ProcessEvent", "Starting", correlationID.ToString());

            var body = await req.ReadAsStringAsync();
            log.LogInformation("{callbackEvent}", body);

            var stravaEvent = StravaEvent.FromJsonString(body);
            await ExecuteEventAction(stravaEvent, log);

            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ProcessEvent", "Success", correlationID.ToString());
            return new OkResult();
        }

        private static async Task ExecuteEventAction(StravaEvent stravaEvent, ILogger log)
        {
            // todo: move this logic to testable class in Api. Will require DI to build resulting operation.
            if (
                stravaEvent.object_type == StravaEvent.ObjectTypes.Activity 
                && stravaEvent.aspect_type == StravaEvent.AspectTypes.Create)
            {
                log.LogDebug("Running set gear operation.");
                await ExecuteSetGear(stravaEvent, log);
            }
            else if (
                stravaEvent.object_type == StravaEvent.ObjectTypes.Athlete 
                && stravaEvent.aspect_type == StravaEvent.AspectTypes.Update
                && stravaEvent.updates.ContainsKey("authorized")
                && stravaEvent.updates["authorized"] == "false")
            {
                log.LogDebug("Unsubscribing athlete.");
                await ExecuteAthleteUnsubscribe(stravaEvent, log);
            }
            else
            {
                log.LogDebug("Ignoring event.");
            }
        }

        private static async Task ExecuteSetGear(StravaEvent stravaEvent, ILogger log)
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                ClientId = Settings.ClientId,
                ClientSecret = Settings.ClientSecret
            };
            var tokenProvider = new TokenClient(options, log);
            var tokenStorage = new TokenStorage(Settings.CosmosConnectionString);
            var activityData = new ActivityClient(log);
            var setGearOperation = new SetGear(activityData, tokenStorage, tokenProvider, log);

            await setGearOperation.Execute(stravaEvent.owner_id, stravaEvent.object_id);
        }

        private static async Task ExecuteAthleteUnsubscribe(StravaEvent stravaEvent, ILogger log)
        {
            // todo: better way to build dependencies?
            var tokenStorage = new TokenStorage(Settings.CosmosConnectionString);
            var unsubscribeAthleteOperation = new UnsubscribeAthlete(tokenStorage, log);
            await unsubscribeAthleteOperation.Execute(stravaEvent.object_id);
        }
    }
}
