using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.StravaApi
{
    internal static class HttpResponseMessageExtensions
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
                throw;
            }
        }
    }
}
       