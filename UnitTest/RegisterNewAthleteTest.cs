using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using Coomes.Equipper.Contracts;
using Coomes.Equipper.Operations;
using System.Threading.Tasks;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class RegisterNewAthleteTest
    {
        [TestMethod]
        public async Task RegisterNewAthlete_ExchangesAuthCodeForToken() 
        {
            // given
            var expectedCode = "expectedCode";
            var expectedToken = "expectedToken";

            var tokenProviderMock = new Mock<ITokenProvider>();
            tokenProviderMock
                .Setup(tp => tp.GetToken(expectedCode))
                .ReturnsAsync(expectedToken);

            var sut = new RegisterNewAthlete(tokenProviderMock.Object);

            // when
            await sut.Execute(expectedCode);

            // then
            tokenProviderMock.Verify(
                tp => tp.GetToken(expectedCode), 
                Times.Once);
        }
    }
}