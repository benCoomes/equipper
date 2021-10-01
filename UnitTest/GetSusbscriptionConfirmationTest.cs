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
            var mockSubClient = new Mock<ISubscriptionClient>();
            mockSubClient
                .SetupGet(sc => sc.VerificationToken)
                .Returns("actualVerificationToken");
            var mockLogger = new Mock<ILogger>();
            
            var sut = new GetSubscriptionConfirmation(mockSubClient.Object, mockLogger.Object);

            // when
            Action tryGet = () => sut.Execute("challenge", "mismatchedToken");

            // then
            tryGet.Should().Throw<BadRequestException>();
        }
    }
}