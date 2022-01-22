using System;
using System.Net;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Azure.Cosmos;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ActivityStorage : CosmosStorageBase, IActivityStorage
    {
        public ActivityStorage(string connectionString) : base(connectionString, "Equipper", "Activities", "/athleteId")
        {
        }

        public ActivityStorage(string connectionString, string databaseId, string containerId) : base(connectionString, databaseId, containerId, "/athleteId")
        {
        }

        public async Task<Domain.ClassificationStats> GetClassificationStats(Guid id, long athleteId)
        {
            await EnsureInitialized();
            var stringId = id.ToString();
            var partitionKey = new PartitionKey(athleteId);
            try 
            {
                var result = await  _container.ReadItemAsync<ClassificationStats>(stringId, partitionKey);
                var dataModel = result.Resource;
                return dataModel.ToDomainModel();
            }
            catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task StoreActivityResults(Domain.Activity activity, Domain.ClassificationStats classificationStats)
        {
            await EnsureInitialized();
            var partitionKey = new PartitionKey(activity.AthleteId);
            var activityClassificationDataModel = new ActivityClassificationStats(activity, classificationStats);
            var classificationStatsDataModel = new ClassificationStats(classificationStats, activity.AthleteId);            

            try
            {
                await _container.CreateItemAsync(activityClassificationDataModel, partitionKey);
                await _container.CreateItemAsync(classificationStatsDataModel, partitionKey);
            }
            catch(CosmosException cex) when (cex.StatusCode == HttpStatusCode.Conflict)
            {
                return;
            }
        }
    }
}