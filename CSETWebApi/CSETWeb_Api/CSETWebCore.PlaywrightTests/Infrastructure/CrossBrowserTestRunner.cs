using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Test runner configuration for cross-browser testing
    /// </summary>
    public static class CrossBrowserTestRunner
    {
        /// <summary>
        /// Supported browsers for cross-browser testing
        /// </summary>
        public static readonly string[] SupportedBrowsers = { "chromium", "firefox", "webkit" };

        /// <summary>
        /// Browser display names
        /// </summary>
        public static readonly Dictionary<string, string> BrowserDisplayNames = new()
        {
            { "chromium", "Chrome" },
            { "firefox", "Firefox" },
            { "webkit", "Safari" }
        };

        /// <summary>
        /// Get browser-specific test categories
        /// </summary>
        public static string[] GetBrowserTestCategories()
        {
            return SupportedBrowsers.Select(browser => $"Browser-{BrowserDisplayNames[browser]}").ToArray();
        }

        /// <summary>
        /// Check if a browser is supported
        /// </summary>
        public static bool IsBrowserSupported(string browserName)
        {
            return SupportedBrowsers.Contains(browserName.ToLower());
        }

        /// <summary>
        /// Get browser-specific configuration
        /// </summary>
        public static BrowserTestConfig GetBrowserConfig(string browserName)
        {
            return browserName.ToLower() switch
            {
                "chromium" => new BrowserTestConfig
                {
                    BrowserName = "chromium",
                    DisplayName = "Chrome",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    LaunchArgs = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor", "--no-sandbox", "--disable-dev-shm-usage" },
                    TimeoutMultiplier = 1.0,
                    MemoryLimitMB = 100
                },
                "firefox" => new BrowserTestConfig
                {
                    BrowserName = "firefox",
                    DisplayName = "Firefox",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:120.0) Gecko/20100101 Firefox/120.0",
                    LaunchArgs = new[] { "--disable-web-security" },
                    TimeoutMultiplier = 1.2, // Firefox sometimes needs more time
                    MemoryLimitMB = 120
                },
                "webkit" => new BrowserTestConfig
                {
                    BrowserName = "webkit",
                    DisplayName = "Safari",
                    UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Safari/605.1.15",
                    LaunchArgs = new[] { "--disable-web-security" },
                    TimeoutMultiplier = 1.5, // Safari/WebKit often needs more time
                    MemoryLimitMB = 150
                },
                _ => new BrowserTestConfig
                {
                    BrowserName = "chromium",
                    DisplayName = "Chrome",
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    LaunchArgs = new[] { "--disable-web-security", "--disable-features=VizDisplayCompositor" },
                    TimeoutMultiplier = 1.0,
                    MemoryLimitMB = 100
                }
            };
        }

        /// <summary>
        /// Get browser-specific test filters
        /// </summary>
        public static string GetBrowserTestFilter(string browserName)
        {
            var displayName = BrowserDisplayNames.GetValueOrDefault(browserName.ToLower(), "Chrome");
            return $"TestCategory=CrossBrowser && TestCategory=Browser-{displayName}";
        }

        /// <summary>
        /// Get all browser test filters
        /// </summary>
        public static string[] GetAllBrowserTestFilters()
        {
            return SupportedBrowsers.Select(GetBrowserTestFilter).ToArray();
        }

        /// <summary>
        /// Check if test should run in specific browser
        /// </summary>
        public static bool ShouldRunInBrowser(ITest test, string browserName)
        {
            var testCategories = test.Properties["Category"]?.Cast<string>() ?? Array.Empty<string>();
            var displayName = BrowserDisplayNames.GetValueOrDefault(browserName.ToLower(), "Chrome");
            return testCategories.Contains("CrossBrowser") && testCategories.Contains($"Browser-{displayName}");
        }

        /// <summary>
        /// Get browser-specific test results summary
        /// </summary>
        public static BrowserTestResults GetBrowserTestResults(ITestResult[] results, string browserName)
        {
            var browserResults = results.Where(r => ShouldRunInBrowser(r.Test, browserName)).ToArray();
            
            return new BrowserTestResults
            {
                BrowserName = browserName,
                DisplayName = BrowserDisplayNames.GetValueOrDefault(browserName.ToLower(), "Chrome"),
                TotalTests = browserResults.Length,
                PassedTests = browserResults.Count(r => r.ResultState == ResultState.Success),
                FailedTests = browserResults.Count(r => r.ResultState == ResultState.Failure),
                SkippedTests = browserResults.Count(r => r.ResultState == ResultState.Skipped),
                InconclusiveTests = browserResults.Count(r => r.ResultState == ResultState.Inconclusive),
                TotalDuration = browserResults.Sum(r => r.Duration.TotalSeconds)
            };
        }

        /// <summary>
        /// Get cross-browser compatibility report
        /// </summary>
        public static CrossBrowserCompatibilityReport GetCompatibilityReport(ITestResult[] results)
        {
            var browserResults = SupportedBrowsers.Select(browser => GetBrowserTestResults(results, browser)).ToArray();
            
            return new CrossBrowserCompatibilityReport
            {
                BrowserResults = browserResults,
                OverallCompatibility = CalculateOverallCompatibility(browserResults),
                GeneratedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Calculate overall compatibility score
        /// </summary>
        private static double CalculateOverallCompatibility(BrowserTestResults[] browserResults)
        {
            if (!browserResults.Any()) return 0.0;

            var totalTests = browserResults.Sum(r => r.TotalTests);
            var totalPassed = browserResults.Sum(r => r.PassedTests);

            return totalTests > 0 ? (double)totalPassed / totalTests * 100.0 : 0.0;
        }
    }

    /// <summary>
    /// Browser-specific test configuration
    /// </summary>
    public class BrowserTestConfig
    {
        public string BrowserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string[] LaunchArgs { get; set; } = Array.Empty<string>();
        public double TimeoutMultiplier { get; set; } = 1.0;
        public int MemoryLimitMB { get; set; } = 100;
    }

    /// <summary>
    /// Browser-specific test results
    /// </summary>
    public class BrowserTestResults
    {
        public string BrowserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalTests { get; set; }
        public int PassedTests { get; set; }
        public int FailedTests { get; set; }
        public int SkippedTests { get; set; }
        public int InconclusiveTests { get; set; }
        public double TotalDuration { get; set; }

        public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100.0 : 0.0;
        public double AverageDuration => TotalTests > 0 ? TotalDuration / TotalTests : 0.0;
    }

    /// <summary>
    /// Cross-browser compatibility report
    /// </summary>
    public class CrossBrowserCompatibilityReport
    {
        public BrowserTestResults[] BrowserResults { get; set; } = Array.Empty<BrowserTestResults>();
        public double OverallCompatibility { get; set; }
        public DateTime GeneratedAt { get; set; }

        public string GetSummary()
        {
            var summary = $"Cross-Browser Compatibility Report ({GeneratedAt:yyyy-MM-dd HH:mm:ss})\n";
            summary += $"Overall Compatibility: {OverallCompatibility:F1}%\n\n";

            foreach (var result in BrowserResults)
            {
                summary += $"{result.DisplayName}:\n";
                summary += $"  Pass Rate: {result.PassRate:F1}% ({result.PassedTests}/{result.TotalTests})\n";
                summary += $"  Average Duration: {result.AverageDuration:F2}s\n";
                summary += $"  Failed: {result.FailedTests}, Skipped: {result.SkippedTests}\n\n";
            }

            return summary;
        }
    }
} 