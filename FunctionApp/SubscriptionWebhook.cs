using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi;
using Operations = Coomes.Equipper.Operations;
using Coomes.Equipper.Operations;
using Microsoft.AspNetCore.Mvc;

namespace Equipper.FunctionApp
{
    public static class SubscriptionWebhook
    {
        [FunctionName("SubscriptionWebhook")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // todo: anonymous endpoint - rate limiting? don't trust anything
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
                    return new BadRequestObjectResult("Unsupported HTTP method.");
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
            var options = new StravaApiOptions()
            {
                 ClientId = Environment.GetEnvironmentVariable("StravaApi__ClientId"),
                 ClientSecret = Environment.GetEnvironmentVariable("StravaApi__ClientSecret")
            };
            var subscriptionClient = new SubscriptionClient(options, logger);
            var getConfirmation = new GetSubscriptionConfirmation(subscriptionClient, logger);

            var confirmation = getConfirmation.Execute(challenge, verifyToken);

            logger.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ConfirmationSubscription", "Success", correlationID.ToString());
            return Task.FromResult<IActionResult>(new OkObjectResult(confirmation));
        }

        private static async Task<IActionResult> ProcessEvent(HttpRequest req, ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook - ProcessEvent", "Starting", correlationID.ToString());

            var body = await req.ReadAsStringAsync();
            log.LogInformation("{callbackEvent}", body);

            log.LogInformation("{function} {status} {cid}", "SubscriptionWebhook  - ProcessEvent", "Success", correlationID.ToString());
            return new OkResult();
        }

    }
}
