using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.Dashboard
{
    /// <summary>
    /// Page object for the CSET Dashboard and Analytics page
    /// </summary>
    public class DashboardPage : BasePageObject
    {
        public override string PageUrl => "/assessment/results/analysis/dashboard";

        // Locators for Dashboard elements
        private ILocator DashboardTitle => Page.Locator("h3:has-text('Analysis Dashboard'), [data-testid='dashboard-title']");
        private ILocator OverallScoreContainer => Page.Locator("#overall-score, [data-testid='overall-score']");
        private ILocator StandardBasedScoreContainer => Page.Locator("#standard-score, [data-testid='standard-score']");
        private ILocator ComponentBasedScoreContainer => Page.Locator("#component-score, [data-testid='component-score']");
        
        // Chart locators
        private ILocator AssessmentComplianceChart => Page.Locator("#canvasAssessmentCompliance, [data-testid='assessment-compliance-chart']");
        private ILocator TopCategoriesChart => Page.Locator("#canvasTopCategories, [data-testid='top-categories-chart']");
        private ILocator StandardsSummaryChart => Page.Locator("#canvasStandardSummary, [data-testid='standards-summary-chart']");
        private ILocator ComponentSummaryChart => Page.Locator("#canvasComponentSummary, [data-testid='component-summary-chart']");
        
        // Section headers
        private ILocator AssessmentComplianceHeader => Page.Locator("h3:has-text('Assessment Compliance'), [data-testid='assessment-compliance-header']");
        private ILocator RankedCategoriesHeader => Page.Locator("h3:has-text('Ranked Categories'), [data-testid='ranked-categories-header']");
        private ILocator StandardsSummaryHeader => Page.Locator("h3:has-text('Standards Summary'), [data-testid='standards-summary-header']");
        private ILocator ComponentsSummaryHeader => Page.Locator("h3:has-text('Components Summary'), [data-testid='components-summary-header']");
        
        // Loading indicators
        private ILocator LoadingSpinner => Page.Locator(".spinner-container, [data-testid='loading-spinner']");
        private ILocator AnalysisDiv => Page.Locator("#analysisDiv, [data-testid='analysis-container']");
        
        // Navigation
        private ILocator BackButton => Page.Locator("button:has-text('Back'), [data-testid='back-button']");
        private ILocator NextButton => Page.Locator("button:has-text('Next'), [data-testid='next-button']");
        
        // No data messages
        private ILocator NoRankingMessage => Page.Locator(".fst-italic:has-text('no ranking'), [data-testid='no-ranking-message']");
        private ILocator NoComponentsMessage => Page.Locator("div:has-text('no components on the diagram'), [data-testid='no-components-message']");

        public DashboardPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await DashboardTitle.IsVisibleAsync() && await AnalysisDiv.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for dashboard to load completely
        /// </summary>
        public async Task WaitForDashboardLoadAsync()
        {
            await DashboardTitle.WaitForAsync();
            await AnalysisDiv.WaitForAsync();
            
            // Wait for loading to complete
            await WaitForLoadingToCompleteAsync();
        }

        /// <summary>
        /// Wait for loading spinner to disappear
        /// </summary>
        public async Task WaitForLoadingToCompleteAsync()
        {
            try
            {
                await LoadingSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 30000 });
            }
            catch
            {
                // Loading might already be complete
            }
        }

        /// <summary>
        /// Check if overall score is displayed
        /// </summary>
        public async Task<bool> IsOverallScoreDisplayedAsync()
        {
            return await OverallScoreContainer.IsVisibleAsync();
        }

        /// <summary>
        /// Get overall score value
        /// </summary>
        public async Task<string> GetOverallScoreAsync()
        {
            if (await OverallScoreContainer.IsVisibleAsync())
            {
                var scoreElement = OverallScoreContainer.Locator("div:last-child");
                return await scoreElement.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if standard based score is displayed
        /// </summary>
        public async Task<bool> IsStandardBasedScoreDisplayedAsync()
        {
            return await StandardBasedScoreContainer.IsVisibleAsync();
        }

        /// <summary>
        /// Get standard based score value
        /// </summary>
        public async Task<string> GetStandardBasedScoreAsync()
        {
            if (await StandardBasedScoreContainer.IsVisibleAsync())
            {
                var scoreElement = StandardBasedScoreContainer.Locator("div:last-child");
                return await scoreElement.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if component based score is displayed
        /// </summary>
        public async Task<bool> IsComponentBasedScoreDisplayedAsync()
        {
            return await ComponentBasedScoreContainer.IsVisibleAsync();
        }

        /// <summary>
        /// Get component based score value
        /// </summary>
        public async Task<string> GetComponentBasedScoreAsync()
        {
            if (await ComponentBasedScoreContainer.IsVisibleAsync())
            {
                var scoreElement = ComponentBasedScoreContainer.Locator("div:last-child");
                return await scoreElement.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if assessment compliance chart is displayed
        /// </summary>
        public async Task<bool> IsAssessmentComplianceChartDisplayedAsync()
        {
            return await AssessmentComplianceChart.IsVisibleAsync();
        }

        /// <summary>
        /// Check if top categories chart is displayed
        /// </summary>
        public async Task<bool> IsTopCategoriesChartDisplayedAsync()
        {
            return await TopCategoriesChart.IsVisibleAsync();
        }

        /// <summary>
        /// Check if standards summary chart is displayed
        /// </summary>
        public async Task<bool> IsStandardsSummaryChartDisplayedAsync()
        {
            return await StandardsSummaryChart.IsVisibleAsync();
        }

        /// <summary>
        /// Check if component summary chart is displayed
        /// </summary>
        public async Task<bool> IsComponentSummaryChartDisplayedAsync()
        {
            return await ComponentSummaryChart.IsVisibleAsync();
        }

        /// <summary>
        /// Check if assessment compliance section is visible
        /// </summary>
        public async Task<bool> IsAssessmentComplianceSectionVisibleAsync()
        {
            return await AssessmentComplianceHeader.IsVisibleAsync();
        }

        /// <summary>
        /// Check if ranked categories section is visible
        /// </summary>
        public async Task<bool> IsRankedCategoriesSectionVisibleAsync()
        {
            return await RankedCategoriesHeader.IsVisibleAsync();
        }

        /// <summary>
        /// Check if standards summary section is visible
        /// </summary>
        public async Task<bool> IsStandardsSummarySectionVisibleAsync()
        {
            return await StandardsSummaryHeader.IsVisibleAsync();
        }

        /// <summary>
        /// Check if components summary section is visible
        /// </summary>
        public async Task<bool> IsComponentsSummarySectionVisibleAsync()
        {
            return await ComponentsSummaryHeader.IsVisibleAsync();
        }

        /// <summary>
        /// Check if no ranking message is displayed
        /// </summary>
        public async Task<bool> IsNoRankingMessageDisplayedAsync()
        {
            return await NoRankingMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Check if no components message is displayed
        /// </summary>
        public async Task<bool> IsNoComponentsMessageDisplayedAsync()
        {
            return await NoComponentsMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Click back button
        /// </summary>
        public async Task ClickBackButtonAsync()
        {
            await BackButton.ClickAsync();
        }

        /// <summary>
        /// Click next button
        /// </summary>
        public async Task ClickNextButtonAsync()
        {
            await NextButton.ClickAsync();
        }

        /// <summary>
        /// Check if back button is enabled
        /// </summary>
        public async Task<bool> IsBackButtonEnabledAsync()
        {
            return await BackButton.IsEnabledAsync();
        }

        /// <summary>
        /// Check if next button is enabled
        /// </summary>
        public async Task<bool> IsNextButtonEnabledAsync()
        {
            return await NextButton.IsEnabledAsync();
        }

        /// <summary>
        /// Wait for charts to be rendered
        /// </summary>
        public async Task WaitForChartsToRenderAsync()
        {
            // Wait for canvas elements to be present
            await AssessmentComplianceChart.WaitForAsync();
            
            // Wait a bit for charts to render
            await Task.Delay(2000);
        }

        /// <summary>
        /// Scroll to analysis div
        /// </summary>
        public async Task ScrollToAnalysisDivAsync()
        {
            await AnalysisDiv.ScrollIntoViewIfNeededAsync();
        }

        /// <summary>
        /// Get dashboard title text
        /// </summary>
        public async Task<string> GetDashboardTitleAsync()
        {
            return await DashboardTitle.TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if dashboard is initialized (not showing loading)
        /// </summary>
        public async Task<bool> IsDashboardInitializedAsync()
        {
            try
            {
                // Wait for loading to complete
                await WaitForLoadingToCompleteAsync();
                
                // Check if main content is visible
                return await DashboardTitle.IsVisibleAsync() && 
                       await AnalysisDiv.IsVisibleAsync() &&
                       !(await LoadingSpinner.IsVisibleAsync());
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if analysis div is visible
        /// </summary>
        public async Task<bool> IsAnalysisDivVisibleAsync()
        {
            return await AnalysisDiv.IsVisibleAsync();
        }

        /// <summary>
        /// Check if loading spinner is visible
        /// </summary>
        public async Task<bool> IsLoadingSpinnerVisibleAsync()
        {
            return await LoadingSpinner.IsVisibleAsync();
        }
    }
} 