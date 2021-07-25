using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using System.Web;

namespace Coomes.Equipper.StravaApi
{
    public class TokenClient : ITokenProvider
    {
        private static HttpClient httpClient = new HttpClient(); 

        public async Task<string> GetToken(string authCode)
        {            
            var uriBuilder = new UriBuilder("https://www.strava.com/api/v3/oauth/token");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = ""; // todo: get from configs
            query["client_secret"] = ""; // todo: get from configs
            query["code"] = authCode;
            query["grant_type"] = "authorization_code";
            uriBuilder.Query = query.ToString();

            using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString());
            using var response = await httpClient.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            var bytes = await response.Content.ReadAsByteArrayAsync();
            var tokenResponse = TokenResponse.FromJsonBytes(bytes);

            return tokenResponse.access_token;      
        }

        private class TokenResponse 
        {
            public static TokenResponse FromJsonBytes(byte[] jsonBytes) {
                var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
                return JsonSerializer.Deserialize<TokenResponse>(readOnlySpan);
            }

            // todo: change to PascalCase
            public string access_token { get; set; }
            public string expires_at { get; set; }
            public string expires_in { get; set; }
            public string refresh_token { get; set; }
        }
    }
}
