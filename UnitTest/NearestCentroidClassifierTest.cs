using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;
using Moq;
using Microsoft.Extensions.Logging;
using Coomes.Equipper.Classifiers;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class NearestCentroidClassifierTest
    {
        [TestMethod]
        public void Classify_ClassifiesWithOneCategory()
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
        public void Classify_ClassifiesWithTwoCategories()
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
    
        [TestMethod]
        public void CrossValidate_ReportsCorrectScore() 
        {
            var bike1 = "bike1";
            var bike2 = "bike2";
            
            // 1, 2, 3 - b1: 12.5, b2: 10 - correct (5 closer to 10)
            // 1, 2, 4 - b1: 12.5, b2: 5 - incorrect (10 closer to 12.5)
            // 1, 3, 4 - b1: 10, b2: 7.5 - correct (15 closer to 12.5)
            // 2, 3, 4 - b1: 15, b2: 7.5 - incorrect (10 closer to 7.5) 
            var activitySetHalfCorrect = new List<Activity>() 
            {
                new Activity() 
                {
                    AverageSpeed = 10,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 15,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 10,
                    GearId = bike2
                },
                new Activity() 
                {
                    AverageSpeed = 5,
                    GearId = bike2
                }
            };

            // 1, 2, 3 - b1: 14, b2: 7 - correct (5 closer to 7)
            // 1, 2, 4 - b1: 14, b2: 5 - correct (7 closer to 5)
            // 1, 3, 4 - b1: 15, b2: 6 - correct (13 closer to 15)
            // 2, 3, 4 - b1: 13, b2: 6 - correct (15 closer to 13)
            var activitySetAllCorrect = new List<Activity>() 
            {
                new Activity() 
                {
                    AverageSpeed = 13,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 15,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 7,
                    GearId = bike2
                },
                new Activity() 
                {
                    AverageSpeed = 5,
                    GearId = bike2
                }
            };

            var sut = new NearestCentroidClassifier(Mock.Of<ILogger>());
            
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
                new Activity() 
                {
                    AverageSpeed = 10,
                    GearId = bike1
                }
            };

            var twoActivitySetWrong = new List<Activity>() 
            {
                new Activity() 
                {
                    AverageSpeed = 10,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 7,
                    GearId = bike2
                }
            };

            var twoActivitySetCorrect = new List<Activity>() 
            {
                new Activity() 
                {
                    AverageSpeed = 10,
                    GearId = bike1
                },
                new Activity() 
                {
                    AverageSpeed = 7,
                    GearId = bike1
                }
            };

            var sut = new NearestCentroidClassifier(Mock.Of<ILogger>());
            
            // act
            int actualEmptyActivityResult = sut.CrossValidateAndLog(emptyActivitySet);
            int actualOneActivityResult = sut.CrossValidateAndLog(oneActivitySet);
            int actualTwoActivityWrongResult = sut.CrossValidateAndLog(twoActivitySetWrong);
            int actualTwoActivityCorrectResult = sut.CrossValidateAndLog(twoActivitySetCorrect);

            // assert
            actualEmptyActivityResult.Should().Be(0);
            actualOneActivityResult.Should().Be(0);
            actualTwoActivityWrongResult.Should().Be(0);
            actualTwoActivityCorrectResult.Should().Be(2);
        }
    }
}
