using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Base test fixture for performance testing with Playwright
    /// </summary>
    [TestFixture]
    public abstract class PerformanceTestFixture : BaseTestFixture
    {
        protected PerformanceMetrics CurrentMetrics { get; private set; } = new();
        protected List<PerformanceMetrics> TestMetrics { get; private set; } = new();
        protected string PerformanceResultsPath { get; private set; } = string.Empty;
        protected bool EnableDetailedMetrics { get; set; } = true;
        protected bool EnableMemoryMonitoring { get; set; } = true;
        protected bool EnableNetworkMonitoring { get; set; } = true;

        protected override async Task SetUpAsync()
        {
            await base.SetUpAsync();
            
            // Setup performance monitoring paths
            var testResultsPath = Path.Combine(Directory.GetCurrentDirectory(), "test-results");
            PerformanceResultsPath = Path.Combine(testResultsPath, "performance");
            Directory.CreateDirectory(PerformanceResultsPath);
            
            // Start performance monitoring
            await StartPerformanceMonitoringAsync();
        }

        protected override async Task TearDownAsync()
        {
            // Stop performance monitoring and collect metrics
            await StopPerformanceMonitoringAsync();
            
            // Save performance results
            await SavePerformanceResultsAsync();
            
            await base.TearDownAsync();
        }

        /// <summary>
        /// Start performance monitoring
        /// </summary>
        protected async Task StartPerformanceMonitoringAsync()
        {
            CurrentMetrics = new PerformanceMetrics
            {
                TestName = TestContext.CurrentContext.Test.Name,
                StartTime = DateTime.Now,
                InitialMemoryUsage = await GetMemoryUsageAsync()
            };

            if (EnableNetworkMonitoring)
            {
                await Page.RouteAsync("**/*", async route =>
                {
                    var request = route.Request;
                    var response = await route.FetchAsync();
                    
                    CurrentMetrics.NetworkRequests.Add(new NetworkRequest
                    {
                        Url = request.Url,
                        Method = request.Method,
                        Status = response.Status,
                        Size = response.Size ?? 0,
                        Duration = response.Timing?.ResponseEnd - response.Timing?.RequestStart ?? 0
                    });
                    
                    await route.FulfillAsync(response);
                });
            }
        }

        /// <summary>
        /// Stop performance monitoring and collect final metrics
        /// </summary>
        protected async Task StopPerformanceMonitoringAsync()
        {
            CurrentMetrics.EndTime = DateTime.Now;
            CurrentMetrics.Duration = CurrentMetrics.EndTime - CurrentMetrics.StartTime;
            CurrentMetrics.FinalMemoryUsage = await GetMemoryUsageAsync();
            CurrentMetrics.MemoryIncrease = CurrentMetrics.FinalMemoryUsage - CurrentMetrics.InitialMemoryUsage;
            
            // Collect performance timing data
            var performanceTiming = await Page.EvaluateAsync<JsonElement>(@"
                () => {
                    const timing = performance.timing;
                    return {
                        navigationStart: timing.navigationStart,
                        loadEventEnd: timing.loadEventEnd,
                        domContentLoadedEventEnd: timing.domContentLoadedEventEnd,
                        firstPaint: performance.getEntriesByType('paint').find(e => e.name === 'first-paint')?.startTime,
                        firstContentfulPaint: performance.getEntriesByType('paint').find(e => e.name === 'first-contentful-paint')?.startTime
                    };
                }
            ");

            if (performanceTiming.TryGetProperty("navigationStart", out var navStart) &&
                performanceTiming.TryGetProperty("loadEventEnd", out var loadEnd))
            {
                CurrentMetrics.PageLoadTime = loadEnd.GetInt64() - navStart.GetInt64();
            }

            if (performanceTiming.TryGetProperty("domContentLoadedEventEnd", out var domReady) &&
                performanceTiming.TryGetProperty("navigationStart", out var navStart2))
            {
                CurrentMetrics.DOMContentLoadedTime = domReady.GetInt64() - navStart2.GetInt64();
            }

            if (performanceTiming.TryGetProperty("firstPaint", out var firstPaint) &&
                performanceTiming.TryGetProperty("navigationStart", out var navStart3))
            {
                CurrentMetrics.FirstPaintTime = firstPaint.GetDouble();
            }

            if (performanceTiming.TryGetProperty("firstContentfulPaint", out var fcp) &&
                performanceTiming.TryGetProperty("navigationStart", out var navStart4))
            {
                CurrentMetrics.FirstContentfulPaintTime = fcp.GetDouble();
            }

            TestMetrics.Add(CurrentMetrics);
        }

        /// <summary>
        /// Get current memory usage in bytes
        /// </summary>
        protected async Task<long> GetMemoryUsageAsync()
        {
            try
            {
                return await Page.EvaluateAsync<long>("performance.memory.usedJSHeapSize");
            }
            catch
            {
                return 0; // Memory API not available
            }
        }

        /// <summary>
        /// Assert performance metrics meet expectations
        /// </summary>
        protected void AssertPerformanceMetrics(PerformanceAssertions assertions)
        {
            var metrics = CurrentMetrics;
            
            if (assertions.MaxDuration.HasValue)
            {
                Assert.That(metrics.Duration.TotalSeconds, Is.LessThan(assertions.MaxDuration.Value),
                    $"Test duration should be less than {assertions.MaxDuration.Value} seconds. Actual: {metrics.Duration.TotalSeconds:F2}s");
            }

            if (assertions.MaxPageLoadTime.HasValue)
            {
                Assert.That(metrics.PageLoadTime, Is.LessThan(assertions.MaxPageLoadTime.Value),
                    $"Page load time should be less than {assertions.MaxPageLoadTime.Value}ms. Actual: {metrics.PageLoadTime}ms");
            }

            if (assertions.MaxDOMContentLoadedTime.HasValue)
            {
                Assert.That(metrics.DOMContentLoadedTime, Is.LessThan(assertions.MaxDOMContentLoadedTime.Value),
                    $"DOM content loaded time should be less than {assertions.MaxDOMContentLoadedTime.Value}ms. Actual: {metrics.DOMContentLoadedTime}ms");
            }

            if (assertions.MaxFirstPaintTime.HasValue)
            {
                Assert.That(metrics.FirstPaintTime, Is.LessThan(assertions.MaxFirstPaintTime.Value),
                    $"First paint time should be less than {assertions.MaxFirstPaintTime.Value}ms. Actual: {metrics.FirstPaintTime:F2}ms");
            }

            if (assertions.MaxFirstContentfulPaintTime.HasValue)
            {
                Assert.That(metrics.FirstContentfulPaintTime, Is.LessThan(assertions.MaxFirstContentfulPaintTime.Value),
                    $"First contentful paint time should be less than {assertions.MaxFirstContentfulPaintTime.Value}ms. Actual: {metrics.FirstContentfulPaintTime:F2}ms");
            }

            if (assertions.MaxMemoryIncreaseMB.HasValue)
            {
                var memoryIncreaseMB = metrics.MemoryIncrease / (1024.0 * 1024.0);
                Assert.That(memoryIncreaseMB, Is.LessThan(assertions.MaxMemoryIncreaseMB.Value),
                    $"Memory increase should be less than {assertions.MaxMemoryIncreaseMB.Value}MB. Actual: {memoryIncreaseMB:F2}MB");
            }

            if (assertions.MaxNetworkRequests.HasValue)
            {
                Assert.That(metrics.NetworkRequests.Count, Is.LessThan(assertions.MaxNetworkRequests.Value),
                    $"Number of network requests should be less than {assertions.MaxNetworkRequests.Value}. Actual: {metrics.NetworkRequests.Count}");
            }

            if (assertions.MaxAverageRequestTime.HasValue)
            {
                var avgRequestTime = metrics.NetworkRequests.Any() 
                    ? metrics.NetworkRequests.Average(r => r.Duration) 
                    : 0;
                Assert.That(avgRequestTime, Is.LessThan(assertions.MaxAverageRequestTime.Value),
                    $"Average network request time should be less than {assertions.MaxAverageRequestTime.Value}ms. Actual: {avgRequestTime:F2}ms");
            }
        }

        /// <summary>
        /// Save performance results to file
        /// </summary>
        protected async Task SavePerformanceResultsAsync()
        {
            try
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var resultsFile = Path.Combine(PerformanceResultsPath, $"performance_{testName}_{timestamp}.json");
                
                var results = new PerformanceTestResults
                {
                    TestName = testName,
                    Timestamp = DateTime.Now,
                    Metrics = TestMetrics,
                    Summary = GeneratePerformanceSummary()
                };

                var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(resultsFile, json);
                
                TestContext.WriteLine($"Performance results saved: {resultsFile}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to save performance results: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate performance summary
        /// </summary>
        protected PerformanceSummary GeneratePerformanceSummary()
        {
            if (!TestMetrics.Any())
                return new PerformanceSummary();

            return new PerformanceSummary
            {
                TotalTests = TestMetrics.Count,
                AverageDuration = TestMetrics.Average(m => m.Duration.TotalSeconds),
                AveragePageLoadTime = TestMetrics.Average(m => m.PageLoadTime),
                AverageDOMContentLoadedTime = TestMetrics.Average(m => m.DOMContentLoadedTime),
                AverageFirstPaintTime = TestMetrics.Average(m => m.FirstPaintTime),
                AverageFirstContentfulPaintTime = TestMetrics.Average(m => m.FirstContentfulPaintTime),
                AverageMemoryIncreaseMB = TestMetrics.Average(m => m.MemoryIncrease / (1024.0 * 1024.0)),
                TotalNetworkRequests = TestMetrics.Sum(m => m.NetworkRequests.Count),
                AverageNetworkRequestTime = TestMetrics
                    .SelectMany(m => m.NetworkRequests)
                    .Any() ? TestMetrics.SelectMany(m => m.NetworkRequests).Average(r => r.Duration) : 0
            };
        }

        /// <summary>
        /// Wait for performance metric to stabilize
        /// </summary>
        protected async Task WaitForPerformanceStabilityAsync(int timeoutMs = 5000)
        {
            var startTime = DateTime.Now;
            var lastMemoryUsage = await GetMemoryUsageAsync();
            var stableCount = 0;
            const int requiredStableCount = 3;

            while (DateTime.Now - startTime < TimeSpan.FromMilliseconds(timeoutMs))
            {
                await Task.Delay(500);
                var currentMemoryUsage = await GetMemoryUsageAsync();
                
                var memoryDiff = Math.Abs(currentMemoryUsage - lastMemoryUsage);
                if (memoryDiff < 1024 * 1024) // Less than 1MB change
                {
                    stableCount++;
                    if (stableCount >= requiredStableCount)
                        break;
                }
                else
                {
                    stableCount = 0;
                }
                
                lastMemoryUsage = currentMemoryUsage;
            }
        }

        /// <summary>
        /// Measure performance of a specific action
        /// </summary>
        protected async Task<TimeSpan> MeasureActionAsync(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Measure performance of a specific action with memory tracking
        /// </summary>
        protected async Task<ActionPerformanceMetrics> MeasureActionWithMemoryAsync(Func<Task> action)
        {
            var initialMemory = await GetMemoryUsageAsync();
            var stopwatch = Stopwatch.StartNew();
            
            await action();
            
            stopwatch.Stop();
            var finalMemory = await GetMemoryUsageAsync();
            
            return new ActionPerformanceMetrics
            {
                Duration = stopwatch.Elapsed,
                MemoryIncrease = finalMemory - initialMemory
            };
        }
    }

    /// <summary>
    /// Performance metrics for a single test
    /// </summary>
    public class PerformanceMetrics
    {
        public string TestName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public long InitialMemoryUsage { get; set; }
        public long FinalMemoryUsage { get; set; }
        public long MemoryIncrease { get; set; }
        public long PageLoadTime { get; set; }
        public long DOMContentLoadedTime { get; set; }
        public double FirstPaintTime { get; set; }
        public double FirstContentfulPaintTime { get; set; }
        public List<NetworkRequest> NetworkRequests { get; set; } = new();
    }

    /// <summary>
    /// Network request information
    /// </summary>
    public class NetworkRequest
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public int Status { get; set; }
        public long Size { get; set; }
        public double Duration { get; set; }
    }

    /// <summary>
    /// Performance assertions for tests
    /// </summary>
    public class PerformanceAssertions
    {
        public double? MaxDuration { get; set; } // seconds
        public long? MaxPageLoadTime { get; set; } // milliseconds
        public long? MaxDOMContentLoadedTime { get; set; } // milliseconds
        public double? MaxFirstPaintTime { get; set; } // milliseconds
        public double? MaxFirstContentfulPaintTime { get; set; } // milliseconds
        public double? MaxMemoryIncreaseMB { get; set; } // megabytes
        public int? MaxNetworkRequests { get; set; }
        public double? MaxAverageRequestTime { get; set; } // milliseconds
    }

    /// <summary>
    /// Performance metrics for a specific action
    /// </summary>
    public class ActionPerformanceMetrics
    {
        public TimeSpan Duration { get; set; }
        public long MemoryIncrease { get; set; }
    }

    /// <summary>
    /// Performance test results
    /// </summary>
    public class PerformanceTestResults
    {
        public string TestName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<PerformanceMetrics> Metrics { get; set; } = new();
        public PerformanceSummary Summary { get; set; } = new();
    }

    /// <summary>
    /// Performance summary across multiple tests
    /// </summary>
    public class PerformanceSummary
    {
        public int TotalTests { get; set; }
        public double AverageDuration { get; set; }
        public double AveragePageLoadTime { get; set; }
        public double AverageDOMContentLoadedTime { get; set; }
        public double AverageFirstPaintTime { get; set; }
        public double AverageFirstContentfulPaintTime { get; set; }
        public double AverageMemoryIncreaseMB { get; set; }
        public int TotalNetworkRequests { get; set; }
        public double AverageNetworkRequestTime { get; set; }
    }
} 