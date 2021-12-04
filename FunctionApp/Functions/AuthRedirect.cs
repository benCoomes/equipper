using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public static class AuthRedirect
    {
        [FunctionName("AuthRedirect")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "Echo", "Starting", correlationID.ToString());

            string value = req.Query["status"];
            string status = "error";

            if(string.Equals("success", value, StringComparison.CurrentCultureIgnoreCase))
            {
                status = "success";
            } 

            log.LogInformation("{function} {status} {cid}", "Echo", "Success", correlationID.ToString());
            var host = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            return new RedirectResult($"?authStatus={status}?host={host}", permanent: false);
        }
    }
}
