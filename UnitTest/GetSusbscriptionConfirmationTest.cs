using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;
using Coomes.Equipper.Operations;
using System;
using Moq;
using Coomes.Equipper.Contracts;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class GetSubscriptionConfirmationTest
    {
        [TestMethod]
        public void GetSubscriptonConfirmation_ThrowsBadRequestOnMismatchedToken() 
        {
            // given
            var mockLogger = new Mock<ILogger>();
            
            var sut = new GetSubscriptionConfirmation(mockLogger.Object);

            // when
            Action tryGet = () => sut.Execute("challenge", "actualToken", "expectedToken");

            // then
            tryGet.Should().Throw<BadRequestException>();
        }

        [TestMethod]
        public void GetSubscriptonConfirmation_ReturnsExpectedResponseWhenTokensMatch() 
        {
            // given
            var mockLogger = new Mock<ILogger>();
            
            var sut = new GetSubscriptionConfirmation(mockLogger.Object);

            // when
            var result = sut.Execute("challenge", "verifyToken", "verifyToken");

            // then
            result.Should().Be("{ \"hub.challenge\": \"challenge\" }");
        }
    }
}