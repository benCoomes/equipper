using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Coomes.Equipper.CosmosStorage
{
    public abstract class CosmosStorageBase
    {
        protected readonly string DatabaseID;
        protected Container _container;
        

        private bool _isInitialized = false;
        private SemaphoreSlim _initLock = new SemaphoreSlim(1);
        private CosmosClient _cosmosClient;
        private Database _cosmosDatabase;
        private ContainerProperties _containerProps;
        
        internal CosmosStorageBase(string connectionString, string databaseID, ContainerProperties containerProps)
        {
            if(string.IsNullOrWhiteSpace(containerProps?.Id))
                throw new ArgumentException($"{nameof(containerProps)} must not be null and must contain an Id.");
            if(string.IsNullOrWhiteSpace(containerProps?.PartitionKeyPath))
                throw new ArgumentException($"{nameof(containerProps)} must not be null and must contain a PartitionKeyPath.");

            DatabaseID = databaseID;
            _containerProps = containerProps;
            _cosmosClient = new CosmosClient(connectionString);
        }

        // todo: make internal, for test class only.
        public async Task EnsureDeleted()
        {
            var database = _cosmosClient.GetDatabase(DatabaseID);
            
            try 
            {
                await database.ReadAsync(); // throws 404 if DNE
                var container = database.GetContainer(_containerProps.Id);
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
                _container = await _cosmosDatabase.CreateContainerIfNotExistsAsync(_containerProps);
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}