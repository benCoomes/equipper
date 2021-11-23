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
    public static class GetMoreSecrets
    {
        [FunctionName("GetMoreSecrets")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var secret = Environment.GetEnvironmentVariable("EXTRA_SUPER_SECRET_APP_SETTING");
            return new OkObjectResult(secret);
        }
    }
}
