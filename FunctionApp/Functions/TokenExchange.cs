using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi;
using Coomes.Equipper.Operations;
using Coomes.Equipper.CosmosStorage;

namespace Coomes.Equipper.FunctionApp
{
    public class TokenExchange
    {
        private readonly ILogger _logger;

        public TokenExchange(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TokenExchange>();
        }

        [Function("TokenExchange")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
        {
            return await ErrorHandler.RunWithErrorHandling(_logger, req, async () => {
                var correlationID = Guid.NewGuid();
                var user = StaticWebAppsAuth.ParseUser(req);
                _logger.LogInformation("{function} {status} {cid} {userId}", "TokenExchange", "Starting", correlationID.ToString(), user.UserId);

                string code = req.Query["_code"]; // see https://github.com/Azure/static-web-apps/issues/165 and auth.html
                string scopeString = req.Query["scope"];
                string error = req.Query["error"];

                _logger.LogInformation("Received auth code with scope '{scope}'", scopeString);

                var token = await ExecuteTokenExchange(code, scopeString, user, error, _logger);
                
                _logger.LogInformation("{function} {status} {cid} {userId}", "TokenExchange", "Success", correlationID.ToString(), user.UserId);
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                return response;
            });
        }

        private static async Task<string> ExecuteTokenExchange(string code, string scopesString, EquipperUser user, string error, ILogger logger) 
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

            var token = await exchangeOperation.Execute(code, scopes, user, error);
            return token;
        }
    }
}
