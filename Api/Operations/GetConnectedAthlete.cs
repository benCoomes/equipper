using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetConnectedAthlete
    {
        private IStravaData _stravaData;
        private ITokenStorage _tokenStorage;
        private ITokenProvider _tokenProvider;
        private ILogger _logger;

        public GetConnectedAthlete(IStravaData stravaData, ITokenStorage tokenStorage, ITokenProvider tokenProvider, ILogger logger) 
        {
            _stravaData = stravaData;
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
            return await _stravaData.GetAthlete(tokens.AccessToken, tokens.AthleteID);
        }
    }
}