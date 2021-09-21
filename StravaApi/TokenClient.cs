using System;
using System.Net.Http;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using System.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi.Models;

namespace Coomes.Equipper.StravaApi
{
    public class TokenClient : ITokenProvider
    {
        private static HttpClient _httpClient = new HttpClient(); 
        
        private  StravaApiOptions _options;
        private ILogger _logger;

        public TokenClient(IOptions<StravaApiOptions> options, ILogger logger = null) : this(options.Value, logger) 
        {   
        }

        public TokenClient(StravaApiOptions options, ILogger logger = null)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<string> GetToken(string authCode)
        {            
            var tokenRequest = GetTokenRequestUrl(authCode);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, tokenRequest);
            using var response = await _httpClient.SendAsync(request);
            
            await LogAndThrowIfNotSuccess(response);

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var tokenResponse = TokenInfo.FromJsonBytes(bytes);

            return tokenResponse.access_token;     
        }

        private string GetTokenRequestUrl(string authCode) 
        {
            var uriBuilder = new UriBuilder("https://www.strava.com/api/v3/oauth/token");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = _options.ClientId;
            query["client_secret"] = _options.ClientSecret;
            query["code"] = authCode;
            query["grant_type"] = "authorization_code";
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }

        private async Task LogAndThrowIfNotSuccess(HttpResponseMessage response) 
        {
            try 
            {
                response.EnsureSuccessStatusCode();
            }
            catch(Exception)
            {
                var reason = await response.Content.ReadAsStringAsync(); 
                _logger?.LogError("Nonsuccess response from Strava. Status {status}, Response {response}.", response.StatusCode, reason);
                throw;
            }
        }
    }
}
