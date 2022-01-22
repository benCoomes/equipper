using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Coomes.Equipper.CosmosStorage
{
    public abstract class CosmosStorageBase
    {
        protected readonly string PartitionKeyPath;
        protected readonly string DatabaseID;
        protected readonly string ContainerID;
        protected Container _container;


        private bool _isInitialized = false;
        private SemaphoreSlim _initLock = new SemaphoreSlim(1);
        private CosmosClient _cosmosClient;
        private Database _cosmosDatabase;
        
        internal CosmosStorageBase(string connectionString, string databaseID, string containerID, string partitionKeyPath)
        {
            PartitionKeyPath = partitionKeyPath;
            DatabaseID = databaseID;
            ContainerID = containerID;
            _cosmosClient = new CosmosClient(connectionString);
        }

        // todo: make internal, for test class only.
        public async Task EnsureDeleted()
        {
            var database = _cosmosClient.GetDatabase(DatabaseID);
            
            try 
            {
                await database.ReadAsync(); // throws 404 if DNE
                var container = database.GetContainer(ContainerID);
                await container.DeleteContainerAsync(); // throws 404 if DNE
            }
            catch(CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }

        protected async Task EnsureInitialized()
        {
            if(_isInitialized) return;
            
            await _initLock.WaitAsync();
            try
            {
                if(_isInitialized) return;

                _cosmosDatabase = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseID);
                var containerProps = new ContainerProperties(ContainerID, PartitionKeyPath);
                _container = await _cosmosDatabase.CreateContainerIfNotExistsAsync(containerProps);
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}