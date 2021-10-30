using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class SetGearTest
    {
        private int _triggerActivityId;
        private int _athleteId;
        private AthleteTokens _athleteTokens;
        private List<Activity> _mostReccentActivities;

        private Mock<ITokenProvider> _tokenProviderMock;
        private Mock<ITokenStorage> _tokenStorageMock;
        private Mock<IActivityData> _activityDataMock;

        public void InitMocks()
        {
            _tokenStorageMock = new Mock<ITokenStorage>();
            _tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(_athleteTokens);

            _tokenProviderMock = new Mock<ITokenProvider>();
            
            _activityDataMock = new Mock<IActivityData>();
            _activityDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("ACCESS DENIED")); // todo: what is actually thrown?
            _activityDataMock
                .Setup(ad => ad.GetActivities(_athleteTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_mostReccentActivities);
        }

        [TestMethod]
        public async Task SetGear_ThrowsSetGearException_IfTriggeringActivityIsNotFound()
        {
            // given
            _triggerActivityId = 4;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostReccentActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                }
            };

            InitMocks();
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object);
            
            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();
        }

        [TestMethod]
        public async Task SetGear_GetsReccentActivitiesAndSetsBestMatchGear() 
        {
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostReccentActivities = new List<Activity>()
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

            InitMocks();
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _activityDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once
            );
        }

        [TestMethod]
        public void SetGear_IgnoresRetiredGear() 
        {
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void SetGear_IgnoresActivity_WhenAlreadyProcessed() 
        {
            // Either need to keep track of processed activities
            // or ignore activies with non-default gear. 
            // Athletes could then set 'null' gear as default.
            Assert.Inconclusive("Not implemented");
        }

        [TestMethod]
        public void SetGear_Throws_WhenUserNotRegistered() 
        {
            Assert.Inconclusive("Not implemented");
        }

    }
}