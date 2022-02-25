using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Coomes.Equipper.Operations;
using Coomes.Equipper.Contracts;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class GetActivityCountTest
    {

        [TestMethod]
        public async Task GetActivityCount_UsesCache()
        {
            // given
            var count = 42;
            var incrementingCountActivityStorageMock = new Mock<IActivityStorage>();
            incrementingCountActivityStorageMock
                .Setup(astore => astore.CountActivityResults())
                .ReturnsAsync(count);

            var sut = new GetProccessedActivityCount(incrementingCountActivityStorageMock.Object, Mock.Of<ILogger>());

            // when
            var firstResult = await sut.Execute();
            await Task.Delay(10);
            var cachedResult = await sut.Execute(maxStalenessMs: 100);
            var recomputedResult = await sut.Execute(maxStalenessMs: 5);
            

            // then
            incrementingCountActivityStorageMock.Verify(asm => asm.CountActivityResults(), Times.Exactly(2));

            firstResult.Should().Be(count);
            cachedResult.Should().Be(count);
            recomputedResult.Should().Be(count);
        }

    }
}