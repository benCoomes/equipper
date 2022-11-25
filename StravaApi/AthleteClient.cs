using System;
using System.Net.Http;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using StravaModels = Coomes.Equipper.StravaApi.Models;
using System.Net.Http.Headers;

namespace Coomes.Equipper.StravaApi
{
    public class AthleteClient : IAthleteClient
    {
        private static HttpClient _httpClient = new HttpClient(); 
        
        private StravaApiOptions _options;
        private ILogger _logger;

        public AthleteClient(IOptions<StravaApiOptions> options, ILogger logger = null) : this(options.Value, logger) 
        {
        }

        public AthleteClient(StravaApiOptions options, ILogger logger = null)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<Athlete> GetAthlete(string accessToken, long athleteId)
        {
            var uri = new Uri($"https://www.strava.com/api/v3/athlete");
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(AthleteClient)}.{nameof(GetAthlete)}");

            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            var detailedAthlete = StravaModels.DetailedAthlete.FromJsonBytes(jsonBytes);
            return detailedAthlete.ToDomainModel();
        }
    }
}
