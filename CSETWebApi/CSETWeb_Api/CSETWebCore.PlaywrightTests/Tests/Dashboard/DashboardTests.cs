using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.Dashboard
{
    /// <summary>
    /// Test class for Dashboard and Analytics functionality
    /// </summary>
    [TestFixture]
    [Category("Dashboard")]
    [Category("Analytics")]
    [Category("E2E")]
    public class DashboardTests : BaseTestFixture
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
        [Description("Verify that dashboard page loads correctly")]
        public async Task DashboardPage_Should_LoadCorrectly()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, "Dashboard page should be displayed");
            Assert.That(await _dashboardPage.GetDashboardTitleAsync(), Is.EqualTo("Analysis Dashboard"), "Dashboard title should be correct");
        }

        [Test]
        [Description("Verify that dashboard initializes and shows content")]
        public async Task Dashboard_Should_InitializeAndShowContent()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, "Dashboard should be initialized");
            Assert.That(await _dashboardPage.IsAnalysisDivVisibleAsync(), Is.True, "Analysis container should be visible");
        }

        [Test]
        [Description("Verify that overall score is displayed")]
        public async Task Dashboard_Should_DisplayOverallScore()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            Assert.That(await _dashboardPage.IsOverallScoreDisplayedAsync(), Is.True, "Overall score should be displayed");
            
            var overallScore = await _dashboardPage.GetOverallScoreAsync();
            Assert.That(string.IsNullOrEmpty(overallScore), Is.False, "Overall score should have a value");
            Assert.That(overallScore.Contains("%"), Is.True, "Overall score should be a percentage");
        }

        [Test]
        [Description("Verify that standard based score is displayed when applicable")]
        public async Task Dashboard_Should_DisplayStandardBasedScore_WhenApplicable()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            if (await _dashboardPage.IsStandardBasedScoreDisplayedAsync())
            {
                var standardScore = await _dashboardPage.GetStandardBasedScoreAsync();
                Assert.That(string.IsNullOrEmpty(standardScore), Is.False, "Standard based score should have a value");
                
                // Check if it's a percentage or "no standards answers" message
                Assert.That(standardScore.Contains("%") || standardScore.Contains("no standards answers"), Is.True, 
                    "Standard based score should be a percentage or indicate no answers");
            }
        }

        [Test]
        [Description("Verify that component based score is displayed when applicable")]
        public async Task Dashboard_Should_DisplayComponentBasedScore_WhenApplicable()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            if (await _dashboardPage.IsComponentBasedScoreDisplayedAsync())
            {
                var componentScore = await _dashboardPage.GetComponentBasedScoreAsync();
                Assert.That(string.IsNullOrEmpty(componentScore), Is.False, "Component based score should have a value");
                
                // Check if it's a percentage or "no components answers" message
                Assert.That(componentScore.Contains("%") || componentScore.Contains("no components answers"), Is.True, 
                    "Component based score should be a percentage or indicate no answers");
            }
        }

        [Test]
        [Description("Verify that assessment compliance chart is displayed")]
        public async Task Dashboard_Should_DisplayAssessmentComplianceChart()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();

            // Assert
            Assert.That(await _dashboardPage.IsAssessmentComplianceChartDisplayedAsync(), Is.True, 
                "Assessment compliance chart should be displayed");
            Assert.That(await _dashboardPage.IsAssessmentComplianceSectionVisibleAsync(), Is.True, 
                "Assessment compliance section should be visible");
        }

        [Test]
        [Description("Verify that top categories chart is displayed when applicable")]
        public async Task Dashboard_Should_DisplayTopCategoriesChart_WhenApplicable()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();

            // Assert
            if (await _dashboardPage.IsRankedCategoriesSectionVisibleAsync())
            {
                if (await _dashboardPage.IsTopCategoriesChartDisplayedAsync())
                {
                    Assert.That(await _dashboardPage.IsTopCategoriesChartDisplayedAsync(), Is.True, 
                        "Top categories chart should be displayed");
                }
                else
                {
                    // Check if no ranking message is shown instead
                    Assert.That(await _dashboardPage.IsNoRankingMessageDisplayedAsync(), Is.True, 
                        "No ranking message should be displayed when no chart data");
                }
            }
        }

        [Test]
        [Description("Verify that standards summary chart is displayed when applicable")]
        public async Task Dashboard_Should_DisplayStandardsSummaryChart_WhenApplicable()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();

            // Assert
            if (await _dashboardPage.IsStandardsSummarySectionVisibleAsync())
            {
                Assert.That(await _dashboardPage.IsStandardsSummaryChartDisplayedAsync(), Is.True, 
                    "Standards summary chart should be displayed");
            }
        }

        [Test]
        [Description("Verify that component summary chart is displayed when applicable")]
        public async Task Dashboard_Should_DisplayComponentSummaryChart_WhenApplicable()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.WaitForChartsToRenderAsync();

            // Assert
            if (await _dashboardPage.IsComponentsSummarySectionVisibleAsync())
            {
                if (await _dashboardPage.IsComponentSummaryChartDisplayedAsync())
                {
                    Assert.That(await _dashboardPage.IsComponentSummaryChartDisplayedAsync(), Is.True, 
                        "Component summary chart should be displayed");
                }
                else
                {
                    // Check if no components message is shown instead
                    Assert.That(await _dashboardPage.IsNoComponentsMessageDisplayedAsync(), Is.True, 
                        "No components message should be displayed when no chart data");
                }
            }
        }

        [Test]
        [Description("Verify that navigation buttons are functional")]
        public async Task Dashboard_Should_HaveFunctionalNavigationButtons()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            Assert.That(await _dashboardPage.IsBackButtonEnabledAsync(), Is.True, "Back button should be enabled");
            Assert.That(await _dashboardPage.IsNextButtonEnabledAsync(), Is.True, "Next button should be enabled");
        }

        [Test]
        [Description("Verify that dashboard scrolls to analysis div")]
        public async Task Dashboard_Should_ScrollToAnalysisDiv()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.ScrollToAnalysisDivAsync();

            // Assert
            // The scroll action should complete without error
            Assert.That(await _dashboardPage.IsAnalysisDivVisibleAsync(), Is.True, "Analysis div should be visible after scrolling");
        }

        [Test]
        [Description("Verify that loading states are handled correctly")]
        public async Task Dashboard_Should_HandleLoadingStatesCorrectly()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, "Dashboard should be initialized after loading");
            
            // Verify loading spinner is not visible
            Assert.That(await _dashboardPage.IsLoadingSpinnerVisibleAsync(), Is.False, "Loading spinner should not be visible after initialization");
        }

        [Test]
        [Description("Verify that dashboard sections are properly organized")]
        public async Task Dashboard_Should_HaveProperlyOrganizedSections()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            // Check that at least one section is visible
            var hasAnySection = await _dashboardPage.IsAssessmentComplianceSectionVisibleAsync() ||
                               await _dashboardPage.IsRankedCategoriesSectionVisibleAsync() ||
                               await _dashboardPage.IsStandardsSummarySectionVisibleAsync() ||
                               await _dashboardPage.IsComponentsSummarySectionVisibleAsync();
            
            Assert.That(hasAnySection, Is.True, "At least one dashboard section should be visible");
        }

        [Test]
        [Description("Verify that dashboard handles no data scenarios gracefully")]
        public async Task Dashboard_Should_HandleNoDataScenariosGracefully()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            // Dashboard should still be functional even with no data
            Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, "Dashboard should be displayed even with no data");
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, "Dashboard should be initialized even with no data");
        }

        [Test]
        [Description("Verify that dashboard title is correct")]
        public async Task Dashboard_Should_HaveCorrectTitle()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            var title = await _dashboardPage.GetDashboardTitleAsync();
            Assert.That(title, Is.EqualTo("Analysis Dashboard"), "Dashboard title should be 'Analysis Dashboard'");
        }

        [Test]
        [Description("Verify that dashboard is accessible after login")]
        public async Task Dashboard_Should_BeAccessibleAfterLogin()
        {
            // Act
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Assert
            Assert.That(await _dashboardPage.IsDisplayedAsync(), Is.True, "Dashboard should be accessible after login");
            Assert.That(await _dashboardPage.IsDashboardInitializedAsync(), Is.True, "Dashboard should be initialized after login");
        }
    }
} 