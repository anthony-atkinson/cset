//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CSETWebCore.BusinessTests.Infrastructure;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Business.Assessment;
using Microsoft.EntityFrameworkCore;
using CSETWebCore.Interfaces.Helpers;
using System.Linq;

namespace CSETWebCore.BusinessTests
{
    /// <summary>
    /// Unit tests for assessment business logic
    /// </summary>
    [TestClass]
    public class AssessmentBusinessTests : BaseBusinessTest
    {
        private AssessmentBusiness _assessmentBusiness = null!;
        private Mock<ITokenManager> _mockTokenManager = null!;
        private Mock<IAssessmentUtil> _mockAssessmentUtil = null!;

        protected override void SetupTest()
        {
            // Setup specific mocks for assessment business
            _mockTokenManager = new Mock<ITokenManager>();
            _mockAssessmentUtil = new Mock<IAssessmentUtil>();

            // Create the business object with mocked dependencies
            _assessmentBusiness = new AssessmentBusiness(
                MockDbContext.Object,
                _mockTokenManager.Object,
                _mockAssessmentUtil.Object);
        }

        [TestMethod]
        public void CreateAssessment_ValidData_ReturnsAssessmentId()
        {
            // Arrange
            var assessmentName = "Test Assessment";
            var userId = 123;
            var expectedAssessmentId = 456;

            var mockDbSet = CreateMockDbSet(new List<ASSESSMENTS>());
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            MockDbContext.Setup(x => x.SaveChanges()).Returns(1);

            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Configure the mock to return the expected ID when adding an assessment
            mockDbSet.Setup(x => x.Add(It.IsAny<ASSESSMENTS>()))
                .Callback<ASSESSMENTS>(a => a.Assessment_Id = expectedAssessmentId);

            // Act
            var result = _assessmentBusiness.CreateAssessment(assessmentName);

            // Assert
            result.Should().Be(expectedAssessmentId);
            mockDbSet.Verify(x => x.Add(It.Is<ASSESSMENTS>(a => 
                a.Assessment_Name == assessmentName && 
                a.CreatedDate != default)), Times.Once);
            MockDbContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void CreateAssessment_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var emptyName = "";

            // Act & Assert
            AssertThrows<ArgumentException>(() => 
                _assessmentBusiness.CreateAssessment(emptyName), 
                "Assessment name cannot be empty");
        }

        [TestMethod]
        public void CreateAssessment_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            string? nullName = null;

            // Act & Assert
            AssertThrows<ArgumentNullException>(() => 
                _assessmentBusiness.CreateAssessment(nullName!));
        }

