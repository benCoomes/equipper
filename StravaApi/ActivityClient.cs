using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;


namespace Coomes.Equipper.StravaApi
{
    public class ActivityClient : IActivityData
    {
        private static HttpClient httpClient = new HttpClient(); 

        public async Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 0, int limit = 0)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.strava.com/api/v3/athlete/activities");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(request);
            
            response.EnsureSuccessStatusCode();
            
            var jsonBytes = await response.Content.ReadAsByteArrayAsync();
            return ToActivityList(jsonBytes);
            
        }

        private List<Activity> ToActivityList(byte[] jsonBytes) 
        {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<List<Activity>>(readOnlySpan);
        }
    }
}
