using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.StravaApi
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task LogAndThrowIfNotSuccess(this HttpResponseMessage response, ILogger logger, string context = "") 
        {
            try 
            {
                response.EnsureSuccessStatusCode();
            }
            catch(HttpRequestException)
            {
                var reason = await response.Content.ReadAsStringAsync(); 
                logger?.LogError("Nonsuccess response from Strava. Context: {context} Status {status}, Response {response}.", context, response.StatusCode, reason);
                
                if(TryGetDomainException(response, out var domainException))
                {
                    throw domainException;
                }
                else
                {
                    throw;
                }
            }
        }

        private static bool TryGetDomainException(HttpResponseMessage response, out Exception domainException)
        {
            switch(response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                {
                    domainException = new UnauthorizedException();
                    return true;
                }
                default:
                {
                    domainException = null;
                    return false;
                }
            }
        }
    }
}
       