using System.Threading.Tasks;
using System.Collections.Generic;

namespace Coomes.Equipper.Contracts
{
    public interface IStravaData 
    {
        // activities
        Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 1, int limit = 50);
        Task<Activity> UpdateGear(string accessToken, Activity activity);

        // athlete
        Task<Athlete> GetAthlete(string accessToken, long athleteId);

        // gear
        Task<Gear> GetGear(string accessToken, string gearId);
    }
}