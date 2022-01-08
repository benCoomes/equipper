using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Classifiers
{
    public class NearestCentroidClassifier : IClassifier
    {
        private ILogger _logger;

        public NearestCentroidClassifier(ILogger logger)
        {
            _logger = logger;
        }

        public string Classify(Activity activity, IEnumerable<Activity> classifiedActivities)
        {
            return InnerClassify(activity, classifiedActivities, doLogging: true);
        }

        public int CrossValidateAndLog(IEnumerable<Activity> activities)
        {
            if(activities == null || activities.Count() <= 1) 
            {
                return 0;
            }

            var factor = 4;
            var totalCount = activities.Count();
            var correctCount = 0;

            // todo: should blocks be contiguous or distributed? 
            // Ex problem: factor of 2, and activities have 2 bikes which alternate every other activity.
            var blocks = activities
                .Select((a, i) => (index: i, activity: a))
                .GroupBy(tuple => tuple.index % factor)
                .Select(g => g.Select(tuple => tuple.activity).ToList())
                .ToList();

            for(int testBlockIndex = 0; testBlockIndex < blocks.Count; testBlockIndex++)
            {
                var testingData = blocks[testBlockIndex];
                var trainingData = blocks
                    .Where(block => block != testingData)
                    .SelectMany(b => b.ToList());
                
                foreach(var testActivity in testingData) 
                {
                    var result = InnerClassify(testActivity, trainingData, doLogging: false);
                    if(result == testActivity.GearId)
                        correctCount++;
                }
            }

            double correctPercent = ((double)correctCount) / (double)(totalCount) * 100.0;
            _logger.LogInformation("Cross-validation results for {algorithm}: {crossValidationCorrect} out of {crossValidationTotal} ({crossValidationPercent}%)",
                nameof(NearestCentroidClassifier),
                correctCount,
                totalCount,
                Math.Round(correctPercent, 2));
            return correctCount;
        }

        private string InnerClassify(Activity activity, IEnumerable<Activity> classifiedActivities, bool doLogging) {
            if(doLogging) 
            {
                _logger.LogInformation("Running {algorithm} classification.", nameof(NearestCentroidClassifier));
            }
            
            var classes = GetClasses(classifiedActivities);
            if(doLogging)
            {
                _logger.LogInformation("Generated classes: {classes}", classes.ToJson());
            }
            
            var closestMatch = GetClosestMatch(activity, classes);
            if(doLogging)
            {
                _logger.LogInformation("Matched {matchedGearId} for activity {activityID} with average speed of {activityAverageSpeed}", 
                    closestMatch.GearId, 
                    activity.Id, 
                    activity.AverageSpeed);
            }

            return closestMatch.GearId;
        }

        private GearClass GetClosestMatch(Activity activity, List<GearClass> classes)
        {
            var closestMatch = classes.First();
            var minDiff = Math.Abs(closestMatch.AvgAvgSpeed - activity.AverageSpeed);
            foreach (var gearClass in classes)
            {
                var newDiff = Math.Abs(gearClass.AvgAvgSpeed - activity.AverageSpeed);
                if (newDiff < minDiff)
                {
                    minDiff = newDiff;
                    closestMatch = gearClass;
                }
            }
            return closestMatch;
        }

        private List<GearClass> GetClasses(IEnumerable<Activity> classifiedActivities)
        {
            return classifiedActivities
                .GroupBy(a => a.GearId)
                .Select(g => new GearClass
                {
                    GearId = g.Key,
                    AvgAvgSpeed = g.Average(a => a.AverageSpeed)
                })
                .ToList();
        }
    }

    internal struct GearClass
    {
        public string GearId { get; set; }
        public double AvgAvgSpeed { get; set; }
    }

    internal static class GearClassExtensions
    {
        public static string ToJson(this IEnumerable<GearClass> gearClasses)
        {
            return JsonSerializer.Serialize(gearClasses);
        }
    }

}