using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetSubscriptionConfirmation
    {
        private ILogger _logger;
        
        public GetSubscriptionConfirmation(ILogger logger) 
        {
            _logger = logger;
        }

        public string Execute(string challenge, string actualVerificationToken, string expectedVerificationToken) 
        {
            if(actualVerificationToken != expectedVerificationToken) 
            {
                _logger.LogWarning(
                    "Recieved verification token '{actualVerificationToken}' that does not match the expected token '{expectedVerificationToken}'", 
                    actualVerificationToken, 
                    expectedVerificationToken);
                throw new BadRequestException("Verification token does not match expected value.");
            }
            
            return $"{{ \"hub.challenge\": \"{challenge}\" }}";
        }
    }
}
