using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.CosmosStorage;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public static class GetActivityCount
    {
        [FunctionName("ActivityCount")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger logger)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            logger.LogInformation("{function} {status} {cid} {userId}", "GetActivityCount", "Starting", correlationID.ToString(), user.UserId);

            var count = await ExecuteGetCount(logger);

            logger.LogInformation("{function} {status} {cid} {userId}", "GetActivityCount", "Success", correlationID.ToString(), user.UserId);
            return new OkObjectResult(count);
        }

        private static Task<int> ExecuteGetCount(ILogger logger)
        {
            // todo: better way to build dependencies?
            var activityStorage = new ActivityStorage(Settings.CosmosConnectionString);
            var countOperation = new GetProccessedActivityCount(activityStorage, logger);
            return countOperation.Execute();
        }
    }
}
