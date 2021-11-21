using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi;
using Operations = Coomes.Equipper.Operations;

namespace Equipper.FunctionApp
{
    public static class CreateSubscription
    {
        [FunctionName("CreateSubscription")]
        public static async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "CreateSubscription", "Starting", correlationID.ToString());

            await ExecuteCreateSubscription(log);
            
            log.LogInformation("{function} {status} {cid}", "CreateSubscription", "Success", correlationID.ToString());
        }

        private static async Task ExecuteCreateSubscription(ILogger logger) 
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                 ClientId = Settings.ClientId,
                 ClientSecret = Settings.ClientSecret
            };
            var callback = Settings.EventCallbackUri;
            var verifyToken = Settings.SubscriptionVerificationToken;
            var subscriptionClient = new SubscriptionClient(options, logger);
            var createOperation = new Operations.CreateSubscription(subscriptionClient);

            await createOperation.Execute(callback, verifyToken);
        }
    }
}
