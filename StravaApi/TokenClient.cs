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

        public async Task<AthleteTokens> GetToken(string authCode)
        {            
            var tokenRequest = GetTokenRequestUrl(authCode);

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenRequest);
            using var response = await _httpClient.SendAsync(request);
            
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(TokenClient)}.{nameof(GetToken)}");

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var tokenResponse = Models.AthleteTokens.FromJsonBytes(bytes);

            return tokenResponse.ToDomainModel();
        }

        public async Task<AthleteTokens> RefreshToken(AthleteTokens athleteTokens)
        {
            _logger.LogInformation("Refreshing tokens for {athleteID}.", athleteTokens.AthleteID);
            var refreshRequest = GetTokenRefreshUrl(athleteTokens.RefreshToken);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, refreshRequest);
            using var response = await _httpClient.SendAsync(request);
            
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(TokenClient)}.{nameof(GetToken)}");

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var tokenResponse = Models.RefreshTokens.FromJsonBytes(bytes);

            return tokenResponse.ToAthleteTokens(athleteTokens.AthleteID);
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

        private string GetTokenRefreshUrl(string refreshToken) 
        {
            var uriBuilder = new UriBuilder("https://www.strava.com/api/v3/oauth/token");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = _options.ClientId;
            query["client_secret"] = _options.ClientSecret;
            query["refresh_token"] = refreshToken;
            query["grant_type"] = "refresh_token";
            uriBuilder.Query = query.ToString();
            return uriBuilder.ToString();
        }
    }
}
