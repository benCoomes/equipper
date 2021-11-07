using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Coomes.Equipper.StravaApi.Models
{
    // https://developers.strava.com/docs/webhooks/#event-data
    public class StravaEvent
    {
        public static StravaEvent FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<StravaEvent>(readOnlySpan);
        }

        public static StravaEvent FromJsonString(string json) {
            return JsonSerializer.Deserialize<StravaEvent>(json);
        }

        public string object_type { get; set; }
        public long object_id { get; set; }
        public string aspect_type { get; set; }
        public Dictionary<string, string> updates { get; set; }
        public long owner_id { get; set; }
        public int subscription_id { get; set; }
        public long event_time { get; set; }
    }
}
