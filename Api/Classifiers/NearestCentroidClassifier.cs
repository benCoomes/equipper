using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Classifiers
{
    public class NearestCentroidClassifier : Classifier
    {
        public override string AlgorithmName => "NearestCentroid";

        public NearestCentroidClassifier(ILogger logger) : base(logger)
        {
        }

        protected override string InnerClassify(Activity activity, IEnumerable<Activity> classifiedActivities, bool doLogging) 
        {
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