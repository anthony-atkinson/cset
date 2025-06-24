//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CSETWebCore.Business.Contact;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Contact;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Contact;

namespace CSETWebCore.BusinessTests.Contact
{
    [TestClass]
    public class ContactBusinessTests : BaseBusinessTest
    {
        private ContactBusiness _contactBusiness;
        private Mock<CSETContext> _mockContext;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private Mock<ITokenManager> _mockTokenManager;
        private Mock<INotificationBusiness> _mockNotificationBusiness;
        private Mock<IUserBusiness> _mockUserBusiness;
        private Mock<ILocalInstallationHelper> _mockLocalInstallationHelper;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _mockAssessmentUtil = new Mock<IAssessmentUtil>();
            _mockTokenManager = new Mock<ITokenManager>();
            _mockNotificationBusiness = new Mock<INotificationBusiness>();
            _mockUserBusiness = new Mock<IUserBusiness>();
            _mockLocalInstallationHelper = new Mock<ILocalInstallationHelper>();
            _contactBusiness = new ContactBusiness(
                _mockContext.Object,
                _mockAssessmentUtil.Object,
                _mockTokenManager.Object,
                _mockNotificationBusiness.Object,
                _mockUserBusiness.Object,
                _mockLocalInstallationHelper.Object
            );
        }

