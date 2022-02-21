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
        private static ContainerProperties _containerProperties = GetContainerProps("Activities");

        public ActivityStorage(string connectionString) : base(connectionString, "Equipper", _containerProperties)
        {
        }

        public ActivityStorage(string connectionString, string databaseId, string containerId) : base(connectionString, databaseId, GetContainerProps(containerId))
        {
        }

        public async Task<bool> ContainsResults(long athleteId, long activityId)
        {
            await EnsureInitialized();
            var stringId = activityId.ToString();
            var partitionKey = new PartitionKey(athleteId);
            try
            {
                await _container.ReadItemAsync<ActivityClassificationStats>(stringId, partitionKey);
                return true;
            }
            catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
            
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

        private static ContainerProperties GetContainerProps(string containerId)
        {
            return new ContainerProperties(containerId, "/athleteId")
            {
                DefaultTimeToLive = -1 // enable TTL but do not expire items by default
            };
        }
    }
}