using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp
{
    public static class Echo
    {
        [FunctionName("Echo")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "Echo", "Starting", correlationID.ToString());

            string value = req.Query["value"];

            string responseMessage = "";
            if(string.Equals("I'm an idiot", value, StringComparison.CurrentCultureIgnoreCase))
            {
                responseMessage = "You're an idiot!";
            }
            else if (!string.IsNullOrEmpty(value))
            {
                responseMessage = $"{value.ToUpper()}, {value}, {value.ToLower()}...";
            } 
            else
            {
                responseMessage = "Saying nothing, you hear only the silence of the void...";
            }

            log.LogInformation("{function} {status} {cid}", "Echo", "Success", correlationID.ToString());
            return new OkObjectResult(responseMessage);
        }
    }
}
