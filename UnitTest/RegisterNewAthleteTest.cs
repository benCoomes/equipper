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
        [TestMethod]
        public async Task RegisterNewAthlete_ExchangesAuthCodeAndStoresTokens() 
        {
            // given
            var user = new LoggedInUser("1234", "User1234");
            var expectedCode = "expectedCode";
            var scopes = new AuthScopes() 
            {
                ReadPublic = true,
                ActivityWrite = true,
                ActivityRead = true
            };
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
            await sut.Execute(expectedCode, scopes, user, error: "");

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
        public async Task RegisterNewAthlete_ThrowsNotAuthorized_ForAnonymousUser() {
            // given
            var anonUser = new AnonymousUser();
            var authCode = "someAuthCode";
            var scopes = new AuthScopes()
            {
                ReadPublic = true,
                ActivityRead = true,
                ActivityWrite = true
            };

            var sut = new RegisterNewAthlete(null, null, null);

            // when
            Func<Task> tryAnonUser = () => sut.Execute(authCode, scopes, anonUser, error: null);
            Func<Task> tryNullUser = () => sut.Execute(authCode, scopes, anonUser, error: null);
            
            // then
            await tryAnonUser.Should().ThrowAsync<NotAuthorizedException>();
            await tryNullUser.Should().ThrowAsync<NotAuthorizedException>();
        }

        [TestMethod]
        public async Task RegisterNewAthlete_ThrowsBadRequest_WhenInsufficientScopes() 
        {
            // given
            var user = new LoggedInUser("1234", "User1234");
            var authCode = "someAuthCode";
            var scopes = new AuthScopes() 
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
            Func<Task> tryExecute = () => sut.Execute(authCode, scopes, user, error: null);

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
            var scopes = new AuthScopes() {};
            var error = "access_denied";

            var tokenProviderMock = new Mock<ITokenProvider>();
            var tokenStorageMock = new Mock<ITokenStorage>();
            var loggerMock = new Mock<ILogger>();

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object, loggerMock.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(authCode, scopes, user, error);

            // then
            await tryExecute.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.Message == "auth_error");
        }
    }
}