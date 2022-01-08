using System;
using System.Linq;
using System.Threading.Tasks;
using Coomes.Equipper.Classifiers;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Operations
{
    public class SetGear
    {
        private IActivityData _activityData;
        private ITokenStorage _tokenStorage;
        private ITokenProvider _tokenProvider;
        private ILogger _logger;
        private NearestCentroidClassifier _matcher;

        public SetGear(IActivityData activityData, ITokenStorage tokenStorage, ITokenProvider tokenProvider, ILogger logger)
        {
            _activityData = activityData;
            _tokenStorage = tokenStorage;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _matcher = new NearestCentroidClassifier(logger);
        }

        public async Task Execute(long athleteID, long activityID)
        {
            var athleteTokens = await GetTokensAndRefreshIfNeeded(athleteID);

            var activities = await _activityData.GetActivities(athleteTokens.AccessToken);

            var newActivity = activities.SingleOrDefault(a => a.Id == activityID);
            if (newActivity == null)
            {
                throw new SetGearException("The triggering activity was not in the most reccent activities");
            }
            
            // todo: don't use activities where the gear has been set by Equipper?
            var otherActivities = activities.Where(a => a.Id != activityID && !string.IsNullOrWhiteSpace(a.GearId)).ToList();
            if(otherActivities.Count == 0) 
            {
                throw new SetGearException("There are no historical activities on which to base a gear selection.");
            }
            
            _matcher.CrossValidateAndLog(otherActivities);

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
                var newTokens = await _tokenProvider.RefreshToken(athleteTokens);
                await _tokenStorage.AddOrUpdateTokens(newTokens); // todo: concurrent updates?? take latest expire time?
                athleteTokens = newTokens;
            }

            return athleteTokens;
        }
    }
}