using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.CosmosStorage;
using Coomes.Equipper.Operations;
using Coomes.Equipper.StravaApi;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public class GetAthlete
    {
        private readonly ILogger _logger;

        public GetAthlete(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetAthlete>();
        }

        [Function("GetAthlete")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            _logger.LogInformation("{function} {status} {cid} {userId}", "GetAthlete", "Starting", correlationID.ToString(), user.UserId);

            var athlete = await ExecuteGetAthlete(user, _logger);

            _logger.LogInformation("{function} {status} {cid} {userId}", "GetAthlete", "Success", correlationID.ToString(), user.UserId);
            
            if(athlete != null) 
            {
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(athlete);
                return response;
            }
            else 
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }
        }

        private static Task<Athlete> ExecuteGetAthlete(EquipperUser user, ILogger logger)
        {
            // todo: better way to build dependencies?
            var options = new StravaApiOptions()
            {
                 ClientId = Settings.ClientId,
                 ClientSecret = Settings.ClientSecret
            };
            var stravaData = new StravaClient(logger);
            var tokenProvider = new TokenClient(options, logger);
            var tokenStorage = new TokenStorage(Settings.CosmosConnectionString);
            var getAthleteOp = new GetConnectedAthlete(stravaData, tokenStorage, tokenProvider, logger);
            return getAthleteOp.Execute(user);
        }
    }
}
