using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class ExchangeAuthCodeForToken
    {
        private ITokenProvider _tokenProvider;
        
        public ExchangeAuthCodeForToken(ITokenProvider tokenProvider) {
            _tokenProvider = tokenProvider;
        }

        public Task<string> Execute(string authCode) 
        {
            return _tokenProvider.GetToken(authCode);
        }
    }
}
