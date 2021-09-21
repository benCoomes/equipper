using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi;
using Coomes.Equipper.Operations;

namespace Equipper.FunctionApp
{
    public static class TokenExchange
    {
        [FunctionName("TokenExchange")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "TokenExchange", "Starting", correlationID.ToString());

            string code = req.Query["code"];
            string scope = req.Query["scope"];
            string error = req.Query["error"];

            if(!string.IsNullOrWhiteSpace(error)) 
            {
                log.LogError("Recieved error at authorization callback: {authError}", error);
                return new BadRequestObjectResult($"Failed to authorize due to error: {error}");
            }

            log.LogInformation("Recieved auth code with scope '{scope}'", scope);

            var token = await ExecuteTokenExchange(code, log);
            
            log.LogInformation("{function} {status} {cid}", "TokenExchange", "Success", correlationID.ToString());
            return new OkObjectResult(token);
        }

        private static async Task<string> ExecuteTokenExchange(string code, ILogger logger) 
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                 ClientId = Environment.GetEnvironmentVariable("StravaApi__ClientId"),
                 ClientSecret = Environment.GetEnvironmentVariable("StravaApi__ClientSecret")
            };
            var tokenProvider = new TokenClient(options, logger);
            var exchangeOperation = new ExchangeAuthCodeForToken(tokenProvider);

            var token = await exchangeOperation.Execute(code);
            return token;
        }
    }
}
