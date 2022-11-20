using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public static class Echo
    {
        [FunctionName("Echo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            log.LogInformation("{function} {status} {cid} {userId}", "Echo", "Starting", correlationID.ToString(), user.UserId);

            string value = req.Query["value"];

            string responseMessage = "";
            if(string.Equals("I'm an idiot", value, StringComparison.CurrentCultureIgnoreCase))
            {
                if(user.Authenticated) {
                    responseMessage = $"You're an idiot, {user.DisplayName}!";
                } else {
                    responseMessage = "You're an idiot!";
                }
            }
            else if (!string.IsNullOrEmpty(value))
            {
                responseMessage = $"{value.ToUpper()}, {value}, {value.ToLower()}...";
            } 
            else
            {
                responseMessage = "Saying nothing, you hear only the silence of the void...";
            }

            log.LogInformation("{function} {status} {cid} {userId}", "Echo", "Success", correlationID.ToString(), user.UserId);
            return new OkObjectResult(responseMessage);
        }
    }
}
