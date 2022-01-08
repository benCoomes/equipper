using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.Classifiers
{
    public class MostFrequentClassifier : IClassifier
    {
        private ILogger _logger;

        public MostFrequentClassifier(ILogger logger) 
        {
            _logger = logger;
        }

        public string Classify(Activity activity, IEnumerable<Activity> classifiedActivities)
        {
            return classifiedActivities
                .GroupBy(a => a.GearId)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
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
                    var result = Classify(testActivity, trainingData);
                    if(result == testActivity.GearId)
                        correctCount++;
                }
            }

            double correctPercent = ((double)correctCount) / (double)(totalCount) * 100.0;
            _logger.LogInformation("Cross-validation results for {algorithm}: {crossValidationCorrect} out of {crossValidationTotal} ({crossValidationPercent}%)",
                nameof(MostFrequentClassifier),
                correctCount,
                totalCount,
                Math.Round(correctPercent, 2));
            return correctCount;
        }
    }
}