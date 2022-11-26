using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetConnectedAthlete
    {
        private IAthleteClient _athleteClient;
        private ITokenStorage _tokenStorage;
        private ITokenProvider _tokenProvider;
        private ILogger _logger;

        public GetConnectedAthlete(IAthleteClient athleteClient, ITokenStorage tokenStorage, ITokenProvider tokenProvider, ILogger logger) 
        {
            _athleteClient = athleteClient;
            _tokenStorage = tokenStorage;
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        public async Task<Athlete> Execute(EquipperUser user) 
        {
            if(user == null || !user.Authenticated) 
            {
                throw new UnauthorizedException();
            }
            
            var tokens = await TokenHelper.GetTokensByUser(_tokenStorage, _tokenProvider, user);
            if(tokens == null) return null;
            return await _athleteClient.GetAthlete(tokens.AccessToken, tokens.AthleteID);
        }
    }
}