using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.VisualRegression
{
    /// <summary>
    /// Visual regression tests for Dashboard and Analytics
    /// </summary>
    [TestFixture]
    [Category("VisualRegression")]
    [Category("Dashboard")]
    [Category("Analytics")]
    [Category("E2E")]
    public class VisualRegressionDashboardTests : VisualRegressionTestFixture
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
            
            // Wait for dashboard to load and stabilize
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();
            await WaitForVisualStabilityAsync();
        }

        [Test]
        [Description("Verify dashboard page visual consistency")]
        public async Task DashboardPage_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_page_default", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard page should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard full page visual consistency")]
        public async Task DashboardPage_FullPage_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_page_fullpage", "body", true);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard full page should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard header visual consistency")]
        public async Task DashboardHeader_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_header", "header", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard header should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard title visual consistency")]
        public async Task DashboardTitle_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_title", "h3", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard title should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify overall score section visual consistency")]
        public async Task OverallScoreSection_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_overall_score", "#overall-score", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Overall score section should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify assessment compliance chart visual consistency")]
        public async Task AssessmentComplianceChart_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_assessment_compliance_chart", "#canvasAssessmentCompliance", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Assessment compliance chart should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify top categories chart visual consistency")]
        public async Task TopCategoriesChart_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_top_categories_chart", "#canvasTopCategories", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Top categories chart should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify standards summary chart visual consistency")]
        public async Task StandardsSummaryChart_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_standards_summary_chart", "#canvasStandardSummary", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Standards summary chart should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify component summary chart visual consistency")]
        public async Task ComponentSummaryChart_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_component_summary_chart", "#canvasComponentSummary", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Component summary chart should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify navigation buttons visual consistency")]
        public async Task NavigationButtons_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_navigation_buttons", "button", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Navigation buttons should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard responsive design at desktop resolution")]
        public async Task Dashboard_DesktopResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(1920, 1080);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_desktop", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard at desktop resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard responsive design at laptop resolution")]
        public async Task Dashboard_LaptopResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(1366, 768);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_laptop", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard at laptop resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard responsive design at tablet resolution")]
        public async Task Dashboard_TabletResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(768, 1024);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_tablet", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard at tablet resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard responsive design at mobile resolution")]
        public async Task Dashboard_MobileResolution_Should_MatchBaseline()
        {
            // Arrange
            await Page.SetViewportSizeAsync(375, 667);
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_mobile", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard at mobile resolution should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard analysis container visual consistency")]
        public async Task AnalysisContainer_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_analysis_container", "#analysisDiv", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Analysis container should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard loading state visual consistency")]
        public async Task Dashboard_LoadingState_Should_MatchBaseline()
        {
            // Navigate to dashboard fresh to capture loading state
            await _dashboardPage.NavigateAsync();
            
            // Take screenshot during loading
            var result = await CompareScreenshotAsync("dashboard_loading", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard loading state should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard with no data state")]
        public async Task Dashboard_NoDataState_Should_MatchBaseline()
        {
            // This test would need to be adjusted based on how to trigger a no-data state
            // For now, we'll test the current state
            
            // Act
            var result = await CompareScreenshotAsync("dashboard_no_data", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard no data state should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard chart interactions visual consistency")]
        public async Task Dashboard_ChartInteractions_Should_MatchBaseline()
        {
            // Arrange
            // Hover over a chart element
            await Page.HoverAsync("#canvasAssessmentCompliance");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_chart_interaction", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard chart interactions should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard accessibility focus indicators")]
        public async Task Dashboard_AccessibilityFocus_Should_MatchBaseline()
        {
            // Arrange
            await Page.Keyboard.PressAsync("Tab");
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_accessibility_focus", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard with accessibility focus should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard with custom threshold")]
        public async Task Dashboard_CustomThreshold_Should_MatchBaseline()
        {
            // Arrange
            SetVisualThreshold(0.05); // 5% threshold

            // Act
            var result = await CompareScreenshotAsync("dashboard_custom_threshold", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard with custom threshold should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard chart legends visual consistency")]
        public async Task Dashboard_ChartLegends_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_chart_legends", "canvas", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard chart legends should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard section headers visual consistency")]
        public async Task Dashboard_SectionHeaders_Should_MatchBaseline()
        {
            // Act
            var result = await CompareScreenshotAsync("dashboard_section_headers", "h3", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard section headers should match baseline in {BrowserName}. {result.GetSummary()}");
        }

        [Test]
        [Description("Verify dashboard with scroll position")]
        public async Task Dashboard_WithScroll_Should_MatchBaseline()
        {
            // Arrange
            await _dashboardPage.ScrollToAnalysisDivAsync();
            await WaitForVisualStabilityAsync();

            // Act
            var result = await CompareScreenshotAsync("dashboard_scrolled", "body", false);

            // Assert
            Assert.That(result.IsMatch, Is.True, 
                $"Dashboard with scroll should match baseline in {BrowserName}. {result.GetSummary()}");
        }
    }
} 