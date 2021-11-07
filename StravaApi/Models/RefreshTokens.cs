using System;
using System.Text.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    // https://developers.strava.com/docs/authentication/#refreshing-expired-access-tokens
    internal class RefreshTokens
    {
        public static RefreshTokens FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<RefreshTokens>(readOnlySpan);
        }

        public Domain.AthleteTokens ToAthleteTokens(long athleteId)
        {
            return new Domain.AthleteTokens()
            {
                AccessToken = access_token,
                RefreshToken = refresh_token,
                ExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expires_at).UtcDateTime,
                AthleteID = athleteId
            };
        }

        public string access_token { get; set; }
        public int expires_at { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
