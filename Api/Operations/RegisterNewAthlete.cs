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

        public Task<string> Execute(string authCode) 
        {
            return _tokenProvider.GetToken(authCode);
        }
    }
}
