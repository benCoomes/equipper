using System.Net;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Azure.Cosmos;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    public class TokenStorage : CosmosStorageBase, ITokenStorage
    {
        private static ContainerProperties _containerProperties = new ContainerProperties("Tokens", "/id");

        public TokenStorage(string connectionString, bool disableSSL=false) : base(connectionString, "Equipper", _containerProperties)
        {
        }

        // TOdo; make internal and share with test class
        public TokenStorage(string connectionString, string databaseId, string containerId) : base(connectionString, databaseId, new ContainerProperties(containerId, "/id"))
        {
        }

        public async Task AddOrUpdateTokens(Domain.AthleteTokens athleteTokens)
        {
            await EnsureInitialized();
            
            var dataModel = new AthleteTokens(athleteTokens);
            var partitionKey = new PartitionKey(dataModel.AthleteID);
            await _container.UpsertItemAsync<AthleteTokens>(dataModel, partitionKey);
        }

        public async Task<Domain.AthleteTokens> GetTokens(long athleteID)
        {
            await EnsureInitialized();

            var id = athleteID.ToString();
            var partitionKey = new PartitionKey(id);
            try 
            {
                var result = await  _container.ReadItemAsync<AthleteTokens>(id, partitionKey);
                var dataModel = result.Resource;
                return dataModel.ToDomainModel();
            }
            catch (CosmosException cex) when (cex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task DeleteTokens(long athleteID)
        {
            await EnsureInitialized();

            var id = athleteID.ToString();
            var partitionKey = new PartitionKey(id);
            
            // todo: does this call return a status code or throw an exception on failure?
            // if status code, verify success
            await _container.DeleteItemAsync<AthleteTokens>(id, partitionKey);
        }
    }
}
