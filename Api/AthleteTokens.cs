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
            // todo: check UserID as well. Right now, existing tokens in prod don't have a user ID.
            // because they were created before authentication was implemented.
            return AthleteID != 0; // && !string.IsNullOrWhiteSpace(UserID);
        }
    }
}