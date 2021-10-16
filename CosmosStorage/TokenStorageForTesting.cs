using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Azure.Cosmos;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage
{
    ///
    /// <summary>
    /// An subclass of TokenStorage intended for testing. 
    /// It allows setting the database and container names and 
    /// exposes a method to ensure the database and container are deleted.
    /// All other behavior is identitcal.
    /// </summary>
    ///
    public class TokenStorageForTesting : TokenStorage
    {
        public TokenStorageForTesting(string connectionString, string database, string container) : base(connectionString)
        {
            DatabaseID = database;
            ContainerID = container;
        }

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
    }
}