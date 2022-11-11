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
            logger.LogInformation("DEVELOPMENT_MODE: {devMode}", Settings.IsDevelopment);


            var correlationID = Guid.NewGuid();
            logger.LogInformation("{function} {status} {cid}", "GetActivityCount", "Starting", correlationID.ToString());

            var count = await ExecuteGetCount(logger);

            logger.LogInformation("{function} {status} {cid}", "GetActivityCount", "Success", correlationID.ToString());
            return new OkObjectResult(count);
        }

        private static Task<int> ExecuteGetCount(ILogger logger)
        {
            // todo: better way to build dependencies?
            var activityStorage = new ActivityStorage(Settings.CosmosConnectionString, disableSSL: Settings.IsDevelopment);
            var countOperation = new GetProccessedActivityCount(activityStorage, logger);
            return countOperation.Execute();
        }
    }
}
