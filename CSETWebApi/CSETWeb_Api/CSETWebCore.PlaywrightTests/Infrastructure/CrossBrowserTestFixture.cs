using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Base test fixture for cross-browser testing with Playwright
    /// </summary>
    [TestFixture]
    public abstract class CrossBrowserTestFixture : BaseTestFixture
    {
        protected IBrowserType BrowserType { get; private set; } = null!;
        protected string BrowserName { get; private set; } = string.Empty;

        [OneTimeSetUp]
        public async Task CrossBrowserOneTimeSetUp()
        {
            Config = TestConfiguration.Instance;
            
            // Create browser instance based on test parameters
            var playwright = await Playwright.CreateAsync();
            BrowserType = GetBrowserType(playwright);
            BrowserName = GetBrowserName();
            
            // Create browser instance
            Browser = await CreateBrowserAsync(playwright, BrowserType);
            
            // Perform any one-time setup
            await OneTimeSetUpAsync();
        }

        /// <summary>
        /// Get browser type based on test parameters or configuration
        /// </summary>
        private IBrowserType GetBrowserType(IPlaywright playwright)
        {
            // Check if browser is specified via TestFixture attribute
            var browserName = TestContext.CurrentContext.Test.Properties.Get("Browser") as string;
            
            if (string.IsNullOrEmpty(browserName))
            {
                browserName = Config.DefaultBrowser;
            }

            return browserName.ToLower() switch
            {
                "firefox" => playwright.Firefox,
                "webkit" => playwright.Webkit,
                "safari" => playwright.Webkit,
                "chrome" => playwright.Chromium,
                "chromium" => playwright.Chromium,
                _ => playwright.Chromium
            };
        }

        /// <summary>
        /// Get browser name for reporting
        /// </summary>
        private string GetBrowserName()
        {
            var browserName = TestContext.CurrentContext.Test.Properties.Get("Browser") as string;
            
            if (string.IsNullOrEmpty(browserName))
            {
                browserName = Config.DefaultBrowser;
            }

            return browserName.ToLower() switch
            {
                "firefox" => "Firefox",
                "webkit" => "Safari",
                "safari" => "Safari",
                "chrome" => "Chrome",
                "chromium" => "Chromium",
                _ => "Chromium"
            };
        }

        /// <summary>
        /// Create browser instance with cross-browser specific options
        /// </summary>
        private async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, IBrowserType browserType)
        {
            var options = new BrowserTypeLaunchOptions
            {
                Headless = Config.Headless,
                SlowMo = Config.SlowMo,
                Args = GetBrowserSpecificArgs(browserType)
            };

            return await browserType.LaunchAsync(options);
        }

        /// <summary>
        /// Get browser-specific launch arguments
        /// </summary>
        private string[] GetBrowserSpecificArgs(IBrowserType browserType)
        {
            var baseArgs = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor" };

            if (browserType == BrowserType.Chromium)
            {
                return new[]
                {
                    "--disable-web-security",
                    "--disable-features=VizDisplayCompositor",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-gpu",
                    "--disable-extensions"
                };
            }

            return baseArgs;
        }

        /// <summary>
        /// Create browser context with cross-browser specific configuration
        /// </summary>
        protected override async Task<IBrowserContext> CreateContextAsync()
        {
            var contextOptions = new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                RecordVideoDir = Config.RecordVideo ? Config.VideoPath : null,
                RecordVideoSize = Config.RecordVideo ? new RecordVideoSize { Width = 1920, Height = 1080 } : null,
                AcceptDownloads = true,
                IgnoreHTTPSErrors = true,
                UserAgent = GetBrowserSpecificUserAgent()
            };

            return await Browser.NewContextAsync(contextOptions);
        }

        /// <summary>
        /// Get browser-specific user agent
        /// </summary>
        private string GetBrowserSpecificUserAgent()
        {
            return BrowserName.ToLower() switch
            {
                "firefox" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
                "safari" => "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/605.1.15",
                "chrome" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                "chromium" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                _ => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            };
        }

        /// <summary>
        /// Take screenshot with browser-specific naming
        /// </summary>
        protected async Task TakeScreenshotAsync(string testName, string suffix = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var browserSuffix = BrowserName.ToLower();
                var screenshotPath = Path.Combine(
                    Config.ScreenshotPath, 
                    $"{testName}_{browserSuffix}_{timestamp}{suffix}.png"
                );
                
                Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
                
                TestContext.WriteLine($"Screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }

        /// <summary>
        /// Wait for browser-specific loading conditions
        /// </summary>
        protected async Task WaitForBrowserSpecificLoadingAsync()
        {
            // Wait for Angular to be ready
            await WaitForAngularAsync();
            
            // Browser-specific additional waits
            switch (BrowserName.ToLower())
            {
                case "firefox":
                    // Firefox sometimes needs extra time for rendering
                    await Task.Delay(500);
                    break;
                case "safari":
                    // Safari may need extra time for WebKit rendering
                    await Task.Delay(1000);
                    break;
                default:
                    // Chrome/Chromium default behavior
                    break;
            }
        }

        /// <summary>
        /// Get browser-specific test category
        /// </summary>
        protected string GetBrowserTestCategory()
        {
            return $"Browser-{BrowserName}";
        }

        /// <summary>
        /// Check if current browser supports specific feature
        /// </summary>
        protected bool IsFeatureSupported(string feature)
        {
            return feature.ToLower() switch
            {
                "webgl" => BrowserName.ToLower() != "firefox", // Firefox has limited WebGL support
                "serviceworkers" => BrowserName.ToLower() != "safari", // Safari has limited Service Worker support
                "webassembly" => true, // All modern browsers support WebAssembly
                "es6modules" => true, // All modern browsers support ES6 modules
                _ => true
            };
        }

        /// <summary>
        /// Execute browser-specific test setup
        /// </summary>
        protected virtual async Task BrowserSpecificSetupAsync()
        {
            // Override in derived classes for browser-specific setup
            await Task.CompletedTask;
        }

        /// <summary>
        /// Execute browser-specific test cleanup
        /// </summary>
        protected virtual async Task BrowserSpecificCleanupAsync()
        {
            // Override in derived classes for browser-specific cleanup
            await Task.CompletedTask;
        }

        protected override async Task SetUpAsync()
        {
            // Create new context and page for each test
            Context = await CreateContextAsync();
            Page = await Context.NewPageAsync();
            
            // Configure page timeouts
            Page.SetDefaultTimeout(Config.DefaultTimeout);
            Page.SetDefaultNavigationTimeout(Config.NavigationTimeout);
            
            // Browser-specific setup
            await BrowserSpecificSetupAsync();
            
            // Perform test-specific setup
            await SetUpAsync();
        }

        protected override async Task TearDownAsync()
        {
            await TearDownAsync();
            
            // Browser-specific cleanup
            await BrowserSpecificCleanupAsync();
            
            // Take screenshot on failure with browser-specific naming
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed && 
                Config.TakeScreenshotOnFailure)
            {
                var testName = TestContext.CurrentContext.Test.Name;
                await TakeScreenshotAsync(testName, "_FAILURE");
            }
            
            // Close context
            if (Context != null)
            {
                await Context.CloseAsync();
            }
        }
    }
} 