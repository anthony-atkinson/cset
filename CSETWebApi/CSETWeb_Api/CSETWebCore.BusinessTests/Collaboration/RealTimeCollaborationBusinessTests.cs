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
using CSETWebCore.Business.Collaboration;
using CSETWebCore.Interfaces.Collaboration;
using CSETWebCore.Model.Collaboration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MSTest;

namespace CSETWebCore.BusinessTests.Collaboration
{
    /// <summary>
    /// Tests for real-time collaboration business logic
    /// </summary>
    [TestClass]
    public class RealTimeCollaborationBusinessTests
    {
        private Mock<ICollaborationRepository> _mockRepository;
        private Mock<IAssessmentBusiness> _mockAssessmentBusiness;
        private Mock<IUserBusiness> _mockUserBusiness;
        private Mock<ILogger<RealTimeCollaborationBusiness>> _mockLogger;
        private RealTimeCollaborationBusiness _collaborationBusiness;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ICollaborationRepository>();
            _mockAssessmentBusiness = new Mock<IAssessmentBusiness>();
            _mockUserBusiness = new Mock<IUserBusiness>();
            _mockLogger = new Mock<ILogger<RealTimeCollaborationBusiness>>();

            _collaborationBusiness = new RealTimeCollaborationBusiness(
                _mockRepository.Object,
                _mockAssessmentBusiness.Object,
                _mockUserBusiness.Object,
                _mockLogger.Object);
        }

        #region User Presence Tests

        [TestMethod]
        public async Task JoinCollaboration_ValidUser_AddsUserToSession()
        {
            // Arrange
            var userId = 1;
            var assessmentId = 1;
            var sessionId = "session-123";
            var userInfo = CreateTestUserInfo(userId);

            _mockUserBusiness.Setup(x => x.GetUserById(userId)).Returns(userInfo);
            _mockRepository.Setup(x => x.AddUserToSession(sessionId, It.IsAny<UserPresence>())).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.JoinCollaboration(userId, assessmentId, sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SessionId.Should().Be(sessionId);
            _mockRepository.Verify(x => x.AddUserToSession(sessionId, It.Is<UserPresence>(u => 
                u.UserId == userId && u.AssessmentId == assessmentId)), Times.Once);
        }

        [TestMethod]
        public async Task JoinCollaboration_InvalidUser_ReturnsError()
        {
            // Arrange
            var invalidUserId = 999;
            var assessmentId = 1;
            var sessionId = "session-123";

            _mockUserBusiness.Setup(x => x.GetUserById(invalidUserId)).Returns((UserInfo)null);

            // Act
            var result = await _collaborationBusiness.JoinCollaboration(invalidUserId, assessmentId, sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("User not found");
        }

        [TestMethod]
        public async Task LeaveCollaboration_ValidUser_RemovesUserFromSession()
        {
            // Arrange
            var userId = 1;
            var sessionId = "session-123";

            _mockRepository.Setup(x => x.RemoveUserFromSession(sessionId, userId)).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.LeaveCollaboration(userId, sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.RemoveUserFromSession(sessionId, userId), Times.Once);
        }

        [TestMethod]
        public async Task GetActiveUsers_ValidSession_ReturnsUserList()
        {
            // Arrange
            var sessionId = "session-123";
            var expectedUsers = new List<UserPresence>
            {
                CreateTestUserPresence(1, "User 1"),
                CreateTestUserPresence(2, "User 2")
            };

            _mockRepository.Setup(x => x.GetActiveUsers(sessionId)).ReturnsAsync(expectedUsers);

            // Act
            var result = await _collaborationBusiness.GetActiveUsers(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].UserId.Should().Be(1);
            result[1].UserId.Should().Be(2);
        }

        [TestMethod]
        public async Task UpdateUserActivity_ValidUser_UpdatesLastActivity()
        {
            // Arrange
            var userId = 1;
            var sessionId = "session-123";
            var activity = "viewing_questions";

            _mockRepository.Setup(x => x.UpdateUserActivity(sessionId, userId, activity)).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.UpdateUserActivity(userId, sessionId, activity);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.UpdateUserActivity(sessionId, userId, activity), Times.Once);
        }

        #endregion

        #region Collaboration Events Tests

        [TestMethod]
        public async Task SendCollaborationEvent_ValidEvent_SendsEvent()
        {
            // Arrange
            var eventData = CreateTestCollaborationEvent("question_answered", new { questionId = 1, answer = "Y" });
            var sessionId = "session-123";

            _mockRepository.Setup(x => x.SaveCollaborationEvent(sessionId, It.IsAny<CollaborationEvent>())).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.SendCollaborationEvent(sessionId, eventData);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.SaveCollaborationEvent(sessionId, It.Is<CollaborationEvent>(e => 
                e.EventType == "question_answered")), Times.Once);
        }

