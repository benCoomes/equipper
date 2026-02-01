using System;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.FunctionApp.Functions
{
    public class Echo
    {
        private readonly ILogger _logger;

        public Echo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Echo>();
        }

        [Function("Echo")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
        {
            var correlationID = Guid.NewGuid();
            var user = StaticWebAppsAuth.ParseUser(req);
            _logger.LogInformation("{function} {status} {cid} {userId}", "Echo", "Starting", correlationID.ToString(), user.UserId);

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

            _logger.LogInformation("{function} {status} {cid} {userId}", "Echo", "Success", correlationID.ToString(), user.UserId);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString(responseMessage);
            return response;
        }
    }
}
