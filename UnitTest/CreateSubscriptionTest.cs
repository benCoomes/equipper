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
            Func<Task> tryCreate = async () => await sut.Execute(badUrl, "sometoken");

            // then
            await tryCreate.Should().ThrowAsync<ArgumentException>();
        }

        public async Task CreateSubscription_ThrowsOnEmptyVerificationToken() 
        {
            // given
            var sut = new CreateSubscription(null);

            // when
            Func<Task> tryCreate = async () => await sut.Execute("https://localhost:8000/endpoint", string.Empty);

            // then
            await tryCreate.Should().ThrowAsync<ArgumentException>();
        }
    }
}