        [TestMethod]
        public async Task GetCollaborationEvents_ValidSession_ReturnsEvents()
        {
            // Arrange
            var sessionId = "session-123";
            var expectedEvents = new List<CollaborationEvent>
            {
                CreateTestCollaborationEvent("question_answered", new { questionId = 1 }),
                CreateTestCollaborationEvent("assessment_updated", new { assessmentId = 1 })
            };

            _mockRepository.Setup(x => x.GetCollaborationEvents(sessionId, It.IsAny<DateTime>())).ReturnsAsync(expectedEvents);

            // Act
            var result = await _collaborationBusiness.GetCollaborationEvents(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].EventType.Should().Be("question_answered");
            result[1].EventType.Should().Be("assessment_updated");
        }

        [TestMethod]
        public async Task GetCollaborationEvents_WithTimeFilter_ReturnsFilteredEvents()
        {
            // Arrange
            var sessionId = "session-123";
            var sinceTime = DateTime.UtcNow.AddHours(-1);
            var expectedEvents = new List<CollaborationEvent>
            {
                CreateTestCollaborationEvent("question_answered", new { questionId = 1 })
            };

            _mockRepository.Setup(x => x.GetCollaborationEvents(sessionId, sinceTime)).ReturnsAsync(expectedEvents);

            // Act
            var result = await _collaborationBusiness.GetCollaborationEvents(sessionId, sinceTime);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            _mockRepository.Verify(x => x.GetCollaborationEvents(sessionId, sinceTime), Times.Once);
        }

        #endregion

        #region Conflict Resolution Tests

