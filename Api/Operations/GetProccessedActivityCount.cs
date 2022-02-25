using System;
using System.Threading;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class GetProccessedActivityCount
    {
        private static SemaphoreSlim _computeLock = new SemaphoreSlim(1);
        private static DateTime _lastComputeTime = DateTime.MinValue;
        private static int _lastComputedCount;

        private IActivityStorage _activityStorage;
        private ILogger _logger;
        
        public GetProccessedActivityCount(IActivityStorage activityStorage, ILogger logger) 
        {
            _activityStorage = activityStorage;
            _logger = logger;
        }

        public async Task<int> Execute(int maxStalenessMs = 60_000) 
        {
            if(ShouldRecompute(maxStalenessMs))
            {
                await _computeLock.WaitAsync();
                try
                {
                    if(ShouldRecompute(maxStalenessMs))
                    {
                        var count = await _activityStorage.CountActivityResults();
                        _lastComputedCount = count;
                        _lastComputeTime = DateTime.UtcNow;
                    }
                }
                finally
                {
                    _computeLock.Release();
                }
            }

            return _lastComputedCount;
        }

        private bool ShouldRecompute(int maxStalenessMs)
        {
            return DateTime.UtcNow >  _lastComputeTime.Add(TimeSpan.FromMilliseconds(maxStalenessMs));
        }
    }
}
