using System.Threading.Tasks;
using System.Collections.Generic;

namespace Coomes.Equipper.Contracts
{
    public interface IActivityData 
    {
        Task<IEnumerable<Activity>> GetActivities(string accessToken, int page = 0, int limit = 0);
    }
}