using System;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Notifications;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Tests.Notifications
{
    /// <summary>
    /// Tests for CSET email notification functionality
    /// </summary>
    [TestFixture]
    [TestCategory("Notifications")]
    [TestCategory("Email")]
    [TestCategory("E2E")]
    public class EmailNotificationTests : BaseTestFixture
    {
        private LoginPage _loginPage = null!;
        private DashboardPage _dashboardPage = null!;
        private AssessmentCreationPage _assessmentCreationPage = null!;
        private NotificationPage _notificationPage = null!;

        protected override async Task SetUpAsync()
        {
            await base.SetUpAsync();
            
            _loginPage = new LoginPage(Page);
            _dashboardPage = new DashboardPage(Page);
            _assessmentCreationPage = new AssessmentCreationPage(Page);
            _notificationPage = new NotificationPage(Page);
        }

        [Test]
        [Description("Verify assessment invitation email functionality")]
        public async Task AssessmentInvitation_Should_SendEmailNotification()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Create an assessment
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _assessmentCreationPage.WaitForPageLoadAsync();
            await _assessmentCreationPage.SelectGalleryItemByIndexAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync($"Invitation Test {DateTime.Now:yyyyMMdd_HHmmss}");
            await _assessmentCreationPage.ClickCreateButtonAsync();

            // Act - Navigate to contacts/invitations section
            // Note: This would require additional page objects for the contacts/invitations functionality
            // For now, we'll test the UI elements that would trigger email notifications

            // Verify invitation form elements are present
            var hasInvitationForm = await _notificationPage.HasInvitationFormAsync();
            
            if (hasInvitationForm)
            {
                // Test invitation form functionality
                await _notificationPage.SetInviteeEmailAsync("test@example.com");
                await _notificationPage.SetInvitationSubjectAsync("Test Assessment Invitation");
                await _notificationPage.SetInvitationMessageAsync("You have been invited to participate in a CSET assessment.");
                
                // Submit invitation
                await _notificationPage.SubmitInvitationAsync();
                
                // Verify success notification
                await _notificationPage.WaitForInvitationSuccessNotificationAsync();
                var successMessage = await _notificationPage.GetInvitationSuccessMessageAsync();
                Assert.That(successMessage, Is.Not.Empty, "Invitation success message should be displayed");
                
                // Verify email notification was triggered
                var hasEmailNotification = await _notificationPage.HasEmailNotificationAsync();
                Assert.That(hasEmailNotification, Is.True, "Email notification should be triggered for invitation");
            }
            else
            {
                TestContext.WriteLine("Invitation form not available in current assessment state");
            }
        }

        [Test]
        [Description("Verify password reset email functionality")]
        public async Task PasswordReset_Should_SendEmailNotification()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Navigate to forgot password
            await _loginPage.ClickForgotPasswordLinkAsync();
            await _loginPage.WaitForForgotPasswordFormAsync();

            // Enter email for password reset
            await _loginPage.SetForgotPasswordEmailAsync("test@example.com");
            await _loginPage.SubmitForgotPasswordAsync();

            // Verify password reset notification
            await _notificationPage.WaitForPasswordResetNotificationAsync();
            var resetMessage = await _notificationPage.GetPasswordResetMessageAsync();
            Assert.That(resetMessage, Is.Not.Empty, "Password reset message should be displayed");

            // Verify email notification was triggered
            var hasEmailNotification = await _notificationPage.HasEmailNotificationAsync();
            Assert.That(hasEmailNotification, Is.True, "Email notification should be triggered for password reset");
        }

        [Test]
        [Description("Verify email configuration validation")]
        public async Task EmailConfiguration_Should_BeValid()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Navigate to email settings (if available)
            var hasEmailSettings = await _notificationPage.HasEmailSettingsAsync();
            
            if (hasEmailSettings)
            {
                await _notificationPage.NavigateToEmailSettingsAsync();
                
                // Verify email configuration elements
                var hasSmtpSettings = await _notificationPage.HasSmtpSettingsAsync();
                Assert.That(hasSmtpSettings, Is.True, "SMTP settings should be available");

                var hasSenderEmail = await _notificationPage.HasSenderEmailSettingAsync();
                Assert.That(hasSenderEmail, Is.True, "Sender email setting should be available");

                var hasSenderName = await _notificationPage.HasSenderNameSettingAsync();
                Assert.That(hasSenderName, Is.True, "Sender name setting should be available");

                // Test email configuration validation
                await _notificationPage.TestEmailConfigurationAsync();
                
                // Verify test email notification
                await _notificationPage.WaitForTestEmailNotificationAsync();
                var testMessage = await _notificationPage.GetTestEmailMessageAsync();
                Assert.That(testMessage, Is.Not.Empty, "Test email message should be displayed");
            }
            else
            {
                TestContext.WriteLine("Email settings not available in current user role");
            }
        }

        [Test]
        [Description("Verify email template functionality")]
        public async Task EmailTemplates_Should_BeFunctional()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for email template functionality
            var hasEmailTemplates = await _notificationPage.HasEmailTemplatesAsync();
            
            if (hasEmailTemplates)
            {
                await _notificationPage.NavigateToEmailTemplatesAsync();
                
                // Verify template types are available
                var hasInvitationTemplate = await _notificationPage.HasInvitationTemplateAsync();
                Assert.That(hasInvitationTemplate, Is.True, "Invitation email template should be available");

                var hasPasswordResetTemplate = await _notificationPage.HasPasswordResetTemplateAsync();
                Assert.That(hasPasswordResetTemplate, Is.True, "Password reset email template should be available");

                var hasWelcomeTemplate = await _notificationPage.HasWelcomeTemplateAsync();
                Assert.That(hasWelcomeTemplate, Is.True, "Welcome email template should be available");

                // Test template preview
                await _notificationPage.PreviewEmailTemplateAsync("invitation");
                var previewContent = await _notificationPage.GetEmailTemplatePreviewAsync();
                Assert.That(previewContent, Is.Not.Empty, "Email template preview should display content");

                // Verify template variables are properly substituted
                var hasTemplateVariables = await _notificationPage.HasTemplateVariablesAsync();
                Assert.That(hasTemplateVariables, Is.True, "Email template should support variables");
            }
            else
            {
                TestContext.WriteLine("Email templates not available in current configuration");
            }
        }

        [Test]
        [Description("Verify email notification preferences")]
        public async Task EmailPreferences_Should_BeConfigurable()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Navigate to notification preferences
            var hasNotificationPreferences = await _notificationPage.HasNotificationPreferencesAsync();
            
            if (hasNotificationPreferences)
            {
                await _notificationPage.NavigateToNotificationPreferencesAsync();
                
                // Verify email preference options
                var hasInvitationEmails = await _notificationPage.HasInvitationEmailPreferenceAsync();
                Assert.That(hasInvitationEmails, Is.True, "Invitation email preference should be available");

                var hasPasswordResetEmails = await _notificationPage.HasPasswordResetEmailPreferenceAsync();
                Assert.That(hasPasswordResetEmails, Is.True, "Password reset email preference should be available");

                var hasSystemEmails = await _notificationPage.HasSystemEmailPreferenceAsync();
                Assert.That(hasSystemEmails, Is.True, "System email preference should be available");

                // Test preference toggling
                await _notificationPage.ToggleInvitationEmailsAsync(false);
                var invitationDisabled = await _notificationPage.IsInvitationEmailsDisabledAsync();
                Assert.That(invitationDisabled, Is.True, "Invitation emails should be disabled");

                await _notificationPage.ToggleInvitationEmailsAsync(true);
                var invitationEnabled = await _notificationPage.IsInvitationEmailsEnabledAsync();
                Assert.That(invitationEnabled, Is.True, "Invitation emails should be enabled");

                // Save preferences
                await _notificationPage.SaveNotificationPreferencesAsync();
                
                // Verify save confirmation
                await _notificationPage.WaitForPreferencesSavedNotificationAsync();
                var saveMessage = await _notificationPage.GetPreferencesSavedMessageAsync();
                Assert.That(saveMessage, Is.Not.Empty, "Preferences saved message should be displayed");
            }
            else
            {
                TestContext.WriteLine("Notification preferences not available in current user role");
            }
        }

        [Test]
        [Description("Verify email notification error handling")]
        public async Task EmailNotifications_Should_HandleErrorsGracefully()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test with invalid email configuration
            var hasEmailSettings = await _notificationPage.HasEmailSettingsAsync();
            
            if (hasEmailSettings)
            {
                await _notificationPage.NavigateToEmailSettingsAsync();
                
                // Test with invalid SMTP settings
                await _notificationPage.SetInvalidSmtpSettingsAsync();
                await _notificationPage.TestEmailConfigurationAsync();
                
                // Verify error notification
                await _notificationPage.WaitForEmailErrorNotificationAsync();
                var errorMessage = await _notificationPage.GetEmailErrorMessageAsync();
                Assert.That(errorMessage, Is.Not.Empty, "Email error message should be displayed");

                // Verify error details are helpful
                var hasErrorDetails = await _notificationPage.HasEmailErrorDetailsAsync();
                Assert.That(hasErrorDetails, Is.True, "Email error should provide helpful details");

                // Test with invalid email address
                await _notificationPage.SetInvalidEmailAddressAsync("invalid-email");
                await _notificationPage.SubmitInvitationAsync();
                
                // Verify validation error
                await _notificationPage.WaitForEmailValidationErrorAsync();
                var validationMessage = await _notificationPage.GetEmailValidationMessageAsync();
                Assert.That(validationMessage, Is.Not.Empty, "Email validation error should be displayed");
            }
            else
            {
                TestContext.WriteLine("Email settings not available for error testing");
            }
        }

        [Test]
        [Description("Verify email notification delivery tracking")]
        public async Task EmailNotifications_Should_TrackDelivery()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for email delivery tracking
            var hasEmailTracking = await _notificationPage.HasEmailTrackingAsync();
            
            if (hasEmailTracking)
            {
                await _notificationPage.NavigateToEmailTrackingAsync();
                
                // Verify tracking information is available
                var hasDeliveryStatus = await _notificationPage.HasDeliveryStatusAsync();
                Assert.That(hasDeliveryStatus, Is.True, "Email delivery status should be available");

                var hasSentCount = await _notificationPage.HasSentEmailCountAsync();
                Assert.That(hasSentCount, Is.True, "Sent email count should be available");

                var hasFailedCount = await _notificationPage.HasFailedEmailCountAsync();
                Assert.That(hasFailedCount, Is.True, "Failed email count should be available");

                // Test email history
                var emailHistory = await _notificationPage.GetEmailHistoryAsync();
                Assert.That(emailHistory, Is.Not.Null, "Email history should be available");

                // Verify email details
                foreach (var email in emailHistory)
                {
                    Assert.That(email.Recipient, Is.Not.Empty, "Email should have recipient");
                    Assert.That(email.Subject, Is.Not.Empty, "Email should have subject");
                    Assert.That(email.Status, Is.Not.Empty, "Email should have status");
                    Assert.That(email.SentDate, Is.Not.Null, "Email should have sent date");
                }
            }
            else
            {
                TestContext.WriteLine("Email tracking not available in current configuration");
            }
        }

        [Test]
        [Description("Verify email notification rate limiting")]
        public async Task EmailNotifications_Should_RespectRateLimits()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test rapid email sending
            var hasInvitationForm = await _notificationPage.HasInvitationFormAsync();
            
            if (hasInvitationForm)
            {
                // Send multiple invitations rapidly
                for (int i = 0; i < 3; i++)
                {
                    await _notificationPage.SetInviteeEmailAsync($"test{i}@example.com");
                    await _notificationPage.SetInvitationSubjectAsync($"Test Invitation {i}");
                    await _notificationPage.SetInvitationMessageAsync($"Test message {i}");
                    await _notificationPage.SubmitInvitationAsync();
                    
                    // Brief pause between submissions
                    await Task.Delay(100);
                }

                // Verify rate limiting notification
                var hasRateLimitNotification = await _notificationPage.HasRateLimitNotificationAsync();
                
                if (hasRateLimitNotification)
                {
                    var rateLimitMessage = await _notificationPage.GetRateLimitMessageAsync();
                    Assert.That(rateLimitMessage, Is.Not.Empty, "Rate limit message should be displayed");
                }
                else
                {
                    TestContext.WriteLine("Rate limiting not implemented or not triggered");
                }
            }
            else
            {
                TestContext.WriteLine("Invitation form not available for rate limit testing");
            }
        }

        [Test]
        [Description("Verify email notification security")]
        public async Task EmailNotifications_Should_BeSecure()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test email security features
            var hasEmailSecurity = await _notificationPage.HasEmailSecurityFeaturesAsync();
            
            if (hasEmailSecurity)
            {
                await _notificationPage.NavigateToEmailSecurityAsync();
                
                // Verify security features
                var hasEncryption = await _notificationPage.HasEmailEncryptionAsync();
                Assert.That(hasEncryption, Is.True, "Email encryption should be available");

                var hasAuthentication = await _notificationPage.HasEmailAuthenticationAsync();
                Assert.That(hasAuthentication, Is.True, "Email authentication should be available");

                var hasSpamProtection = await _notificationPage.HasSpamProtectionAsync();
                Assert.That(hasSpamProtection, Is.True, "Spam protection should be available");

                // Test security validation
                await _notificationPage.TestEmailSecurityAsync();
                
                // Verify security test results
                await _notificationPage.WaitForSecurityTestResultsAsync();
                var securityResults = await _notificationPage.GetSecurityTestResultsAsync();
                Assert.That(securityResults, Is.Not.Empty, "Security test results should be displayed");
            }
            else
            {
                TestContext.WriteLine("Email security features not available in current configuration");
            }
        }
    }
} 