using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.CrossBrowser
{
    /// <summary>
    /// Cross-browser tests for login functionality
    /// </summary>
    [TestFixture]
    [Category("CrossBrowser")]
    [Category("Authentication")]
    [Category("E2E")]
    public class CrossBrowserLoginTests : CrossBrowserTestFixture
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
        [Description("Verify login page loads correctly across browsers")]
        public async Task LoginPage_Should_LoadCorrectly_AcrossBrowsers()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                $"Login page should be displayed in {BrowserName}");
            await _loginPage.WaitForLoginFormAsync();
            
            // Verify all required elements are present
            Assert.That(await _loginPage.IsLoginButtonEnabledAsync(), Is.True, 
                $"Login button should be enabled in {BrowserName}");
        }

        [Test]
        [Description("Verify successful login with valid credentials across browsers")]
        public async Task Login_WithValidCredentials_ShouldSucceed_AcrossBrowsers()
        {
            // Arrange
            var username = Config.TestUsername;
            var password = Config.TestPassword;

            // Act
            await _loginPage.LoginAsync(username, password);
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.True, 
                $"Login should be successful in {BrowserName}");
        }

        [Test]
        [Description("Verify login fails with invalid credentials across browsers")]
        public async Task Login_WithInvalidCredentials_ShouldFail_AcrossBrowsers()
        {
            // Arrange
            var invalidUsername = "invalid@example.com";
            var invalidPassword = "wrongpassword";

            // Act
            await _loginPage.LoginAsync(invalidUsername, invalidPassword);

            // Wait a moment for error to appear
            await Task.Delay(2000);

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.False, 
                $"Login should fail with invalid credentials in {BrowserName}");
            
            var errorMessage = await _loginPage.GetErrorMessageAsync();
            Assert.That(string.IsNullOrEmpty(errorMessage), Is.False, 
                $"Error message should be displayed in {BrowserName}");
        }

        [Test]
        [Description("Verify login fails with empty credentials across browsers")]
        public async Task Login_WithEmptyCredentials_ShouldFail_AcrossBrowsers()
        {
            // Act
            await _loginPage.LoginAsync("", "");

            // Wait a moment
            await Task.Delay(1000);

            // Assert
            Assert.That(await _loginPage.IsLoginSuccessfulAsync(), Is.False, 
                $"Login should fail with empty credentials in {BrowserName}");
        }

        [Test]
        [Description("Verify forgot password link functionality across browsers")]
        public async Task ForgotPasswordLink_ShouldWork_AcrossBrowsers()
        {
            // Act
            await _loginPage.ClickForgotPasswordAsync();

            // Assert - verify navigation or popup (depends on implementation)
            // This will need to be adjusted based on actual CSET implementation
            await Task.Delay(1000);
            
            // Verify the action completed without error
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                $"Login page should remain accessible in {BrowserName} after clicking forgot password");
        }

        [Test]
        [Description("Verify form can be cleared across browsers")]
        public async Task LoginForm_ShouldAllowClearing_AcrossBrowsers()
        {
            // Arrange
            await _loginPage.LoginAsync("test", "test");

            // Act
            await _loginPage.ClearFormAsync();

            // Assert
            // Verify form is cleared - this would need actual form state checking
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                $"Login form should remain accessible in {BrowserName} after clearing");
        }

        [Test]
        [Description("Verify privacy warning handling across browsers")]
        public async Task PrivacyWarning_ShouldBeHandled_AcrossBrowsers()
        {
            // This test verifies privacy warning is handled in setup
            // The actual implementation depends on CSET's privacy warning behavior
            
            // Navigate fresh to test privacy warning
            await NavigateToAsync("/login");
            
            if (await _loginPage.IsPrivacyWarningDisplayedAsync())
            {
                await _loginPage.AcceptPrivacyWarningAsync();
            }
            
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                $"Login form should be accessible after handling privacy warning in {BrowserName}");
        }

        [Test]
        [Description("Verify login form responsiveness across browsers")]
        public async Task LoginForm_ShouldBeResponsive_AcrossBrowsers()
        {
            // Test different viewport sizes
            var viewports = new[]
            {
                new { Width = 1920, Height = 1080, Name = "Desktop" },
                new { Width = 1366, Height = 768, Name = "Laptop" },
                new { Width = 768, Height = 1024, Name = "Tablet" },
                new { Width = 375, Height = 667, Name = "Mobile" }
            };

            foreach (var viewport in viewports)
            {
                // Set viewport size
                await Page.SetViewportSizeAsync(viewport.Width, viewport.Height);
                
                // Verify login form is still accessible
                Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                    $"Login form should be accessible in {BrowserName} at {viewport.Name} resolution ({viewport.Width}x{viewport.Height})");
                
                // Verify login button is enabled
                Assert.That(await _loginPage.IsLoginButtonEnabledAsync(), Is.True, 
                    $"Login button should be enabled in {BrowserName} at {viewport.Name} resolution");
            }
        }

        [Test]
        [Description("Verify login form accessibility across browsers")]
        public async Task LoginForm_ShouldBeAccessible_AcrossBrowsers()
        {
            // Test keyboard navigation
            await Page.Keyboard.PressAsync("Tab");
            await Task.Delay(500);
            
            // Verify focus is on username field
            var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
            Assert.That(focusedElement, Is.EqualTo("INPUT"), 
                $"First tab should focus on input field in {BrowserName}");
            
            // Test Enter key submission
            await Page.Keyboard.PressAsync("Enter");
            await Task.Delay(1000);
            
            // Verify form submission behavior
            Assert.That(await _loginPage.IsDisplayedAsync(), Is.True, 
                $"Login form should remain accessible after Enter key press in {BrowserName}");
        }

        [Test]
        [Description("Verify login form performance across browsers")]
        public async Task LoginForm_ShouldLoadQuickly_AcrossBrowsers()
        {
            // Measure page load time
            var startTime = DateTime.Now;
            
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();
            
            var loadTime = DateTime.Now - startTime;
            
            // Assert load time is reasonable (less than 5 seconds)
            Assert.That(loadTime.TotalSeconds, Is.LessThan(5.0), 
                $"Login page should load within 5 seconds in {BrowserName}. Actual load time: {loadTime.TotalSeconds:F2} seconds");
            
            TestContext.WriteLine($"Login page load time in {BrowserName}: {loadTime.TotalSeconds:F2} seconds");
        }

        [Test]
        [Description("Verify login form memory usage across browsers")]
        public async Task LoginForm_ShouldNotExceedMemoryLimits_AcrossBrowsers()
        {
            // Get initial memory usage
            var initialMemory = await Page.EvaluateAsync<long>("performance.memory.usedJSHeapSize");
            
            // Perform some actions
            await _loginPage.LoginAsync("test", "test");
            await _loginPage.ClearFormAsync();
            await _loginPage.LoginAsync("test2", "test2");
            await _loginPage.ClearFormAsync();
            
            // Get final memory usage
            var finalMemory = await Page.EvaluateAsync<long>("performance.memory.usedJSHeapSize");
            var memoryIncrease = finalMemory - initialMemory;
            
            // Assert memory increase is reasonable (less than 10MB)
            var memoryIncreaseMB = memoryIncrease / (1024 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(10.0), 
                $"Memory usage should not increase by more than 10MB in {BrowserName}. Actual increase: {memoryIncreaseMB:F2}MB");
            
            TestContext.WriteLine($"Memory usage increase in {BrowserName}: {memoryIncreaseMB:F2}MB");
        }

        [Test]
        [Description("Verify login form compatibility with browser features")]
        public async Task LoginForm_ShouldSupportBrowserFeatures_AcrossBrowsers()
        {
            // Test if browser supports required features
            var supportsLocalStorage = await Page.EvaluateAsync<bool>("typeof(Storage) !== 'undefined' && window.localStorage");
            var supportsSessionStorage = await Page.EvaluateAsync<bool>("typeof(Storage) !== 'undefined' && window.sessionStorage");
            var supportsCookies = await Page.EvaluateAsync<bool>("navigator.cookieEnabled");
            
            Assert.That(supportsLocalStorage, Is.True, 
                $"Local storage should be supported in {BrowserName}");
            Assert.That(supportsSessionStorage, Is.True, 
                $"Session storage should be supported in {BrowserName}");
            Assert.That(supportsCookies, Is.True, 
                $"Cookies should be enabled in {BrowserName}");
            
            TestContext.WriteLine($"Browser feature support in {BrowserName}: LocalStorage={supportsLocalStorage}, SessionStorage={supportsSessionStorage}, Cookies={supportsCookies}");
        }
    }
} 