using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class RegisterNewAthlete
    {
        private ITokenProvider _tokenProvider;
        private ITokenStorage _tokenStorage;
        
        public RegisterNewAthlete(ITokenProvider tokenProvider, ITokenStorage tokenStorage) {
            _tokenProvider = tokenProvider;
            _tokenStorage = tokenStorage;
        }

        public async Task<string> Execute(string authCode) 
        {
            var athleteTokens = await _tokenProvider.GetToken(authCode);
            await _tokenStorage.AddOrUpdateTokens(athleteTokens);
            return athleteTokens.AccessToken;
        }
    }
}
