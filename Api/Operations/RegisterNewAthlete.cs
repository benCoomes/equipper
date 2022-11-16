using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class RegisterNewAthlete
    {
        private ITokenProvider _tokenProvider;
        private ITokenStorage _tokenStorage;
        private ILogger _logger;

        public RegisterNewAthlete(ITokenProvider tokenProvider, ITokenStorage tokenStorage, ILogger logger) {
            _tokenProvider = tokenProvider;
            _tokenStorage = tokenStorage;
            _logger = logger;
        }

        public async Task<string> Execute(string authCode, AuthScopes scopes, EquipperUser user, string error) 
        {
            if(user == null || !user.Authenticated) {
                throw new NotAuthorizedException();
            }

            if(!string.IsNullOrWhiteSpace(error))
            {
                _logger.LogError("Error returned from authorization: {authError}", error);
                throw new BadRequestException("auth_error");
            }

            if(!scopes.CanSetGear())
            {
                _logger.LogError("Failed to register new athlete due to insufficient scopes.");
                throw new BadRequestException("insufficient_scopes");
            }

            var athleteTokens = await _tokenProvider.GetToken(authCode);
            
            var existingTokenForAthlete = await _tokenStorage.GetTokens(athleteTokens.AthleteID);
            if(existingTokenForAthlete?.UserID != null && existingTokenForAthlete.UserID != user.UserId) {
                throw new BadRequestException("Athlete is already associated to an existing Equipper account.");
            }

            // TODO: Do not check if user is associated to other athletes?
            // Do we supported a single Equipper user to have multiple linked Strava accounts?
            // See https://github.com/benCoomes/equipper/issues/46

            await _tokenStorage.AddOrUpdateTokens(athleteTokens);
            return athleteTokens.AccessToken;
        }
    }
}
