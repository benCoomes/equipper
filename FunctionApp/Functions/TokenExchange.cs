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

namespace Coomes.Equipper.FunctionApp
{
    public static class TokenExchange
    {
        // see https://github.com/Azure/static-web-apps/issues/165
        // todo: when issue fixed: 
        //   change this back to 'TokenExchange' 
        //   remove the proxy in proxies.json
        //   change param back to 'code'
        [FunctionName("TokenExchangeWorkaround")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await ErrorHandler.RunWithErrorHandling(log, async () => {
                var correlationID = Guid.NewGuid();
                log.LogInformation("{function} {status} {cid}", "TokenExchange", "Starting", correlationID.ToString());

                string code = req.Query["_code"]; // see proxies.json and https://github.com/Azure/static-web-apps/issues/165
                string scopeString = req.Query["scope"];
                string error = req.Query["error"];

                log.LogInformation("Received auth code with scope '{scope}'", scopeString);

                var token = await ExecuteTokenExchange(code, scopeString, error, log);
                
                log.LogInformation("{function} {status} {cid}", "TokenExchange", "Success", correlationID.ToString());
                return new OkResult();
            });
        }

        private static async Task<string> ExecuteTokenExchange(string code, string scopesString, string error, ILogger logger) 
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                 ClientId = Settings.ClientId,
                 ClientSecret = Settings.ClientSecret
            };
            var tokenProvider = new TokenClient(options, logger);
            var tokenStorage = new TokenStorage(Settings.CosmosConnectionString);
            var exchangeOperation = new RegisterNewAthlete(tokenProvider, tokenStorage, logger);
            var scopes = StravaApi.Models.AuthScopes.Create(scopesString).ToDomainModel();

            var token = await exchangeOperation.Execute(code, scopes, error);
            return token;
        }
    }
}
