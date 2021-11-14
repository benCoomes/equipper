using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi;
using Coomes.Equipper.Operations;
using Coomes.Equipper.CosmosStorage;

namespace Equipper.FunctionApp
{
    public static class TokenExchange
    {
        [FunctionName("TokenExchange")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var correlationID = Guid.NewGuid();
            log.LogInformation("{function} {status} {cid}", "TokenExchange", "Starting", correlationID.ToString());

            string code = req.Query["code"];
            string scope = req.Query["scope"];
            string error = req.Query["error"];

            if(!string.IsNullOrWhiteSpace(error)) 
            {
                log.LogError("Received error at authorization callback: {authError}", error);
                return new BadRequestObjectResult($"Failed to authorize due to error: {error}");
            }

            log.LogInformation("Received auth code with scope '{scope}'", scope);

            var token = await ExecuteTokenExchange(code, log);
            
            log.LogInformation("{function} {status} {cid}", "TokenExchange", "Success", correlationID.ToString());
            return new OkObjectResult(@"
Authorization Success!
Equipper will now set the bike you most likely used on each ride you upload.
You can revoke access at any time by visiting https://www.strava.com/settings/apps.");
        }

        private static async Task<string> ExecuteTokenExchange(string code, ILogger logger) 
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                 ClientId = Settings.ClientId,
                 ClientSecret = Settings.ClientSecret
            };
            var tokenProvider = new TokenClient(options, logger);
            var tokenStorage = new TokenStorage(Settings.CosmosConnectionString);
            var exchangeOperation = new RegisterNewAthlete(tokenProvider, tokenStorage);

            var token = await exchangeOperation.Execute(code);
            return token;
        }
    }
}
