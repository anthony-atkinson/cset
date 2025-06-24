using System;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Questions;
using CSETWebCore.PlaywrightTests.PageObjects.Reports;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Tests.Performance
{
    /// <summary>
    /// Performance tests for main CSET application workflows
    /// </summary>
    [TestFixture]
    [TestCategory("Performance")]
    [TestCategory("E2E")]
    public class ApplicationPerformanceTests : PerformanceTestFixture
    {
        private LoginPage _loginPage = null!;
        private DashboardPage _dashboardPage = null!;
        private AssessmentCreationPage _assessmentCreationPage = null!;
        private QuestionNavigationPage _questionNavigationPage = null!;
        private ReportGenerationPage _reportGenerationPage = null!;

        protected override async Task SetUpAsync()
        {
            await base.SetUpAsync();
            
            _loginPage = new LoginPage(Page);
            _dashboardPage = new DashboardPage(Page);
            _assessmentCreationPage = new AssessmentCreationPage(Page);
            _questionNavigationPage = new QuestionNavigationPage(Page);
            _reportGenerationPage = new ReportGenerationPage(Page);
        }

        [Test]
        [Description("Verify login page loads quickly")]
        public async Task LoginPage_Should_LoadQuickly()
        {
            // Act
            var loadTime = await MeasureActionAsync(async () =>
            {
                await _loginPage.NavigateAsync();
                await _loginPage.WaitForLoginFormAsync();
            });

            // Assert
            Assert.That(loadTime.TotalSeconds, Is.LessThan(3.0), 
                $"Login page should load within 3 seconds. Actual: {loadTime.TotalSeconds:F2}s");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxPageLoadTime = 3000,
                MaxDOMContentLoadedTime = 2000,
                MaxFirstPaintTime = 1500,
                MaxFirstContentfulPaintTime = 2000,
                MaxMemoryIncreaseMB = 10
            });
        }

        [Test]
        [Description("Verify login workflow performance")]
        public async Task LoginWorkflow_Should_CompleteQuickly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act
            var loginTime = await MeasureActionWithMemoryAsync(async () =>
            {
                await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
                await _dashboardPage.WaitForDashboardLoadAsync();
            });

            // Assert
            Assert.That(loginTime.Duration.TotalSeconds, Is.LessThan(5.0), 
                $"Login workflow should complete within 5 seconds. Actual: {loginTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = loginTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(20.0), 
                $"Login should not increase memory by more than 20MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 5.0,
                MaxPageLoadTime = 4000,
                MaxDOMContentLoadedTime = 2500,
                MaxFirstPaintTime = 2000,
                MaxFirstContentfulPaintTime = 2500,
                MaxMemoryIncreaseMB = 20,
                MaxNetworkRequests = 15
            });
        }

        [Test]
        [Description("Verify dashboard loads and renders quickly")]
        public async Task Dashboard_Should_LoadAndRenderQuickly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act
            var chartRenderTime = await MeasureActionWithMemoryAsync(async () =>
            {
                await _dashboardPage.WaitForChartsToRenderAsync();
                await WaitForPerformanceStabilityAsync();
            });

            // Assert
            Assert.That(chartRenderTime.Duration.TotalSeconds, Is.LessThan(3.0), 
                $"Dashboard charts should render within 3 seconds. Actual: {chartRenderTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = chartRenderTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(15.0), 
                $"Chart rendering should not increase memory by more than 15MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 8.0,
                MaxPageLoadTime = 4000,
                MaxDOMContentLoadedTime = 2500,
                MaxFirstPaintTime = 2000,
                MaxFirstContentfulPaintTime = 2500,
                MaxMemoryIncreaseMB = 25,
                MaxNetworkRequests = 20
            });
        }

        [Test]
        [Description("Verify assessment creation performance")]
        public async Task AssessmentCreation_Should_CompleteQuickly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act
            var creationTime = await MeasureActionWithMemoryAsync(async () =>
            {
                await _dashboardPage.ClickCreateAssessmentAsync();
                await _assessmentCreationPage.WaitForPageLoadAsync();
                await _assessmentCreationPage.SelectGalleryItemByIndexAsync(0);
                await _assessmentCreationPage.SetAssessmentNameAsync($"Performance Test {DateTime.Now:yyyyMMdd_HHmmss}");
                await _assessmentCreationPage.ClickCreateButtonAsync();
                await _questionNavigationPage.WaitForQuestionLoadAsync();
            });

            // Assert
            Assert.That(creationTime.Duration.TotalSeconds, Is.LessThan(8.0), 
                $"Assessment creation should complete within 8 seconds. Actual: {creationTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = creationTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(30.0), 
                $"Assessment creation should not increase memory by more than 30MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 8.0,
                MaxPageLoadTime = 5000,
                MaxDOMContentLoadedTime = 3000,
                MaxFirstPaintTime = 2500,
                MaxFirstContentfulPaintTime = 3000,
                MaxMemoryIncreaseMB = 30,
                MaxNetworkRequests = 25
            });
        }

        [Test]
        [Description("Verify question navigation performance")]
        public async Task QuestionNavigation_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _assessmentCreationPage.WaitForPageLoadAsync();
            await _assessmentCreationPage.SelectGalleryItemByIndexAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync($"Performance Test {DateTime.Now:yyyyMMdd_HHmmss}");
            await _assessmentCreationPage.ClickCreateButtonAsync();
            await _questionNavigationPage.WaitForQuestionLoadAsync();

            // Act - Navigate through multiple questions
            var navigationTime = await MeasureActionWithMemoryAsync(async () =>
            {
                // Answer first question
                await _questionNavigationPage.SelectAnswerAsync("Yes");
                await _questionNavigationPage.SaveAnswerAsync();

                // Navigate to next question
                await _questionNavigationPage.ClickNextButtonAsync();
                await _questionNavigationPage.WaitForQuestionLoadAsync();

                // Answer second question
                await _questionNavigationPage.SelectAnswerAsync("No");
                await _questionNavigationPage.SaveAnswerAsync();

                // Navigate to next question
                await _questionNavigationPage.ClickNextButtonAsync();
                await _questionNavigationPage.WaitForQuestionLoadAsync();

                // Answer third question
                await _questionNavigationPage.SelectAnswerAsync("Unanswered");
                await _questionNavigationPage.SaveAnswerAsync();
            });

            // Assert
            Assert.That(navigationTime.Duration.TotalSeconds, Is.LessThan(10.0), 
                $"Question navigation should complete within 10 seconds. Actual: {navigationTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = navigationTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(20.0), 
                $"Question navigation should not increase memory by more than 20MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 10.0,
                MaxPageLoadTime = 3000,
                MaxDOMContentLoadedTime = 2000,
                MaxFirstPaintTime = 1500,
                MaxFirstContentfulPaintTime = 2000,
                MaxMemoryIncreaseMB = 20,
                MaxNetworkRequests = 15
            });
        }

        [Test]
        [Description("Verify report generation performance")]
        public async Task ReportGeneration_Should_CompleteQuickly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _assessmentCreationPage.WaitForPageLoadAsync();
            await _assessmentCreationPage.SelectGalleryItemByIndexAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync($"Performance Test {DateTime.Now:yyyyMMdd_HHmmss}");
            await _assessmentCreationPage.ClickCreateButtonAsync();
            await _questionNavigationPage.WaitForQuestionLoadAsync();

            // Answer a few questions to have data for report
            await _questionNavigationPage.SelectAnswerAsync("Yes");
            await _questionNavigationPage.SaveAnswerAsync();
            await _questionNavigationPage.ClickNextButtonAsync();
            await _questionNavigationPage.WaitForQuestionLoadAsync();
            await _questionNavigationPage.SelectAnswerAsync("No");
            await _questionNavigationPage.SaveAnswerAsync();

            // Navigate to reports
            await _questionNavigationPage.ClickReportsButtonAsync();
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            var reportGenerationTime = await MeasureActionWithMemoryAsync(async () =>
            {
                await _reportGenerationPage.SelectReportTypeAsync("Executive Summary");
                await _reportGenerationPage.SelectFormatAsync("PDF");
                await _reportGenerationPage.ClickGenerateButtonAsync();
                await _reportGenerationPage.WaitForReportGenerationAsync();
            });

            // Assert
            Assert.That(reportGenerationTime.Duration.TotalSeconds, Is.LessThan(15.0), 
                $"Report generation should complete within 15 seconds. Actual: {reportGenerationTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = reportGenerationTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(40.0), 
                $"Report generation should not increase memory by more than 40MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 15.0,
                MaxPageLoadTime = 5000,
                MaxDOMContentLoadedTime = 3000,
                MaxFirstPaintTime = 2500,
                MaxFirstContentfulPaintTime = 3000,
                MaxMemoryIncreaseMB = 40,
                MaxNetworkRequests = 30
            });
        }

        [Test]
        [Description("Verify full application workflow performance")]
        public async Task FullWorkflow_Should_CompleteWithinReasonableTime()
        {
            // Act - Complete full workflow from login to report generation
            var fullWorkflowTime = await MeasureActionWithMemoryAsync(async () =>
            {
                // Login
                await _loginPage.NavigateAsync();
                await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
                await _dashboardPage.WaitForDashboardLoadAsync();

                // Create assessment
                await _dashboardPage.ClickCreateAssessmentAsync();
                await _assessmentCreationPage.WaitForPageLoadAsync();
                await _assessmentCreationPage.SelectGalleryItemByIndexAsync(0);
                await _assessmentCreationPage.SetAssessmentNameAsync($"Full Workflow Test {DateTime.Now:yyyyMMdd_HHmmss}");
                await _assessmentCreationPage.ClickCreateButtonAsync();
                await _questionNavigationPage.WaitForQuestionLoadAsync();

                // Answer questions
                await _questionNavigationPage.SelectAnswerAsync("Yes");
                await _questionNavigationPage.SaveAnswerAsync();
                await _questionNavigationPage.ClickNextButtonAsync();
                await _questionNavigationPage.WaitForQuestionLoadAsync();
                await _questionNavigationPage.SelectAnswerAsync("No");
                await _questionNavigationPage.SaveAnswerAsync();

                // Generate report
                await _questionNavigationPage.ClickReportsButtonAsync();
                await _reportGenerationPage.WaitForReportOptionsLoadAsync();
                await _reportGenerationPage.SelectReportTypeAsync("Executive Summary");
                await _reportGenerationPage.SelectFormatAsync("PDF");
                await _reportGenerationPage.ClickGenerateButtonAsync();
                await _reportGenerationPage.WaitForReportGenerationAsync();
            });

            // Assert
            Assert.That(fullWorkflowTime.Duration.TotalSeconds, Is.LessThan(30.0), 
                $"Full workflow should complete within 30 seconds. Actual: {fullWorkflowTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = fullWorkflowTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(50.0), 
                $"Full workflow should not increase memory by more than 50MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 30.0,
                MaxPageLoadTime = 5000,
                MaxDOMContentLoadedTime = 3000,
                MaxFirstPaintTime = 2500,
                MaxFirstContentfulPaintTime = 3000,
                MaxMemoryIncreaseMB = 50,
                MaxNetworkRequests = 50
            });
        }

        [Test]
        [Description("Verify memory usage remains stable during extended use")]
        public async Task MemoryUsage_Should_RemainStable()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            var initialMemory = await GetMemoryUsageAsync();
            var memoryReadings = new List<long>();

            // Act - Perform multiple operations to test memory stability
            for (int i = 0; i < 5; i++)
            {
                // Navigate to dashboard
                await _dashboardPage.NavigateAsync();
                await _dashboardPage.WaitForDashboardLoadAsync();
                await _dashboardPage.WaitForChartsToRenderAsync();
                memoryReadings.Add(await GetMemoryUsageAsync());

                // Navigate to assessment creation
                await _dashboardPage.ClickCreateAssessmentAsync();
                await _assessmentCreationPage.WaitForPageLoadAsync();
                memoryReadings.Add(await GetMemoryUsageAsync());

                // Go back to dashboard
                await _assessmentCreationPage.ClickCancelButtonAsync();
                await _dashboardPage.WaitForDashboardLoadAsync();
                memoryReadings.Add(await GetMemoryUsageAsync());

                await Task.Delay(1000); // Brief pause between iterations
            }

            var finalMemory = await GetMemoryUsageAsync();
            var totalMemoryIncrease = finalMemory - initialMemory;
            var memoryIncreaseMB = totalMemoryIncrease / (1024.0 * 1024.0);

            // Assert
            Assert.That(memoryIncreaseMB, Is.LessThan(30.0), 
                $"Memory usage should remain stable during extended use. Total increase: {memoryIncreaseMB:F2}MB");

            // Check for memory leaks (memory should not continuously increase)
            var maxMemory = memoryReadings.Max();
            var minMemory = memoryReadings.Min();
            var memoryVariationMB = (maxMemory - minMemory) / (1024.0 * 1024.0);

            Assert.That(memoryVariationMB, Is.LessThan(20.0), 
                $"Memory variation should be minimal during extended use. Variation: {memoryVariationMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 60.0,
                MaxMemoryIncreaseMB = 30
            });
        }

        [Test]
        [Description("Verify network request efficiency")]
        public async Task NetworkRequests_Should_BeEfficient()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForPerformanceStabilityAsync();

            // Assert
            var metrics = CurrentMetrics;
            
            // Check total number of requests
            Assert.That(metrics.NetworkRequests.Count, Is.LessThan(25), 
                $"Total network requests should be less than 25. Actual: {metrics.NetworkRequests.Count}");

            // Check average request time
            if (metrics.NetworkRequests.Any())
            {
                var avgRequestTime = metrics.NetworkRequests.Average(r => r.Duration);
                Assert.That(avgRequestTime, Is.LessThan(1000), 
                    $"Average network request time should be less than 1000ms. Actual: {avgRequestTime:F2}ms");

                // Check for slow requests
                var slowRequests = metrics.NetworkRequests.Where(r => r.Duration > 2000).ToList();
                Assert.That(slowRequests.Count, Is.LessThan(3), 
                    $"Number of slow requests (>2s) should be less than 3. Actual: {slowRequests.Count}");
            }

            // Check request sizes
            var largeRequests = metrics.NetworkRequests.Where(r => r.Size > 1024 * 1024).ToList(); // >1MB
            Assert.That(largeRequests.Count, Is.LessThan(5), 
                $"Number of large requests (>1MB) should be less than 5. Actual: {largeRequests.Count}");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxNetworkRequests = 25,
                MaxAverageRequestTime = 1000
            });
        }
    }
} 