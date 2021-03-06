using System.Threading.Tasks;
using System.Collections.Generic;

namespace Coomes.Equipper.Contracts
{
    public interface IActivityData 
    {
        Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 1, int limit = 50);
        Task<Activity> UpdateGear(string accessToken, Activity activity);
    }
}