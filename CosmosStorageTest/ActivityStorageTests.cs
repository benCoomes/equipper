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
            var cv1 = new Domain.CrossValidationResult()
            {
                AlgorithmName = "Excellent guessing",
                Total = 50,
                Correct = 26
            };
            var cv2 = new Domain.CrossValidationResult()
            {
                AlgorithmName = "Omniscient knowledge",
                Total = 232,
                Correct = 232
            };
            var classificationStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<Domain.CrossValidationResult>()
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
            afterAdd.Id.Should().Be(classificationStats.Id);
            afterAdd.CrossValidations.Count.Should().Be(2);
            afterAdd.CrossValidations.Should().Contain(cv => cv.Correct == cv1.Correct && cv.Total == cv1.Total && cv.AlgorithmName == cv1.AlgorithmName);
            afterAdd.CrossValidations.Should().Contain(cv => cv.Correct == cv2.Correct && cv.Total == cv2.Total && cv.AlgorithmName == cv2.AlgorithmName);
        }
    }
}
