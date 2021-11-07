using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Azure.Cosmos;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class TokenStorage : ITokenStorage
    {
        protected string DatabaseID = "Equipper";
        protected string ContainerID = "Tokens";

        protected CosmosClient _cosmosClient;
        protected Database _cosmosDatabase;
        protected Container _tokenContainer;
        private SemaphoreSlim _initLock = new SemaphoreSlim(1);
        private bool _isInitialized = false;

        public TokenStorage(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
        }

        public async Task AddOrUpdateTokens(Domain.AthleteTokens athleteTokens)
        {
            await EnsureInitialized();
            
            var dataModel = new AthleteTokens(athleteTokens);
            var partitionKey = new PartitionKey(dataModel.id);
            await _tokenContainer.UpsertItemAsync<AthleteTokens>(dataModel, partitionKey);
        }

        public async Task<Domain.AthleteTokens> GetTokens(long athleteID)
        {
            await EnsureInitialized();

            var id = athleteID.ToString();
            var partitionKey = new PartitionKey(id);
            try 
            {
                var result = await  _tokenContainer.ReadItemAsync<AthleteTokens>(id, partitionKey);
                var dataModel = result.Resource;
                return dataModel.ToDomainModel();
            }
            catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task EnsureInitialized()
        {
            if(_isInitialized) return;
            
            await _initLock.WaitAsync();
            try
            {
                if(_isInitialized) return;

                _cosmosDatabase = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseID);
                var containerProps = new ContainerProperties(ContainerID, "/id");
                _tokenContainer = await _cosmosDatabase.CreateContainerIfNotExistsAsync(containerProps);
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
