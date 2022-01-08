using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Classifiers;

namespace Coomes.Equipper.UnitTest
{
    internal class TestingClassifier : Classifier
    {
        public TestingClassifier(ILogger logger) : base(logger)
        {
        }
        
        public override string AlgorithmName => "Testing Classifier";

        public string ClassificationResult { get; set; } = "Default";

        protected override string InnerClassify(Activity activity, IEnumerable<Activity> classifiedActivities, bool doLogging = false)
        {
            return ClassificationResult;
        }
    }

    [TestClass]
    public class ClassifierTest
    {
        [TestMethod]
        public void CrossValidate_ReportsCorrectScore() 
        {
            var bike1 = "bike1";
            var bike2 = "bike2";
            
            var activitySetHalfCorrect = new List<Activity>() 
            {
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike2 },
                new Activity() { GearId = bike2 }
            };

            var activitySetAllCorrect = new List<Activity>() 
            {
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike1 }
            };

            var sut = new TestingClassifier(Mock.Of<ILogger>());
            sut.ClassificationResult = bike1;
            
            // act
            int actualAllCorrect = sut.CrossValidateAndLog(activitySetAllCorrect);
            int actualHalfCorrect = sut.CrossValidateAndLog(activitySetHalfCorrect);

            // assert
            actualAllCorrect.Should().Be(4);
            actualHalfCorrect.Should().Be(2);
        }

        [TestMethod]
        public void CrossValidate_WorksWithSmallSets() 
        {
            var bike1 = "bike1";
            var bike2 = "bike2";
            
            var emptyActivitySet = new List<Activity>();

            var oneActivitySet = new List<Activity>() 
            {
                new Activity() { GearId = bike1 }
            };

            var twoActivitySetOneCorrect = new List<Activity>() 
            {
                new Activity() { GearId = bike1},
                new Activity() { GearId = bike2}
            };

            var twoActivitySetZeroCorrect = new List<Activity>() 
            {
                new Activity() { GearId = bike2},
                new Activity() { GearId = bike2}
            };

            var sut = new TestingClassifier(Mock.Of<ILogger>());
            sut.ClassificationResult = bike1;
            
            // act
            int actualEmptyActivityResult = sut.CrossValidateAndLog(emptyActivitySet);
            int actualOneActivityResult = sut.CrossValidateAndLog(oneActivitySet);
            int actualTwoActivityOneCorrectResult = sut.CrossValidateAndLog(twoActivitySetOneCorrect);
            int actualTwoActivityZeroCorrectResult = sut.CrossValidateAndLog(twoActivitySetZeroCorrect);

            // assert
            actualEmptyActivityResult.Should().Be(0);
            actualOneActivityResult.Should().Be(0);
            actualTwoActivityZeroCorrectResult.Should().Be(0);
            actualTwoActivityOneCorrectResult.Should().Be(1);
        }
    }
}
