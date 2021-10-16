using System;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.CosmosStorage
{
    public class TokenStorage : ITokenStorage
    {
        public Task AddOrUpdateTokens(AthleteTokens tokens)
        {
            return Task.CompletedTask;
        }

        public Task<AthleteTokens> GetTokens(long athleteID)
        {
            throw new NotImplementedException();
        }
    }
}
