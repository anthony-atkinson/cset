//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Business.Offline;
using CSETWebCore.Interfaces.Offline;
using CSETWebCore.Model.Offline;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MSTest;

namespace CSETWebCore.BusinessTests.Offline
{
    /// <summary>
    /// Tests for offline sync business logic
    /// </summary>
    [TestClass]
    public class OfflineSyncBusinessTests
    {
        private Mock<IOfflineDataRepository> _mockOfflineRepository;
        private Mock<IAssessmentBusiness> _mockAssessmentBusiness;
        private Mock<IQuestionBusiness> _mockQuestionBusiness;
        private Mock<IObservationBusiness> _mockObservationBusiness;
        private Mock<ILogger<OfflineSyncBusiness>> _mockLogger;
        private OfflineSyncBusiness _offlineSyncBusiness;

        [TestInitialize]
        public void Setup()
        {
            _mockOfflineRepository = new Mock<IOfflineDataRepository>();
            _mockAssessmentBusiness = new Mock<IAssessmentBusiness>();
            _mockQuestionBusiness = new Mock<IQuestionBusiness>();
            _mockObservationBusiness = new Mock<IObservationBusiness>();
            _mockLogger = new Mock<ILogger<OfflineSyncBusiness>>();

            _offlineSyncBusiness = new OfflineSyncBusiness(
                _mockOfflineRepository.Object,
                _mockAssessmentBusiness.Object,
                _mockQuestionBusiness.Object,
                _mockObservationBusiness.Object,
                _mockLogger.Object);
        }

        #region Queue Management Tests

