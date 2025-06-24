using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.Reports
{
    /// <summary>
    /// Page object for the CSET report generation page
    /// </summary>
    public class ReportGenerationPage : BasePageObject
    {
        public override string PageUrl => "/assessment/reports";

        // Locators
        private ILocator ReportContainer => Page.Locator(".report-container, [data-testid='report-container']");
        private ILocator ReportTypeSelector => Page.Locator(".report-type-selector, [data-testid='report-type']");
        private ILocator ReportOptions => Page.Locator(".report-option, [data-testid='report-option']");
        private ILocator GenerateButton => Page.Locator("button:has-text('Generate'), [data-testid='generate-report']");
        private ILocator ExportButton => Page.Locator("button:has-text('Export'), [data-testid='export-report']");
        private ILocator DownloadButton => Page.Locator("button:has-text('Download'), [data-testid='download-report']");
        private ILocator PreviewButton => Page.Locator("button:has-text('Preview'), [data-testid='preview-report']");
        private ILocator LoadingSpinner => Page.Locator(".loading, .spinner, [data-testid='loading']");
        private ILocator ErrorMessage => Page.Locator(".error, .alert-danger, [data-testid='error-message']");
        private ILocator SuccessMessage => Page.Locator(".success, .alert-success, [data-testid='success-message']");
        private ILocator ProgressBar => Page.Locator(".progress-bar, [data-testid='progress']");

        // Report type specific locators
        private ILocator ExecutiveSummaryOption => Page.Locator("[data-testid='executive-summary'], input[value='executive']");
        private ILocator DetailedReportOption => Page.Locator("[data-testid='detailed-report'], input[value='detailed']");
        private ILocator MaturityReportOption => Page.Locator("[data-testid='maturity-report'], input[value='maturity']");
        private ILocator ComplianceReportOption => Page.Locator("[data-testid='compliance-report'], input[value='compliance']");

        // Format options
        private ILocator PDFFormatOption => Page.Locator("[data-testid='pdf-format'], input[value='pdf']");
        private ILocator ExcelFormatOption => Page.Locator("[data-testid='excel-format'], input[value='excel']");
        private ILocator WordFormatOption => Page.Locator("[data-testid='word-format'], input[value='word']");

        // Customization options
        private ILocator IncludeChartsCheckbox => Page.Locator("[data-testid='include-charts'], input[type='checkbox']");
        private ILocator IncludeCommentsCheckbox => Page.Locator("[data-testid='include-comments'], input[type='checkbox']");
        private ILocator IncludeObservationsCheckbox => Page.Locator("[data-testid='include-observations'], input[type='checkbox']");
        private ILocator CustomTitleInput => Page.Locator("[data-testid='custom-title'], input[name='title']");

        public ReportGenerationPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await ReportContainer.IsVisibleAsync() && await GenerateButton.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for report options to load
        /// </summary>
        public async Task WaitForReportOptionsLoadAsync()
        {
            await WaitForVisibleAsync(ReportContainer);
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Get the number of available report types
        /// </summary>
        public async Task<int> GetReportTypeCountAsync()
        {
            return await ReportOptions.CountAsync();
        }

        /// <summary>
        /// Select a report type by index
        /// </summary>
        public async Task SelectReportTypeAsync(int index)
        {
            var options = ReportOptions;
            if (await options.CountAsync() > index)
            {
                await options.Nth(index).ClickAsync();
                await WaitForLoadingAsync();
            }
            else
            {
                throw new ArgumentException($"Report type at index {index} does not exist");
            }
        }

        /// <summary>
        /// Select Executive Summary report
        /// </summary>
        public async Task SelectExecutiveSummaryAsync()
        {
            await ExecutiveSummaryOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Detailed Report
        /// </summary>
        public async Task SelectDetailedReportAsync()
        {
            await DetailedReportOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Maturity Report
        /// </summary>
        public async Task SelectMaturityReportAsync()
        {
            await MaturityReportOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Compliance Report
        /// </summary>
        public async Task SelectComplianceReportAsync()
        {
            await ComplianceReportOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select PDF format
        /// </summary>
        public async Task SelectPDFFormatAsync()
        {
            await PDFFormatOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Excel format
        /// </summary>
        public async Task SelectExcelFormatAsync()
        {
            await ExcelFormatOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Word format
        /// </summary>
        public async Task SelectWordFormatAsync()
        {
            await WordFormatOption.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Toggle include charts option
        /// </summary>
        public async Task ToggleIncludeChartsAsync(bool include = true)
        {
            var isChecked = await IncludeChartsCheckbox.IsCheckedAsync();
            if (isChecked != include)
            {
                await IncludeChartsCheckbox.ClickAsync();
            }
        }

        /// <summary>
        /// Toggle include comments option
        /// </summary>
        public async Task ToggleIncludeCommentsAsync(bool include = true)
        {
            var isChecked = await IncludeCommentsCheckbox.IsCheckedAsync();
            if (isChecked != include)
            {
                await IncludeCommentsCheckbox.ClickAsync();
            }
        }

        /// <summary>
        /// Toggle include observations option
        /// </summary>
        public async Task ToggleIncludeObservationsAsync(bool include = true)
        {
            var isChecked = await IncludeObservationsCheckbox.IsCheckedAsync();
            if (isChecked != include)
            {
                await IncludeObservationsCheckbox.ClickAsync();
            }
        }

        /// <summary>
        /// Set custom report title
        /// </summary>
        public async Task SetCustomTitleAsync(string title)
        {
            await FillAsync(CustomTitleInput, title);
        }

        /// <summary>
        /// Get the current custom title
        /// </summary>
        public async Task<string> GetCustomTitleAsync()
        {
            return await GetAttributeAsync(CustomTitleInput, "value") ?? string.Empty;
        }

        /// <summary>
        /// Check if generate button is enabled
        /// </summary>
        public async Task<bool> IsGenerateButtonEnabledAsync()
        {
            return await GenerateButton.IsEnabledAsync();
        }

        /// <summary>
        /// Generate the report
        /// </summary>
        public async Task GenerateReportAsync()
        {
            await GenerateButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Export the report
        /// </summary>
        public async Task ExportReportAsync()
        {
            await ExportButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Download the report
        /// </summary>
        public async Task DownloadReportAsync()
        {
            await DownloadButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Preview the report
        /// </summary>
        public async Task PreviewReportAsync()
        {
            await PreviewButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Get the current progress percentage
        /// </summary>
        public async Task<string> GetProgressAsync()
        {
            return await GetTextAsync(ProgressBar);
        }

        /// <summary>
        /// Check if there's an error message displayed
        /// </summary>
        public async Task<bool> HasErrorMessageAsync()
        {
            return await ErrorMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Get the error message text
        /// </summary>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await HasErrorMessageAsync())
            {
                return await ErrorMessage.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if there's a success message displayed
        /// </summary>
        public async Task<bool> HasSuccessMessageAsync()
        {
            return await SuccessMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Get the success message text
        /// </summary>
        public async Task<string> GetSuccessMessageAsync()
        {
            if (await HasSuccessMessageAsync())
            {
                return await SuccessMessage.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Wait for report generation to complete
        /// </summary>
        public async Task<bool> WaitForReportGenerationAsync()
        {
            try
            {
                // Wait for loading to complete
                await WaitForHiddenAsync(LoadingSpinner);
                
                // Wait for success message or download button
                await Page.WaitForSelectorAsync("[data-testid='download-report'], .success", 
                    new PageWaitForSelectorOptions { Timeout = 60000 });
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Complete a full report generation workflow
        /// </summary>
        public async Task<bool> GenerateReportWorkflowAsync(string reportType = "Executive Summary", string format = "PDF")
        {
            try
            {
                // Wait for report options to load
                await WaitForReportOptionsLoadAsync();

                // Select report type
                switch (reportType.ToLower())
                {
                    case "executive summary":
                        await SelectExecutiveSummaryAsync();
                        break;
                    case "detailed report":
                        await SelectDetailedReportAsync();
                        break;
                    case "maturity report":
                        await SelectMaturityReportAsync();
                        break;
                    case "compliance report":
                        await SelectComplianceReportAsync();
                        break;
                    default:
                        await SelectReportTypeAsync(0); // Default to first option
                        break;
                }

                // Select format
                switch (format.ToLower())
                {
                    case "pdf":
                        await SelectPDFFormatAsync();
                        break;
                    case "excel":
                        await SelectExcelFormatAsync();
                        break;
                    case "word":
                        await WordFormatOption.ClickAsync();
                        break;
                    default:
                        await SelectPDFFormatAsync(); // Default to PDF
                        break;
                }

                // Verify generate button is enabled
                if (!await IsGenerateButtonEnabledAsync())
                {
                    return false;
                }

                // Generate report
                await GenerateReportAsync();

                // Wait for generation to complete
                return await WaitForReportGenerationAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate report with custom options
        /// </summary>
        public async Task<bool> GenerateCustomReportAsync(string reportType, string format, bool includeCharts = true, 
            bool includeComments = true, bool includeObservations = true, string customTitle = "")
        {
            try
            {
                // Wait for report options to load
                await WaitForReportOptionsLoadAsync();

                // Select report type and format
                await GenerateReportWorkflowAsync(reportType, format);

                // Set custom options
                await ToggleIncludeChartsAsync(includeCharts);
                await ToggleIncludeCommentsAsync(includeComments);
                await ToggleIncludeObservationsAsync(includeObservations);

                if (!string.IsNullOrEmpty(customTitle))
                {
                    await SetCustomTitleAsync(customTitle);
                }

                // Generate report
                await GenerateReportAsync();

                // Wait for generation to complete
                return await WaitForReportGenerationAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the page is in a loading state
        /// </summary>
        public async Task<bool> IsLoadingAsync()
        {
            return await LoadingSpinner.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for any loading operations to complete
        /// </summary>
        public async Task WaitForLoadingCompleteAsync()
        {
            await WaitForHiddenAsync(LoadingSpinner);
        }

        /// <summary>
        /// Get the current URL to verify navigation
        /// </summary>
        public async Task<string> GetCurrentUrlAsync()
        {
            return Page.Url;
        }

        /// <summary>
        /// Check if the page contains specific text
        /// </summary>
        public async Task<bool> ContainsTextAsync(string text)
        {
            return await Page.GetByText(text).IsVisibleAsync();
        }

        /// <summary>
        /// Take a screenshot for debugging
        /// </summary>
        public async Task TakeScreenshotAsync(string name = "report-generation")
        {
            await TakeScreenshotAsync(name);
        }

        /// <summary>
        /// Wait for file download to complete
        /// </summary>
        public async Task<IDownload> WaitForDownloadAsync()
        {
            return await Page.WaitForDownloadAsync();
        }

        /// <summary>
        /// Check if download button is available
        /// </summary>
        public async Task<bool> IsDownloadAvailableAsync()
        {
            return await DownloadButton.IsVisibleAsync();
        }

        /// <summary>
        /// Check if preview button is available
        /// </summary>
        public async Task<bool> IsPreviewAvailableAsync()
        {
            return await PreviewButton.IsVisibleAsync();
        }
    }
} 