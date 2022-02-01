using System;
using Newtonsoft.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ActivityClassificationStats
    {
        [JsonProperty("id")]
        public string StravaActivityId { get; set; }

        [JsonProperty("athleteId")]
        public long AthleteId { get; set; }
        
        [JsonProperty("classificationStatsId")]
        public string ClassificationStatsId { get; set; }

        [JsonProperty("ttl")]
        public int TimeToLive { get; set; } = 604800; // 7 days, in seconds

        public ActivityClassificationStats()
        { }

        public ActivityClassificationStats(Domain.Activity activityDomainModel, Domain.ClassificationStats classificationStatsDomainModel)
        {
            StravaActivityId = activityDomainModel.Id.ToString();
            AthleteId = activityDomainModel.AthleteId;
            ClassificationStatsId = classificationStatsDomainModel.Id.ToString();
        }
    }
}