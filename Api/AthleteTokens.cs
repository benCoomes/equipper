using System;

namespace Coomes.Equipper
{
    public class AthleteTokens
    {
        // The unique ID for the linked Strava User
        public long AthleteID { get; set; }
        // The unique ID for the Equipper User
        public string UserID { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

        public bool IsValid() 
        {
            return AthleteID != 0 && !string.IsNullOrWhiteSpace(UserID);
        }
    }
}