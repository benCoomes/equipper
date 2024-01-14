using System;
using System.Text.Json;
using domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{   
    // https://developers.strava.com/docs/reference/#api-models-DetailedGear
    // 'retired' field exists but is not documented
    public class Gear : StravaModel<domain.Gear>
    {
        public static Gear FromJsonBytes(byte[] jsonBytes) {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            return JsonSerializer.Deserialize<Gear>(readOnlySpan);
        }

        public domain.Gear ToDomainModel()
        {
            return new domain.Gear()
            {
                Id = id,
                Name = name,
                Retired = retired
            };
        }

        public string id { get; set; }
        
        public string name { get; set; }
        
        public bool retired { get; set; }
    }
}