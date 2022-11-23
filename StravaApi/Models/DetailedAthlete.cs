using System;
using System.Text.Json;
using domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    public class DetailedAthlete : StravaModel<domain.Athlete>
    {
        public static DetailedAthlete FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<DetailedAthlete>(readOnlySpan);
        }

        public domain.Athlete ToDomainModel()
        {
            return new domain.Athlete()
            {
                Id = id,
                FirstName = firstname,
                LastName = lastname,
                ProfileMediumUrl = profile_medium,
                ProfileUrl = profile
            };
        }

        public long id { get; set; }
        
        public string firstname { get; set; }
        
        public string lastname { get; set; }
        
        // URL to a 62x62 pixel profile picture.
        public string profile_medium { get; set; }
        
        // URL to a 124x124 pixel profile picture.
        public string profile { get; set; }

        // The athlete's preferred unit system. May take one of the following values: feet, meters
        public string measurement_preference { get; set; }
    }
}