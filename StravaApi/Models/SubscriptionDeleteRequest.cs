using System.Text.Json;

namespace Coomes.Equipper.StravaApi.Models
{
    internal class SubscriptionDeleteRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
