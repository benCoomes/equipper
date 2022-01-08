using System.Collections.Generic;

namespace Coomes.Equipper.Contracts
{
    public interface IClassifier 
    {
        string Classify(Activity activity, IEnumerable<Activity> classifiedActivities);
        int CrossValidateAndLog(IEnumerable<Activity> activities);
    }
}