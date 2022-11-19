using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage.Test
{
    [TestClass]
    public class TokenStorageTests
    {
        private static Random _rand = new Random(); 
        private static Task<TokenStorage> _sutInitTask;

        [ClassInitialize]
        public static void TestClassSetup(TestContext _)
        {
            var sut = new TokenStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, TestConstants.TokenContainerName);
            Func<Task<TokenStorage>> sutInit = async () => {
                await sut.EnsureDeleted();
                await sut.GetTokens(_rand.NextInt64());
                return sut;
            };
            _sutInitTask = sutInit();
        }

        private static Task<TokenStorage> GetSut() 
        {
            return _sutInitTask;
        }

        [TestMethod]
        public async Task TokenStorage_AddOrUpdate_AddsNewTokens()
        {
            // given 
            var athleteID = _rand.NextInt64();
            var expectedTokens = new Domain.AthleteTokens()
            {
                UserID = Guid.NewGuid().ToString(),
                AccessToken = "accessToken",
                RefreshToken = "refreshToken",
                AthleteID = athleteID,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(4),
            };

            var sut = await GetSut();

            // when
            var beforeAdd = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(expectedTokens);
            var afterAdd = await sut.GetTokens(athleteID);

            // then
            beforeAdd.Should().BeNull();
            afterAdd.Should().NotBeNull();
            
            afterAdd.UserID.Should().Be(expectedTokens.UserID);
            afterAdd.AccessToken.Should().Be(expectedTokens.AccessToken);
            afterAdd.RefreshToken.Should().Be(expectedTokens.RefreshToken);
            afterAdd.AthleteID.Should().Be(expectedTokens.AthleteID);
            afterAdd.ExpiresAtUtc.Should().Be(expectedTokens.ExpiresAtUtc);
        }

        [TestMethod]
        public async Task TokenStorage_AddOrUpdate_UpdatesExistingTokens()
        {
            // given 
            var athleteID = _rand.NextInt64();
            var originalTokens = new Domain.AthleteTokens()
            {
                UserID = "originalUserID",
                AccessToken = "ogAccessToken",
                RefreshToken = "ogRefreshToken",
                AthleteID = athleteID,
                ExpiresAtUtc = DateTime.Parse("2021-02-03 13:14:15").ToUniversalTime()
            };
            var updatedTokens = new Domain.AthleteTokens()
            {
                UserID = "newUserID", // changing the UserID is possible but should _never_ be done. This must be protected against in domain code.
                AccessToken = "newAccessToken",
                RefreshToken = null, // null values still overwrite existing value
                AthleteID = athleteID,
                ExpiresAtUtc = DateTime.Parse("2021-02-04 14:15:16").ToUniversalTime()
            };

            var sut = await GetSut();

            // when
            var beforeOriginal = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(originalTokens);
            var afterOriginal = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(updatedTokens);
            var afterUpdate = await sut.GetTokens(athleteID);

            // then
            beforeOriginal.Should().BeNull();
            afterOriginal.UserID.Should().Be(originalTokens.UserID);
            afterOriginal.AccessToken.Should().Be(originalTokens.AccessToken);
            afterOriginal.RefreshToken.Should().Be(originalTokens.RefreshToken);
            afterUpdate.UserID.Should().Be(updatedTokens.UserID);
            afterUpdate.AccessToken.Should().Be(updatedTokens.AccessToken);
            afterUpdate.RefreshToken.Should().Be(updatedTokens.RefreshToken);
        }

        [TestMethod]
        public async Task TokenStorage_GetTokenForUser_GetsCorrectTokens() 
        {
            // given
            var storedTokens = new Domain.AthleteTokens() {
                AthleteID = _rand.NextInt64(),
                UserID = Guid.NewGuid().ToString(),
                AccessToken = "accessToken",
                RefreshToken = "refreshToken"
            };

            var sut = await GetSut();

            // when
            await sut.AddOrUpdateTokens(new Domain.AthleteTokens{
                AthleteID = _rand.NextInt64(),
                UserID = Guid.NewGuid().ToString(),
                AccessToken = "random seeded user"
            });
            var beforeAdd = await sut.GetTokenForUser(storedTokens.UserID);
            await sut.AddOrUpdateTokens(storedTokens);
            var afterAdd = await sut.GetTokenForUser(storedTokens.UserID);

            // then
            beforeAdd.Should().BeNull();
            afterAdd.UserID.Should().Be(storedTokens.UserID);
            afterAdd.AthleteID.Should().Be(storedTokens.AthleteID);
            afterAdd.AccessToken.Should().Be(storedTokens.AccessToken);
            afterAdd.RefreshToken.Should().Be(storedTokens.RefreshToken);
        }
    
        [TestMethod]
        public async Task TokenStorage_Delete_RemovesTokens()
        {
            // given 
            var athleteID = _rand.NextInt64();
            var existingTokens = new Domain.AthleteTokens()
            {
                UserID = Guid.NewGuid().ToString(),
                AccessToken = "accessToken",
                RefreshToken = "refreshToken",
                AthleteID = athleteID,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(4)
            };

            var sut = await GetSut();

            // when
            await sut.AddOrUpdateTokens(existingTokens);
            var beforeDelete = await sut.GetTokens(athleteID);
            await sut.DeleteTokens(athleteID);
            var afterDelete = await sut.GetTokens(athleteID);

            // then
            beforeDelete.Should().NotBeNull();
            afterDelete.Should().BeNull();
        }

        [TestMethod]
        public async Task TokenStorage_Delete_ThrowsWhenTokensDoNotExist()
        {
            // given 
            var athleteID = _rand.NextInt64();

            var sut = await GetSut();

            // when
            var beforeDelete = await sut.GetTokens(athleteID);
            Func<Task> tryDelete = () =>  sut.DeleteTokens(athleteID);

            // then
            beforeDelete.Should().BeNull();
            await tryDelete.Should().ThrowAsync<Exception>();
        }
    }
}
