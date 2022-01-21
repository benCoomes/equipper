using System;
using Newtonsoft.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ClassificationStats
    {
        [JsonProperty("id")]
        public string ClassificationStatsId { get; set; }
        
        [JsonProperty("pk")]
        public string PartitionKey { get; set; }

        public ClassificationStats()
        { }

        public ClassificationStats(Domain.ClassificationStats domainModel)
        {
            var guidString = domainModel.Id.ToString();
            ClassificationStatsId = guidString;
            PartitionKey = guidString;
        }

        public Domain.ClassificationStats ToDomainModel()
        {
            return new Domain.ClassificationStats()
            {
                Id = Guid.Parse(ClassificationStatsId)
            };
        }
    }
}