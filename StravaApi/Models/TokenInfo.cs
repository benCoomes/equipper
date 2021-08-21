using System;
using System.Text.Json;

namespace Coomes.Equipper.StravaApi.Models
{
    public class TokenInfo
    {
        public static TokenInfo FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<TokenInfo>(readOnlySpan);
        }

        public string access_token { get; set; }
        public int expires_at { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
