using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;

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
        private Mock<ILogger> _loggerMock;

        private void InitMocks()
        {
            _tokenStorageMock = new Mock<ITokenStorage>();
            _tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(_athleteTokens);

            _tokenProviderMock = new Mock<ITokenProvider>();
            
            _activityDataMock = new Mock<IActivityData>();
            _activityDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new UnauthorizedException());
            _activityDataMock
                .Setup(ad => ad.GetActivities(_athleteTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_mostReccentActivities);

            _loggerMock = new Mock<ILogger>();
        }

        private void InitMocksForRefresh(AthleteTokens refreshedTokens)
        {
            _tokenStorageMock = new Mock<ITokenStorage>();
            _tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(_athleteTokens);
            _tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()));

            _tokenProviderMock = new Mock<ITokenProvider>();
            _tokenProviderMock
                .Setup(tp => tp.RefreshToken(_athleteTokens))
                .ReturnsAsync(refreshedTokens);
            
            _activityDataMock = new Mock<IActivityData>();
            _activityDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new UnauthorizedException());
            _activityDataMock
                .Setup(ad => ad.GetActivities(refreshedTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_mostReccentActivities);

            _loggerMock = new Mock<ILogger>();
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
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object, _loggerMock.Object);
            
            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();
        }

        [TestMethod]
        public async Task SetGear_ThrowsSetGearException_IfNoHistoricalActivities()
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
                    Id =  _triggerActivityId,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                }
            };

            InitMocks();
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object, _loggerMock.Object);
            
            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();
        }

        [TestMethod]
        public async Task SetGear_ThrowsSetGearException_IfNoHistoricalActivitiesWithGear()
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
                    Id =  1,
                    AverageSpeed = 10,
                    GearId = null
                },
                new Activity()
                {
                    Id =  2,
                    AverageSpeed = 10,
                    GearId = ""
                },
                new Activity()
                {
                    Id =  _triggerActivityId,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                }
            };

            InitMocks();
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object, _loggerMock.Object);
            
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
                },
                new Activity() 
                {
                    Id = 4,
                    AverageSpeed = 10000,
                    GearId = null
                }
            };

            InitMocks();
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object, _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _activityDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task SetGear_RefreshesTokenIfExpired()
        {
            // given
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "oldAccessToken",
                RefreshToken = "oldRefreshToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1))
            };
            var refreshedTokens = new AthleteTokens()
            {
                AccessToken = "newAccessToken",
                RefreshToken = "newRefreshToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostReccentActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AverageSpeed = 21,
                    GearId = "gear_2"
                },
                new Activity()
                {
                    Id = _triggerActivityId,
                    AverageSpeed = 20,
                    GearId = "gear_2"
                }
            };

            InitMocksForRefresh(refreshedTokens);
            var sut = new SetGear(_activityDataMock.Object, _tokenStorageMock.Object, _tokenProviderMock.Object, _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _tokenProviderMock.Verify(
                tp => tp.RefreshToken(_athleteTokens), 
                Times.Once);

            _activityDataMock.Verify(
                ad => ad.GetActivities(refreshedTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);

            _tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(refreshedTokens),
                Times.Once);
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
        public async Task SetGear_Throws_WhenUserNotRegistered() 
        {
            //given
            // todo: standard mock setup doesn't work when _athleteTokens is null.
            var tokenProviderMock = new Mock<ITokenProvider>();
            var activityDataMock = new Mock<IActivityData>();
            var tokenStorageMock = new Mock<ITokenStorage>();
            var loggerMock = new Mock<ILogger>();
            tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(default(AthleteTokens)); // ITokenStorage returns null if no athlete
            var sut = new SetGear(activityDataMock.Object, tokenStorageMock.Object, tokenProviderMock.Object, loggerMock.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();

            activityDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never
            );
        }

    }
}