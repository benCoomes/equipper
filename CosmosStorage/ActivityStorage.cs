using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Azure.Cosmos;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class ActivityStorage : IActivityStorage
    {
        protected string DatabaseID = "Equipper";
        protected string ContainerID = "Activities";

        protected CosmosClient _cosmosClient;
        protected Database _cosmosDatabase;
        protected Container _activityContainer;
        private SemaphoreSlim _initLock = new SemaphoreSlim(1);
        private bool _isInitialized = false;

        public ActivityStorage(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
        }

        public Task<bool> ActivityHasBeenProcessed(long stravaActivityID)
        {
            throw new NotImplementedException();
        }

        public async Task<Domain.ClassificationStats> GetClassificationStats(Guid id)
        {
            await EnsureInitialized();
            var stringId = id.ToString();
            var partitionKey = new PartitionKey(stringId);
            try 
            {
                var result = await  _activityContainer.ReadItemAsync<ClassificationStats>(stringId, partitionKey);
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
            var activityClassificationDataModel = new ActivityClassificationStats(activity, classificationStats);
            var classificationStatsDataModel = new ClassificationStats(classificationStats);
            // TODO: what if concurrent requests? what if exceptions?
            await _activityContainer.UpsertItemAsync(classificationStatsDataModel, new PartitionKey(classificationStatsDataModel.PartitionKey));
            await _activityContainer.UpsertItemAsync(activityClassificationDataModel, new PartitionKey(activityClassificationDataModel.PartitionKey));
        }

        private async Task EnsureInitialized()
        {
            if(_isInitialized) return;
            
            await _initLock.WaitAsync();
            try
            {
                if(_isInitialized) return;

                _cosmosDatabase = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseID);
                var containerProps = new ContainerProperties(ContainerID, "/pk");
                _activityContainer = await _cosmosDatabase.CreateContainerIfNotExistsAsync(containerProps);
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}