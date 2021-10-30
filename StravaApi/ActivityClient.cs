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

namespace Coomes.Equipper.StravaApi
{
    public class ActivityClient : IActivityData
    {
        private static HttpClient httpClient = new HttpClient();
        private ILogger _logger;

        public ActivityClient(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 1, int limit = 50)
        {
            var uriBuilder = new UriBuilder("https://www.strava.com/api/v3/athlete/activities");
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["page"] = page.ToString();
            query["per_page"] = limit.ToString();
            uriBuilder.Query = query.ToString();

            using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(request);
            
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(ActivityClient)}.{nameof(GetActivities)}");
            
            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            return ToActivityList(jsonBytes);
        }

        private List<Activity> ToActivityList(byte[] jsonBytes) 
        {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            var stravaActivities = JsonSerializer.Deserialize<List<StravaModel.Activity>>(readOnlySpan);
            return stravaActivities
                .Select(sa => sa.ToDomainModel())
                .ToList();
        }
    }
}
