using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Operations;
using Coomes.Equipper.StravaApi.Models;
using Coomes.Equipper.StravaApi;
using Coomes.Equipper.CosmosStorage;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public class SubscriptionWebhook
    {
        private readonly ILogger _logger;

        public SubscriptionWebhook(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SubscriptionWebhook>();
        }

        [Function("__SubscriptionWebhookPlaceholder__")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestData req)
        {
            switch(req.Method)
            {
                case "GET":
                {
                    return await ConfirmSubscription(req, _logger);
                }
                case "POST":
                {
                    return await ProcessEvent(req, _logger);
                }
                default:
                {
                    _logger.LogWarning($"The SubscriptionWebhook function was called with the unsupported HTTP method {req.Method}");
                    return req.CreateResponse(HttpStatusCode.MethodNotAllowed);
                }
            }
        }

        private static async Task<HttpResponseData> ConfirmSubscription(HttpRequestData req, ILogger logger)
        {
            var correlationID = Guid.NewGuid();
            logger.LogInformation("{function} {status} {cid}", "SubscriptionWebhook - ConfirmationSubscription", "Starting", correlationID.ToString());

            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var challenge = query["hub.challenge"];
            if(challenge == String.Empty)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Subscription confirmations must contain a 'hub.challenge' query parameter.");
                return errorResponse;
            }

            var verifyToken = query["hub.verify_token"];
            if(verifyToken == String.Empty)
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Subscription confirmations must contain a 'hub.verify_token' query parameter.");
                return errorResponse;
            }

            // todo: better way to build dependencies?
            var expectedToken = Settings.SubscriptionVerificationToken;
            var getConfirmation = new GetSubscriptionConfirmation(logger);

            var confirmation = getConfirmation.Execute(challenge, verifyToken, expectedToken);

            logger.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ConfirmationSubscription", "Success", correlationID.ToString());
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(confirmation);
            return response;
        }

        private static async Task<HttpResponseData> ProcessEvent(HttpRequestData req, ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook - ProcessEvent", "Starting", correlationID.ToString());

            var body = await req.ReadAsStringAsync();
            log.LogInformation("{callbackEvent}", body);

            var stravaEvent = StravaEvent.FromJsonString(body);
            await ExecuteEventAction(stravaEvent, log);

            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ProcessEvent", "Success", correlationID.ToString());
            
            return req.CreateResponse(HttpStatusCode.OK);
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
            var stravaData = new StravaClient(log);
            var activityStorage = new ActivityStorage(Settings.CosmosConnectionString);
            var setGearOperation = new SetGear(stravaData, activityStorage, tokenStorage, tokenProvider, log);

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
