using System;
using System.Net.Http;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using System.Web;
using Microsoft.Extensions.Options;
using Coomes.Equipper.StravaApi.Models;

namespace Coomes.Equipper.StravaApi
{
    public class TokenClient : ITokenProvider
    {
        private static HttpClient _httpClient = new HttpClient(); 
        
        private  StravaApiOptions _options;

        public TokenClient(IOptions<StravaApiOptions> options) : this(options.Value) 
        {
        }

        public TokenClient(StravaApiOptions options)
        {
            _options = options;
        }

        public async Task<string> GetToken(string authCode)
        {            
            var uriBuilder = new UriBuilder("https://www.strava.com/api/v3/oauth/token");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = _options.ClientId;
            query["client_secret"] = _options.ClientSecret;
            query["code"] = authCode;
            query["grant_type"] = "authorization_code";
            uriBuilder.Query = query.ToString();

            using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
            using var response = await _httpClient.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var tokenResponse = TokenInfo.FromJsonBytes(bytes);

            return tokenResponse.access_token;     
        }
    }
}
