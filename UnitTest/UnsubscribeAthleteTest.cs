using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;
using Coomes.Equipper.Operations;
using Coomes.Equipper.Contracts;
using System;
using Moq;
using Microsoft.Extensions.Logging;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class UnsubscribeAthleteTest
    {
        [TestMethod]
        public async Task UnsubscribeAthlete_DeletesStoredData()
        {
            // given
            var athleteID = 123;

            var mockLogger = new Mock<ILogger>();
            var mockTokenStorage = new Mock<ITokenStorage>();

            var sut = new UnsubscribeAthleteOperation(mockTokenStorage.Object, mockLogger.Object);

            // when
            await sut.Execute(athleteID);

            // then
            mockTokenStorage.Verify(ts => ts.DeleteTokens(athleteID), Times.Once);
        }

        [TestMethod]
        public async Task UnsubscribeAthlete_LogsAndThrowsWhenDeleteFails()
        {
            // given
            var athleteID = 123;
            var expectedException = new Exception("Something horrible!");

            var mockLogger = new Mock<ILogger>();
            var mockTokenStorage = new Mock<ITokenStorage>();
            mockTokenStorage
                .Setup(ts => ts.DeleteTokens(athleteID))
                .ThrowsAsync(expectedException);

            var sut = new UnsubscribeAthleteOperation(mockTokenStorage.Object, mockLogger.Object);

            // when
            Func<Task> tryExecute = () => sut.Execute(athleteID);

            // then
            await tryExecute.Should().ThrowAsync<Exception>().Where(e => e.Message == expectedException.Message);
            mockTokenStorage.Verify(ts => ts.DeleteTokens(athleteID), Times.Once);
            mockLogger
                .Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), expectedException, It.IsAny<Func<It.IsAnyType, Exception, string>>()), 
                Times.Once);
        }
    }
}