using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.CrossBrowser
{
    /// <summary>
    /// Cross-browser tests for Dashboard and Analytics functionality
    /// </summary>
    [TestFixture]
    [Category("CrossBrowser")]
    [Category("Dashboard")]
    [Category("Analytics")]
    [Category("E2E")]
    public class CrossBrowserDashboardTests : CrossBrowserTestFixture
    {
        private DashboardPage _dashboardPage = null!;
        private LoginPage _loginPage = null!;

        protected override async Task SetUpAsync()
        {
            _dashboardPage = new DashboardPage(Page);
            _loginPage = new LoginPage(Page);
            
            // Login first to access dashboard
            await _loginPage.NavigateAsync();
            await _loginPage.AcceptPrivacyWarningAsync();
            await _loginPage.LoginWithTestCredentialsAsync();
            
            // Wait for login to complete
            await _loginPage.WaitForLoginFormAsync();
            
            // Navigate to dashboard
            await _dashboardPage.NavigateAsync();
        }

        [Test]
        [Description("Verify dashboard page loads correctly across browsers")]
        public async Task DashboardPage_Should_LoadCorrectly_AcrossBrowsers()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, 
                $"Dashboard page should be displayed in {BrowserName}");
            Assert.That(await _dashboardPage.GetDashboardTitleAsync(), Is.EqualTo("Analysis Dashboard"), 
                $"Dashboard title should be correct in {BrowserName}");
        }

        [Test]
        [Description("Verify dashboard initializes and shows content across browsers")]
        public async Task Dashboard_Should_InitializeAndShowContent_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, 
                $"Dashboard should be initialized in {BrowserName}");
            Assert.That(await _dashboardPage.IsAnalysisDivVisibleAsync(), Is.True, 
                $"Analysis container should be visible in {BrowserName}");
        }

        [Test]
        [Description("Verify overall score is displayed across browsers")]
        public async Task Dashboard_Should_DisplayOverallScore_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _dashboardPage.IsOverallScoreDisplayedAsync(), Is.True, 
                $"Overall score should be displayed in {BrowserName}");
            
            var overallScore = await _dashboardPage.GetOverallScoreAsync();
            Assert.That(string.IsNullOrEmpty(overallScore), Is.False, 
                $"Overall score should have a value in {BrowserName}");
            Assert.That(overallScore.Contains("%"), Is.True, 
                $"Overall score should be a percentage in {BrowserName}");
        }

        [Test]
        [Description("Verify assessment compliance chart is displayed across browsers")]
        public async Task Dashboard_Should_DisplayAssessmentComplianceChart_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _dashboardPage.IsAssessmentComplianceChartDisplayedAsync(), Is.True, 
                $"Assessment compliance chart should be displayed in {BrowserName}");
            Assert.That(await _dashboardPage.IsAssessmentComplianceSectionVisibleAsync(), Is.True, 
                $"Assessment compliance section should be visible in {BrowserName}");
        }

        [Test]
        [Description("Verify navigation buttons are functional across browsers")]
        public async Task Dashboard_Should_HaveFunctionalNavigationButtons_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _dashboardPage.IsBackButtonEnabledAsync(), Is.True, 
                $"Back button should be enabled in {BrowserName}");
            Assert.That(await _dashboardPage.IsNextButtonEnabledAsync(), Is.True, 
                $"Next button should be enabled in {BrowserName}");
        }

        [Test]
        [Description("Verify dashboard responsiveness across browsers")]
        public async Task Dashboard_Should_BeResponsive_AcrossBrowsers()
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
                
                // Wait for dashboard to adjust
                await Task.Delay(1000);
                
                // Verify dashboard is still accessible
                Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, 
                    $"Dashboard should be accessible in {BrowserName} at {viewport.Name} resolution ({viewport.Width}x{viewport.Height})");
                
                // Verify navigation buttons are enabled
                Assert.That(await _dashboardPage.IsBackButtonEnabledAsync(), Is.True, 
                    $"Back button should be enabled in {BrowserName} at {viewport.Name} resolution");
                Assert.That(await _dashboardPage.IsNextButtonEnabledAsync(), Is.True, 
                    $"Next button should be enabled in {BrowserName} at {viewport.Name} resolution");
            }
        }

        [Test]
        [Description("Verify dashboard performance across browsers")]
        public async Task Dashboard_Should_LoadQuickly_AcrossBrowsers()
        {
            // Measure dashboard load time
            var startTime = DateTime.Now;
            
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            var loadTime = DateTime.Now - startTime;
            
            // Assert load time is reasonable (less than 10 seconds for dashboard)
            Assert.That(loadTime.TotalSeconds, Is.LessThan(10.0), 
                $"Dashboard should load within 10 seconds in {BrowserName}. Actual load time: {loadTime.TotalSeconds:F2} seconds");
            
            TestContext.WriteLine($"Dashboard load time in {BrowserName}: {loadTime.TotalSeconds:F2} seconds");
        }

        [Test]
        [Description("Verify chart rendering performance across browsers")]
        public async Task Charts_Should_RenderQuickly_AcrossBrowsers()
        {
            // Wait for dashboard to load
            await _dashboardPage.WaitForDashboardLoadAsync();
            
            // Measure chart rendering time
            var startTime = DateTime.Now;
            
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            var renderTime = DateTime.Now - startTime;
            
            // Assert chart rendering time is reasonable (less than 5 seconds)
            Assert.That(renderTime.TotalSeconds, Is.LessThan(5.0), 
                $"Charts should render within 5 seconds in {BrowserName}. Actual render time: {renderTime.TotalSeconds:F2} seconds");
            
            TestContext.WriteLine($"Chart render time in {BrowserName}: {renderTime.TotalSeconds:F2} seconds");
        }

        [Test]
        [Description("Verify dashboard memory usage across browsers")]
        public async Task Dashboard_ShouldNotExceedMemoryLimits_AcrossBrowsers()
        {
            // Get initial memory usage
            var initialMemory = await Page.EvaluateAsync<long>("performance.memory.usedJSHeapSize");
            
            // Navigate to dashboard and wait for full load
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            // Perform some interactions
            await _dashboardPage.ScrollToAnalysisDivAsync();
            await Task.Delay(1000);
            
            // Get final memory usage
            var finalMemory = await Page.EvaluateAsync<long>("performance.memory.usedJSHeapSize");
            var memoryIncrease = finalMemory - initialMemory;
            
            // Assert memory increase is reasonable (less than 50MB for dashboard with charts)
            var memoryIncreaseMB = memoryIncrease / (1024 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(50.0), 
                $"Memory usage should not increase by more than 50MB in {BrowserName}. Actual increase: {memoryIncreaseMB:F2}MB");
            
            TestContext.WriteLine($"Dashboard memory usage increase in {BrowserName}: {memoryIncreaseMB:F2}MB");
        }

        [Test]
        [Description("Verify dashboard accessibility across browsers")]
        public async Task Dashboard_Should_BeAccessible_AcrossBrowsers()
        {
            // Wait for dashboard to load
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            // Test keyboard navigation
            await Page.Keyboard.PressAsync("Tab");
            await Task.Delay(500);
            
            // Verify focus is on a focusable element
            var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
            Assert.That(focusedElement, Is.Not.EqualTo("BODY"), 
                $"Tab navigation should focus on interactive element in {BrowserName}");
            
            // Test scroll functionality
            await _dashboardPage.ScrollToAnalysisDivAsync();
            await Task.Delay(500);
            
            // Verify scroll worked
            Assert.That(await _dashboardPage.IsAnalysisDivVisibleAsync(), Is.True, 
                $"Analysis div should be visible after scrolling in {BrowserName}");
        }

        [Test]
        [Description("Verify dashboard chart compatibility across browsers")]
        public async Task Dashboard_Should_SupportChartFeatures_AcrossBrowsers()
        {
            // Wait for dashboard to load
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            // Test if browser supports canvas (required for charts)
            var supportsCanvas = await Page.EvaluateAsync<bool>("typeof(HTMLCanvasElement) !== 'undefined'");
            var supportsWebGL = await Page.EvaluateAsync<bool>("typeof(WebGLRenderingContext) !== 'undefined'");
            var supports2DContext = await Page.EvaluateAsync<bool>("typeof(CanvasRenderingContext2D) !== 'undefined'");
            
            Assert.That(supportsCanvas, Is.True, 
                $"Canvas should be supported in {BrowserName}");
            Assert.That(supports2DContext, Is.True, 
                $"2D Canvas context should be supported in {BrowserName}");
            
            // WebGL support is optional but nice to have
            TestContext.WriteLine($"Chart feature support in {BrowserName}: Canvas={supportsCanvas}, WebGL={supportsWebGL}, 2DContext={supports2DContext}");
        }

        [Test]
        [Description("Verify dashboard error handling across browsers")]
        public async Task Dashboard_Should_HandleErrorsGracefully_AcrossBrowsers()
        {
            // Wait for dashboard to load
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();
            
            // Verify dashboard is functional even with potential errors
            Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, 
                $"Dashboard should be displayed in {BrowserName} even with potential errors");
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, 
                $"Dashboard should be initialized in {BrowserName} even with potential errors");
            
            // Check for any console errors
            var consoleErrors = await Page.EvaluateAsync<string[]>("window.consoleErrors || []");
            TestContext.WriteLine($"Console errors in {BrowserName}: {string.Join(", ", consoleErrors)}");
        }

        [Test]
        [Description("Verify dashboard loading states across browsers")]
        public async Task Dashboard_Should_HandleLoadingStates_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, 
                $"Dashboard should be initialized after loading in {BrowserName}");
            
            // Verify loading spinner is not visible
            Assert.That(await _dashboardPage.IsLoadingSpinnerVisibleAsync(), Is.False, 
                $"Loading spinner should not be visible after initialization in {BrowserName}");
        }

        [Test]
        [Description("Verify dashboard sections are properly organized across browsers")]
        public async Task Dashboard_Should_HaveProperlyOrganizedSections_AcrossBrowsers()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await WaitForBrowserSpecificLoadingAsync();

            // Assert
            // Check that at least one section is visible
            var hasAnySection = await _dashboardPage.IsAssessmentComplianceSectionVisibleAsync() ||
                               await _dashboardPage.IsRankedCategoriesSectionVisibleAsync() ||
                               await _dashboardPage.IsStandardsSummarySectionVisibleAsync() ||
                               await _dashboardPage.IsComponentsSummarySectionVisibleAsync();
            
            Assert.That(hasAnySection, Is.True, 
                $"At least one dashboard section should be visible in {BrowserName}");
        }
    }
} 