        [TestMethod]
        public async Task ResolveEditConflict_ValidConflict_ResolvesWithLatestTimestamp()
        {
            // Arrange
            var conflict = new EditConflict
            {
                QuestionId = 1,
                LocalEdit = new QuestionEdit { Answer = "Y", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
                RemoteEdit = new QuestionEdit { Answer = "N", Timestamp = DateTime.UtcNow }
            };

            // Act
            var result = await _collaborationBusiness.ResolveEditConflict(conflict);

            // Assert
            result.Should().NotBeNull();
            result.ResolvedAnswer.Should().Be("N"); // Remote edit is newer
            result.ResolutionMethod.Should().Be(ConflictResolutionMethod.LatestTimestamp);
        }

        [TestMethod]
        public async Task ResolveEditConflict_SameTimestamp_UsesLocalEdit()
        {
            // Arrange
            var sameTime = DateTime.UtcNow;
            var conflict = new EditConflict
            {
                QuestionId = 1,
                LocalEdit = new QuestionEdit { Answer = "Y", Timestamp = sameTime },
                RemoteEdit = new QuestionEdit { Answer = "N", Timestamp = sameTime }
            };

            // Act
            var result = await _collaborationBusiness.ResolveEditConflict(conflict);

            // Assert
            result.Should().NotBeNull();
            result.ResolvedAnswer.Should().Be("Y"); // Local edit wins on tie
            result.ResolutionMethod.Should().Be(ConflictResolutionMethod.LocalPreference);
        }

        [TestMethod]
        public async Task ResolveEditConflict_ManualResolution_UsesManualChoice()
        {
            // Arrange
            var conflict = new EditConflict
            {
                QuestionId = 1,
                LocalEdit = new QuestionEdit { Answer = "Y", Timestamp = DateTime.UtcNow.AddMinutes(-5) },
                RemoteEdit = new QuestionEdit { Answer = "N", Timestamp = DateTime.UtcNow },
                ManualResolution = "Y"
            };

            // Act
            var result = await _collaborationBusiness.ResolveEditConflict(conflict);

            // Assert
            result.Should().NotBeNull();
            result.ResolvedAnswer.Should().Be("Y");
            result.ResolutionMethod.Should().Be(ConflictResolutionMethod.Manual);
        }

        #endregion

        #region Session Management Tests

        [TestMethod]
        public async Task CreateCollaborationSession_ValidAssessment_CreatesSession()
        {
            // Arrange
            var assessmentId = 1;
            var sessionName = "Test Session";
            var expectedSessionId = "session-123";

            _mockRepository.Setup(x => x.CreateSession(It.IsAny<CollaborationSession>())).ReturnsAsync(expectedSessionId);

            // Act
            var result = await _collaborationBusiness.CreateCollaborationSession(assessmentId, sessionName);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.SessionId.Should().Be(expectedSessionId);
            _mockRepository.Verify(x => x.CreateSession(It.Is<CollaborationSession>(s => 
                s.AssessmentId == assessmentId && s.SessionName == sessionName)), Times.Once);
        }

        [TestMethod]
        public async Task GetCollaborationSession_ValidSessionId_ReturnsSession()
        {
            // Arrange
            var sessionId = "session-123";
            var expectedSession = CreateTestCollaborationSession(sessionId, 1);

            _mockRepository.Setup(x => x.GetSession(sessionId)).ReturnsAsync(expectedSession);

            // Act
            var result = await _collaborationBusiness.GetCollaborationSession(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.SessionId.Should().Be(sessionId);
            result.AssessmentId.Should().Be(1);
        }

        [TestMethod]
        public async Task EndCollaborationSession_ValidSession_EndsSession()
        {
            // Arrange
            var sessionId = "session-123";

            _mockRepository.Setup(x => x.EndSession(sessionId)).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.EndCollaborationSession(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.EndSession(sessionId), Times.Once);
        }

        #endregion

        #region Notification Tests

        [TestMethod]
        public async Task SendUserNotification_ValidNotification_SendsNotification()
        {
            // Arrange
            var userId = 1;
            var notification = CreateTestNotification("User joined session", "info");

            _mockRepository.Setup(x => x.SaveUserNotification(userId, It.IsAny<UserNotification>())).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.SendUserNotification(userId, notification);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.SaveUserNotification(userId, It.Is<UserNotification>(n => 
                n.Message == "User joined session")), Times.Once);
        }

        [TestMethod]
        public async Task GetUserNotifications_ValidUser_ReturnsNotifications()
        {
            // Arrange
            var userId = 1;
            var expectedNotifications = new List<UserNotification>
            {
                CreateTestNotification("User joined session", "info"),
                CreateTestNotification("Question answered", "success")
            };

            _mockRepository.Setup(x => x.GetUserNotifications(userId)).ReturnsAsync(expectedNotifications);

            // Act
            var result = await _collaborationBusiness.GetUserNotifications(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Message.Should().Be("User joined session");
            result[1].Message.Should().Be("Question answered");
        }

        [TestMethod]
        public async Task MarkNotificationAsRead_ValidNotification_MarksAsRead()
        {
            // Arrange
            var notificationId = "notification-123";

            _mockRepository.Setup(x => x.MarkNotificationAsRead(notificationId)).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.MarkNotificationAsRead(notificationId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.MarkNotificationAsRead(notificationId), Times.Once);
        }

        #endregion

        #region Audit Trail Tests

        [TestMethod]
        public async Task LogCollaborationActivity_ValidActivity_LogsActivity()
        {
            // Arrange
            var activity = CreateTestCollaborationActivity("question_answered", 1, 1);

            _mockRepository.Setup(x => x.LogCollaborationActivity(It.IsAny<CollaborationActivity>())).ReturnsAsync(true);

            // Act
            var result = await _collaborationBusiness.LogCollaborationActivity(activity);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            _mockRepository.Verify(x => x.LogCollaborationActivity(It.Is<CollaborationActivity>(a => 
                a.ActivityType == "question_answered")), Times.Once);
        }

        [TestMethod]
        public async Task GetCollaborationAuditTrail_ValidSession_ReturnsAuditTrail()
        {
            // Arrange
            var sessionId = "session-123";
            var expectedActivities = new List<CollaborationActivity>
            {
                CreateTestCollaborationActivity("user_joined", 1, 1),
                CreateTestCollaborationActivity("question_answered", 1, 1)
            };

            _mockRepository.Setup(x => x.GetCollaborationAuditTrail(sessionId)).ReturnsAsync(expectedActivities);

            // Act
            var result = await _collaborationBusiness.GetCollaborationAuditTrail(sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].ActivityType.Should().Be("user_joined");
            result[1].ActivityType.Should().Be("question_answered");
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task JoinCollaboration_RepositoryException_LogsErrorAndReturnsFailure()
        {
            // Arrange
            var userId = 1;
            var assessmentId = 1;
            var sessionId = "session-123";
            var userInfo = CreateTestUserInfo(userId);

            _mockUserBusiness.Setup(x => x.GetUserById(userId)).Returns(userInfo);
            _mockRepository.Setup(x => x.AddUserToSession(sessionId, It.IsAny<UserPresence>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _collaborationBusiness.JoinCollaboration(userId, assessmentId, sessionId);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Error joining collaboration");

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error joining collaboration")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task SendCollaborationEvent_RepositoryException_LogsErrorAndReturnsFailure()
        {
            // Arrange
            var eventData = CreateTestCollaborationEvent("question_answered", new { questionId = 1 });
            var sessionId = "session-123";

            _mockRepository.Setup(x => x.SaveCollaborationEvent(sessionId, It.IsAny<CollaborationEvent>())).ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _collaborationBusiness.SendCollaborationEvent(sessionId, eventData);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Contain("Error sending collaboration event");

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error sending collaboration event")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private UserInfo CreateTestUserInfo(int userId)
        {
            return new UserInfo
            {
                UserId = userId,
                UserName = $"User {userId}",
                Email = $"user{userId}@test.com",
                IsActive = true
            };
        }

        private UserPresence CreateTestUserPresence(int userId, string userName)
        {
            return new UserPresence
            {
                UserId = userId,
                UserName = userName,
                AssessmentId = 1,
                SessionId = "session-123",
                LastActivity = DateTime.UtcNow,
                CurrentActivity = "viewing_questions"
            };
        }

        private CollaborationEvent CreateTestCollaborationEvent(string eventType, object data)
        {
            return new CollaborationEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = eventType,
                EventData = System.Text.Json.JsonSerializer.Serialize(data),
                Timestamp = DateTime.UtcNow,
                UserId = 1
            };
        }

        private CollaborationSession CreateTestCollaborationSession(string sessionId, int assessmentId)
        {
            return new CollaborationSession
            {
                SessionId = sessionId,
                AssessmentId = assessmentId,
                SessionName = "Test Session",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        private UserNotification CreateTestNotification(string message, string type)
        {
            return new UserNotification
            {
                NotificationId = Guid.NewGuid().ToString(),
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };
        }

        private CollaborationActivity CreateTestCollaborationActivity(string activityType, int userId, int assessmentId)
        {
            return new CollaborationActivity
            {
                ActivityId = Guid.NewGuid().ToString(),
                ActivityType = activityType,
                UserId = userId,
                AssessmentId = assessmentId,
                SessionId = "session-123",
                Timestamp = DateTime.UtcNow,
                ActivityData = "{}"
            };
        }

        #endregion
    }
} 