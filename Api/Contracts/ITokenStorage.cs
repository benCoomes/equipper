using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ITokenStorage
    {
        Task Initialize();
        Task AddOrUpdateTokens(AthleteTokens tokens);
        Task<AthleteTokens> GetTokens(long athleteID);
    }
}