using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class RegisterNewAthleteTest
    {
        private static AuthScopes _validAuthScopes = new AuthScopes() 
        {
            ReadPublic = true,
            ActivityWrite = true,
            ActivityRead = true
        };

        [TestMethod]
        public async Task RegisterNewAthlete_ExchangesAuthCodeAndStoresTokens() 
        {
            // given
            var user = new LoggedInUser("5678", "User5678");
            var storedTokens = default(AthleteTokens);
            var expectedCode = "expectedCode";
            var expiresAt = DateTime.UtcNow.AddHours(4);
            var stravaTokens = new AthleteTokens()
            {
                UserID = null, // Strava response does not include UserID, which comes from SWA authentication
                AthleteID = 1234,
                AccessToken = "expectedAccessToken",
                RefreshToken = "expectedRefreshToken",
                ExpiresAtUtc = expiresAt
            };

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(expectedCode))
                .ReturnsAsync(stravaTokens);

            var tokenStorageMock = new Mock<ITokenStorage>();
            tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()))
                .Callback<AthleteTokens>(tokens => storedTokens = tokens);
            
            var loggerMock = new Mock<ILogger>();

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, loggerMock.Object);

            // when
            await sut.Execute(expectedCode, _validAuthScopes, user, error: "");

            // then
            tokenProviderMock.Verify(
                tp => tp.GetToken(expectedCode), 
                Times.Once);
            tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()),
                Times.Once
            );
            storedTokens.AccessToken.Should().Be("expectedAccessToken");
            storedTokens.RefreshToken.Should().Be("expectedRefreshToken");
            storedTokens.AthleteID.Should().Be(1234);
            storedTokens.ExpiresAtUtc.Should().Be(expiresAt);
            storedTokens.UserID.Should().Be(user.UserId);
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsNotAuthorized_ForAnonymousUser() 
        {
            // given
            var anonUser = new AnonymousUser();
            var authCode = "someAuthCode";

            var sut = new RegisterNewAthlete(null, null, null);

            // when
            Func<Task> tryAnonUser = () => sut.Execute(authCode, _validAuthScopes, anonUser, error: null);
            Func<Task> tryNullUser = () => sut.Execute(authCode, _validAuthScopes, anonUser, error: null);
            
            // then
            await tryAnonUser.Should().ThrowAsync<UnauthorizedException>();
            await tryNullUser.Should().ThrowAsync<UnauthorizedException>();
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsBadRequest_ForUserWithDifferentAthleteRegistration() 
        {
            // given
            var user = new LoggedInUser("user", "User Name");
            var authCode = "someAuthCode";
            var existingAthleteID = 1234;
            var newAthleteID = 5678;
            var stravaTokens = new AthleteTokens()
            {
                AthleteID = newAthleteID
            };
            var existingTokens = new AthleteTokens()
            {
                UserID = user.UserId,
                AthleteID = existingAthleteID
            };

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(authCode))
                .ReturnsAsync(stravaTokens);
            var tokenStorageMock = new Mock<ITokenStorage>();
            tokenStorageMock
                .Setup(ts => ts.GetTokenForUser(user.UserId))
                .ReturnsAsync(existingTokens);
            tokenStorageMock
                .Setup(ts => ts.GetTokens(newAthleteID))
                .Returns(Task.FromResult<AthleteTokens>(null));
            tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(stravaTokens));

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, Mock.Of<ILogger>());

            // when
            Func<Task> tryRegister = () => sut.Execute(authCode, _validAuthScopes, user, error: null);

            // then
            await tryRegister.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.Message == "existing_strava_account");;
            tokenStorageMock.Verify(ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()), Times.Never);
        }

        [TestMethod]
        public async Task RegisterNewAthlete_UpdatesTokens_ForUserWithSameAthleteRegistration() 
        {
            // given
            var user = new LoggedInUser("user", "User Name");
            var authCode = "someAuthCode";
            var athleteID = 1234;
            var stravaTokens = new AthleteTokens()
            {
                AthleteID = athleteID,
                AccessToken = "brandNewAccessToken"
            };
            var existingTokens = new AthleteTokens()
            {
                UserID = user.UserId,
                AthleteID = athleteID
            };

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(authCode))
                .ReturnsAsync(stravaTokens);
            var tokenStorageMock = new Mock<ITokenStorage>();
            tokenStorageMock
                .Setup(ts => ts.GetTokenForUser(user.UserId))
                .ReturnsAsync(existingTokens);
            tokenStorageMock
                .Setup(ts => ts.GetTokens(athleteID))
                .ReturnsAsync(existingTokens);
            tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(stravaTokens));

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, Mock.Of<ILogger>());

            // when
            var storedToken = await sut.Execute(authCode, _validAuthScopes, user, error: null);

            // then
            storedToken.Should().Be(stravaTokens.AccessToken);
            tokenStorageMock.Verify(ts => ts.AddOrUpdateTokens(stravaTokens), Times.Once);
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsBadRequest_ForExistingAthlete_RegisteredToDifferentUser() 
        {
            // given
            var user = new LoggedInUser("requesting_user", "Requesting User");
            var authCode = "someAuthCode";
            var athleteID = 1234;
            var stravaTokens = new AthleteTokens()
            {
                AthleteID = athleteID
            };
            var storedTokens = new AthleteTokens()
            {
                UserID = "other_user",
                AthleteID = athleteID
            };

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(authCode))
                .ReturnsAsync(stravaTokens);

            var tokenStorageMock = new Mock<ITokenStorage>();
            tokenStorageMock
                .Setup(ts => ts.GetTokens(athleteID))
                .ReturnsAsync(storedTokens);
            tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()));

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, Mock.Of<ILogger>());

            // when
            Func<Task> tryUpdate = () => sut.Execute(authCode, _validAuthScopes, user, error: null);

            // then
            await tryUpdate.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.Message == "existing_equipper_account");
            tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(It.IsAny<AthleteTokens>()),
                Times.Never
            );
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsBadRequest_WhenInsufficientScopes() 
        {
            // given
            var user = new LoggedInUser("1234", "User1234");
            var authCode = "someAuthCode";
            var invalidScopes = new AuthScopes() 
            {
                ReadPublic = true,
                ActivityRead = true,
                ActivityWrite = false
            };
            
            var tokenProviderMock = new Mock<ITokenProvider>();
            var tokenStorageMock = new Mock<ITokenStorage>();
            var loggerMock = new Mock<ILogger>();

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, loggerMock.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(authCode, invalidScopes, user, error: null);

            // then
            await tryExecute.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.Message == "insufficient_scopes");
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsBadRequest_WhenError() 
        {
            // given
            var user = new LoggedInUser("1234", "User1234");
            var authCode = "someAuthCode";
            var error = "access_denied";

            var tokenProviderMock = new Mock<ITokenProvider>();
            var tokenStorageMock = new Mock<ITokenStorage>();
            var loggerMock = new Mock<ILogger>();

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, loggerMock.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(authCode, _validAuthScopes, user, error);

            // then
            await tryExecute.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.Message == "auth_error");
        }
    }
}