using System;
using Newtonsoft.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ActivityClassificationStats
    {
        [JsonProperty("id")]
        public string StravaActivityId { get; set; }
        
        [JsonProperty("pk")]
        public string PartitionKey { get; set; }

        public ActivityClassificationStats()
        { }

        public ActivityClassificationStats(Domain.Activity domainModel, Domain.ClassificationStats classificationStats)
        {
            StravaActivityId = domainModel.Id.ToString();
            PartitionKey = classificationStats.Id.ToString();
        }
    }
}