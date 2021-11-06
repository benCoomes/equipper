using System;
using System.Text.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    internal class UpdatableActivity
    {
        public static UpdatableActivity FromJsonBytes(byte[] jsonBytes)
        {
            return JsonSerializer.Deserialize<UpdatableActivity>(jsonBytes);
        }

        public bool commute { get; set; }
        public bool trainer { get; set; }
        public bool hide_from_home { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string gear_id { get; set; }
    }
}