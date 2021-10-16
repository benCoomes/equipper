using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ITokenProvider
    {
        Task<AthleteTokens> GetToken(string accessCode);
    }
}