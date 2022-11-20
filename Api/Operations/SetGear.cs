using System;
using System.Collections.Generic;
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
        private IActivityStorage _activityStorage;
        private ITokenStorage _tokenStorage;
        private ITokenProvider _tokenProvider;
        private ILogger _logger;
        private NearestCentroidClassifier _matcher;
        private List<Classifier> _candidateMatchers;

        public SetGear(IActivityData activityData, IActivityStorage activityStorage, ITokenStorage tokenStorage, ITokenProvider tokenProvider, ILogger logger)
        {
            _activityData = activityData;
            _activityStorage = activityStorage;
            _tokenStorage = tokenStorage;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _matcher = new NearestCentroidClassifier(logger);
            _candidateMatchers = new List<Classifier> 
            { 
                new NearestCentroidClassifier(logger),
                new MostFrequentClassifier(logger) 
            };
        }

        public async Task Execute(long athleteID, long activityID)
        {
            var athleteTokens = await GetTokensAndRefreshIfNeeded(athleteID);

            if(await _activityStorage.ContainsResults(athleteID, activityID))
            {
                _logger.LogInformation("Ignoring activity event for activity {activityId} because it has already been processed.");
                return;
            }
            
            var activities = await _activityData.GetActivities(athleteTokens.AccessToken);

            var newActivity = activities.SingleOrDefault(a => a.Id == activityID);
            if (newActivity == null)
            {
                throw new SetGearException("The triggering activity was not in the most recent activities");
            }
            
            var otherActivities = activities.Where(a => a.Id != activityID && !string.IsNullOrWhiteSpace(a.GearId)).ToList();
            if(otherActivities.Count == 0) 
            {
                throw new SetGearException("There are no historical activities on which to base a gear selection.");
            }
            
            await RecordActivityClassification(newActivity, otherActivities);

            var bestMatchGearId = _matcher.Classify(newActivity, otherActivities); 
            newActivity.GearId = bestMatchGearId;

            await _activityData.UpdateGear(athleteTokens.AccessToken, newActivity);
        }

        private async Task RecordActivityClassification(Activity newActivity, IEnumerable<Activity> activities) 
        {
            try
            {
                var results = new ClassificationStats() { Id = Guid.NewGuid() };
                foreach(var matcher in _candidateMatchers) 
                {
                    if(matcher.TryDoCrossValidation(activities, out var crossValidation))
                    {
                        results.CrossValidations.Add(crossValidation);
                    }
                }
                await _activityStorage.StoreActivityResults(newActivity, results);
            }
            catch(Exception e)
            {
                _logger.LogWarning(e, "Failed to store activity classifications.");
            }
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
                newTokens.UserID = athleteTokens.UserID;
                await _tokenStorage.AddOrUpdateTokens(newTokens); // todo: concurrent updates?? take latest expire time?
                athleteTokens = newTokens;
            }

            return athleteTokens;
        }
    }
}