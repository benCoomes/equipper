using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Classifiers;
using System.Linq;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class MostFrequentClassifierTest
    {
        const string bike1 = "bike1";
        const string bike2 = "bike2";

        [DataTestMethod]
        [DataRow(1, 0, bike1)]
        [DataRow(0, 1, bike2)]
        [DataRow(2, 1, bike1)]
        [DataRow(1, 2, bike2)]
        public void Classify_ClassifiesAsExpected(int bike1Count, int bike2Count, string expectedGearID)
        {
            // arrange
            var manuallyClassifiedActivities = new List<Activity>();
            var bike1Activities = Enumerable.Range(0, bike1Count).Select(_ => new Activity() { GearId = bike1 });
            var bike2Activities = Enumerable.Range(0, bike2Count).Select(_ => new Activity() { GearId = bike2 });
            manuallyClassifiedActivities.AddRange(bike1Activities);
            manuallyClassifiedActivities.AddRange(bike2Activities);

            var unclassifiedActivity = new Activity();

            var sut = new MostFrequentClassifier(Mock.Of<ILogger>());
            
            // act
            var actualGearID = sut.Classify(unclassifiedActivity, manuallyClassifiedActivities);

            // assert
            actualGearID.Should().Be(expectedGearID);
        }

        public void Classify_ChoosesOneWhenTie()
        {
            // arrange
            var manuallyClassifiedActivities = new List<Activity>()
            {
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike1 },
                new Activity() { GearId = bike2 },
                new Activity() { GearId = bike2 },
                new Activity() { GearId = "pogoStick"}
            };

            var unclassifiedActivity = new Activity();

            var sut = new MostFrequentClassifier(Mock.Of<ILogger>());
            
            // act
            var actualGearID = sut.Classify(unclassifiedActivity, manuallyClassifiedActivities);

            // assert
            actualGearID.Should().BeOneOf(bike1, bike2);
        }
    }
}
