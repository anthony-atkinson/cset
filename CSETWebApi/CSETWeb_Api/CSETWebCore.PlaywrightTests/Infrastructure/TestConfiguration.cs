using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Centralized configuration management for Playwright tests
    /// </summary>
    public class TestConfiguration
    {
        private static readonly Lazy<TestConfiguration> _instance = new(() => new TestConfiguration());
        public static TestConfiguration Instance => _instance.Value;

        private readonly IConfiguration _configuration;

        private TestConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("testsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"testsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        // Base URLs
        public string BaseUrl => _configuration["BaseUrl"] ?? "http://localhost:4200";
        public string ApiUrl => _configuration["ApiUrl"] ?? "http://localhost:5000";

        // Browser Configuration
        public bool Headless => bool.Parse(_configuration["Browser:Headless"] ?? "true");
        public string DefaultBrowser => _configuration["Browser:Default"] ?? "chromium";
        public int SlowMo => int.Parse(_configuration["Browser:SlowMo"] ?? "0");
        public string VideoPath => _configuration["Browser:VideoPath"] ?? "test-results/videos";
        public string ScreenshotPath => _configuration["Browser:ScreenshotPath"] ?? "test-results/screenshots";

        // Cross-Browser Configuration
        public bool CrossBrowserEnabled => bool.Parse(_configuration["CrossBrowser:Enabled"] ?? "false");
        public string[] CrossBrowserBrowsers => _configuration.GetSection("CrossBrowser:Browsers").Get<string[]>() ?? new[] { "chromium" };
        public bool CrossBrowserParallelExecution => bool.Parse(_configuration["CrossBrowser:ParallelExecution"] ?? "false");
        public bool CrossBrowserScreenshotOnFailure => bool.Parse(_configuration["CrossBrowser:ScreenshotOnFailure"] ?? "true");
        public bool CrossBrowserVideoRecording => bool.Parse(_configuration["CrossBrowser:VideoRecording"] ?? "false");
        public bool CrossBrowserPerformanceMonitoring => bool.Parse(_configuration["CrossBrowser:PerformanceMonitoring"] ?? "true");
        public bool CrossBrowserMemoryMonitoring => bool.Parse(_configuration["CrossBrowser:MemoryMonitoring"] ?? "true");
        public bool CrossBrowserAccessibilityTesting => bool.Parse(_configuration["CrossBrowser:AccessibilityTesting"] ?? "false");

        // Test Settings
        public int DefaultTimeout => int.Parse(_configuration["Test:DefaultTimeout"] ?? "30000");
        public int NavigationTimeout => int.Parse(_configuration["Test:NavigationTimeout"] ?? "30000");
        public bool TakeScreenshotOnFailure => bool.Parse(_configuration["Test:TakeScreenshotOnFailure"] ?? "true");
        public bool RecordVideo => bool.Parse(_configuration["Test:RecordVideo"] ?? "false");

        // Test User Credentials
        public string TestUsername => _configuration["TestCredentials:Username"] ?? "test@example.com";
        public string TestPassword => _configuration["TestCredentials:Password"] ?? "TestPassword123!";
        public string TestAccessKey => _configuration["TestCredentials:AccessKey"] ?? "";

        // Database
        public string DatabaseConnectionString => _configuration["Database:ConnectionString"] ?? "";
        public bool ResetDatabaseBeforeTests => bool.Parse(_configuration["Database:ResetBeforeTests"] ?? "false");

        // Feature Flags
        public bool EnableSecurityTests => bool.Parse(_configuration["Features:EnableSecurityTests"] ?? "true");
        public bool EnablePerformanceTests => bool.Parse(_configuration["Features:EnablePerformanceTests"] ?? "false");
        public bool EnableAccessibilityTests => bool.Parse(_configuration["Features:EnableAccessibilityTests"] ?? "false");
        public bool EnableCrossBrowserTests => bool.Parse(_configuration["Features:EnableCrossBrowserTests"] ?? "true");

        // Environment Specific Settings
        public bool IsCI => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        public bool IsLocalDevelopment => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        /// <summary>
        /// Get browser-specific timeout based on configuration
        /// </summary>
        public int GetBrowserTimeout(string browserName)
        {
            var baseTimeout = DefaultTimeout;
            var config = CrossBrowserTestRunner.GetBrowserConfig(browserName);
            return (int)(baseTimeout * config.TimeoutMultiplier);
        }

        /// <summary>
        /// Get browser-specific memory limit
        /// </summary>
        public int GetBrowserMemoryLimit(string browserName)
        {
            var config = CrossBrowserTestRunner.GetBrowserConfig(browserName);
            return config.MemoryLimitMB;
        }

        /// <summary>
        /// Check if cross-browser testing is enabled and browser is supported
        /// </summary>
        public bool ShouldRunCrossBrowserTest(string browserName)
        {
            return CrossBrowserEnabled && 
                   EnableCrossBrowserTests && 
                   CrossBrowserTestRunner.IsBrowserSupported(browserName) &&
                   CrossBrowserBrowsers.Contains(browserName.ToLower());
        }
    }
} 