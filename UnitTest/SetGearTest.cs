using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class SetGearTest
    {
        [TestMethod]
        public async Task SetGearGetsReccentActivitiesAndSetsBestMatchGear() 
        {
            var activityId = 4;
            var athleteId = 1000;
            var _existingActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                },
                new Activity()
                {
                    Id = 2,
                    AverageSpeed = 11,
                    GearId = "gear_1"
                },
                new Activity()
                {
                    Id = 3,
                    AverageSpeed = 20,
                    GearId = "gear_2"
                }
            };

            var activityDataMock = new Mock<IActivityData>();
            activityDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_existingActivities);

            var sut = new SetGear(activityDataMock.Object);

            // when
            await sut.Execute(athleteId, activityId);

            // then
            activityDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once
            );
        }

        [TestMethod]
        public void SetGearIgnoresRetiredGear() 
        {
            Assert.Inconclusive("Not implmented");
        }

        [TestMethod]
        public void SetGearIgnoresAlreadyProcessedActivities() 
        {
            // Either need to keep track of processed activities
            // or ignore activies with non-default gear. 
            // Althetes could then set 'null' gear as default.
            Assert.Inconclusive("Not implmented");
        }

        [TestMethod]
        public void SetGearThrowsWhenUserNotRegistered() 
        {
            Assert.Inconclusive("Not implmented");
        }

    }
}