using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class SetGear
    {
        IActivityData _activityData;

        public SetGear(IActivityData activityData)
        {
            _activityData = activityData;
        }

        public Task Execute(long athleteID, long activityID)
        {
            // get athlete from storage
            
            // if DNE, log and throw

            // if expired
                // refresh token
                // update data
                // concurrent updates?? take latest expire time?
            
            // request most reccent activities

            // get new activity details

            // find best gear match

            // udpate activity

            return Task.CompletedTask;
        }
    }
}