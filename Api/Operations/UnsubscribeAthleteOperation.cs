using System;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class UnsubscribeAthleteOperation
    {
        ITokenStorage _tokenStorage;
        ILogger _logger;

        public UnsubscribeAthleteOperation(ITokenStorage tokenStorage, ILogger logger)
        {
            _tokenStorage = tokenStorage;
            _logger = logger;
        }

        public async Task Execute(long athleteID)
        {
            try
            {
                await _tokenStorage.DeleteTokens(athleteID);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to remove data for athlete {athleteID}", athleteID);
                throw;
            }

            _logger.LogInformation("Unsubscribed athlete {athleteID}", athleteID);
        }
    }
}