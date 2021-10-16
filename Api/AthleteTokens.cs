using System;

namespace Coomes.Equipper
{
    public class AthleteTokens
    {
        public long AthleteID { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
    }
}