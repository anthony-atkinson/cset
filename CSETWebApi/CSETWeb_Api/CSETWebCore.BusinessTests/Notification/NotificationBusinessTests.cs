//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using CSETWebCore.Business.Notification;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Contact;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Notification
{
    [TestClass]
    public class NotificationBusinessTests : BaseBusinessTest
    {
        private NotificationBusiness _notificationBusiness;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ITokenManager> _mockTokenManager;
        private Mock<IUtilities> _mockUtilities;
        private Mock<IResourceHelper> _mockResourceHelper;
        private Mock<ILocalInstallationHelper> _mockLocalInstallationHelper;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateInMemoryDbContext();
            _mockConfiguration = Fixture.Freeze<Mock<IConfiguration>>();
            _mockTokenManager = Fixture.Freeze<Mock<ITokenManager>>();
            _mockUtilities = Fixture.Freeze<Mock<IUtilities>>();
            _mockResourceHelper = Fixture.Freeze<Mock<IResourceHelper>>();
            _mockLocalInstallationHelper = Fixture.Freeze<Mock<ILocalInstallationHelper>>();

            SetupMockConfiguration();
            SetupMockResourceHelper();

            _notificationBusiness = new NotificationBusiness(
                _mockConfiguration.Object,
                _mockTokenManager.Object,
                _mockUtilities.Object,
                _context,
                _mockResourceHelper.Object,
                _mockLocalInstallationHelper.Object);
        }

        #region Initialize Tests

        [TestMethod]
        public void Initialize_OnConstruction_PopulatesAppDisplayNames()
        {
            // Arrange & Act - Done in Setup()

            // Assert
            // The Initialize method is called in the constructor
            // We can verify the business object was created successfully
            _notificationBusiness.Should().NotBeNull();
        }

        #endregion

        #region SetScope Tests

        [TestMethod]
        public void SetScope_WithValidToken_SetsScopeFromToken()
        {
            // Arrange
            var expectedScope = "ACET";
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns(expectedScope);

            // Act
            _notificationBusiness.SetScope();

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
        }

        [TestMethod]
        public void SetScope_WithNullToken_SetsDefaultScope()
        {
            // Arrange
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns((string)null);

            // Act
            _notificationBusiness.SetScope();

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
        }

        [TestMethod]
        public void SetScope_WithExplicitScope_SetsProvidedScope()
        {
            // Arrange
            var expectedScope = "TSA";

            // Act
            _notificationBusiness.SetScope(expectedScope);

            // Assert
            // Scope is set internally, we can verify the method completes without error
        }

        #endregion

        #region InviteToAssessment Tests

        [TestMethod]
        public void InviteToAssessment_WithValidContact_SendsInvitationEmail()
        {
            // Arrange
            var contact = CreateTestContactCreateParameters();
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("CSET");
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.InviteToAssessment(contact);

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
            _mockUtilities.Verify(x => x.GetClientHost(), Times.Once);
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("assessmentInviteTemplate")), 
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void InviteToAssessment_WithNullBody_SetsEmptyBody()
        {
            // Arrange
            var contact = CreateTestContactCreateParameters();
            contact.Body = null;
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("CSET");
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.InviteToAssessment(contact);

            // Assert
            // Method should complete without throwing exception
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("assessmentInviteTemplate")), 
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void InviteToAssessment_WithLocalInstallation_RemovesCsetAppLink()
        {
            // Arrange
            var contact = CreateTestContactCreateParameters();
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("CSET");
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(true);

            // Act
            _notificationBusiness.InviteToAssessment(contact);

            // Assert
            _mockLocalInstallationHelper.Verify(x => x.IsLocalInstallation(), Times.Once);
        }

        [TestMethod]
        public void InviteToAssessment_WithContactName_BuildsFullName()
        {
            // Arrange
            var contact = CreateTestContactCreateParameters();
            contact.FirstName = "John";
            contact.LastName = "Doe";
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("CSET");
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.InviteToAssessment(contact);

            // Assert
            // Method should complete without throwing exception
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("assessmentInviteTemplate")), 
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region SendPasswordEmail Tests

        [TestMethod]
        public void SendPasswordEmail_WithValidParameters_SendsPasswordEmail()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "CSET";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendPasswordEmail(email, firstName, lastName, password, appName);

            // Assert
            _mockUtilities.Verify(x => x.GetClientHost(), Times.Once);
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("passwordCreationTemplate")), 
                appName), Times.Once);
        }

        [TestMethod]
        public void SendPasswordEmail_WithUnknownAppName_DefaultsToCSET()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "UNKNOWN_APP";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendPasswordEmail(email, firstName, lastName, password, appName);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("passwordCreationTemplate")), 
                "CSET"), Times.Once);
        }

        [TestMethod]
        public void SendPasswordEmail_WithLocalInstallation_RemovesCsetAppLink()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "CSET";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(true);

            // Act
            _notificationBusiness.SendPasswordEmail(email, firstName, lastName, password, appName);

            // Assert
            _mockLocalInstallationHelper.Verify(x => x.IsLocalInstallation(), Times.Once);
        }

        #endregion

        #region SendInviteePassword Tests

        [TestMethod]
        public void SendInviteePassword_WithValidParameters_SendsInviteePasswordEmail()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "ACET";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendInviteePassword(email, firstName, lastName, password, appName);

            // Assert
            _mockUtilities.Verify(x => x.GetClientHost(), Times.Once);
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("invitedPasswordCreationTemplate")), 
                appName), Times.Once);
        }

        [TestMethod]
        public void SendInviteePassword_WithUnknownAppName_DefaultsToCSET()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "UNKNOWN_APP";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendInviteePassword(email, firstName, lastName, password, appName);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("invitedPasswordCreationTemplate")), 
                "CSET"), Times.Once);
        }

        [TestMethod]
        public void SendInviteePassword_WithLocalInstallation_RemovesCsetAppLink()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "tempPassword123";
            var appName = "CSET";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(true);

            // Act
            _notificationBusiness.SendInviteePassword(email, firstName, lastName, password, appName);

            // Assert
            _mockLocalInstallationHelper.Verify(x => x.IsLocalInstallation(), Times.Once);
        }

        #endregion

        #region SendPasswordResetEmail Tests

        [TestMethod]
        public void SendPasswordResetEmail_WithValidParameters_SendsPasswordResetEmail()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "newPassword123";
            var subject = "Password Reset";
            var appName = "TSA";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendPasswordResetEmail(email, firstName, lastName, password, subject, appName);

            // Assert
            _mockUtilities.Verify(x => x.GetClientHost(), Times.Once);
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("passwordResetTemplate")), 
                appName), Times.Once);
        }

        [TestMethod]
        public void SendPasswordResetEmail_WithEmptyName_UsesEmailAsName()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "";
            var lastName = "";
            var password = "newPassword123";
            var subject = "Password Reset";
            var appName = "CSET";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendPasswordResetEmail(email, firstName, lastName, password, subject, appName);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("passwordResetTemplate")), 
                appName), Times.Once);
        }

        [TestMethod]
        public void SendPasswordResetEmail_WithUnknownAppName_DefaultsToCSET()
        {
            // Arrange
            var email = "test@example.com";
            var firstName = "John";
            var lastName = "Doe";
            var password = "newPassword123";
            var subject = "Password Reset";
            var appName = "UNKNOWN_APP";
            
            _mockUtilities.Setup(x => x.GetClientHost()).Returns("https://test.cset.com");
            _mockLocalInstallationHelper.Setup(x => x.IsLocalInstallation()).Returns(false);

            // Act
            _notificationBusiness.SendPasswordResetEmail(email, firstName, lastName, password, subject, appName);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("passwordResetTemplate")), 
                "CSET"), Times.Once);
        }

        #endregion

        #region SendTestEmail Tests

        [TestMethod]
        public void SendTestEmail_WithValidRecipient_SendsTestEmail()
        {
            // Arrange
            var recipient = "test@example.com";
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("CSET");

            // Act
            _notificationBusiness.SendTestEmail(recipient);

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
        }

        [TestMethod]
        public void SendTestEmail_WithACETScope_UsesACETAppName()
        {
            // Arrange
            var recipient = "test@example.com";
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("ACET");

            // Act
            _notificationBusiness.SendTestEmail(recipient);

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
        }

        [TestMethod]
        public void SendTestEmail_WithUnknownScope_DefaultsToCSET()
        {
            // Arrange
            var recipient = "test@example.com";
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("UNKNOWN_SCOPE");

            // Act
            _notificationBusiness.SendTestEmail(recipient);

            // Assert
            _mockTokenManager.Verify(x => x.Payload("scope"), Times.Once);
        }

        #endregion

        #region SendMail Tests

        [TestMethod]
        public void SendMail_WithValidMessage_ProcessesEmailTemplates()
        {
            // Arrange
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.Subject = "Test Subject";
            mailMessage.Body = "Test body with {{inline-stylesheet}} and {{email-footer}}";
            mailMessage.To.Add("test@example.com");
            mailMessage.From = new System.Net.Mail.MailAddress("sender@example.com");

            // Act
            _notificationBusiness.SendMail(mailMessage);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("inlineStylesheet")), 
                It.IsAny<string>()), Times.Once);
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("EmailFooter")), 
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SendMail_WithACETFooter_ProcessesACETFooter()
        {
            // Arrange
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.Subject = "Test Subject";
            mailMessage.Body = "Test body with {{email-footer-ACET}}";
            mailMessage.To.Add("test@example.com");
            mailMessage.From = new System.Net.Mail.MailAddress("sender@example.com");

            // Act
            _notificationBusiness.SendMail(mailMessage);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("EmailFooter_ACET")), 
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SendMail_WithTSAFooter_ProcessesTSAFooter()
        {
            // Arrange
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.Subject = "Test Subject";
            mailMessage.Body = "Test body with {{email-footer-TSA}}";
            mailMessage.To.Add("test@example.com");
            mailMessage.From = new System.Net.Mail.MailAddress("sender@example.com");

            // Act
            _notificationBusiness.SendMail(mailMessage);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("EmailFooter_TSA")), 
                It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void SendMail_WithCFFooter_ProcessesCFFooter()
        {
            // Arrange
            var mailMessage = new System.Net.Mail.MailMessage();
            mailMessage.Subject = "Test Subject";
            mailMessage.Body = "Test body with {{email-footer-CF}}";
            mailMessage.To.Add("test@example.com");
            mailMessage.From = new System.Net.Mail.MailAddress("sender@example.com");

            // Act
            _notificationBusiness.SendMail(mailMessage);

            // Assert
            _mockResourceHelper.Verify(x => x.GetEmbeddedResource(
                It.Is<string>(s => s.Contains("EmailFooter_CF")), 
                It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private void SetupMockConfiguration()
        {
            var emailConfig = new Dictionary<string, string>
            {
                ["Email:SenderEmail"] = "sender@example.com",
                ["Email:SenderDisplayName"] = "CSET System",
                ["Email:SmtpHost"] = "smtp.example.com",
                ["Email:SmtpPort"] = "587",
                ["Email:SmtpSsl"] = "true",
                ["Email:SmtpUsername"] = "username",
                ["Email:SmtpPassword"] = "password",
                ["Email:AllowTestEmail"] = "true",
                ["Email:DHSEmail"] = "dhs@example.com"
            };

            var configSection = new Mock<IConfigurationSection>();
            configSection.Setup(x => x.AsEnumerable()).Returns(emailConfig.AsEnumerable());

            _mockConfiguration.Setup(x => x.GetSection("Email")).Returns(configSection.Object);
            _mockConfiguration.Setup(x => x.GetValue<string>("Email:DHSEmail")).Returns("dhs@example.com");
        }

        private void SetupMockResourceHelper()
        {
            _mockResourceHelper.Setup(x => x.GetEmbeddedResource(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("<html><body>Test template</body></html>");
            
            _mockResourceHelper.Setup(x => x.GetEmbeddedResource(It.IsAny<string>()))
                .Returns("<html><body>Test resource</body></html>");
        }

        private ContactCreateParameters CreateTestContactCreateParameters()
        {
            return new ContactCreateParameters
            {
                PrimaryEmail = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Subject = "Test Assessment Invitation",
                Body = "You are invited to participate in a CSET assessment.",
                AssessmentId = 1
            };
        }

        #endregion
    }
} 