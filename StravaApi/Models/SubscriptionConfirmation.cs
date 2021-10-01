namespace Coomes.Equipper.StravaApi.Models
{
    internal class SubscriptionConfirmation
    {
        public SubscriptionConfirmation(string challenge) 
        {
            this.challenge = challenge;
        }

        public string challenge { get; }
        
        public string ToJson() {
            return $"{{ \"hub.challenge\": \"{challenge}\"}}";
        }
    }
}
