using System;
using System.Collections.Generic;
using System.Linq;

namespace Coomes.Equipper
{
    public class NearestCentroidClassifier
    {
        public string Classify(Activity activity, IEnumerable<Activity> classifiedActivities)
        {
            var classes = GetClasses(classifiedActivities);
            var closestMatch = GetClosestMatch(activity, classes);

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

        private struct GearClass
        {
            public string GearId { get; set; }
            public double AvgAvgSpeed { get; set; }
        }
    }

}