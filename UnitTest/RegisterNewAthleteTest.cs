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
            var user = new LoggedInUser("1234", "User1234");
            var expectedCode = "expectedCode";
            var expectedTokens = new AthleteTokens()
            {
                AthleteID = 1234,
                AccessToken = "expectedAccessToken",
                RefreshToken = "expectedRefreshToken",
                ExpiresAtUtc = DateTime.UtcNow.AddHours(4)
            };

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(expectedCode))
                .ReturnsAsync(expectedTokens);

            var tokenStorageMock = new Mock<ITokenStorage>();
            tokenStorageMock
                .Setup(ts => ts.AddOrUpdateTokens(expectedTokens));
            
            var loggerMock = new Mock<ILogger>();

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, loggerMock.Object);

            // when
            await sut.Execute(expectedCode, _validAuthScopes, user, error: "");

            // then
            tokenProviderMock.Verify(
                tp => tp.GetToken(expectedCode), 
                Times.Once);
            tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(expectedTokens),
                Times.Once
            );
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
            await tryAnonUser.Should().ThrowAsync<NotAuthorizedException>();
            await tryNullUser.Should().ThrowAsync<NotAuthorizedException>();
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
            await tryUpdate.Should().ThrowAsync<BadRequestException>();
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