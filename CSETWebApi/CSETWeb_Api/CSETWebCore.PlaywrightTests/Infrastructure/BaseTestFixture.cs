using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Base test fixture providing common setup and teardown for all Playwright tests
    /// </summary>
    [TestFixture]
    public abstract class BaseTestFixture
    {
        protected TestConfiguration Config { get; private set; } = null!;
        protected IBrowser Browser { get; private set; } = null!;
        protected IBrowserContext Context { get; private set; } = null!;
        protected IPage Page { get; private set; } = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            Config = TestConfiguration.Instance;
            
            // Create browser instance
            var playwright = await Playwright.CreateAsync();
            Browser = await CreateBrowserAsync(playwright);
            
            // Perform any one-time setup
            await OneTimeSetUpAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await OneTimeTearDownAsync();
            
            if (Browser != null)
            {
                await Browser.CloseAsync();
            }
        }

        [SetUp]
        public async Task SetUp()
        {
            // Create new context and page for each test
            Context = await CreateContextAsync();
            Page = await Context.NewPageAsync();
            
            // Configure page timeouts
            Page.SetDefaultTimeout(Config.DefaultTimeout);
            Page.SetDefaultNavigationTimeout(Config.NavigationTimeout);
            
            // Perform test-specific setup
            await SetUpAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await TearDownAsync();
            
            // Take screenshot on failure
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed && 
                Config.TakeScreenshotOnFailure)
            {
                await TakeFailureScreenshotAsync();
            }
            
            // Close context
            if (Context != null)
            {
                await Context.CloseAsync();
            }
        }

        /// <summary>
        /// Override for one-time setup logic
        /// </summary>
        protected virtual Task OneTimeSetUpAsync() => Task.CompletedTask;

        /// <summary>
        /// Override for one-time teardown logic
        /// </summary>
        protected virtual Task OneTimeTearDownAsync() => Task.CompletedTask;

        /// <summary>
        /// Override for per-test setup logic
        /// </summary>
        protected virtual Task SetUpAsync() => Task.CompletedTask;

        /// <summary>
        /// Override for per-test teardown logic
        /// </summary>
        protected virtual Task TearDownAsync() => Task.CompletedTask;

        /// <summary>
        /// Create browser instance based on configuration
        /// </summary>
        private async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright)
        {
            var browserType = Config.DefaultBrowser.ToLower() switch
            {
                "firefox" => playwright.Firefox,
                "webkit" => playwright.Webkit,
                "safari" => playwright.Webkit,
                _ => playwright.Chromium
            };

            var options = new BrowserTypeLaunchOptions
            {
                Headless = Config.Headless,
                SlowMo = Config.SlowMo,
                Args = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor" }
            };

            return await browserType.LaunchAsync(options);
        }

        /// <summary>
        /// Create browser context with common configuration
        /// </summary>
        private async Task<IBrowserContext> CreateContextAsync()
        {
            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                RecordVideoDir = Config.RecordVideo ? Config.VideoPath : null,
                RecordVideoSize = Config.RecordVideo ? new RecordVideoSize { Width = 1920, Height = 1080 } : null,
                AcceptDownloads = true,
                IgnoreHTTPSErrors = true
            };

            return await Browser.NewContextAsync(contextOptions);
        }

        /// <summary>
        /// Take screenshot on test failure
        /// </summary>
        private async Task TakeFailureScreenshotAsync()
        {
            try
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var screenshotPath = Path.Combine(Config.ScreenshotPath, $"FAILURE_{testName}_{timestamp}.png");
                
                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
                
                TestContext.WriteLine($"Failure screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to take failure screenshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigate to the application base URL
        /// </summary>
        protected async Task NavigateToBaseAsync()
        {
            await Page.GotoAsync(Config.BaseUrl);
        }

        /// <summary>
        /// Navigate to a specific relative URL
        /// </summary>
        protected async Task NavigateToAsync(string relativePath)
        {
            var fullUrl = $"{Config.BaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
            await Page.GotoAsync(fullUrl);
        }

        /// <summary>
        /// Wait for Angular to be ready (if applicable)
        /// </summary>
        protected async Task WaitForAngularAsync()
        {
            try
            {
                await Page.WaitForFunctionAsync("() => window.getAllAngularTestabilities().findIndex(x => !x.isStable()) === -1");
            }
            catch
            {
                // Angular might not be present, continue
            }
        }

        /// <summary>
        /// Login with test credentials
        /// </summary>
        protected async Task LoginAsync(string? username = null, string? password = null)
        {
            username ??= Config.TestUsername;
            password ??= Config.TestPassword;

            // Navigate to login page
            await NavigateToAsync("/login");
            
            // Fill credentials
            await Page.FillAsync("[data-testid='username'], [name='username'], #username", username);
            await Page.FillAsync("[data-testid='password'], [name='password'], #password", password);
            
            // Submit form
            await Page.ClickAsync("[data-testid='login-button'], [type='submit'], button:has-text('Login')");
            
            // Wait for successful login
            await Page.WaitForURLAsync("**/assessment/**", new PageWaitForURLOptions { Timeout = 10000 });
        }

        /// <summary>
        /// Logout from the application
        /// </summary>
        protected async Task LogoutAsync()
        {
            try
            {
                await Page.ClickAsync("[data-testid='logout'], [href*='logout'], button:has-text('Logout')");
                await Page.WaitForURLAsync("**/login", new PageWaitForURLOptions { Timeout = 5000 });
            }
            catch
            {
                // Might already be logged out
            }
        }

        /// <summary>
        /// Wait for loading spinners to disappear
        /// </summary>
        protected async Task WaitForLoadingAsync()
        {
            // Wait for common loading indicators
            await Page.WaitForSelectorAsync(".loading, .spinner, [data-testid='loading']", new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Hidden, 
                Timeout = 10000 
            });
        }

        /// <summary>
        /// Accept any browser dialogs/alerts
        /// </summary>
        protected void HandleDialogs(bool accept = true)
        {
            Page.Dialog += async (_, dialog) =>
            {
                if (accept)
                    await dialog.AcceptAsync();
                else
                    await dialog.DismissAsync();
            };
        }

        /// <summary>
        /// Check if element exists on page
        /// </summary>
        protected async Task<bool> ElementExistsAsync(string selector)
        {
            try
            {
                return await Page.Locator(selector).CountAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for API response
        /// </summary>
        protected async Task<IResponse> WaitForApiResponseAsync(string urlPattern, int timeout = 10000)
        {
            return await Page.WaitForResponseAsync(response => 
                response.Url.Contains(urlPattern) && response.Status < 400, 
                new PageWaitForResponseOptions { Timeout = timeout });
        }
    }
} 