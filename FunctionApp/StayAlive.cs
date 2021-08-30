using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace FunctionApp
{
    public static class StayAlive
    {
        [FunctionName("StayAlive")]
        public static void Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "StayAlive", "Starting", correlationID.ToString());
            log.LogInformation("{function} {status} {cid}", "StayAlive", "Success", correlationID.ToString());
        }
    }
}
