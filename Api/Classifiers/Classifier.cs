using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.Classifiers
{
    public abstract class Classifier 
    {
        protected readonly ILogger _logger;

        public abstract string AlgorithmName { get; }

        protected Classifier(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract string InnerClassify(Activity activity, IEnumerable<Activity> classifiedActivities, bool doLogging = false);
        
        public string Classify(Activity activity, IEnumerable<Activity> classifiedActivities)
        {
            _logger.LogInformation("Running {algorithm} classification.", AlgorithmName);
            return InnerClassify(activity, classifiedActivities, doLogging: true);
        }
        
        public bool TryDoCrossValidation(IEnumerable<Activity> activities, out CrossValidationResult crossValidationResult)
        {
            if(activities == null || activities.Count() <= 1) 
            {
                crossValidationResult = null;
                return false;
            }
            
            try
            {
                crossValidationResult = DoCrossValidation(activities);
                double correctPercent = ((double)crossValidationResult.Correct) / (double)(crossValidationResult.Total) * 100.0;
                _logger.LogInformation("Cross-validation results for {algorithm}: {crossValidationCorrect} out of {crossValidationTotal} ({crossValidationPercent}%)",
                    crossValidationResult.AlgorithmName,
                    crossValidationResult.Correct,
                    crossValidationResult.Total,
                    Math.Round(correctPercent, 2));
                return true;
            }
            catch(Exception e)
            {
                crossValidationResult = null;
                _logger.LogWarning(e, "Cross Validation because of an unexpected exception.");
                return false;
            }
        }

        private CrossValidationResult DoCrossValidation(IEnumerable<Activity> activities)
        {
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

            return new CrossValidationResult()
            {
                AlgorithmName = this.AlgorithmName,
                Correct = correctCount,
                Total = totalCount
            };
        }
    }
}