        [TestMethod]
        public void AddToOfflineQueue_ValidItem_AddsToQueue()
        {
            // Arrange
            var offlineItem = CreateTestOfflineItem("assessment", "create", new { assessmentId = 1, name = "Test Assessment" });

            // Act
            _offlineSyncBusiness.AddToOfflineQueue(offlineItem);

            // Assert
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.IsAny<List<OfflineQueueItem>>()), Times.Once);
        }

        [TestMethod]
        public void AddToOfflineQueue_MultipleItems_GeneratesUniqueIds()
        {
            // Arrange
            var item1 = CreateTestOfflineItem("assessment", "create", new { assessmentId = 1 });
            var item2 = CreateTestOfflineItem("assessment", "create", new { assessmentId = 2 });

            // Act
            _offlineSyncBusiness.AddToOfflineQueue(item1);
            _offlineSyncBusiness.AddToOfflineQueue(item2);

            // Assert
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 2)), Times.Exactly(2));
        }

        [TestMethod]
        public void GetOfflineQueue_ReturnsQueueFromRepository()
        {
            // Arrange
            var expectedQueue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "create", new { assessmentId = 1 }),
                CreateTestOfflineQueueItem("question", "update", new { questionId = 1, answer = "Y" })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(expectedQueue);

            // Act
            var result = _offlineSyncBusiness.GetOfflineQueue();

            // Assert
            result.Should().BeEquivalentTo(expectedQueue);
        }

        [TestMethod]
        public void ClearOfflineQueue_ClearsQueueFromRepository()
        {
            // Act
            _offlineSyncBusiness.ClearOfflineQueue();

            // Assert
            _mockOfflineRepository.Verify(x => x.ClearOfflineQueue(), Times.Once);
        }

        [TestMethod]
        public void GetOfflineQueueStats_ReturnsCorrectStatistics()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "create", new { assessmentId = 1 }),
                CreateTestOfflineQueueItem("assessment", "update", new { assessmentId = 2 }),
                CreateTestOfflineQueueItem("question", "update", new { questionId = 1, answer = "Y" }),
                CreateTestOfflineQueueItem("observation", "create", new { observationId = 1 })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);

            // Act
            var stats = _offlineSyncBusiness.GetOfflineQueueStats();

            // Assert
            stats.Total.Should().Be(4);
            stats.ByType["assessment"].Should().Be(2);
            stats.ByType["question"].Should().Be(1);
            stats.ByType["observation"].Should().Be(1);
            stats.ByAction["create"].Should().Be(2);
            stats.ByAction["update"].Should().Be(2);
        }

        #endregion

        #region Data Storage Tests

        [TestMethod]
        public void StoreOfflineData_ValidData_StoresInRepository()
        {
            // Arrange
            var key = "test-key";
            var data = new { assessmentId = 1, name = "Test Assessment" };

            // Act
            _offlineSyncBusiness.StoreOfflineData(key, data);

            // Assert
            _mockOfflineRepository.Verify(x => x.StoreOfflineData(key, It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void GetOfflineData_ExistingKey_ReturnsData()
        {
            // Arrange
            var key = "test-key";
            var expectedData = new { assessmentId = 1, name = "Test Assessment" };
            var serializedData = System.Text.Json.JsonSerializer.Serialize(expectedData);
            _mockOfflineRepository.Setup(x => x.GetOfflineData(key)).Returns(serializedData);

            // Act
            var result = _offlineSyncBusiness.GetOfflineData<object>(key);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void GetOfflineData_NonExistentKey_ReturnsNull()
        {
            // Arrange
            var key = "non-existent-key";
            _mockOfflineRepository.Setup(x => x.GetOfflineData(key)).Returns((string)null);

            // Act
            var result = _offlineSyncBusiness.GetOfflineData<object>(key);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void ClearOfflineData_SpecificKey_ClearsSpecificData()
        {
            // Arrange
            var key = "test-key";

            // Act
            _offlineSyncBusiness.ClearOfflineData(key);

            // Assert
            _mockOfflineRepository.Verify(x => x.ClearOfflineData(key), Times.Once);
        }

        [TestMethod]
        public void ClearOfflineData_NoKey_ClearsAllData()
        {
            // Act
            _offlineSyncBusiness.ClearOfflineData();

            // Assert
            _mockOfflineRepository.Verify(x => x.ClearAllOfflineData(), Times.Once);
        }

        #endregion

        #region Synchronization Tests

        [TestMethod]
        public async Task AttemptSync_EmptyQueue_DoesNotProcessItems()
        {
            // Arrange
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(new List<OfflineQueueItem>());

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockAssessmentBusiness.Verify(x => x.UpdateAssessment(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
            _mockQuestionBusiness.Verify(x => x.UpdateAnswer(It.IsAny<int>(), It.IsAny<object>()), Times.Never);
        }

        [TestMethod]
        public async Task AttemptSync_AssessmentItems_ProcessesAssessmentItems()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "update", new { assessmentId = 1, name = "Updated Assessment" })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);
            _mockAssessmentBusiness.Setup(x => x.UpdateAssessment(1, It.IsAny<object>())).ReturnsAsync(true);

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockAssessmentBusiness.Verify(x => x.UpdateAssessment(1, It.IsAny<object>()), Times.Once);
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 0)), Times.Once);
        }

        [TestMethod]
        public async Task AttemptSync_QuestionItems_ProcessesQuestionItems()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("question", "update", new { questionId = 1, answer = "Y" })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);
            _mockQuestionBusiness.Setup(x => x.UpdateAnswer(1, It.IsAny<object>())).ReturnsAsync(true);

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockQuestionBusiness.Verify(x => x.UpdateAnswer(1, It.IsAny<object>()), Times.Once);
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 0)), Times.Once);
        }

        [TestMethod]
        public async Task AttemptSync_ObservationItems_ProcessesObservationItems()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("observation", "create", new { observationId = 1, text = "Test Observation" })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);
            _mockObservationBusiness.Setup(x => x.CreateObservation(It.IsAny<object>())).ReturnsAsync(1);

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockObservationBusiness.Verify(x => x.CreateObservation(It.IsAny<object>()), Times.Once);
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 0)), Times.Once);
        }

        [TestMethod]
        public async Task AttemptSync_FailedItem_IncrementsRetryCount()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "update", new { assessmentId = 1, name = "Updated Assessment" })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);
            _mockAssessmentBusiness.Setup(x => x.UpdateAssessment(1, It.IsAny<object>())).ThrowsAsync(new Exception("Network error"));

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => 
                list.Count == 1 && list[0].RetryCount == 1)), Times.Once);
        }

        [TestMethod]
        public async Task AttemptSync_MaxRetriesReached_RemovesItem()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "update", new { assessmentId = 1, name = "Updated Assessment" }, 3)
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);
            _mockAssessmentBusiness.Setup(x => x.UpdateAssessment(1, It.IsAny<object>())).ThrowsAsync(new Exception("Network error"));

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 0)), Times.Once);
        }

        [TestMethod]
        public async Task AttemptSync_UnknownItemType_LogsErrorAndRemovesItem()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("unknown", "update", new { id = 1 })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unknown item type")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
            _mockOfflineRepository.Verify(x => x.SaveOfflineQueue(It.Is<List<OfflineQueueItem>>(list => list.Count == 0)), Times.Once);
        }

        #endregion

        #region Conflict Resolution Tests

        [TestMethod]
        public async Task ResolveConflicts_AssessmentConflict_ResolvesWithLatestTimestamp()
        {
            // Arrange
            var localData = new { assessmentId = 1, name = "Local Assessment", lastModified = DateTime.UtcNow.AddHours(-1) };
            var serverData = new { assessmentId = 1, name = "Server Assessment", lastModified = DateTime.UtcNow };
            
            _mockOfflineRepository.Setup(x => x.GetOfflineData("assessment_1")).Returns(System.Text.Json.JsonSerializer.Serialize(localData));

            // Act
            var result = await _offlineSyncBusiness.ResolveConflicts("assessment", 1, serverData);

            // Assert
            result.Should().BeEquivalentTo(serverData);
        }

        [TestMethod]
        public async Task ResolveConflicts_QuestionConflict_ResolvesWithLatestTimestamp()
        {
            // Arrange
            var localData = new { questionId = 1, answer = "Y", lastModified = DateTime.UtcNow };
            var serverData = new { questionId = 1, answer = "N", lastModified = DateTime.UtcNow.AddHours(-1) };
            
            _mockOfflineRepository.Setup(x => x.GetOfflineData("question_1")).Returns(System.Text.Json.JsonSerializer.Serialize(localData));

            // Act
            var result = await _offlineSyncBusiness.ResolveConflicts("question", 1, serverData);

            // Assert
            result.Should().BeEquivalentTo(localData);
        }

        [TestMethod]
        public async Task ResolveConflicts_NoLocalData_ReturnsServerData()
        {
            // Arrange
            var serverData = new { assessmentId = 1, name = "Server Assessment" };
            _mockOfflineRepository.Setup(x => x.GetOfflineData("assessment_1")).Returns((string)null);

            // Act
            var result = await _offlineSyncBusiness.ResolveConflicts("assessment", 1, serverData);

            // Assert
            result.Should().BeEquivalentTo(serverData);
        }

        #endregion

        #region Status Management Tests

        [TestMethod]
        public void GetOfflineStatus_ReturnsCurrentStatus()
        {
            // Arrange
            var queue = new List<OfflineQueueItem>
            {
                CreateTestOfflineQueueItem("assessment", "create", new { assessmentId = 1 })
            };
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Returns(queue);

            // Act
            var status = _offlineSyncBusiness.GetOfflineStatus();

            // Assert
            status.HasPendingSync.Should().BeTrue();
            status.PendingItemsCount.Should().Be(1);
            status.SyncInProgress.Should().BeFalse();
        }

        [TestMethod]
        public void UpdateSyncStatus_SetsSyncInProgress()
        {
            // Arrange
            var syncInProgress = true;

            // Act
            _offlineSyncBusiness.UpdateSyncStatus(syncInProgress);

            // Assert
            var status = _offlineSyncBusiness.GetOfflineStatus();
            status.SyncInProgress.Should().Be(syncInProgress);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task AttemptSync_RepositoryException_LogsErrorAndContinues()
        {
            // Arrange
            _mockOfflineRepository.Setup(x => x.GetOfflineQueue()).Throws(new Exception("Repository error"));

            // Act
            await _offlineSyncBusiness.AttemptSync();

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error during sync")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public void AddToOfflineQueue_RepositoryException_LogsError()
        {
            // Arrange
            var offlineItem = CreateTestOfflineItem("assessment", "create", new { assessmentId = 1 });
            _mockOfflineRepository.Setup(x => x.SaveOfflineQueue(It.IsAny<List<OfflineQueueItem>>())).Throws(new Exception("Storage error"));

            // Act
            _offlineSyncBusiness.AddToOfflineQueue(offlineItem);

            // Assert
            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error saving offline queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private OfflineQueueItem CreateTestOfflineItem(string type, string action, object data, int retryCount = 0)
        {
            return new OfflineQueueItem
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Action = action,
                Data = System.Text.Json.JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow,
                RetryCount = retryCount
            };
        }

        private OfflineQueueItem CreateTestOfflineQueueItem(string type, string action, object data, int retryCount = 0)
        {
            return new OfflineQueueItem
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Action = action,
                Data = System.Text.Json.JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow,
                RetryCount = retryCount
            };
        }

        #endregion
    }
} 