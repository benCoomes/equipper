using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Operations
{
    public class SetGear
    {
        private IActivityData _activityData;
        private ITokenStorage _tokenStorage;
        private ITokenProvider _tokenProvider;

        public SetGear(IActivityData activityData, ITokenStorage tokenStorage, ITokenProvider tokenProvider)
        {
            _activityData = activityData;
            _tokenStorage = tokenStorage;
            _tokenProvider = tokenProvider;
        }

        public async Task Execute(long athleteID, long activityID)
        {
            // get athlete from storage
            var athleteTokens = await _tokenStorage.GetTokens(athleteID);

            // if DNE, log and throw

            // if expired
            var refreshAt = athleteTokens.ExpiresAtUtc.Subtract(TimeSpan.FromMinutes(5));
            var now = DateTime.UtcNow;
            if(refreshAt < now)
            {
                var newTokens = await _tokenProvider.RefreshToken(athleteTokens.RefreshToken);
                await _tokenStorage.AddOrUpdateTokens(newTokens); // todo: concurrent updates?? take latest expire time?
                athleteTokens = newTokens;
            }
            
            // request most reccent activities
            var activities = await _activityData.GetActivities(athleteTokens.AccessToken);

            // get new activity details
            var newActivity = activities.SingleOrDefault(a => a.Id == activityID);
            if(newActivity == null)
            {
                // todo: log
                throw new SetGearException("The triggering activity was not in the most reccent activities");
            }

            // find best gear match

            // update activity
        }
    }
}