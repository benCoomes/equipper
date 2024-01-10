using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using StravaModel = Coomes.Equipper.StravaApi.Models;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Coomes.Equipper.StravaApi
{
    public class GearClient : IGearData
    {
        private static HttpClient _httpClient = new HttpClient();
        private ILogger _logger;

        public GearClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Gear> GetGear(string accessToken, string gearId)
        {
            var uriBuilder = new UriBuilder($"https://www.strava.com/api/v3/gear/{gearId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(GearClient)}.{nameof(GetGear)}");
            
            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            var stravaModel = StravaModel.Gear.FromJsonBytes(jsonBytes);
            return stravaModel.ToDomainModel();
        }
    }
}
