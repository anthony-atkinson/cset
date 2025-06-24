using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.Authentication
{
    /// <summary>
    /// Test class for login functionality
    /// </summary>
    [TestFixture]
    [Category("Authentication")]
    [Category("Critical")]
    public class LoginTests : BaseTestFixture
    {
        private LoginPage _loginPage = null!;

        protected override async Task SetUpAsync()
        {
            _loginPage = new LoginPage(Page);
            
            // Navigate to login page for each test
            await _loginPage.NavigateAsync();
            
            // Handle privacy warning if present
            await _loginPage.AcceptPrivacyWarningAsync();
        }

        [Test]
        [Description("Verify that login page loads correctly")]
        public async Task LoginPage_Should_LoadCorrectly()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, "Login page should be displayed");
            await _loginPage.WaitForLoginFormAsync();
            
            // Verify all required elements are present
            Assert.That(await _loginPage.IsLoginButtonEnabledAsync(), Is.True, "Login button should be enabled");
        }

        [Test]
        [Description("Verify successful login with valid credentials")]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            // Arrange
            var username = Config.TestUsername;
            var password = Config.TestPassword;

            // Act
            await _loginPage.LoginAsync(username, password);

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.True, "Login should be successful");
        }

        [Test]
        [Description("Verify login fails with invalid credentials")]
        public async Task Login_WithInvalidCredentials_ShouldFail()
        {
            // Arrange
            var invalidUsername = "invalid@example.com";
            var invalidPassword = "wrongpassword";

            // Act
            await _loginPage.LoginAsync(invalidUsername, invalidPassword);

            // Wait a moment for error to appear
            await Task.Delay(2000);

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.False, "Login should fail with invalid credentials");
            
            var errorMessage = await _loginPage.GetErrorMessageAsync();
            Assert.That(string.IsNullOrEmpty(errorMessage), Is.False, "Error message should be displayed");
        }

        [Test]
        [Description("Verify login fails with empty credentials")]
        public async Task Login_WithEmptyCredentials_ShouldFail()
        {
            // Act
            await _loginPage.LoginAsync("", "");

            // Wait a moment
            await Task.Delay(1000);

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.False, "Login should fail with empty credentials");
        }

        [Test]
        [Description("Verify forgot password link functionality")]
        public async Task ForgotPasswordLink_ShouldWork()
        {
            // Act
            await _loginPage.ClickForgotPasswordAsync();

            // Assert - verify navigation or popup (depends on implementation)
            // This will need to be adjusted based on actual CSET implementation
            await Task.Delay(1000);
        }

        [Test]
        [Description("Verify form can be cleared")]
        public async Task LoginForm_ShouldAllowClearing()
        {
            // Arrange
            await _loginPage.LoginAsync("test", "test");

            // Act
            await _loginPage.ClearFormAsync();

            // Assert
            // Verify form is cleared - this would need actual form state checking
        }

        [Test]
        [Description("Verify privacy warning handling")]
        public async Task PrivacyWarning_ShouldBeHandled()
        {
            // This test verifies privacy warning is handled in setup
            // The actual implementation depends on CSET's privacy warning behavior
            
            // Navigate fresh to test privacy warning
            await NavigateToAsync("/login");
            
            if (await _loginPage.IsPrivacyWarningDisplayedAsync())
            {
                await _loginPage.AcceptPrivacyWarningAsync();
            }
            
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, "Login form should be accessible after handling privacy warning");
        }

        [Test, Ignore("Demo test - update with actual CSET access key login if available")]
        [Description("Verify access key login functionality")]
        public async Task Login_WithAccessKey_ShouldSucceed()
        {
            // This test would be for access key login functionality
            // Implementation depends on CSET's access key feature
            
            if (!string.IsNullOrEmpty(Config.TestAccessKey))
            {
                // Implement access key login test
                Assert.Inconclusive("Access key login test not yet implemented");
            }
            else
            {
                Assert.Ignore("No test access key configured");
            }
        }
    }
} 