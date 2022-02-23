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
            return _activityStorage.CountActivityResults();
        }
    }
}
