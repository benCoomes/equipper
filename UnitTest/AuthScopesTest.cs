using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace Coomes.Equipper.UnitTest
{
    [TestClass]
    public class AuthScopesTest
    {
        [TestMethod]
        public void AuthScopes_CanSetGear_ReturnsExpectedValue() 
        {
            var allRequiredPrivs = new AuthScopes() 
            {
                ActivityWrite = true,
                ActivityRead = true,
                ReadPublic = true
            };

            var noActivityWrite = new AuthScopes() 
            {
                ActivityWrite = false,
                ActivityRead = true,
                ReadPublic = true
            };

            var noActivityRead = new AuthScopes() 
            {
                ActivityWrite = true,
                ActivityRead = false,
                ReadPublic = true
            };

            var noRead = new AuthScopes() 
            {
                ActivityWrite = false,
                ActivityRead = true,
                ReadPublic = false
            };

            allRequiredPrivs.CanSetGear().Should().BeTrue();
            noRead.CanSetGear().Should().BeFalse();
            noActivityRead.CanSetGear().Should().BeFalse();
            noActivityWrite.CanSetGear().Should().BeFalse();
        }
    }
}