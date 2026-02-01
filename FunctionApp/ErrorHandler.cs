using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.FunctionApp
{
    public static class ErrorHandler
    {
        public static async Task<HttpResponseData> RunWithErrorHandling(ILogger logger, HttpRequestData req, Func<Task<HttpResponseData>> method)
        {
            try 
            {
                return await method();
            }
            catch (BadRequestException bre) 
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync(bre.Message);
                return response;
            }
            catch(UnauthorizedException)
            {
                var response = req.CreateResponse(HttpStatusCode.Unauthorized);
                return response;
            }
        }
    }

}