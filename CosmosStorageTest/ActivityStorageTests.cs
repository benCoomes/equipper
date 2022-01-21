using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain = Coomes.Equipper;

namespace Coomes.Equipper.CosmosStorage.Test
{
    [TestClass]
    public class ActivityStorageTests
    {
        [TestMethod]
        public async Task ActivityStorage_RecordsActivityResults()
        {
            // given 
            var activity = new Domain.Activity()
            {
                Id = 12345,
                GearId = "b100"
            };
            var cv1 = new CrossValidationResult()
            {
                AlgorithmName = "Excellent guessing",
                Total = 50,
                Correct = 26
            };
            var cv2 = new CrossValidationResult()
            {
                AlgorithmName = "Omniscient knowledge",
                Total = 232,
                Correct = 232
            };
            var classificationStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<CrossValidationResult>()
                {
                    cv1, 
                    cv2
                }
            };

            var containerName = nameof(ActivityStorage_RecordsActivityResults);
            var sut = new ActivityStorageForTesting(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeAdd = await sut.GetClassificationStats(classificationStats.Id);
            await sut.StoreActivityResults(activity, classificationStats);
            var afterAdd = await sut.GetClassificationStats(classificationStats.Id);

            // then
            beforeAdd.Should().BeNull();
            afterAdd.Should().NotBeNull();
            
            // afterAdd.AccessToken.Should().Be(expectedTokens.AccessToken);
            // afterAdd.RefreshToken.Should().Be(expectedTokens.RefreshToken);
            // afterAdd.AthleteID.Should().Be(expectedTokens.AthleteID);
            // afterAdd.ExpiresAtUtc.Should().Be(expectedTokens.ExpiresAtUtc);
        }
    }
}
