using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface ITokenProvider
    {
        Task<string> GetToken(string accessCode);
    }
}