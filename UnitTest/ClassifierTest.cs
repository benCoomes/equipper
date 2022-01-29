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
            sut.TryDoCrossValidation(activitySetAllCorrect, out var actualAllCorrect);
            sut.TryDoCrossValidation(activitySetHalfCorrect, out var actualHalfCorrect);

            // assert
            actualAllCorrect.Correct.Should().Be(4);
            actualHalfCorrect.Correct.Should().Be(2);
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
            var emptyStatus = sut.TryDoCrossValidation(emptyActivitySet, out var actualEmptyActivityResult);
            var oneActivityStatus = sut.TryDoCrossValidation(oneActivitySet, out var actualOneActivityResult);
            sut.TryDoCrossValidation(twoActivitySetOneCorrect, out var actualTwoActivityOneCorrectResult);
            sut.TryDoCrossValidation(twoActivitySetZeroCorrect, out var actualTwoActivityZeroCorrectResult);

            // assert
            emptyStatus.Should().BeFalse();
            actualEmptyActivityResult.Should().BeNull();

            oneActivityStatus.Should().BeFalse();
            actualOneActivityResult.Should().BeNull();

            actualTwoActivityZeroCorrectResult.Correct.Should().Be(0);
            actualTwoActivityOneCorrectResult.Correct.Should().Be(1);
        }
    }
}
