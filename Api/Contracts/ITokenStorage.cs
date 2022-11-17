using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ITokenStorage
    {
        Task AddOrUpdateTokens(AthleteTokens tokens);
        Task<AthleteTokens> GetTokens(long athleteID);
        Task<AthleteTokens> GetTokenForUser(string userID);
        Task DeleteTokens(long athleteID);
    }
}