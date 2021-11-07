using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class NearestCentroidClassifierTest
    {
        [TestMethod]
        public void ClassifiesWithOneCategory()
        {
            // arrange
            var bike = "bike";
            var expectedGearID = bike;
            
            var manuallyClassifiedActivities = new List<Activity>()
            {
                new Activity()
                {
                    AverageSpeed = 10,
                    GearId = bike
                },
                new Activity()
                {
                    AverageSpeed = 20,
                    GearId = bike
                }
            };

            var unclassifiedActivity = new Activity() 
            {
                AverageSpeed = 10    
            };

            var sut = new NearestCentroidClassifier(Mock.Of<ILogger>());
            
            // act
            var actualGearID = sut.Classify(unclassifiedActivity, manuallyClassifiedActivities);

            // assert
            actualGearID.Should().Be(expectedGearID);
        }

        [TestMethod]
        public void ClassifiesWithTwoCategories()
        {
            // arrange
            var bike1 = "bike1";
            var bike2 = "bike2";
            var expectedGearID = bike1;
            
            var manuallyClassifiedActivities = new List<Activity>()
            {
                new Activity()
                {
                    AverageSpeed = 10,
                    GearId = bike1
                },
                new Activity()
                {
                    AverageSpeed = 20,
                    GearId = bike2
                }
            };

            var unclassifiedActivity = new Activity() 
            {
                AverageSpeed = 12
            };

            var sut = new NearestCentroidClassifier(Mock.Of<ILogger>());
            
            // act
            var actualGearID = sut.Classify(unclassifiedActivity, manuallyClassifiedActivities);

            // assert
            actualGearID.Should().Be(expectedGearID);
        }
    
    }
}
