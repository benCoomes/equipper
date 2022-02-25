using System;
using System.Linq;
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
            var athletePartition = new PartitionKey(athleteId);
            try
            {
                await _container.ReadItemAsync<ActivityClassificationStats>(stringId, athletePartition);
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
            var athletePartition = new PartitionKey(athleteId);
            try 
            {
                var result = await  _container.ReadItemAsync<ClassificationStats>(stringId, athletePartition);
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
            var athletePartition = new PartitionKey(activity.AthleteId);
            var activityClassificationDataModel = new ActivityClassificationStats(activity, classificationStats);
            var classificationStatsDataModel = new ClassificationStats(classificationStats, activity.AthleteId);            

            try
            {
                await _container.CreateItemAsync(activityClassificationDataModel, athletePartition);
                await _container.CreateItemAsync(classificationStatsDataModel, athletePartition);
            }
            catch(CosmosException cex) when (cex.StatusCode == HttpStatusCode.Conflict)
            {
                return;
            }
        }

        public async Task<int> CountActivityResults()
        {
            await EnsureInitialized();

            var query = new QueryDefinition("SELECT VALUE count(1) FROM c Where IS_DEFINED(c.crossValidations)");
            var resultIterator = _container.GetItemQueryIterator<int>(query);
            var response = await resultIterator.ReadNextAsync();
            var count = response.Resource.First();
            return count;
        }

        public async Task DeleteActivityData(Domain.Activity activity)
        {
            await EnsureInitialized();
            
            var athletePartition = new PartitionKey(activity.AthleteId);
            await _container.DeleteItemAsync<ActivityClassificationStats>(activity.Id.ToString(), athletePartition);
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