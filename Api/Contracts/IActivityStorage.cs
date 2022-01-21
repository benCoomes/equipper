using System;
using System.Threading.Tasks;

namespace Coomes.Equipper.Contracts
{
    public interface IActivityStorage
    {
        Task<bool> ActivityHasBeenProcessed(long stravaActivityID);
        Task StoreActivityResults(Activity activity, ClassificationStats classificationStats);
        Task<ClassificationStats> GetClassificationStats(Guid id);
    }
}