using System;

namespace Coomes.Equipper
{
    public class Activity 
    {
        public double Distance { get; set; }
        public int MovingTime { get; set; }
        public double TotalElevationGain { get; set; }
        public string Type { get; set; }
        public int? WorkoutType { get; set; }
        public long Id { get; set; }
        public string StartDate { get; set; }
        public double[] StartLatLng { get; set; }
        public double[] EndLatLng { get; set; }
        public string GearId { get; set; }
        public double AverageSpeed { get; set; }
        public bool Trainer { get; set; }
        public bool Commute { get; set; }
        public bool Manual { get; set; }
        public bool DeviceWatts { get; set; }
    }
}