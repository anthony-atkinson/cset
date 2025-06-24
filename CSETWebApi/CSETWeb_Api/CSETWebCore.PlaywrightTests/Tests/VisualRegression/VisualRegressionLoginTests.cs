using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.VisualRegression
{
    /// <summary>
    /// Visual regression tests for login page
    /// </summary>
    [TestFixture]
    [Category("VisualRegression")]
    [Category("Authentication")]
    [Category("E2E")]
    public class VisualRegressionLoginTests : VisualRegressionTestFixture
    {
        private LoginPage _loginPage = null!;

        protected override async Task SetUpAsync()
        {
            _loginPage = new LoginPage(Page);
            
            // Navigate to login page for each test
            await _loginPage.NavigateAsync();
            
            // Handle privacy warning if present
            await _loginPage.AcceptPrivacyWarningAsync();
            
            // Wait for visual stability
            await WaitForVisualStabilityAsync();
        }

        [Test]
        [Description("Verify login page visual consistency")]
        public async Task LoginPage_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_page_default", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login form visual consistency")]
        public async Task LoginForm_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_form", "form", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login form should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login button visual consistency")]
        public async Task LoginButton_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_button", "button[type='submit']", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login button should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with filled form visual consistency")]
        public async Task LoginPage_WithFilledForm_Should_MatchBaseline()
        {
            // Arrange
            await _loginPage.LoginAsync("test@example.com", "password123");

            // Wait for visual stability after form filling
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_filled", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with filled form should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with error message visual consistency")]
        public async Task LoginPage_WithErrorMessage_Should_MatchBaseline()
        {
            // Arrange
            await _loginPage.LoginAsync("invalid@example.com", "wrongpassword");
            
            // Wait for error message to appear
            await Task.Delay(2000);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_error", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with error message should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page responsive design at desktop resolution")]
        public async Task LoginPage_DesktopResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(1920, 1080);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_desktop", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page at desktop resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page responsive design at tablet resolution")]
        public async Task LoginPage_TabletResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(768, 1024);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_tablet", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page at tablet resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page responsive design at mobile resolution")]
        public async Task LoginPage_MobileResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(375, 667);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_mobile", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page at mobile resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page full page screenshot consistency")]
        public async Task LoginPage_FullPage_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_page_fullpage", "body", true);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page full page should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page header visual consistency")]
        public async Task LoginPage_Header_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_page_header", "header", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page header should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page footer visual consistency")]
        public async Task LoginPage_Footer_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_page_footer", "footer", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page footer should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page input fields visual consistency")]
        public async Task LoginPage_InputFields_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("login_page_inputs", "input", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page input fields should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with focus on username field")]
        public async Task LoginPage_UsernameFocused_Should_MatchBaseline()
        {
            // Arrange
            await Page.FocusAsync("input[name='username']");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_username_focused", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with username focused should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with focus on password field")]
        public async Task LoginPage_PasswordFocused_Should_MatchBaseline()
        {
            // Arrange
            await Page.FocusAsync("input[name='password']");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_password_focused", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with password focused should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with hover on login button")]
        public async Task LoginPage_ButtonHover_Should_MatchBaseline()
        {
            // Arrange
            await Page.HoverAsync("button[type='submit']");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_button_hover", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with button hover should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page accessibility focus indicators")]
        public async Task LoginPage_AccessibilityFocus_Should_MatchBaseline()
        {
            // Arrange
            await Page.Keyboard.PressAsync("Tab");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_accessibility_focus", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with accessibility focus should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with disabled state")]
        public async Task LoginPage_DisabledState_Should_MatchBaseline()
        {
            // Arrange
            // Disable the form (this would depend on actual implementation)
            await Page.EvaluateAsync("document.querySelector('form').disabled = true");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_disabled", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page in disabled state should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page loading state")]
        public async Task LoginPage_LoadingState_Should_MatchBaseline()
        {
            // Arrange
            // Simulate loading state (this would depend on actual implementation)
            await Page.EvaluateAsync("document.querySelector('button[type=\"submit\"]').disabled = true");
            await Page.EvaluateAsync("document.querySelector('button[type=\"submit\"]').textContent = 'Loading...'");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_loading", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page in loading state should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with validation errors")]
        public async Task LoginPage_ValidationErrors_Should_MatchBaseline()
        {
            // Arrange
            // Trigger validation errors
            await Page.ClickAsync("button[type='submit']");
            await Task.Delay(1000);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("login_page_validation", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with validation errors should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify login page with custom threshold")]
        public async Task LoginPage_CustomThreshold_Should_MatchBaseline()
        {
            // Arrange
            SetVisualThreshold(0.05); // 5% threshold

            // Act
            var result = await CompareScreenshotAsync("login_page_custom_threshold", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Login page with custom threshold should match baseline in {BrowserName}. {result.GetSummary()}");
        }
    }
} 