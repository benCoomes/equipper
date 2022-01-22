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
        [TestMethod]
        public async Task TokenStorage_AddOrUpdate_AddsNewTokens()
        {
            // given 
            var athleteID = 1234;
            var expectedTokens = new Domain.AthleteTokens()
            {
                AccessToken = "accessToken",
                RefreshToken = "refreshToken",
                AthleteID = 1234,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(4)
            };

            var containerName = nameof(TokenStorage_AddOrUpdate_AddsNewTokens);
            var sut = new TokenStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeAdd = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(expectedTokens);
            var afterAdd = await sut.GetTokens(athleteID);

            // then
            beforeAdd.Should().BeNull();
            afterAdd.Should().NotBeNull();
            
            afterAdd.AccessToken.Should().Be(expectedTokens.AccessToken);
            afterAdd.RefreshToken.Should().Be(expectedTokens.RefreshToken);
            afterAdd.AthleteID.Should().Be(expectedTokens.AthleteID);
            afterAdd.ExpiresAtUtc.Should().Be(expectedTokens.ExpiresAtUtc);
        }

        [TestMethod]
        public async Task TokenStorage_AddOrUpdate_UpdatesExistingTokens()
        {
            // given 
            var athleteID = 1234;
            var originalTokens = new Domain.AthleteTokens()
            {
                AccessToken = "ogAccessToken",
                RefreshToken = "ogRefreshToken",
                AthleteID = 1234,
                ExpiresAtUtc = DateTime.Parse("2021-02-03 13:14:15").ToUniversalTime()
            };
            var udpatedTokens = new Domain.AthleteTokens()
            {
                AccessToken = "newAccessToken",
                RefreshToken = null, // null values still overwrite existing value
                AthleteID = 1234,
                ExpiresAtUtc = DateTime.Parse("2021-02-04 14:15:16").ToUniversalTime()
            };

            var containerName = nameof(TokenStorage_AddOrUpdate_UpdatesExistingTokens);
            var sut = new TokenStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeOriginal = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(originalTokens);
            var afterOriginal = await sut.GetTokens(athleteID);
            await sut.AddOrUpdateTokens(udpatedTokens);
            var afterUpdate = await sut.GetTokens(athleteID);

            // then
            beforeOriginal.Should().BeNull();
            afterOriginal.AccessToken.Should().Be(originalTokens.AccessToken);
            afterOriginal.RefreshToken.Should().Be(originalTokens.RefreshToken);
            afterUpdate.AccessToken.Should().Be(udpatedTokens.AccessToken);
            afterUpdate.RefreshToken.Should().Be(udpatedTokens.RefreshToken);
        }
    
        [TestMethod]
        public async Task TokenStorage_Delete_RemovesTokens()
        {
            // given 
            var athleteID = 1234;
            var existingTokens = new Domain.AthleteTokens()
            {
                AccessToken = "accessToken",
                RefreshToken = "refreshToken",
                AthleteID = athleteID,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(4)
            };

            var containerName = nameof(TokenStorage_Delete_RemovesTokens);
            var sut = new TokenStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

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
            var athleteID = 1234;

            var containerName = nameof(TokenStorage_Delete_ThrowsWhenTokensDoNotExist);
            var sut = new TokenStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeDelete = await sut.GetTokens(athleteID);
            Func<Task> tryDelete = () =>  sut.DeleteTokens(athleteID);

            // then
            beforeDelete.Should().BeNull();
            await tryDelete.Should().ThrowAsync<Exception>();
        }
    }
}