        [TestMethod]
        public void GetAssessment_ValidId_ReturnsAssessment()
        {
            // Arrange
            var assessmentId = 123;
            var userId = 456;
            var expectedAssessment = CreateTestAssessment("Test Assessment");
            expectedAssessment.Assessment_Id = assessmentId;

            var assessments = new List<ASSESSMENTS> { expectedAssessment };
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Setup assessment access validation
            _mockAssessmentUtil.Setup(x => x.UserHasAssessmentAccess(userId, assessmentId))
                .Returns(true);

            // Act
            var result = _assessmentBusiness.GetAssessment(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Assessment_Id.Should().Be(assessmentId);
            result.Assessment_Name.Should().Be(expectedAssessment.Assessment_Name);
        }

        [TestMethod]
        public void GetAssessment_InvalidId_ReturnsNull()
        {
            // Arrange
            var invalidAssessmentId = 999;
            var userId = 456;

            var assessments = new List<ASSESSMENTS>();
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Act
            var result = _assessmentBusiness.GetAssessment(invalidAssessmentId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetAssessment_UnauthorizedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var assessmentId = 123;
            var userId = 456;
            var expectedAssessment = CreateTestAssessment("Test Assessment");
            expectedAssessment.Assessment_Id = assessmentId;

            var assessments = new List<ASSESSMENTS> { expectedAssessment };
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Setup assessment access validation to deny access
            _mockAssessmentUtil.Setup(x => x.UserHasAssessmentAccess(userId, assessmentId))
                .Returns(false);

            // Act & Assert
            AssertThrows<UnauthorizedAccessException>(() => 
                _assessmentBusiness.GetAssessment(assessmentId),
                "User does not have access to this assessment");
        }

        [TestMethod]
        public void UpdateAssessment_ValidData_UpdatesSuccessfully()
        {
            // Arrange
            var assessmentId = 123;
            var userId = 456;
            var originalAssessment = CreateTestAssessment("Original Name");
            originalAssessment.Assessment_Id = assessmentId;

            var updatedAssessment = CreateTestAssessment("Updated Name");
            updatedAssessment.Assessment_Id = assessmentId;

            var assessments = new List<ASSESSMENTS> { originalAssessment };
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            MockDbContext.Setup(x => x.SaveChanges()).Returns(1);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            _mockAssessmentUtil.Setup(x => x.UserHasAssessmentAccess(userId, assessmentId))
                .Returns(true);

            // Act
            var result = _assessmentBusiness.UpdateAssessment(updatedAssessment);

            // Assert
            result.Should().BeTrue();
            originalAssessment.Assessment_Name.Should().Be(updatedAssessment.Assessment_Name);
            originalAssessment.LastModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            MockDbContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void DeleteAssessment_ValidId_DeletesSuccessfully()
        {
            // Arrange
            var assessmentId = 123;
            var userId = 456;
            var assessment = CreateTestAssessment("Test Assessment");
            assessment.Assessment_Id = assessmentId;

            var assessments = new List<ASSESSMENTS> { assessment };
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            MockDbContext.Setup(x => x.SaveChanges()).Returns(1);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            _mockAssessmentUtil.Setup(x => x.UserHasAssessmentAccess(userId, assessmentId))
                .Returns(true);

            // Act
            var result = _assessmentBusiness.DeleteAssessment(assessmentId);

            // Assert
            result.Should().BeTrue();
            mockDbSet.Verify(x => x.Remove(It.Is<ASSESSMENTS>(a => a.Assessment_Id == assessmentId)), Times.Once);
            MockDbContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void GetUserAssessments_ValidUser_ReturnsAssessmentList()
        {
            // Arrange
            var userId = 456;
            var assessment1 = CreateTestAssessment("Assessment 1");
            var assessment2 = CreateTestAssessment("Assessment 2");
            
            var assessments = new List<ASSESSMENTS> { assessment1, assessment2 };
            var mockDbSet = CreateMockDbSet(assessments);
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Mock the user assessment access
            _mockAssessmentUtil.Setup(x => x.GetUserAssessmentIds(userId))
                .Returns(new List<int> { assessment1.Assessment_Id, assessment2.Assessment_Id });

            // Act
            var result = _assessmentBusiness.GetUserAssessments();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(a => a.Assessment_Name == "Assessment 1");
            result.Should().Contain(a => a.Assessment_Name == "Assessment 2");
        }

        [TestMethod]
        public void ValidateAssessmentData_ValidAssessment_ReturnsTrue()
        {
            // Arrange
            var validAssessment = CreateTestAssessment("Valid Assessment");

            // Act
            var result = _assessmentBusiness.ValidateAssessmentData(validAssessment);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void ValidateAssessmentData_InvalidAssessment_ReturnsFalse()
        {
            // Arrange
            var invalidAssessment = new ASSESSMENTS
            {
                Assessment_Name = "", // Invalid empty name
                Assessment_Date = default // Invalid date
            };

            // Act
            var result = _assessmentBusiness.ValidateAssessmentData(invalidAssessment);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task CreateAssessmentAsync_ValidData_ReturnsAssessmentId()
        {
            // Arrange
            var assessmentName = "Async Test Assessment";
            var userId = 123;
            var expectedAssessmentId = 789;

            var mockDbSet = CreateMockDbSet(new List<ASSESSMENTS>());
            MockDbContext.Setup(x => x.ASSESSMENTS).Returns(mockDbSet.Object);
            MockDbContext.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);
            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            mockDbSet.Setup(x => x.AddAsync(It.IsAny<ASSESSMENTS>(), default))
                .Callback<ASSESSMENTS, CancellationToken>((a, ct) => a.Assessment_Id = expectedAssessmentId);

            // Act
            var result = await _assessmentBusiness.CreateAssessmentAsync(assessmentName);

            // Assert
            result.Should().Be(expectedAssessmentId);
            MockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod]
        public void GetAssessmentStatistics_ValidAssessmentId_ReturnsStatistics()
        {
            // Arrange
            var assessmentId = 123;
            var userId = 456;

            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);
            _mockAssessmentUtil.Setup(x => x.UserHasAssessmentAccess(userId, assessmentId))
                .Returns(true);

            // Mock assessment statistics data
            var mockAnswers = new List<ANSWER>
            {
                new() { Assessment_Id = assessmentId, Answer_Text = "Y" },
                new() { Assessment_Id = assessmentId, Answer_Text = "N" },
                new() { Assessment_Id = assessmentId, Answer_Text = "NA" }
            };

            var mockAnswersDbSet = CreateMockDbSet(mockAnswers);
            MockDbContext.Setup(x => x.ANSWER).Returns(mockAnswersDbSet.Object);

            // Act
            var result = _assessmentBusiness.GetAssessmentStatistics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.TotalQuestions.Should().Be(3);
            result.YesAnswers.Should().Be(1);
            result.NoAnswers.Should().Be(1);
            result.NotApplicableAnswers.Should().Be(1);
        }
    }
} 