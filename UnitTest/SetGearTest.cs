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
        private List<Activity> _mostRecentActivities;
        private Dictionary<string, Gear> _athleteGear = new Dictionary<string, Gear>()
        {
            { "gear_1", new Gear()
                {
                    Id = "gear_1",
                    Name = "My Best Bike",
                    Retired = false
                }
            },
            { "gear_2", new Gear()
                {
                    Id = "gear_2",
                    Name = "My Second Best Bike",
                    Retired = false
                }
            },
            { "retired_gear", new Gear()
                {
                    Id = "retired_gear",
                    Name = "My Old Bike",
                    Retired = true
                }
            }
        };

        private Mock<ITokenProvider> _tokenProviderMock;
        private Mock<ITokenStorage> _tokenStorageMock;
        private Mock<IStravaData> _stravaDataMock;
        private Mock<IActivityStorage> _activityStorageMock;
        private Mock<ILogger> _loggerMock;

        private void InitMocks()
        {
            _tokenStorageMock = new Mock<ITokenStorage>();
            _tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(_athleteTokens);

            _tokenProviderMock = new Mock<ITokenProvider>();
            
            _stravaDataMock = new Mock<IStravaData>();
            _stravaDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new UnauthorizedException());
            _stravaDataMock
                .Setup(ad => ad.GetActivities(_athleteTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_mostRecentActivities);
            _stravaDataMock
                .Setup(gd => gd.GetGear(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedException());
            _stravaDataMock
                .Setup(gd => gd.GetGear(_athleteTokens.AccessToken, It.IsAny<string>()))
                .ReturnsAsync((string _, string gearId) => {
                    if (_athleteGear.TryGetValue(gearId, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        // TODO: strava 404's if bike not found. Need to treat this same as retired gear (do not set).
                        // Update real client to throw an error, and throw that same error here.
                        throw new NotImplementedException("Gear dont not exist - test behavior not defined"); 
                    }                    
                });

            _activityStorageMock = new Mock<IActivityStorage>();
            _activityStorageMock
                .Setup(storage => storage.StoreActivityResults(It.IsAny<Activity>(), It.IsAny<ClassificationStats>()))
                .Returns(Task.CompletedTask);

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
            
            _stravaDataMock = new Mock<IStravaData>();
            _stravaDataMock
                .Setup(ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new UnauthorizedException());
            _stravaDataMock
                .Setup(ad => ad.GetActivities(refreshedTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(_mostRecentActivities);
            _stravaDataMock
                .Setup(gd => gd.GetGear(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new UnauthorizedException());
            _stravaDataMock
                .Setup(gd => gd.GetGear(refreshedTokens.AccessToken, It.IsAny<string>()))
                .ReturnsAsync((string _, string gearId) => {
                    if (_athleteGear.TryGetValue(gearId, out var value))
                    {
                        return value;
                    }
                    else
                    {
                        // TODO: strava 404's if bike not found. Need to treat this same as retired gear (do not set).
                        // Update real client to throw an error, and throw that same error here.
                        throw new NotImplementedException("Gear dont not exist - test behavior not defined"); 
                    }                    
                });

            _activityStorageMock = new Mock<IActivityStorage>();
            _activityStorageMock
                .Setup(storage => storage.StoreActivityResults(It.IsAny<Activity>(), It.IsAny<ClassificationStats>()))
                .Returns(Task.CompletedTask);

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
            _mostRecentActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                }
            };

            InitMocks();
            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);
            
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
            _mostRecentActivities = new List<Activity>()
            { 
                new Activity()
                {
                    Id =  _triggerActivityId,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                }
            };

            InitMocks();
            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);
            
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
            _mostRecentActivities = new List<Activity>()
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
            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);
            
            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetGear_GetsRecentActivitiesAndSetsBestMatchGear(bool activityStoreThrows) 
        {
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostRecentActivities = new List<Activity>()
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
                    AverageSpeed = 21,
                    GearId = "gear_2"
                },
                new Activity()
                {
                    Id = 3,
                    AverageSpeed = 20,
                    GearId = null
                }
            };

            InitMocks();
            if(activityStoreThrows)
            {
                _activityStorageMock.Reset();
                _activityStorageMock
                    .Setup(storage => storage.StoreActivityResults(It.IsAny<Activity>(), It.IsAny<ClassificationStats>()))
                    .ThrowsAsync(new Exception("There is a problem with the activity storage."));
            }

            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _stravaDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);
            _stravaDataMock.Verify(
                ad => ad.UpdateGear(It.IsAny<string>(), It.Is<Activity>(a => a.GearId == "gear_2")),
                Times.Once);
        }

        [TestMethod]
        public async Task SetGear_RecordsClassificationStats()
        {
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostRecentActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AthleteId = _athleteId,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                },
                new Activity()
                {
                    Id = 2,
                    AthleteId = _athleteId,
                    AverageSpeed = 21,
                    GearId = "gear_2"
                },
                new Activity()
                {
                    Id = 3,
                    AthleteId = _athleteId,
                    AverageSpeed = 20,
                    GearId = null
                }
            };

            InitMocks();
            var sut = new SetGear(
                _stravaDataMock.Object,
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _activityStorageMock.Verify(
                storage => storage.StoreActivityResults(
                    It.Is<Activity>(a => a.AthleteId == _athleteId && a.Id == _triggerActivityId), 
                    It.IsAny<ClassificationStats>()),
                Times.Once
            );
        }

        [TestMethod]
        public async Task SetGear_RefreshesTokenIfExpired()
        {
            // given
            var storedTokens = default(AthleteTokens);
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                UserID = "5678", // these tokens are already stored and should have a UserID
                AccessToken = "oldAccessToken",
                RefreshToken = "oldRefreshToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1))
            };
            var refreshedTokens = new AthleteTokens()
            {
                UserID = null, // Strava does not include UserID in response
                AccessToken = "newAccessToken",
                RefreshToken = "newRefreshToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostRecentActivities = new List<Activity>()
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
             _tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()))
                .Callback<AthleteTokens>(tokens => storedTokens = tokens);
            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _tokenProviderMock.Verify(
                tp => tp.RefreshToken(_athleteTokens), 
                Times.Once);

            _stravaDataMock.Verify(
                ad => ad.GetActivities(refreshedTokens.AccessToken, It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);

            _tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(refreshedTokens),
                Times.Once);
            storedTokens.UserID.Should().Be(_athleteTokens.UserID);
        }
        
        [TestMethod]
        public async Task SetGear_IgnoresRetiredGear() 
        {
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostRecentActivities = new List<Activity>()
            {
                new Activity()
                {
                    Id = 1,
                    AthleteId = _athleteId,
                    AverageSpeed = 10,
                    GearId = "gear_1"
                },
                new Activity()
                {
                    Id = 2,
                    AthleteId = _athleteId,
                    AverageSpeed = 21,
                    GearId = "retired_gear"
                },
                new Activity()
                {
                    Id = 3,
                    AthleteId = _athleteId,
                    AverageSpeed = 20,
                    GearId = null
                }
            };

            InitMocks();
            var sut = new SetGear(
                _stravaDataMock.Object,
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            // retired_gear is a better match, except that it is retired. So, expect gear_1
            _stravaDataMock.Verify(
                gd => gd.GetGear(It.IsAny<string>(), "gear_1"),
                Times.Once);
            _stravaDataMock.Verify(
                gd => gd.GetGear(It.IsAny<string>(), "retired_gear"),
                Times.Once);
            _stravaDataMock.Verify(
                ad => ad.UpdateGear(It.IsAny<string>(), It.Is<Activity>(a => a.GearId == "gear_1")),
                Times.Once);
        }

        [TestMethod]
        public async Task SetGear_IgnoresActivity_WhenAlreadyProcessed() 
        {
            _triggerActivityId = 3;
            _athleteId = 1000;
            _athleteTokens = new AthleteTokens()
            {
                AccessToken = "validAccessToken",
                AthleteID = _athleteId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };
            _mostRecentActivities = new List<Activity>()
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
                    AverageSpeed = 21,
                    GearId = "gear_2"
                },
                new Activity()
                {
                    Id = 3,
                    AverageSpeed = 20,
                    GearId = "gear_2"
                }
            };

            InitMocks();

            _activityStorageMock
                .Setup(astore => astore.ContainsResults(_athleteId, _triggerActivityId))
                .ReturnsAsync(true);

            var sut = new SetGear(
                _stravaDataMock.Object, 
                _activityStorageMock.Object,
                _tokenStorageMock.Object,
                _tokenProviderMock.Object,
                _loggerMock.Object);

            // when
            await sut.Execute(_athleteId, _triggerActivityId);

            // then
            _activityStorageMock.Verify(
                astore => astore.ContainsResults(It.IsAny<long>(), It.IsAny<long>()),
                Times.Once);
            _stravaDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
            _stravaDataMock.Verify(
                ad => ad.UpdateGear(It.IsAny<string>(), It.Is<Activity>(a => a.GearId == "gear_2")),
                Times.Never);
        }

        [TestMethod]
        public async Task SetGear_Throws_WhenUserNotRegistered() 
        {
            //given
            // todo: standard mock setup doesn't work when _athleteTokens is null.
            var tokenProviderMock = new Mock<ITokenProvider>();
            var stravaDataMock = new Mock<IStravaData>();
            var tokenStorageMock = new Mock<ITokenStorage>();
            var activityStorageMock = new Mock<IActivityStorage>();
            var loggerMock = new Mock<ILogger>();
            tokenStorageMock
                .Setup(ts => ts.GetTokens(_athleteId))
                .ReturnsAsync(default(AthleteTokens)); // ITokenStorage returns null if no athlete
            var sut = new SetGear(
                stravaDataMock.Object, 
                activityStorageMock.Object,
                tokenStorageMock.Object,
                tokenProviderMock.Object,
                loggerMock.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(_athleteId, _triggerActivityId);

            // then
            await tryExecute.Should().ThrowAsync<SetGearException>();

            stravaDataMock.Verify(
                ad => ad.GetActivities(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never
            );
        }

    }
}