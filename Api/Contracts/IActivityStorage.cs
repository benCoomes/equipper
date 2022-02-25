using System;
using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface IActivityStorage
    {
        Task StoreActivityResults(Activity activity, ClassificationStats classificationStats);

        Task<bool> ContainsResults(long athleteId, long activityId);

        Task<int> CountActivityResults();
    }
}