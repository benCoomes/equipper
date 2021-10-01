using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ISubscriptionClient
    {
        string VerificationToken { get; }

        Task CreateSubscription(Subscription subscription);

        Task<Subscription> GetSubscription();

        Task DeleteSubscription(Subscription subscription);

        string GetSubscriptionConfirmation(string challenge, string verificationCode);
    }
}