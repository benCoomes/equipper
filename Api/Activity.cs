using System;

namespace Coomes.Equipper
{
    public class Activity 
    {
        public string name { get; set; }
        public double distance { get; set; }
        public int moving_time { get; set; }
        public int elapsed_time { get; set; }
        public string gear_id { get; set; }
        public double average_speed { get; set; }
    }
}