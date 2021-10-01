using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Threading.Tasks;
using Coomes.Equipper.Operations;
using System;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class CreateSubscriptionTest
    {
        [DataTestMethod]
        [DataRow("")]
        [DataRow(null)]
        [DataRow("somesite.com")]
        [DataRow("https://somesite.com")]
        [DataRow("foobar")]
        public async Task CreateSubscription_ThrowsOnInvalidUrl(string badUrl) 
        {
            // given
            var sut = new CreateSubscription(null);

            // when
            Func<Task> tryCreate = async () => await sut.Execute(badUrl);

            // then
            await tryCreate.Should().ThrowAsync<ArgumentException>();
        }
    }
}