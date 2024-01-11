using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.CosmosStorage;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using Coomes.Equipper.StravaApi;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public static class GetAthlete
    {
        [FunctionName("GetAthlete")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger logger)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            logger.LogInformation("{function} {status} {cid} {userId}", "GetAthlete", "Starting", correlationID.ToString(), user.UserId);

            var athlete = await ExecuteGetAthlete(user, logger);

            logger.LogInformation("{function} {status} {cid} {userId}", "GetAthlete", "Success", correlationID.ToString(), user.UserId);
            
            if(athlete != null) return new OkObjectResult(athlete);
            else return new NotFoundResult();
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
