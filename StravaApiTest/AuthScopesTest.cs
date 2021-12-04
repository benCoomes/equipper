using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Coomes.Equipper.StravaApi.Test
{
    [TestClass]
    public class AuthScopesTest
    {
        [TestMethod]
        public void AuthScopes_ParsesAuthString_ReadPublic()
        {
            ValidateSingleScope("read", s => s.ReadPublic);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_ReadAll()
        {
            ValidateSingleScope("read_all", s => s.ReadAll);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_ProfileReadAll()
        {
            ValidateSingleScope("profile:read_all", s => s.ProfileReadAll);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_ActivityRead()
        {
            ValidateSingleScope("activity:read", s => s.ActivityRead);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_ActivityReadAll()
        {
            ValidateSingleScope("activity:read_all", s => s.ActivityReadAll);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_ActivityWrite()
        {
            ValidateSingleScope("activity:write", s => s.ActivityWrite);
        }

        [TestMethod]
        public void AuthScopes_ParsesAuthString_Multiple() 
        {
            // given
            var scopesString = "activity:read,activity:write,read";
            
            // when
            var sut = StravaApi.Models.AuthScopes.Create(scopesString);
            
            // then
            sut.ActivityWrite.Should().BeTrue();
            sut.ActivityRead.Should().BeTrue();
            sut.ActivityWrite.Should().BeTrue();

            sut.ReadAll.Should().BeFalse();
            sut.ProfileReadAll.Should().BeFalse();
            sut.ProfileWrite.Should().BeFalse();
            sut.ActivityReadAll.Should().BeFalse();
        }

        [TestMethod]
        public void AuthScopes_ParsesEmptyAndNull()
        {
            var emptyScopes = StravaApi.Models.AuthScopes.Create("");
            var nullScopes = StravaApi.Models.AuthScopes.Create(null);

            emptyScopes.ReadPublic.Should().BeFalse();
            nullScopes.ReadPublic.Should().BeFalse();
        }

        [TestMethod]
        public void AuthScopes_ToDTO_ReturnsCorrectModel() 
        {
            var scopesString = "read,activity:write,read_all";
            
            // when 
            var sut = StravaApi.Models.AuthScopes.Create(scopesString);
            var domainModel = sut.ToDomainModel();
            
            // then
            domainModel.ReadPublic.Should().Be(sut.ReadPublic);
            domainModel.ReadAll.Should().Be(sut.ReadAll);
            domainModel.ActivityRead.Should().Be(sut.ActivityRead);
            domainModel.ActivityWrite.Should().Be(sut.ActivityWrite);
            domainModel.ActivityReadAll.Should().Be(sut.ActivityReadAll);
            domainModel.ProfileWrite.Should().Be(sut.ProfileWrite);
            domainModel.ProfileReadAll.Should().Be(sut.ProfileReadAll);
        }


        private void ValidateSingleScope(string scope, Func<StravaApi.Models.AuthScopes, bool> actualValue) 
        {
            var sut = StravaApi.Models.AuthScopes.Create(scope);
            actualValue(sut).Should().BeTrue();
        }
    }
}