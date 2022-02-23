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
                AthleteId = 1,
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
            var sut = new ActivityStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeAdd = await sut.GetClassificationStats(classificationStats.Id, activity.AthleteId);
            await sut.StoreActivityResults(activity, classificationStats);
            var afterAdd = await sut.GetClassificationStats(classificationStats.Id, activity.AthleteId);

            // then
            beforeAdd.Should().BeNull();
            afterAdd.Should().NotBeNull();
            afterAdd.Id.Should().Be(classificationStats.Id);
            afterAdd.CrossValidations.Count.Should().Be(2);
            afterAdd.CrossValidations.Should().Contain(cv => cv.Correct == cv1.Correct && cv.Total == cv1.Total && cv.AlgorithmName == cv1.AlgorithmName);
            afterAdd.CrossValidations.Should().Contain(cv => cv.Correct == cv2.Correct && cv.Total == cv2.Total && cv.AlgorithmName == cv2.AlgorithmName);
        }

        [TestMethod]
        public async Task ActivityStorage_ChecksIfActivityHasResults()
        {
            // given 
            var activity = new Domain.Activity()
            {
                Id = 12345,
                AthleteId = 1,
                GearId = "b100"
            };
            var classificationStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<Domain.CrossValidationResult>() 
                {  
                    new Domain.CrossValidationResult()
                    {
                        AlgorithmName = "Excellent guessing",
                        Total = 50,
                        Correct = 26
                    }
                }
            };

            var containerName = nameof(ActivityStorage_ChecksIfActivityHasResults);
            var sut = new ActivityStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var beforeAdd = await sut.ContainsResults(activity.AthleteId, activity.Id);
            await sut.StoreActivityResults(activity, classificationStats);
            var afterAdd = await sut.ContainsResults(activity.AthleteId, activity.Id);

            // then
            beforeAdd.Should().BeFalse();
            afterAdd.Should().BeTrue();
        }
    
        [TestMethod]
        public async Task ActivityStorage_DoesNothingWhenActivityAlreadyRecorded()
        {
            // given
            var originalActivity = new Domain.Activity()
            {
                Id = 12345,
                AthleteId = 1,
                GearId = "b100"
            };
            var updatedActivity = new Domain.Activity()
            {
                Id = originalActivity.Id,
                AthleteId = originalActivity.AthleteId,
                GearId = "somethingDifferent"
            };
            var originalClassStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<Domain.CrossValidationResult>()
                {
                    new Domain.CrossValidationResult()
                    {
                        AlgorithmName = "Excellent guessing",
                        Total = 50,
                        Correct = 26
                    }
                }
            };
            var updatedClassStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<Domain.CrossValidationResult>()
                {
                    new Domain.CrossValidationResult()
                    {
                        AlgorithmName = "Most frequent",
                        Total = 50,
                        Correct = 34
                    }
                }
            };

            var containerName = nameof(ActivityStorage_DoesNothingWhenActivityAlreadyRecorded);
            var sut = new ActivityStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            await sut.StoreActivityResults(originalActivity, originalClassStats);
            var originalAfterFirstCall = await sut.GetClassificationStats(originalClassStats.Id, originalActivity.AthleteId);
            var updatedAfterFirstCall = await sut.GetClassificationStats(updatedClassStats.Id, updatedActivity.AthleteId);

            await sut.StoreActivityResults(updatedActivity, updatedClassStats);
            var originalAfterSecondCall = await sut.GetClassificationStats(originalClassStats.Id, originalActivity.AthleteId);
            var updatedAfterSecondCall = await sut.GetClassificationStats(updatedClassStats.Id, updatedActivity.AthleteId);

            // then
            originalAfterFirstCall.Id.Should().Be(originalClassStats.Id.ToString());
            updatedAfterFirstCall.Should().BeNull();

            originalAfterSecondCall.Id.Should().Be(originalClassStats.Id.ToString());
            updatedAfterSecondCall.Should().BeNull();
        }
    
        [TestMethod]
        public async Task ActivityStorage_CountsActivityResults()
        {
            // given 
            var activity = new Domain.Activity()
            {
                Id = 12345,
                AthleteId = 1,
                GearId = "b100"
            };
            var classificationStats = new Domain.ClassificationStats()
            {
                Id = Guid.NewGuid(),
                CrossValidations = new List<Domain.CrossValidationResult>()
                {
                    new Domain.CrossValidationResult()
                    {
                        AlgorithmName = "Excellent guessing",
                        Total = 50,
                        Correct = 26
                    }
                }
            };

            var containerName = nameof(ActivityStorage_RecordsActivityResults);
            var sut = new ActivityStorage(TestConstants.EmulatorConnectionString, TestConstants.DatabaseName, containerName);
            await sut.EnsureDeleted();

            // when
            var countBeforeAdd = await sut.CountActivityResults();
            await sut.StoreActivityResults(activity, classificationStats);
            var countAfterAdd = await sut.CountActivityResults();

            // then
            countBeforeAdd.Should().Be(0);
            countAfterAdd.Should().Be(1);
        }
    }
}
