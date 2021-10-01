using System;
using System.Net.Http;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.StravaApi.Models;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace Coomes.Equipper.StravaApi
{
    public class SubscriptionClient : ISubscriptionClient
    {
        // todo: one http client per library?
        private static HttpClient _httpClient = new HttpClient();
        private static string subscriptionEndpoint = "https://www.strava.com/api/v3/push_subscriptions";
        private  StravaApiOptions _options;
        private ILogger _logger;
        
        private static string _verificationToken;
        public string VerificationToken => _verificationToken;

        static SubscriptionClient()
        {
            _verificationToken = GenerateVerificationToken();

            string GenerateVerificationToken()
            {
                using(var rng = new RNGCryptoServiceProvider())
                {
                    var data = new byte[64]; 
                    rng.GetBytes(data);
                    return Convert.ToBase64String(data);
                }
            }
        }

        public SubscriptionClient(IOptions<StravaApiOptions> options, ILogger logger = null) : this(options.Value, logger) 
        {   
        }

        public SubscriptionClient(StravaApiOptions options, ILogger logger = null)
        {
            _options = options;
            _logger = logger;
        }

        public async Task CreateSubscription(Subscription subscription)
        {
            var requestObj = new Models.SubscriptionCreateRequest() 
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret,
                callback_url = subscription.CallbackUrl,
                verify_token = VerificationToken
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, subscriptionEndpoint);
            using var content = new StringContent(requestObj.ToJson(), Encoding.UTF8, "application/json");
            request.Content = content;

            // a response will only be returned after confirmation succeeds or fails
            var response = await _httpClient.SendAsync(request);
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(SubscriptionClient)}.{nameof(CreateSubscription)}");
        }

        public string GetSubscriptionConfirmation(string challenge, string verificationToken)
        {
            var confirmation = new SubscriptionConfirmation(challenge);
            return confirmation.ToJson();
        }

        public async Task<Subscription> GetSubscription()
        {
            var uriBuilder = new UriBuilder(subscriptionEndpoint);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["client_id"] = _options.ClientId;
            query["client_secret"] = _options.ClientSecret;
            uriBuilder.Query = query.ToString();

            using var response = await _httpClient.GetAsync(uriBuilder.ToString());
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(SubscriptionClient)}.{nameof(GetSubscription)}");
            
            // todo: figure out what the response looks like
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation(body);
            return null;
        }

        public async Task DeleteSubscription(Subscription subscription)
        {        
            var deleteUri = subscriptionEndpoint.TrimEnd('/') + $"/{subscription.Id}";
            var deleteObj = new Models.SubscriptionDeleteRequest() 
            {
                client_id = _options.ClientId,
                client_secret = _options.ClientSecret
            };

            using var request = new HttpRequestMessage(HttpMethod.Delete, deleteUri);
            request.Content = new StringContent(deleteObj.ToJson(), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            await response.LogAndThrowIfNotSuccess(_logger, $"{nameof(SubscriptionClient)}.{nameof(DeleteSubscription)}");
        }
    }
}
