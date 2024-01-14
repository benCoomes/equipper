using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class ResponseExtensionsTest
    {
        [DataTestMethod]
        [DataRow(HttpStatusCode.Unauthorized, typeof(UnauthorizedException))]
        [DataRow(HttpStatusCode.NotFound, typeof(NotFoundException))]
        public async Task LogAndThrow_ThrowsExpectedExceptionForResponseCode(HttpStatusCode code, Type expectedExceptionType)
        {
            var mockLogger = new Mock<ILogger>();
            var response = new HttpResponseMessage(code);
            response.Content = new StreamContent(new MemoryStream());

            try
            {
                await response.LogAndThrowIfNotSuccess(mockLogger.Object, "unit test");
                Assert.Fail("Expected an exception but none was thrown");
            }
            catch(Exception actualException)
            {
                actualException.GetType().Should().Be(expectedExceptionType);
            }
        }
    }
}