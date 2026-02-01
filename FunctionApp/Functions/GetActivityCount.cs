using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.CosmosStorage;
using Coomes.Equipper.Operations;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public class GetActivityCount
    {
        private readonly ILogger _logger;

        public GetActivityCount(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetActivityCount>();
        }

        [Function("ActivityCount")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            _logger.LogInformation("{function} {status} {cid} {userId}", "GetActivityCount", "Starting", correlationID.ToString(), user.UserId);

            var count = await ExecuteGetCount(_logger);

            _logger.LogInformation("{function} {status} {cid} {userId}", "GetActivityCount", "Success", correlationID.ToString(), user.UserId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(count);
            return response;
        }

        private static Task<int> ExecuteGetCount(ILogger logger)
        {
            // todo: better way to build dependencies?
            var activityStorage = new ActivityStorage(Settings.CosmosConnectionString);
            var countOperation = new GetProccessedActivityCount(activityStorage, logger);
            return countOperation.Execute();
        }
    }
}
