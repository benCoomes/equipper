using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetProccessedActivityCount
    {
        private IActivityStorage _activityStorage;
        private ILogger _logger;
        
        public GetProccessedActivityCount(IActivityStorage activityStorage, ILogger logger) 
        {
            _activityStorage = activityStorage;
            _logger = logger;
        }

        public Task<int> Execute() 
        {
            // do not check in!
            Task.Delay(5000).GetAwaiter().GetResult();
            // do not check in!

            return _activityStorage.CountActivityResults();
        }
    }
}
