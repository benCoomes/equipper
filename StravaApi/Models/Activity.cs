using System;
using System.Text.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.StravaApi.Models
{
    internal class Activity : StravaModel<Domain.Activity>
    {
        public static Activity FromJsonBytes(byte[] jsonBytes)
        {
            return JsonSerializer.Deserialize<Activity>(jsonBytes);
        }

        public double distance { get; set; }
        public int moving_time { get; set; }
        public double total_elevation_gain { get; set; }
        public string type { get; set; }
        public int? workout_type { get; set; }
        public long id { get; set; }
        public string start_date { get; set; }
        public double[] start_latlng { get; set; }
        public double[] end_latlng { get; set; }
        public string gear_id { get; set; }
        public double average_speed { get; set; }
        public bool trainer { get; set; }
        public bool commute { get; set; }
        public bool manual { get; set; }
        public bool device_watts { get; set; }

        public Domain.Activity ToDomainModel()
        {
            return new Domain.Activity()
            {
                Distance = distance,
                MovingTime = moving_time,
                TotalElevationGain = total_elevation_gain,
                Type = type,
                WorkoutType = workout_type,
                Id = id,
                StartDate = start_date,
                StartLatLng = start_latlng,
                EndLatLng = end_latlng,
                GearId = gear_id,
                AverageSpeed = average_speed,
                Trainer = trainer,
                Commute = commute,
                Manual = manual,
                DeviceWatts = device_watts
            };
        }
    }
}