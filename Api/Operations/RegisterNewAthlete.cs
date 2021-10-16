using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class RegisterNewAthlete
    {
        private ITokenProvider _tokenProvider;
        
        public RegisterNewAthlete(ITokenProvider tokenProvider) {
            _tokenProvider = tokenProvider;
        }

        public async Task<string> Execute(string authCode) 
        {
            var athleteTokens = await _tokenProvider.GetToken(authCode);
            return athleteTokens.AccessToken;
        }
    }
}
