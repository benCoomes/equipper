using System;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class GetMostReccentActivities
    {
        private IActivityData _activityData;
        
        public GetMostReccentActivities(IActivityData activityData) {
            _activityData = activityData;
        }

        public void Execute(int athleteID) 
        {
            throw new NotImplementedException();
        }
    }
}
