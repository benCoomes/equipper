using System;
using Newtonsoft.Json;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ClassificationStats
    {
        [JsonProperty("id")]
        public string ClassificationStatsId { get; set; }
        
        [JsonProperty("athleteId")]
        public long AthleteId { get; set; }

        [JsonProperty("crossValidations")]
        public CrossValidationResult[] CrossValidations { get; set; }

        public ClassificationStats()
        { }

        public ClassificationStats(Domain.ClassificationStats domainModel, long athleteId)
        {
            ClassificationStatsId = domainModel.Id.ToString();
            AthleteId = athleteId;
            CrossValidations = domainModel.CrossValidations.ToDataModels();
        }

        public Domain.ClassificationStats ToDomainModel()
        {
            return new Domain.ClassificationStats()
            {
                Id = Guid.Parse(ClassificationStatsId),
                CrossValidations = CrossValidations.ToDomainModels()
            };
        }
    }
}