        [TestMethod]
        public void GetContacts_WithValidAssessmentId_ReturnsContacts()
        {
            // Arrange
            int assessmentId = 1;
            var testContact = new ASSESSMENT_CONTACTS
            {
                Assessment_Id = assessmentId,
                FirstName = "John",
                LastName = "Doe",
                PrimaryEmail = "john.doe@example.com",
                AssessmentRoleId = 2,
                Invited = false,
                UserId = 10,
                Assessment_Contact_Id = 100,
                Title = "Manager",
                Phone = "123-456-7890",
                Cell_Phone = "987-654-3210",
                Reports_To = "Jane Smith",
                Organization_Name = "TestOrg",
                Site_Name = "TestSite",
                Emergency_Communications_Protocol = "Call 911",
                Is_Site_Participant = true,
                Is_Primary_POC = false
            };
            var mockDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { testContact });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockDbSet.Object);

            // Act
            var result = _contactBusiness.GetContacts(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].FirstName.Should().Be("John");
            result[0].LastName.Should().Be("Doe");
            result[0].PrimaryEmail.Should().Be("john.doe@example.com");
        }

        [TestMethod]
        public void GetContactsByAssessmentId_WithMultipleIds_ReturnsContacts()
        {
            // Arrange
            int id1 = 1, id2 = 2;
            var contact1 = new ASSESSMENT_CONTACTS { Assessment_Id = id1, FirstName = "Alice", LastName = "Smith", PrimaryEmail = "alice@example.com" };
            var contact2 = new ASSESSMENT_CONTACTS { Assessment_Id = id2, FirstName = "Bob", LastName = "Jones", PrimaryEmail = "bob@example.com" };
            var mockDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { contact1, contact2 });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockDbSet.Object);

            // Act
            var result = _contactBusiness.GetContactsByAssessmentId(id1, id2);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Any(c => c.FirstName == "Alice").Should().BeTrue();
            result.Any(c => c.FirstName == "Bob").Should().BeTrue();
        }

        [TestMethod]
        public void SearchContacts_WithValidParameters_ReturnsMatchingContacts()
        {
            // Arrange
            int userId = 1;
            var user = new USERS { UserId = userId, PrimaryEmail = "user@example.com" };
            var contact1 = new ASSESSMENT_CONTACTS { FirstName = "Jane", LastName = "Doe", PrimaryEmail = "jane@example.com", Assessment_Id = 1 };
            var contact2 = new ASSESSMENT_CONTACTS { FirstName = "John", LastName = "Smith", PrimaryEmail = "john@example.com", Assessment_Id = 2 };
            var attachedContact = new ASSESSMENT_CONTACTS { PrimaryEmail = "jane@example.com", Assessment_Id = 1 };
            var mockUserDbSet = CreateMockDbSet(new List<USERS> { user });
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { contact1, contact2, attachedContact });
            var mockAssessmentDbSet = CreateMockDbSet(new List<ASSESSMENTS> { new ASSESSMENTS { Assessment_Id = 1 }, new ASSESSMENTS { Assessment_Id = 2 } });
            _mockContext.Setup(x => x.USERS).Returns(mockUserDbSet.Object);
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);
            _mockContext.Setup(x => x.ASSESSMENTS).Returns(mockAssessmentDbSet.Object);

            var searchParms = new ContactSearchParameters
            {
                FirstName = "Jane",
                LastName = "Doe",
                PrimaryEmail = "jane@example.com",
                AssessmentId = 1
            };

            // Act
            var result = _contactBusiness.SearchContacts(userId, searchParms).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Already attached, so should be filtered out
        }

        [TestMethod]
        public void SearchContacts_WithNullSearchParms_ReturnsEmptyList()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = _contactBusiness.SearchContacts(userId, null);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void AddContactToAssessment_WithValidData_AddsContactAndReturnsDetail()
        {
            // Arrange
            int assessmentId = 1;
            int userId = 10;
            int roleId = 2;
            bool invited = true;
            var user = new USERS { UserId = userId, FirstName = "Jane", LastName = "Smith", PrimaryEmail = "jane.smith@example.com" };
            var mockUserDbSet = CreateMockDbSet(new List<USERS> { user });
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS>());
            _mockContext.Setup(x => x.USERS).Returns(mockUserDbSet.Object);
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _contactBusiness.AddContactToAssessment(assessmentId, userId, roleId, invited);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Jane");
            result.LastName.Should().Be("Smith");
            result.PrimaryEmail.Should().Be("jane.smith@example.com");
            result.AssessmentRoleId.Should().Be(roleId);
            result.Invited.Should().Be(invited);
        }

        [TestMethod]
        public void CreateAndAddContactToAssessment_NewContact_AddsContactAndReturnsDetail()
        {
            // Arrange
            var newContact = new ContactCreateParameters
            {
                FirstName = "Sam",
                LastName = "Taylor",
                PrimaryEmail = "sam.taylor@example.com",
                AssessmentRoleId = 2,
                Title = "Engineer",
                Phone = "555-1234",
                CellPhone = "555-5678",
                ReportsTo = "Manager",
                OrganizationName = "Org",
                SiteName = "Site",
                EmergencyCommunicationsProtocol = "Protocol",
                IsSiteParticipant = true,
                IsPrimaryPoc = false
            };
            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(1);
            _mockTokenManager.Setup(x => x.Payload(It.IsAny<string>())).Returns("test");
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS>());
            var mockUserDbSet = CreateMockDbSet(new List<USERS>());
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);
            _mockContext.Setup(x => x.USERS).Returns(mockUserDbSet.Object);

            // Act
            var result = _contactBusiness.CreateAndAddContactToAssessment(newContact, false);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Sam");
            result.LastName.Should().Be("Taylor");
            result.PrimaryEmail.Should().Be("sam.taylor@example.com");
        }

        [TestMethod]
        public void UpdateContact_WithValidContact_UpdatesContact()
        {
            // Arrange
            int userId = 10;
            int assessmentId = 1;
            var contactDetail = new ContactDetail
            {
                UserId = userId,
                FirstName = "Updated",
                LastName = "User",
                PrimaryEmail = "updated.user@example.com",
                AssessmentId = assessmentId,
                AssessmentRoleId = 2,
                Title = "Lead",
                Phone = "555-0000",
                CellPhone = "555-1111",
                ReportsTo = "Boss",
                OrganizationName = "Org",
                SiteName = "HQ",
                EmergencyCommunicationsProtocol = "Protocol",
                IsSiteParticipant = true,
                IsPrimaryPoc = true
            };
            var ac = new ASSESSMENT_CONTACTS { UserId = userId, Assessment_Id = assessmentId };
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { ac });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);

            // Act
            _contactBusiness.UpdateContact(contactDetail, userId);

            // Assert
            ac.FirstName.Should().Be("Updated");
            ac.LastName.Should().Be("User");
            ac.PrimaryEmail.Should().Be("updated.user@example.com");
            ac.Title.Should().Be("Lead");
            ac.Phone.Should().Be("555-0000");
            ac.Cell_Phone.Should().Be("555-1111");
            ac.Reports_To.Should().Be("Boss");
            ac.Organization_Name.Should().Be("Org");
            ac.Site_Name.Should().Be("HQ");
            ac.Emergency_Communications_Protocol.Should().Be("Protocol");
            ac.Is_Site_Participant.Should().BeTrue();
            ac.Is_Primary_POC.Should().BeTrue();
        }

        [TestMethod]
        public void GetUserRoleOnAssessment_WithExistingContact_ReturnsRoleId()
        {
            // Arrange
            int userId = 10;
            int assessmentId = 1;
            var ac = new ASSESSMENT_CONTACTS { UserId = userId, Assessment_Id = assessmentId, AssessmentRoleId = 2 };
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { ac });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);

            // Act
            var result = _contactBusiness.GetUserRoleOnAssessment(userId, assessmentId);

            // Assert
            result.Should().Be(2);
        }

        [TestMethod]
        public void RemoveContact_WithValidId_RemovesContactAndReturnsUpdatedList()
        {
            // Arrange
            int assessmentContactId = 100;
            int assessmentId = 1;
            var ac = new ASSESSMENT_CONTACTS { Assessment_Contact_Id = assessmentContactId, Assessment_Id = assessmentId };
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { ac });
            var mockFindingContactDbSet = CreateMockDbSet(new List<FINDING_CONTACT>());
            var mockDemoDbSet = CreateMockDbSet(new List<DEMOGRAPHICS>());
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);
            _mockContext.Setup(x => x.FINDING_CONTACT).Returns(mockFindingContactDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemoDbSet.Object);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _contactBusiness.RemoveContact(assessmentContactId);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void MarkContactInvited_WithExistingContact_SetsInvitedTrue()
        {
            // Arrange
            int userId = 10;
            int assessmentId = 1;
            var ac = new ASSESSMENT_CONTACTS { UserId = userId, Assessment_Id = assessmentId, Invited = false };
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { ac });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);

            // Act
            _contactBusiness.MarkContactInvited(userId, assessmentId);

            // Assert
            ac.Invited.Should().BeTrue();
        }

        [TestMethod]
        public void RefreshContactNameFromUserDetails_WithValidUserAndAssessment_UpdatesContactName()
        {
            // Arrange
            int userId = 10;
            int assessmentId = 1;
            var ac = new ASSESSMENT_CONTACTS { UserId = userId, Assessment_Id = assessmentId, FirstName = "Old", LastName = "Name" };
            var user = new USERS { UserId = userId, FirstName = "New", LastName = "Name" };
            var mockContactDbSet = CreateMockDbSet(new List<ASSESSMENT_CONTACTS> { ac });
            var mockUserDbSet = CreateMockDbSet(new List<USERS> { user });
            _mockContext.Setup(x => x.ASSESSMENT_CONTACTS).Returns(mockContactDbSet.Object);
            _mockContext.Setup(x => x.USERS).Returns(mockUserDbSet.Object);
            _mockTokenManager.Setup(x => x.PayloadInt(It.IsAny<string>())).Returns((string key) =>
            {
                if (key.Contains("UserId")) return userId;
                if (key.Contains("AssessmentId")) return assessmentId;
                return null;
            });

            // Act
            _contactBusiness.RefreshContactNameFromUserDetails();

            // Assert
            ac.FirstName.Should().Be("New");
            ac.LastName.Should().Be("Name");
        }

        // Helper to create a mock DbSet
        private Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return mockSet;
        }
    }
} 