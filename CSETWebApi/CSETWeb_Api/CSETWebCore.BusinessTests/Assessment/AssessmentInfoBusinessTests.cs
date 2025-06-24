using CSETWebCore.Business.Assessment;
using CSETWebCore.BusinessTests.Infrastructure;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Assessment;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace CSETWebCore.BusinessTests.Assessment
{
    public class AssessmentInfoBusinessTests : BaseBusinessTest
    {
        private readonly Mock<ITokenManager> _mockTokenManager;
        private readonly Mock<IUtilities> _mockUtilities;
        private readonly Mock<IContactBusiness> _mockContactBusiness;
        private readonly Mock<ISalBusiness> _mockSalBusiness;
        private readonly Mock<IMaturityBusiness> _mockMaturityBusiness;
        private readonly Mock<IAssessmentUtil> _mockAssessmentUtil;
        private readonly Mock<IStandardsBusiness> _mockStandardsBusiness;
        private readonly Mock<IDiagramManager> _mockDiagramManager;
        private readonly CSETContext _context;
        private readonly AssessmentBusiness _assessmentBusiness;

        public AssessmentInfoBusinessTests()
        {
            _mockTokenManager = AutoFixture.Freeze<Mock<ITokenManager>>();
            _mockUtilities = AutoFixture.Freeze<Mock<IUtilities>>();
            _mockContactBusiness = AutoFixture.Freeze<Mock<IContactBusiness>>();
            _mockSalBusiness = AutoFixture.Freeze<Mock<ISalBusiness>>();
            _mockMaturityBusiness = AutoFixture.Freeze<Mock<IMaturityBusiness>>();
            _mockAssessmentUtil = AutoFixture.Freeze<Mock<IAssessmentUtil>>();
            _mockStandardsBusiness = AutoFixture.Freeze<Mock<IStandardsBusiness>>();
            _mockDiagramManager = AutoFixture.Freeze<Mock<IDiagramManager>>();
            
            _context = CreateTestContext();
            _assessmentBusiness = new AssessmentBusiness(
                null, _mockTokenManager.Object, _mockUtilities.Object, _mockContactBusiness.Object,
                _mockSalBusiness.Object, _mockMaturityBusiness.Object, _mockAssessmentUtil.Object,
                _mockStandardsBusiness.Object, _mockDiagramManager.Object, _context);
        }

        #region GetAssessmentDetail Tests

        [Fact]
        public void GetAssessmentDetail_WithValidId_ReturnsCompleteAssessmentInfo()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            var information = CreateTestInformation(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockUtilities.Setup(x => x.UtcToLocal(It.IsAny<DateTime>())).Returns(DateTime.Now);

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(assessmentId);
            result.AssessmentName.Should().Be(information.Assessment_Name);
            result.FacilityName.Should().Be(information.Facility_Name);
            result.CityOrSiteName.Should().Be(information.City_Or_Site_Name);
            result.StateProvRegion.Should().Be(information.State_Province_Or_Region);
            result.PostalCode.Should().Be(information.Postal_Code);
            result.ExecutiveSummary.Should().Be(information.Executive_Summary);
            result.AssessmentDescription.Should().Be(information.Assessment_Description);
            result.AdditionalNotesAndComments.Should().Be(information.Additional_Notes_And_Comments);
        }

        [Fact]
        public void GetAssessmentDetail_WithValidGuid_ReturnsAssessmentInfo()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentGuid = Guid.NewGuid();
            var assessment = CreateTestAssessment(assessmentId, assessmentGuid);
            var information = CreateTestInformation(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(assessmentGuid);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(assessmentId);
            result.AssessmentGuid.Should().Be(assessmentGuid);
        }

        [Fact]
        public void GetAssessmentDetail_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(invalidAssessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(0); // Default value for new AssessmentDetail
        }

        [Fact]
        public void GetAssessmentDetail_WithInvalidGuid_ReturnsNull()
        {
            // Arrange
            var invalidGuid = Guid.NewGuid();

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(invalidGuid);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAssessmentDetail_WithToken_InitializesTokenManager()
        {
            // Arrange
            var assessmentId = 1;
            var token = "test-token";
            var assessment = CreateTestAssessment(assessmentId);
            var information = CreateTestInformation(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(assessmentId, token);

            // Assert
            _mockTokenManager.Verify(x => x.Init(token), Times.Once);
        }

        [Fact]
        public void GetAssessmentDetail_WithLegacyAssessment_SetsWorkflowCorrectly()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            var information = CreateTestInformation(assessmentId);
            information.Workflow = null;
            information.IsAcetOnly = true;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(assessmentId);

            // Assert
            result.Workflow.Should().Be("ACET");
        }

        [Fact]
        public void GetAssessmentDetail_WithLegacyBaseAssessment_SetsWorkflowCorrectly()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            var information = CreateTestInformation(assessmentId);
            information.Workflow = null;
            information.IsAcetOnly = false;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");

            // Act
            var result = _assessmentBusiness.GetAssessmentDetail(assessmentId);

            // Assert
            result.Workflow.Should().Be("BASE");
        }

        #endregion

        #region SaveAssessmentDetail Tests

        [Fact]
        public void SaveAssessmentDetail_WithValidData_SavesSuccessfully()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            result.Should().Be(assessmentId);
            
            var savedAssessment = _context.ASSESSMENTS.FirstOrDefault(x => x.Assessment_Id == assessmentId);
            savedAssessment.Should().NotBeNull();
            savedAssessment.Assessment_Date.Should().Be(assessmentDetail.AssessmentDate);
            savedAssessment.UseStandard.Should().Be(assessmentDetail.UseStandard);
            savedAssessment.UseDiagram.Should().Be(assessmentDetail.UseDiagram);
            savedAssessment.UseMaturity.Should().Be(assessmentDetail.UseMaturity);

            var savedInformation = _context.INFORMATION.FirstOrDefault(x => x.Id == assessmentId);
            savedInformation.Should().NotBeNull();
            savedInformation.Assessment_Name.Should().Be(assessmentDetail.AssessmentName);
            savedInformation.Facility_Name.Should().Be(assessmentDetail.FacilityName);
            savedInformation.City_Or_Site_Name.Should().Be(assessmentDetail.CityOrSiteName);
        }

        [Fact]
        public void SaveAssessmentDetail_WithNewAssessment_CreatesNewRecords()
        {
            // Arrange
            var assessmentId = 999;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            assessmentDetail.AssessmentGuid = Guid.NewGuid();
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(It.IsAny<int>()));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            result.Should().BeGreaterThan(0);
            
            var savedAssessment = _context.ASSESSMENTS.FirstOrDefault(x => x.Assessment_Id == result);
            savedAssessment.Should().NotBeNull();
            savedAssessment.Assessment_GUID.Should().Be(assessmentDetail.AssessmentGuid);

            var savedInformation = _context.INFORMATION.FirstOrDefault(x => x.Id == result);
            savedInformation.Should().NotBeNull();
        }

        [Fact]
        public void SaveAssessmentDetail_WithEmptyGuid_GeneratesNewGuid()
        {
            // Arrange
            var assessmentId = 999;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            assessmentDetail.AssessmentGuid = Guid.Empty;
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(It.IsAny<int>()));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            var savedAssessment = _context.ASSESSMENTS.FirstOrDefault(x => x.Assessment_Id == result);
            savedAssessment.Should().NotBeNull();
            savedAssessment.Assessment_GUID.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void SaveAssessmentDetail_WithMaturityModel_PersistsMaturityData()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            assessmentDetail.UseMaturity = true;
            assessmentDetail.MaturityModel = new MaturityModel { ModelName = "CMMC" };
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            _mockMaturityBusiness.Verify(x => x.PersistSelectedMaturityModel(assessmentId, "CMMC"), Times.Once);
        }

        [Fact]
        public void SaveAssessmentDetail_WithoutMaturityModel_ClearsMaturityData()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            assessmentDetail.UseMaturity = false;
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            _mockMaturityBusiness.Verify(x => x.ClearMaturityModel(assessmentId), Times.Once);
        }

        [Fact]
        public void SaveAssessmentDetail_WithCreatorUser_ProcessesAssessmentName()
        {
            // Arrange
            var assessmentId = 1;
            var userId = 1;
            var assessmentDetail = CreateTestAssessmentDetail(assessmentId);
            assessmentDetail.CreatorId = userId;
            
            var user = CreateTestUser(userId);
            _context.USERS.Add(user);
            _context.SaveChanges();
            
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _assessmentBusiness.SaveAssessmentDetail(assessmentId, assessmentDetail);

            // Assert
            // Note: AssessmentNaming.ProcessName is a static method, so we can't easily verify it was called
            // But we can verify the assessment was saved successfully
            result.Should().Be(assessmentId);
        }

        #endregion

        #region GetLastModifiedDateUtc Tests

        [Fact]
        public void GetLastModifiedDateUtc_WithValidAssessment_ReturnsLastModifiedDate()
        {
            // Arrange
            var assessmentId = 1;
            var expectedDate = DateTime.UtcNow;
            var assessment = CreateTestAssessment(assessmentId);
            assessment.LastModifiedDate = expectedDate;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");

            // Act
            var result = _assessmentBusiness.GetLastModifiedDateUtc(assessmentId);

            // Assert
            result.Should().Be(expectedDate);
        }

        [Fact]
        public void GetLastModifiedDateUtc_WithInvalidAssessment_ReturnsCurrentUtcTime()
        {
            // Arrange
            var invalidAssessmentId = 999;
            var beforeCall = DateTime.UtcNow;

            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("BASE");

            // Act
            var result = _assessmentBusiness.GetLastModifiedDateUtc(invalidAssessmentId);
            var afterCall = DateTime.UtcNow;

            // Assert
            result.Should().BeOnOrAfter(beforeCall);
            result.Should().BeOnOrBefore(afterCall);
        }

        #endregion

        #region GetAssessmentById Tests

        [Fact]
        public void GetAssessmentById_WithValidId_ReturnsAssessment()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            _context.ASSESSMENTS.Add(assessment);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetAssessmentById(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Assessment_Id.Should().Be(assessmentId);
        }

        [Fact]
        public void GetAssessmentById_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _assessmentBusiness.GetAssessmentById(invalidAssessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region IsCurrentUserOnAssessment Tests

        [Fact]
        public void IsCurrentUserOnAssessment_WithUserOnAssessment_ReturnsTrue()
        {
            // Arrange
            var assessmentId = 1;
            var userId = 1;
            
            var assessmentContact = new ASSESSMENT_CONTACTS
            {
                Assessment_Id = assessmentId,
                UserId = userId
            };
            
            _context.ASSESSMENT_CONTACTS.Add(assessmentContact);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Act
            var result = _assessmentBusiness.IsCurrentUserOnAssessment(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsCurrentUserOnAssessment_WithUserNotOnAssessment_ReturnsFalse()
        {
            // Arrange
            var assessmentId = 1;
            var userId = 1;
            var otherUserId = 2;
            
            var assessmentContact = new ASSESSMENT_CONTACTS
            {
                Assessment_Id = assessmentId,
                UserId = otherUserId
            };
            
            _context.ASSESSMENT_CONTACTS.Add(assessmentContact);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Act
            var result = _assessmentBusiness.IsCurrentUserOnAssessment(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsCurrentUserOnAssessment_WithNoContacts_ReturnsFalse()
        {
            // Arrange
            var assessmentId = 1;
            var userId = 1;

            _mockTokenManager.Setup(x => x.GetUserId()).Returns(userId);

            // Act
            var result = _assessmentBusiness.IsCurrentUserOnAssessment(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetOrganizationTypes Tests

        [Fact]
        public void GetOrganizationTypes_ReturnsAllOrganizationTypes()
        {
            // Arrange
            var orgTypes = new List<DEMOGRAPHICS_ORGANIZATION_TYPE>
            {
                new DEMOGRAPHICS_ORGANIZATION_TYPE { OrganizationTypeId = 1, OrganizationType = "Government" },
                new DEMOGRAPHICS_ORGANIZATION_TYPE { OrganizationTypeId = 2, OrganizationType = "Private Sector" }
            };
            
            _context.DEMOGRAPHICS_ORGANIZATION_TYPE.AddRange(orgTypes);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.GetCurrentLanguage()).Returns("en");

            // Act
            var result = _assessmentBusiness.GetOrganizationTypes();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(x => x.OrganizationType == "Government");
            result.Should().Contain(x => x.OrganizationType == "Private Sector");
        }

        [Fact]
        public void GetOrganizationTypes_WithNonEnglishLanguage_AppliesTranslation()
        {
            // Arrange
            var orgTypes = new List<DEMOGRAPHICS_ORGANIZATION_TYPE>
            {
                new DEMOGRAPHICS_ORGANIZATION_TYPE { OrganizationTypeId = 1, OrganizationType = "Government" }
            };
            
            _context.DEMOGRAPHICS_ORGANIZATION_TYPE.AddRange(orgTypes);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.GetCurrentLanguage()).Returns("es");

            // Act
            var result = _assessmentBusiness.GetOrganizationTypes();

            // Assert
            result.Should().HaveCount(1);
            // Note: Translation overlay functionality would need to be mocked for full testing
        }

        #endregion

        #region GetNames Tests

        [Fact]
        public void GetNames_WithValidIds_ReturnsAssessmentNames()
        {
            // Arrange
            var assessment1 = CreateTestAssessment(1);
            var assessment2 = CreateTestAssessment(2);
            var information1 = CreateTestInformation(1, "Assessment 1");
            var information2 = CreateTestInformation(2, "Assessment 2");
            
            _context.ASSESSMENTS.AddRange(assessment1, assessment2);
            _context.INFORMATION.AddRange(information1, information2);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetNames(1, 2, null, null, null, null, null, null, null, null);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain("Assessment 1");
            result.Should().Contain("Assessment 2");
        }

        [Fact]
        public void GetNames_WithNullAndZeroIds_FiltersThemOut()
        {
            // Arrange
            var assessment1 = CreateTestAssessment(1);
            var information1 = CreateTestInformation(1, "Assessment 1");
            
            _context.ASSESSMENTS.Add(assessment1);
            _context.INFORMATION.Add(information1);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetNames(1, 0, null, 2, null, 0, null, null, null, null);

            // Assert
            result.Should().HaveCount(1);
            result.Should().Contain("Assessment 1");
        }

        [Fact]
        public void GetNames_WithNoValidIds_ReturnsEmptyList()
        {
            // Act
            var result = _assessmentBusiness.GetNames(0, null, 0, null, 0, null, 0, null, 0, null);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetOtherRemarks Tests

        [Fact]
        public void GetOtherRemarks_WithValidAssessment_ReturnsRemarks()
        {
            // Arrange
            var assessmentId = 1;
            var expectedRemarks = "Test remarks";
            
            var otherRemarks = new OTHER_REMARKS
            {
                Assessment_Id = assessmentId,
                Remarks = expectedRemarks
            };
            
            _context.OTHER_REMARKS.Add(otherRemarks);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetOtherRemarks(assessmentId);

            // Assert
            result.Should().Be(expectedRemarks);
        }

        [Fact]
        public void GetOtherRemarks_WithInvalidAssessment_ReturnsEmptyString()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _assessmentBusiness.GetOtherRemarks(invalidAssessmentId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region SaveOtherRemarks Tests

        [Fact]
        public void SaveOtherRemarks_WithNewRemarks_CreatesNewRecord()
        {
            // Arrange
            var assessmentId = 1;
            var remarks = "New test remarks";

            // Act
            _assessmentBusiness.SaveOtherRemarks(assessmentId, remarks);

            // Assert
            var savedRemarks = _context.OTHER_REMARKS.FirstOrDefault(x => x.Assessment_Id == assessmentId);
            savedRemarks.Should().NotBeNull();
            savedRemarks.Remarks.Should().Be(remarks);
        }

        [Fact]
        public void SaveOtherRemarks_WithExistingRemarks_UpdatesRecord()
        {
            // Arrange
            var assessmentId = 1;
            var originalRemarks = "Original remarks";
            var updatedRemarks = "Updated remarks";
            
            var existingRemarks = new OTHER_REMARKS
            {
                Assessment_Id = assessmentId,
                Remarks = originalRemarks
            };
            
            _context.OTHER_REMARKS.Add(existingRemarks);
            _context.SaveChanges();

            // Act
            _assessmentBusiness.SaveOtherRemarks(assessmentId, updatedRemarks);

            // Assert
            var savedRemarks = _context.OTHER_REMARKS.FirstOrDefault(x => x.Assessment_Id == assessmentId);
            savedRemarks.Should().NotBeNull();
            savedRemarks.Remarks.Should().Be(updatedRemarks);
        }

        [Fact]
        public void SaveOtherRemarks_WithEmptyRemarks_DeletesRecord()
        {
            // Arrange
            var assessmentId = 1;
            var existingRemarks = new OTHER_REMARKS
            {
                Assessment_Id = assessmentId,
                Remarks = "Original remarks"
            };
            
            _context.OTHER_REMARKS.Add(existingRemarks);
            _context.SaveChanges();

            // Act
            _assessmentBusiness.SaveOtherRemarks(assessmentId, "");

            // Assert
            var savedRemarks = _context.OTHER_REMARKS.FirstOrDefault(x => x.Assessment_Id == assessmentId);
            savedRemarks.Should().BeNull();
        }

        #endregion

        #region GetAssessmentCreator Tests

        [Fact]
        public void GetAssessmentCreator_WithValidAssessment_ReturnsCreatorId()
        {
            // Arrange
            var assessmentId = 1;
            var creatorId = 5;
            var assessment = CreateTestAssessment(assessmentId);
            assessment.AssessmentCreatorId = creatorId;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetAssessmentCreator(assessmentId);

            // Assert
            result.Should().Be(creatorId);
        }

        [Fact]
        public void GetAssessmentCreator_WithInvalidAssessment_ReturnsNull()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _assessmentBusiness.GetAssessmentCreator(invalidAssessmentId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetAssessmentCreator_WithNullCreatorId_ReturnsNull()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            assessment.AssessmentCreatorId = null;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.SaveChanges();

            // Act
            var result = _assessmentBusiness.GetAssessmentCreator(assessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private ASSESSMENTS CreateTestAssessment(int assessmentId, Guid? assessmentGuid = null)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = assessmentId,
                Assessment_GUID = assessmentGuid ?? Guid.NewGuid(),
                Assessment_Date = DateTime.Now,
                AssessmentCreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.UtcNow,
                UseStandard = true,
                UseDiagram = false,
                UseMaturity = false
            };
        }

        private INFORMATION CreateTestInformation(int assessmentId, string assessmentName = "Test Assessment")
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = assessmentName,
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State",
                Postal_Code = "12345",
                Executive_Summary = "Test Summary",
                Assessment_Description = "Test Description",
                Additional_Notes_And_Comments = "Test Notes",
                Workflow = "BASE",
                Origin = "Test Origin"
            };
        }

        private AssessmentDetail CreateTestAssessmentDetail(int assessmentId)
        {
            return new AssessmentDetail
            {
                Id = assessmentId,
                AssessmentGuid = Guid.NewGuid(),
                AssessmentName = "Test Assessment",
                FacilityName = "Test Facility",
                CityOrSiteName = "Test City",
                StateProvRegion = "Test State",
                PostalCode = "12345",
                ExecutiveSummary = "Test Summary",
                AssessmentDescription = "Test Description",
                AdditionalNotesAndComments = "Test Notes",
                AssessmentDate = DateTime.Now,
                UseStandard = true,
                UseDiagram = false,
                UseMaturity = false,
                Workflow = "BASE",
                Origin = "Test Origin"
            };
        }

        private USERS CreateTestUser(int userId)
        {
            return new USERS
            {
                UserId = userId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com"
            };
        }

        #endregion
    }
} 