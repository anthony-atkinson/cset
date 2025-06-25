using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CSETWebCore.PlaywrightTests.Tests.Performance
{
    [TestFixture]
    public class LoadTestingTests : PlaywrightTestBase
    {
        [Test]
        [Description("Verify application handles concurrent user load")]
        public async Task ConcurrentUsers_Should_BeHandled()
        {
            var tasks = new List<Task>();
            var results = new List<bool>();

            // Simulate 10 concurrent users
            for (int i = 0; i < 10; i++)
            {
                var userIndex = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var context = await Browser.NewContextAsync();
                        var page = await context.NewPageAsync();

                        // Login
                        await page.GotoAsync($"{BaseUrl}/login");
                        await page.FillAsync("#username", $"user{userIndex}");
                        await page.FillAsync("#password", "TestPassword123!");
                        await page.ClickAsync("#login-btn");
                        await page.WaitForURLAsync("**/dashboard");

                        // Navigate to assessment page
                        await page.GotoAsync($"{BaseUrl}/assessment");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Create assessment
                        await page.ClickAsync("#create-assessment-btn");
                        await page.FillAsync("#assessment-name", $"Load Test Assessment {userIndex}");
                        await page.ClickAsync("#save-assessment-btn");

                        // Verify success
                        await page.WaitForSelectorAsync(".success-message", new() { Timeout = 10000 });
                        results.Add(true);

                        await context.CloseAsync();
                    }
                    catch
                    {
                        results.Add(false);
                    }
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Verify success rate
            var successCount = results.Count(r => r);
            Assert.That(successCount, Is.GreaterThanOrEqualTo(8), "At least 80% of concurrent users should succeed");
        }

        [Test]
        [Description("Verify database performance under load")]
        public async Task DatabasePerformance_Should_HandleLoad()
        {
            var startTime = DateTime.UtcNow;
            var tasks = new List<Task>();

            // Simulate database-intensive operations
            for (int i = 0; i < 20; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    // Login
                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    // Perform database operations
                    await page.GotoAsync($"{BaseUrl}/assessment");
                    await page.ClickAsync("#create-assessment-btn");
                    await page.FillAsync("#assessment-name", $"DB Load Test {index}");
                    await page.ClickAsync("#save-assessment-btn");

                    // Generate report (database intensive)
                    await page.GotoAsync($"{BaseUrl}/assessment/{index + 1}/reports");
                    await page.ClickAsync("#generate-report-btn");
                    await page.WaitForSelectorAsync(".report-generated", new() { Timeout = 15000 });

                    await context.CloseAsync();
                }));
            }

            // Wait for all operations to complete
            await Task.WhenAll(tasks);
            var totalTime = DateTime.UtcNow - startTime;

            // Verify performance within acceptable limits
            Assert.That(totalTime.TotalSeconds, Is.LessThan(60), "Database operations should complete within 60 seconds");
        }

        [Test]
        [Description("Verify memory usage under sustained load")]
        public async Task MemoryUsage_Should_RemainStable()
        {
            var memorySnapshots = new List<double>();

            // Monitor memory usage over time
            for (int i = 0; i < 5; i++)
            {
                var context = await Browser.NewContextAsync();
                var page = await context.NewPageAsync();

                // Login
                await page.GotoAsync($"{BaseUrl}/login");
                await page.FillAsync("#username", "testuser");
                await page.FillAsync("#password", "TestPassword123!");
                await page.ClickAsync("#login-btn");
                await page.WaitForURLAsync("**/dashboard");

                // Perform memory-intensive operations
                for (int j = 0; j < 10; j++)
                {
                    await page.GotoAsync($"{BaseUrl}/assessment");
                    await page.ClickAsync("#create-assessment-btn");
                    await page.FillAsync("#assessment-name", $"Memory Test {i}-{j}");
                    await page.ClickAsync("#save-assessment-btn");
                    await page.WaitForTimeoutAsync(500);
                }

                // Get memory usage
                var memoryUsage = await page.EvaluateAsync<double>("() => performance.memory.usedJSHeapSize / 1024 / 1024");
                memorySnapshots.Add(memoryUsage);

                await context.CloseAsync();
                await Task.Delay(2000); // Wait between iterations
            }

            // Verify memory usage is stable (no significant increase)
            var initialMemory = memorySnapshots[0];
            var finalMemory = memorySnapshots[^1];
            var memoryIncrease = finalMemory - initialMemory;

            Assert.That(memoryIncrease, Is.LessThan(100), "Memory usage should not increase by more than 100MB");
        }

        [Test]
        [Description("Verify API response times under load")]
        public async Task APIResponseTimes_Should_RemainAcceptable()
        {
            var responseTimes = new List<double>();
            var tasks = new List<Task>();

            // Simulate API load
            for (int i = 0; i < 15; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    // Login
                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    // Measure API response times
                    var startTime = DateTime.UtcNow;
                    await page.GotoAsync($"{BaseUrl}/assessment");
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    var loadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    responseTimes.Add(loadTime);

                    // Test API calls
                    startTime = DateTime.UtcNow;
                    await page.ClickAsync("#create-assessment-btn");
                    await page.FillAsync("#assessment-name", $"API Load Test {index}");
                    await page.ClickAsync("#save-assessment-btn");
                    await page.WaitForSelectorAsync(".success-message");
                    var apiTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    responseTimes.Add(apiTime);

                    await context.CloseAsync();
                }));
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Calculate average response time
            var averageResponseTime = responseTimes.Average();

            // Verify response times are acceptable
            Assert.That(averageResponseTime, Is.LessThan(2000), "Average API response time should be under 2000ms");
        }

        [Test]
        [Description("Verify session management under load")]
        public async Task SessionManagement_Should_HandleLoad()
        {
            var sessions = new List<bool>();
            var tasks = new List<Task>();

            // Simulate multiple user sessions
            for (int i = 0; i < 25; i++)
            {
                var userIndex = i;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var context = await Browser.NewContextAsync();
                        var page = await context.NewPageAsync();

                        // Login and establish session
                        await page.GotoAsync($"{BaseUrl}/login");
                        await page.FillAsync("#username", $"sessionuser{userIndex}");
                        await page.FillAsync("#password", "TestPassword123!");
                        await page.ClickAsync("#login-btn");
                        await page.WaitForURLAsync("**/dashboard");

                        // Verify session is active
                        await page.GotoAsync($"{BaseUrl}/assessment");
                        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                        // Perform session-dependent operations
                        await page.ClickAsync("#create-assessment-btn");
                        await page.FillAsync("#assessment-name", $"Session Test {userIndex}");
                        await page.ClickAsync("#save-assessment-btn");

                        // Verify session maintained
                        await page.WaitForSelectorAsync(".success-message");
                        sessions.Add(true);

                        await context.CloseAsync();
                    }
                    catch
                    {
                        sessions.Add(false);
                    }
                }));
            }

            // Wait for all sessions to complete
            await Task.WhenAll(tasks);

            // Verify session success rate
            var successfulSessions = sessions.Count(s => s);
            Assert.That(successfulSessions, Is.GreaterThanOrEqualTo(20), "At least 80% of sessions should be successful");
        }

        [Test]
        [Description("Verify file upload performance under load")]
        public async Task FileUploadPerformance_Should_HandleLoad()
        {
            var uploadTimes = new List<double>();
            var tasks = new List<Task>();

            // Simulate concurrent file uploads
            for (int i = 0; i < 8; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    // Login
                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    // Navigate to file upload
                    await page.GotoAsync($"{BaseUrl}/assessment/123/documents");

                    // Create test file
                    var testFile = new FilePayload
                    {
                        Name = $"load-test-file-{index}.pdf",
                        MimeType = "application/pdf",
                        Buffer = System.Text.Encoding.UTF8.GetBytes($"Test file content {index}")
                    };

                    // Measure upload time
                    var startTime = DateTime.UtcNow;
                    await page.SetInputFilesAsync("#file-upload", testFile);
                    await page.WaitForSelectorAsync(".file-upload-success");
                    var uploadTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    uploadTimes.Add(uploadTime);

                    await context.CloseAsync();
                }));
            }

            // Wait for all uploads to complete
            await Task.WhenAll(tasks);

            // Verify upload performance
            var averageUploadTime = uploadTimes.Average();
            Assert.That(averageUploadTime, Is.LessThan(5000), "Average file upload time should be under 5000ms");
        }

        [Test]
        [Description("Verify report generation performance under load")]
        public async Task ReportGeneration_Should_HandleLoad()
        {
            var generationTimes = new List<double>();
            var tasks = new List<Task>();

            // Simulate concurrent report generation
            for (int i = 0; i < 6; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    // Login
                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    // Navigate to reports
                    await page.GotoAsync($"{BaseUrl}/assessment/123/reports");

                    // Measure report generation time
                    var startTime = DateTime.UtcNow;
                    await page.ClickAsync("#generate-report-btn");
                    await page.WaitForSelectorAsync(".report-generated");
                    var generationTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    generationTimes.Add(generationTime);

                    await context.CloseAsync();
                }));
            }

            // Wait for all reports to complete
            await Task.WhenAll(tasks);

            // Verify report generation performance
            var averageGenerationTime = generationTimes.Average();
            Assert.That(averageGenerationTime, Is.LessThan(10000), "Average report generation time should be under 10000ms");
        }

        [Test]
        [Description("Verify search performance under load")]
        public async Task SearchPerformance_Should_HandleLoad()
        {
            var searchTimes = new List<double>();
            var tasks = new List<Task>();

            // Simulate concurrent searches
            for (int i = 0; i < 12; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    // Login
                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    // Navigate to search
                    await page.GotoAsync($"{BaseUrl}/search");

                    // Perform search
                    var startTime = DateTime.UtcNow;
                    await page.FillAsync("#search-input", $"test search {index}");
                    await page.ClickAsync("#search-btn");
                    await page.WaitForSelectorAsync(".search-results");
                    var searchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                    searchTimes.Add(searchTime);

                    await context.CloseAsync();
                }));
            }

            // Wait for all searches to complete
            await Task.WhenAll(tasks);

            // Verify search performance
            var averageSearchTime = searchTimes.Average();
            Assert.That(averageSearchTime, Is.LessThan(3000), "Average search time should be under 3000ms");
        }

        [Test]
        [Description("Verify application recovery after load test")]
        public async Task ApplicationRecovery_Should_WorkAfterLoad()
        {
            // Perform load test first
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                tasks.Add(Task.Run(async () =>
                {
                    var context = await Browser.NewContextAsync();
                    var page = await context.NewPageAsync();

                    await page.GotoAsync($"{BaseUrl}/login");
                    await page.FillAsync("#username", "testuser");
                    await page.FillAsync("#password", "TestPassword123!");
                    await page.ClickAsync("#login-btn");
                    await page.WaitForURLAsync("**/dashboard");

                    await page.GotoAsync($"{BaseUrl}/assessment");
                    await page.ClickAsync("#create-assessment-btn");
                    await page.FillAsync("#assessment-name", $"Recovery Test {index}");
                    await page.ClickAsync("#save-assessment-btn");

                    await context.CloseAsync();
                }));
            }

            await Task.WhenAll(tasks);

            // Wait for system to recover
            await Task.Delay(5000);

            // Test normal operation after load
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify application is responsive
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Recovery Test Assessment");
            await Page.ClickAsync("#save-assessment-btn");

            // Verify normal operation
            await Expect(Page.Locator(".success-message")).ToBeVisibleAsync();
            await Expect(Page.Locator(".success-message")).ToContainTextAsync("Assessment created successfully");
        }
    }
} 