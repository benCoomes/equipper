using System;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class CreateSubscription
    {
        private ISubscriptionClient _subscriptionClient;
        
        public CreateSubscription(ISubscriptionClient subscriptionClient) {
            _subscriptionClient = subscriptionClient;
        }

        public Task Execute(string callbackUrl) 
        {
            ValidateUrl(callbackUrl);

            // todo: check if exists already
                // if yes, and url matches, return
                // if yes, and url does not match, delete existing
            
            var subscription = new Subscription() 
            {
                CallbackUrl = callbackUrl
            };
            
            return _subscriptionClient.CreateSubscription(subscription);
        }

        private void ValidateUrl(string callbackUrl) 
        {
            if(string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ArgumentException($"{nameof(callbackUrl)} cannot be null or empty.", nameof(callbackUrl));
            }

            var isValid = Uri.TryCreate(callbackUrl, UriKind.Absolute, out var uri);
            if(!isValid || string.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath == "/")
            {
                throw new ArgumentException($"{nameof(callbackUrl)} must be a valid URI with a path", nameof(callbackUrl));
            }
        }
    }
}
