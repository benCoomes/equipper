using System;
using System.Text.Json;
using domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    internal class DetailedAthlete : StravaModel<domain.Athlete>
    {
        public static DetailedAthlete FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<DetailedAthlete>(readOnlySpan);
        }

        public domain.Athlete ToDomainModel()
        {
            return new domain.Athlete()
            {
                
            };
        }
    }
}