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
        private NearestCentroidClassifier _matcher;

        public SetGear(IActivityData activityData, ITokenStorage tokenStorage, ITokenProvider tokenProvider)
        {
            _activityData = activityData;
            _tokenStorage = tokenStorage;
            _tokenProvider = tokenProvider;
            _matcher = new NearestCentroidClassifier();
        }

        public async Task Execute(long athleteID, long activityID)
        {
            var athleteTokens = await GetTokensAndRefreshIfNeeded(athleteID);

            var activities = await _activityData.GetActivities(athleteTokens.AccessToken);

            var newActivity = activities.SingleOrDefault(a => a.Id == activityID);
            var otherActivities = activities.Where(a => a.Id != activityID);
            if (newActivity == null)
            {
                // todo: log
                throw new SetGearException("The triggering activity was not in the most reccent activities");
            }

            // todo: don't use activities where the gear has been set by Equipper?
            var bestMatchGearId = _matcher.Classify(newActivity, otherActivities); 
            newActivity.GearId = bestMatchGearId;

            await _activityData.UpdateGear(athleteTokens.AccessToken, newActivity);
        }

        private async Task<AthleteTokens> GetTokensAndRefreshIfNeeded(long athleteID)
        {
            var athleteTokens = await _tokenStorage.GetTokens(athleteID);

            if (athleteTokens == null)
            {
                throw new SetGearException($"No athlete with ID {athleteID} is registered.");
            }

            var refreshAt = athleteTokens.ExpiresAtUtc.Subtract(TimeSpan.FromMinutes(5));
            var now = DateTime.UtcNow;
            if (refreshAt < now)
            {
                var newTokens = await _tokenProvider.RefreshToken(athleteTokens.RefreshToken);
                await _tokenStorage.AddOrUpdateTokens(newTokens); // todo: concurrent updates?? take latest expire time?
                athleteTokens = newTokens;
            }

            return athleteTokens;
        }
    }
}