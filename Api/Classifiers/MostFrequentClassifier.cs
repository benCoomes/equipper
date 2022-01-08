using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Classifiers
{
    public class MostFrequentClassifier : Classifier
    {
        public override string AlgorithmName => "MostFrequent";

        public MostFrequentClassifier(ILogger logger) : base(logger)
        {
        }

        protected override string InnerClassify(Activity activity, IEnumerable<Activity> classifiedActivities, bool doLogging = false)
        {
            return classifiedActivities
                .GroupBy(a => a.GearId)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }
    }
}