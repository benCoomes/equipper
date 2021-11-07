using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;
using System;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class RegisterNewAthleteTest
    {
        [TestMethod]
        public async Task RegisterNewAthlete_ExchangesAuthCodeAndStoresTokens() 
        {
            // given
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

            var sut = new RegisterNewAthlete(tokenProviderMock.Object, tokenStorageMock.Object);

            // when
            await sut.Execute(expectedCode);

            // then
            tokenProviderMock.Verify(
                tp => tp.GetToken(expectedCode), 
                Times.Once);
            tokenStorageMock.Verify(
                ts => ts.AddOrUpdateTokens(expectedTokens),
                Times.Once
            );
        }
    }
}