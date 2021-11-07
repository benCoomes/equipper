using System;
using System.Text.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    // https://developers.strava.com/docs/authentication/#token-exchange
    public class AthleteTokens : StravaModel<Domain.AthleteTokens>
    {
        public static AthleteTokens FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<AthleteTokens>(readOnlySpan);
        }

        public Domain.AthleteTokens ToDomainModel()
        {
            return new Domain.AthleteTokens()
            {
                AccessToken = access_token,
                RefreshToken = refresh_token,
                ExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expires_at).UtcDateTime,
                AthleteID = athlete.id
            };
        }

        public string access_token { get; set; }
        public long expires_at { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public SummaryAthlete athlete { get; set; }
    }
}
