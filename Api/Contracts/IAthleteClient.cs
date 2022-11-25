using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface IAthleteClient 
    {
        Task<Athlete> GetAthlete(string accessToken, long athleteId);
    }
}