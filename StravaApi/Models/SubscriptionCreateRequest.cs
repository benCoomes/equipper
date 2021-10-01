using System.Text.Json;

namespace Coomes.Equipper.StravaApi.Models
{
    internal class SubscriptionCreateRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string callback_url { get; set; }
        public string verify_token { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
