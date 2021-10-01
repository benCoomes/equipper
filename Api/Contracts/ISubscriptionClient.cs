using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ISubscriptionClient
    {
        Task CreateSubscription(Subscription subscription, string verificationToken);

        Task<Subscription> GetSubscription();

        Task DeleteSubscription(Subscription subscription);
    }
}