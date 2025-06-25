using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.Performance
{
    [TestFixture]
    public class PerformanceMonitoringTests : PlaywrightTestBase
    {
        [Test]
        [Description("Verify application performance metrics collection")]
        public async Task PerformanceMetrics_Should_BeCollected()
        {
            // Navigate to dashboard
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/dashboard");

            // Verify performance monitoring is active
            await Expect(Page.Locator(".performance-monitor")).ToBeVisibleAsync();

            // Perform actions to generate metrics
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Performance Test Assessment");
            await Page.ClickAsync("#save-assessment-btn");

            // Wait for metrics to be collected
            await Page.WaitForTimeoutAsync(2000);

            // Verify performance metrics are displayed
            await Expect(Page.Locator(".performance-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".response-time")).ToBeVisibleAsync();
            await Expect(Page.Locator(".memory-usage")).ToBeVisibleAsync();
            await Expect(Page.Locator(".cpu-usage")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify page load performance monitoring")]
        public async Task PageLoadPerformance_Should_BeMonitored()
        {
            // Start performance monitoring
            await Page.RouteAsync("**/*", async route =>
            {
                await route.ContinueAsync();
            });

            // Navigate to assessment page and measure load time
            var startTime = DateTime.UtcNow;
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            var loadTime = DateTime.UtcNow - startTime;

            // Verify load time is within acceptable limits
            Assert.That(loadTime.TotalSeconds, Is.LessThan(5.0), "Page load time should be under 5 seconds");

            // Verify performance metrics are captured
            await Expect(Page.Locator(".page-load-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".load-time-indicator")).ToContainTextAsync("Load Time:");

            // Navigate to questions page and measure load time
            startTime = DateTime.UtcNow;
            await Page.GotoAsync($"{BaseUrl}/assessment/123/questions");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            loadTime = DateTime.UtcNow - startTime;

            // Verify load time is within acceptable limits
            Assert.That(loadTime.TotalSeconds, Is.LessThan(3.0), "Questions page load time should be under 3 seconds");
        }

        [Test]
        [Description("Verify API response time monitoring")]
        public async Task APIResponseTime_Should_BeMonitored()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/assessment");

            // Monitor API calls
            var apiCalls = new List<DateTime>();

            await Page.RouteAsync("**/api/**", async route =>
            {
                apiCalls.Add(DateTime.UtcNow);
                await route.ContinueAsync();
            });

            // Perform API-intensive operations
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "API Performance Test");
            await Page.ClickAsync("#save-assessment-btn");

            await Page.WaitForTimeoutAsync(2000);

            // Verify API performance metrics
            await Expect(Page.Locator(".api-performance-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".api-response-time")).ToBeVisibleAsync();
            await Expect(Page.Locator(".api-call-count")).ToBeVisibleAsync();

            // Verify response times are within limits
            var responseTimeElement = await Page.Locator(".api-response-time").TextContentAsync();
            var responseTime = double.Parse(responseTimeElement.Replace("ms", ""));
            Assert.That(responseTime, Is.LessThan(1000), "API response time should be under 1000ms");
        }

        [Test]
        [Description("Verify memory usage monitoring")]
        public async Task MemoryUsage_Should_BeMonitored()
        {
            // Navigate to dashboard
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/dashboard");

            // Verify memory monitoring is active
            await Expect(Page.Locator(".memory-monitor")).ToBeVisibleAsync();

            // Perform memory-intensive operations
            for (int i = 0; i < 10; i++)
            {
                await Page.ClickAsync("#create-assessment-btn");
                await Page.FillAsync("#assessment-name", $"Memory Test {i}");
                await Page.ClickAsync("#save-assessment-btn");
                await Page.WaitForTimeoutAsync(500);
            }

            // Verify memory usage metrics
            await Expect(Page.Locator(".memory-usage-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".memory-usage-indicator")).ToBeVisibleAsync();

            // Verify memory usage is within limits
            var memoryUsageElement = await Page.Locator(".memory-usage-indicator").TextContentAsync();
            var memoryUsage = double.Parse(memoryUsageElement.Replace("MB", ""));
            Assert.That(memoryUsage, Is.LessThan(500), "Memory usage should be under 500MB");
        }

        [Test]
        [Description("Verify CPU usage monitoring")]
        public async Task CPUUsage_Should_BeMonitored()
        {
            // Navigate to dashboard
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/dashboard");

            // Verify CPU monitoring is active
            await Expect(Page.Locator(".cpu-monitor")).ToBeVisibleAsync();

            // Perform CPU-intensive operations
            for (int i = 0; i < 5; i++)
            {
                await Page.ClickAsync("#generate-report-btn");
                await Page.WaitForTimeoutAsync(1000);
            }

            // Verify CPU usage metrics
            await Expect(Page.Locator(".cpu-usage-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".cpu-usage-indicator")).ToBeVisibleAsync();

            // Verify CPU usage is within limits
            var cpuUsageElement = await Page.Locator(".cpu-usage-indicator").TextContentAsync();
            var cpuUsage = double.Parse(cpuUsageElement.Replace("%", ""));
            Assert.That(cpuUsage, Is.LessThan(80), "CPU usage should be under 80%");
        }

        [Test]
        [Description("Verify performance alerts and thresholds")]
        public async Task PerformanceAlerts_Should_BeTriggered()
        {
            // Navigate to dashboard
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "testuser");
            await Page.FillAsync("#password", "TestPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/dashboard");

            // Simulate slow performance
            await Page.RouteAsync("**/api/**", async route =>
            {
                await Task.Delay(3000); // Simulate 3-second delay
                await route.ContinueAsync();
            });

            // Perform operation that triggers performance alert
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Slow Performance Test");
            await Page.ClickAsync("#save-assessment-btn");

            // Wait for performance alert
            await Page.WaitForTimeoutAsync(5000);

            // Verify performance alert is triggered
            await Expect(Page.Locator(".performance-alert")).ToBeVisibleAsync();
            await Expect(Page.Locator(".performance-alert")).ToContainTextAsync("Performance degradation detected");

            // Verify alert details
            await Expect(Page.Locator(".alert-details")).ToBeVisibleAsync();
            await Expect(Page.Locator(".alert-details")).ToContainTextAsync("Response time exceeded threshold");
        }

        [Test]
        [Description("Verify performance trend analysis")]
        public async Task PerformanceTrends_Should_BeAnalyzed()
        {
            // Navigate to performance dashboard
            await Page.GotoAsync($"{BaseUrl}/admin/performance");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/admin/performance");

            // Verify performance trends are displayed
            await Expect(Page.Locator(".performance-trends")).ToBeVisibleAsync();
            await Expect(Page.Locator(".trend-chart")).ToBeVisibleAsync();

            // Verify trend metrics
            await Expect(Page.Locator(".response-time-trend")).ToBeVisibleAsync();
            await Expect(Page.Locator(".memory-usage-trend")).ToBeVisibleAsync();
            await Expect(Page.Locator(".cpu-usage-trend")).ToBeVisibleAsync();

            // Verify trend analysis
            await Expect(Page.Locator(".trend-analysis")).ToBeVisibleAsync();
            await Expect(Page.Locator(".trend-analysis")).ToContainTextAsync("Performance trend analysis");
        }

        [Test]
        [Description("Verify performance baseline monitoring")]
        public async Task PerformanceBaseline_Should_BeMonitored()
        {
            // Navigate to performance dashboard
            await Page.GotoAsync($"{BaseUrl}/admin/performance");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/admin/performance");

            // Verify baseline metrics
            await Expect(Page.Locator(".performance-baseline")).ToBeVisibleAsync();
            await Expect(Page.Locator(".baseline-response-time")).ToBeVisibleAsync();
            await Expect(Page.Locator(".baseline-memory-usage")).ToBeVisibleAsync();
            await Expect(Page.Locator(".baseline-cpu-usage")).ToBeVisibleAsync();

            // Verify baseline comparison
            await Expect(Page.Locator(".baseline-comparison")).ToBeVisibleAsync();
            await Expect(Page.Locator(".current-vs-baseline")).ToBeVisibleAsync();

            // Verify baseline deviation alerts
            await Expect(Page.Locator(".baseline-deviation")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify performance optimization recommendations")]
        public async Task PerformanceRecommendations_Should_BeProvided()
        {
            // Navigate to performance dashboard
            await Page.GotoAsync($"{BaseUrl}/admin/performance");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/admin/performance");

            // Verify performance recommendations
            await Expect(Page.Locator(".performance-recommendations")).ToBeVisibleAsync();
            await Expect(Page.Locator(".optimization-suggestions")).ToBeVisibleAsync();

            // Verify recommendation categories
            await Expect(Page.Locator(".database-optimization")).ToBeVisibleAsync();
            await Expect(Page.Locator(".caching-recommendations")).ToBeVisibleAsync();
            await Expect(Page.Locator(".code-optimization")).ToBeVisibleAsync();

            // Verify actionable recommendations
            await Expect(Page.Locator(".actionable-recommendation")).ToHaveCountAsync(3);
        }

        [Test]
        [Description("Verify performance monitoring configuration")]
        public async Task PerformanceMonitoring_Should_BeConfigurable()
        {
            // Navigate to performance settings
            await Page.GotoAsync($"{BaseUrl}/admin/performance/settings");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/admin/performance/settings");

            // Verify monitoring configuration options
            await Expect(Page.Locator(".monitoring-config")).ToBeVisibleAsync();

            // Configure response time threshold
            await Page.FillAsync("#response-time-threshold", "2000");
            await Page.ClickAsync("#save-settings-btn");

            // Verify settings saved
            await Expect(Page.Locator(".settings-saved")).ToBeVisibleAsync();

            // Configure memory usage threshold
            await Page.FillAsync("#memory-usage-threshold", "400");
            await Page.ClickAsync("#save-settings-btn");

            // Verify settings saved
            await Expect(Page.Locator(".settings-saved")).ToBeVisibleAsync();

            // Configure CPU usage threshold
            await Page.FillAsync("#cpu-usage-threshold", "70");
            await Page.ClickAsync("#save-settings-btn");

            // Verify settings saved
            await Expect(Page.Locator(".settings-saved")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify performance monitoring data retention")]
        public async Task PerformanceData_Should_BeRetained()
        {
            // Navigate to performance dashboard
            await Page.GotoAsync($"{BaseUrl}/admin/performance");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Login first
            await Page.GotoAsync($"{BaseUrl}/login");
            await Page.FillAsync("#username", "adminuser");
            await Page.FillAsync("#password", "AdminPassword123!");
            await Page.ClickAsync("#login-btn");
            await Page.WaitForURLAsync("**/dashboard");

            await Page.GotoAsync($"{BaseUrl}/admin/performance");

            // Verify historical data
            await Expect(Page.Locator(".historical-data")).ToBeVisibleAsync();
            await Expect(Page.Locator(".data-retention-info")).ToBeVisibleAsync();

            // Verify data retention period
            await Expect(Page.Locator(".retention-period")).ToContainTextAsync("90 days");

            // Verify data export functionality
            await Expect(Page.Locator(".export-performance-data")).ToBeVisibleAsync();
            await Page.ClickAsync("#export-data-btn");

            // Verify export options
            await Expect(Page.Locator(".export-options")).ToBeVisibleAsync();
            await Expect(Page.Locator(".export-csv")).ToBeVisibleAsync();
            await Expect(Page.Locator(".export-json")).ToBeVisibleAsync();
        }
    }
} 