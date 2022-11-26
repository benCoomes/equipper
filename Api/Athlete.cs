using System;

namespace Coomes.Equipper
{
    public class Athlete
    {
        public long Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        // URL to a 124x124 pixel profile picture.
        public string ProfileMediumUrl { get; set; }
        
        // URL to a 62x62 pixel profile picture.
        public string ProfileUrl { get; set; }
    }
}