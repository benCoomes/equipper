using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetSubscriptionConfirmation
    {
        private ISubscriptionClient _subscriptionClient;
        private ILogger _logger;
        
        public GetSubscriptionConfirmation(ISubscriptionClient subscriptionClient, ILogger logger) 
        {
            _subscriptionClient = subscriptionClient;
            _logger = logger;
        }

        public string Execute(string challenge, string verificationCode) 
        {
            if(verificationCode != _subscriptionClient.VerificationToken) 
            {
                _logger.LogWarning("Recieved verification token '{actualVerificationToken}' that does not match the expected token '{expectedVerificationToken}'", verificationCode, _subscriptionClient.VerificationToken);
                throw new BadRequestException("Verification token does not match expected value.");
            }

            return _subscriptionClient.GetSubscriptionConfirmation(challenge, verificationCode);
        }
    }
}
