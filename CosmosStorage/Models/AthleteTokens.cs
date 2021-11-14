using System;
using Newtonsoft.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class AthleteTokens
    {
        [JsonProperty("id")]
        public string AthleteID { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }

        public AthleteTokens()
        { }

        public AthleteTokens(Domain.AthleteTokens domainModel)
        {
            AthleteID = domainModel.AthleteID.ToString();
            ExpiresAtUtc = domainModel.ExpiresAtUtc;
            RefreshToken = domainModel.RefreshToken;
            AccessToken = domainModel.AccessToken;
        }

        public Domain.AthleteTokens ToDomainModel()
        {
            return new Domain.AthleteTokens() 
            {
                AthleteID = long.Parse(AthleteID),
                ExpiresAtUtc = ExpiresAtUtc,
                RefreshToken = RefreshToken,
                AccessToken = AccessToken
            };
        }
    }
}