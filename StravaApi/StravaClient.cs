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
    public class StravaClient : IStravaData
    {
        private static string _stravaApiUrl = "https://www.strava.com/api/v3";
        private static HttpClient _httpClient = new HttpClient();
        private ILogger _logger;

        public StravaClient(ILogger logger = null)
        {
            _logger = logger;
        }

        public async Task<Athlete> GetAthlete(string accessToken, long athleteId)
        {
            var uri = new Uri($"{_stravaApiUrl}/athlete");
            using var request = BuildRequest(HttpMethod.Get, uri, accessToken);
            using var response = await SendRequest(request, nameof(GetAthlete));

            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            var detailedAthlete = StravaModel.DetailedAthlete.FromJsonBytes(jsonBytes);
            return detailedAthlete.ToDomainModel();
        }

        public async Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 1, int limit = 50)
        {
            var uriBuilder = new UriBuilder("{_stravaApiUrl}/athlete/activities");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["page"] = page.ToString();
            query["per_page"] = limit.ToString();
            uriBuilder.Query = query.ToString();

            using var request = BuildRequest(HttpMethod.Get, uriBuilder.ToString(), accessToken);
            using var response = await SendRequest(request, nameof(GetActivities));
            
            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            return ToActivityList(jsonBytes);
        }

        public async Task<Activity> UpdateGear(string accessToken, Activity activity)
        {
            if(string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException($"{nameof(accessToken)} is required.");
            if((activity?.Id).GetValueOrDefault() == default(long))
                throw new ArgumentException($"{nameof(activity)} must not be null and must have an Id.");
            if(string.IsNullOrWhiteSpace(activity?.GearId))
                throw new ArgumentException($"{nameof(activity)} must not be null and must have a GearId.");

            var updaterActivity = await GetActivityForUpdate(accessToken, activity);
            updaterActivity.gear_id = activity.GearId;

            var uri = new Uri($"{_stravaApiUrl}/activities/{activity.Id}");
            using var request = BuildRequest(HttpMethod.Put, uri, accessToken);
            request.Content = new StringContent(JsonSerializer.Serialize(updaterActivity), Encoding.UTF8, "application/json");
            
            using var response = await SendRequest(request, nameof(UpdateGear));

            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            var stravaModel = StravaModel.Activity.FromJsonBytes(jsonBytes);
            return stravaModel.ToDomainModel();
        }

        public async Task<Gear> GetGear(string accessToken, string gearId)
        {
            var uri = new Uri($"{_stravaApiUrl}/gear/{gearId}");
            using var request = BuildRequest(HttpMethod.Get, uri, accessToken);
            using var response = await SendRequest(request, nameof(GetGear));
            
            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            var stravaModel = StravaModel.Gear.FromJsonBytes(jsonBytes);
            return stravaModel.ToDomainModel();
        }

        private async Task<StravaModel.UpdatableActivity> GetActivityForUpdate(string accessToken, Activity activity)
        {
            var uri = new Uri($"{_stravaApiUrl}/activities/{activity.Id}");
            using var request = BuildRequest(HttpMethod.Get, uri, accessToken);
            using var response = await SendRequest(request, nameof(GetActivityForUpdate));

            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            return StravaModel.UpdatableActivity.FromJsonBytes(jsonBytes);
        }

        private List<Activity> ToActivityList(byte[] jsonBytes) 
        {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            var stravaActivities = JsonSerializer.Deserialize<List<StravaModel.Activity>>(readOnlySpan);
            return stravaActivities
                .Select(sa => sa.ToDomainModel())
                .ToList();
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string uri, string accessToken)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, Uri uri, string accessToken)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return request;
        }

        private async Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, string methodName) 
        {
            var response = await _httpClient.SendAsync(request);
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(StravaClient)}.{methodName}");
            return response;
        }
    }
}
