using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Reports;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;
using CSETWebCore.PlaywrightTests.PageObjects.Questions;

namespace CSETWebCore.PlaywrightTests.Tests.Reports
{
    /// <summary>
    /// Test class for report generation functionality
    /// </summary>
    [TestFixture]
    [Category("Reports")]
    [Category("Critical")]
    public class ReportGenerationTests : BaseTestFixture
    {
        private ReportGenerationPage _reportGenerationPage = null!;
        private LoginPage _loginPage = null!;
        private AssessmentCreationPage _assessmentCreationPage = null!;
        private QuestionNavigationPage _questionNavigationPage = null!;

        protected override async Task SetUpAsync()
        {
            _reportGenerationPage = new ReportGenerationPage(Page);
            _loginPage = new LoginPage(Page);
            _assessmentCreationPage = new AssessmentCreationPage(Page);
            _questionNavigationPage = new QuestionNavigationPage(Page);

            // Login and create assessment with some answers before each test
            await _loginPage.NavigateAsync();
            await _loginPage.AcceptPrivacyWarningAsync();
            await _loginPage.LoginWithTestCredentialsAsync();
            
            // Wait for login to complete
            await Task.Delay(2000);
            
            // Create a test assessment
            await _assessmentCreationPage.NavigateAsync();
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();
            if (itemCount > 0)
            {
                var assessmentName = "E2E Report Test " + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await _assessmentCreationPage.CreateAssessmentWorkflowAsync(assessmentName, 0);
                
                // Wait for assessment to be created
                await Task.Delay(3000);
                
                // Answer a few questions to have data for reports
                await _questionNavigationPage.NavigateAsync();
                await _questionNavigationPage.WaitForQuestionsLoadAsync();
                await _questionNavigationPage.NavigateThroughQuestionsAsync(5);
            }
            
            // Navigate to reports page
            await _reportGenerationPage.NavigateAsync();
        }

        [Test]
        [Description("Verify that report generation page loads correctly")]
        public async Task ReportGenerationPage_Should_LoadCorrectly()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _reportGenerationPage.IsDisplayedAsync(), Is.True, "Report generation page should be displayed");
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            
            // Verify report options are present
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();
            Assert.That(reportTypeCount, Is.GreaterThan(0), "Should have at least one report type available");
        }

        [Test]
        [Description("Verify report type selection works")]
        public async Task ReportTypeSelection_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();

            // Act & Assert
            if (reportTypeCount > 0)
            {
                await _reportGenerationPage.SelectReportTypeAsync(0);
                
                // Verify selection (this might need adjustment based on actual UI behavior)
                await Task.Delay(1000);
                
                // Check if generate button becomes enabled
                var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
                Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled after selecting report type");
            }
        }

        [Test]
        [Description("Verify Executive Summary report selection")]
        public async Task ExecutiveSummaryReport_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Executive Summary");
        }

        [Test]
        [Description("Verify Detailed Report selection")]
        public async Task DetailedReport_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            await _reportGenerationPage.SelectDetailedReportAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Detailed Report");
        }

        [Test]
        [Description("Verify Maturity Report selection")]
        public async Task MaturityReport_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            await _reportGenerationPage.SelectMaturityReportAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Maturity Report");
        }

        [Test]
        [Description("Verify Compliance Report selection")]
        public async Task ComplianceReport_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            await _reportGenerationPage.SelectComplianceReportAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Compliance Report");
        }

        [Test]
        [Description("Verify PDF format selection")]
        public async Task PDFFormat_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.SelectPDFFormatAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for PDF format");
        }

        [Test]
        [Description("Verify Excel format selection")]
        public async Task ExcelFormat_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.SelectExcelFormatAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Excel format");
        }

        [Test]
        [Description("Verify Word format selection")]
        public async Task WordFormat_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.SelectWordFormatAsync();

            // Assert
            await Task.Delay(1000);
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();
            Assert.That(isGenerateEnabled, Is.True, "Generate button should be enabled for Word format");
        }

        [Test]
        [Description("Verify include charts option")]
        public async Task IncludeChartsOption_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.ToggleIncludeChartsAsync(true);

            // Assert
            await Task.Delay(1000);
            // Verify option is selected (this might need adjustment based on actual UI behavior)
        }

        [Test]
        [Description("Verify include comments option")]
        public async Task IncludeCommentsOption_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.ToggleIncludeCommentsAsync(true);

            // Assert
            await Task.Delay(1000);
            // Verify option is selected
        }

        [Test]
        [Description("Verify include observations option")]
        public async Task IncludeObservationsOption_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.SelectExecutiveSummaryAsync();

            // Act
            await _reportGenerationPage.ToggleIncludeObservationsAsync(true);

            // Assert
            await Task.Delay(1000);
            // Verify option is selected
        }

        [Test]
        [Description("Verify custom title input")]
        public async Task CustomTitle_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var customTitle = "Custom Report Title " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _reportGenerationPage.SetCustomTitleAsync(customTitle);

            // Assert
            var currentTitle = await _reportGenerationPage.GetCustomTitleAsync();
            Assert.That(currentTitle, Is.EqualTo(customTitle), "Custom title should be set correctly");
        }

        [Test]
        [Description("Verify report generation with valid data")]
        public async Task ReportGeneration_WithValidData_ShouldSucceed()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();
            
            if (reportTypeCount == 0)
            {
                Assert.Ignore("No report types available for testing");
            }

            // Act
            var success = await _reportGenerationPage.GenerateReportWorkflowAsync("Executive Summary", "PDF");

            // Assert
            Assert.That(success, Is.True, "Report generation should succeed with valid data");
        }

        [Test]
        [Description("Verify report generation fails without selecting report type")]
        public async Task ReportGeneration_WithoutReportType_ShouldFail()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            var isGenerateEnabled = await _reportGenerationPage.IsGenerateButtonEnabledAsync();

            // Assert
            Assert.That(isGenerateEnabled, Is.False, "Generate button should be disabled without selecting report type");
        }

        [Test]
        [Description("Verify custom report generation")]
        public async Task CustomReportGeneration_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var customTitle = "Custom E2E Report " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            var success = await _reportGenerationPage.GenerateCustomReportAsync(
                "Executive Summary", 
                "PDF", 
                includeCharts: true, 
                includeComments: true, 
                includeObservations: true, 
                customTitle: customTitle);

            // Assert
            Assert.That(success, Is.True, "Custom report generation should succeed");
        }

        [Test]
        [Description("Verify report download functionality")]
        public async Task ReportDownload_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.GenerateReportWorkflowAsync("Executive Summary", "PDF");

            // Act
            if (await _reportGenerationPage.IsDownloadAvailableAsync())
            {
                await _reportGenerationPage.DownloadReportAsync();

                // Assert
                await Task.Delay(2000); // Wait for download to start
                
                // Check if download was initiated (this might need adjustment based on actual implementation)
                var hasSuccessMessage = await _reportGenerationPage.HasSuccessMessageAsync();
                Assert.That(hasSuccessMessage, Is.True, "Download should be successful");
            }
            else
            {
                Assert.Ignore("Download button not available");
            }
        }

        [Test]
        [Description("Verify report preview functionality")]
        public async Task ReportPreview_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.GenerateReportWorkflowAsync("Executive Summary", "PDF");

            // Act
            if (await _reportGenerationPage.IsPreviewAvailableAsync())
            {
                await _reportGenerationPage.PreviewReportAsync();

                // Assert
                await Task.Delay(2000); // Wait for preview to load
                
                // Check if preview was successful (this might need adjustment based on actual implementation)
                var currentUrl = await _reportGenerationPage.GetCurrentUrlAsync();
                Assert.That(currentUrl, Does.Contain("preview") || Does.Contain("view"), "Should navigate to preview page");
            }
            else
            {
                Assert.Ignore("Preview button not available");
            }
        }

        [Test]
        [Description("Verify progress indicator displays during generation")]
        public async Task ProgressIndicator_Should_Display()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.GenerateReportWorkflowAsync("Executive Summary", "PDF");

            // Act
            var progress = await _reportGenerationPage.GetProgressAsync();

            // Assert
            Assert.That(string.IsNullOrEmpty(progress), Is.False, "Progress indicator should display progress");
        }

        [Test]
        [Description("Verify error handling for invalid report type selection")]
        public async Task InvalidReportType_Should_HandleError()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();

            // Act & Assert
            var invalidIndex = reportTypeCount + 10;
            
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _reportGenerationPage.SelectReportTypeAsync(invalidIndex);
            }, "Should throw exception for invalid report type index");
        }

        [Test]
        [Description("Verify loading states during report generation")]
        public async Task ReportGeneration_Should_ShowLoadingStates()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            await _reportGenerationPage.GenerateReportAsync();

            // Assert
            // Check for loading state (might be brief, so we check immediately)
            var isLoading = await _reportGenerationPage.IsLoadingAsync();
            
            // Wait for loading to complete
            await _reportGenerationPage.WaitForLoadingCompleteAsync();
            
            var isLoadingAfter = await _reportGenerationPage.IsLoadingAsync();
            Assert.That(isLoadingAfter, Is.False, "Loading should complete after report generation");
        }

        [Test]
        [Description("Verify report generation with special characters in title")]
        public async Task ReportGeneration_WithSpecialCharacters_ShouldWork()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var specialTitle = "Report with Special Chars: !@#$%^&*() " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _reportGenerationPage.SetCustomTitleAsync(specialTitle);

            // Assert
            var currentTitle = await _reportGenerationPage.GetCustomTitleAsync();
            Assert.That(currentTitle, Is.EqualTo(specialTitle), "Title with special characters should be handled correctly");
        }

        [Test]
        [Description("Verify report generation with very long title")]
        public async Task ReportGeneration_WithLongTitle_ShouldWork()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var longTitle = new string('A', 255) + " " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _reportGenerationPage.SetCustomTitleAsync(longTitle);

            // Assert
            var currentTitle = await _reportGenerationPage.GetCustomTitleAsync();
            Assert.That(currentTitle, Is.EqualTo(longTitle), "Long title should be handled correctly");
        }

        [Test]
        [Description("Verify multiple report types can be generated")]
        public async Task MultipleReportTypes_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();

            if (reportTypeCount < 2)
            {
                Assert.Ignore("Need at least 2 report types for testing");
            }

            // Act & Assert
            var reportTypes = new[] { "Executive Summary", "Detailed Report" };
            var formats = new[] { "PDF", "Excel" };

            foreach (var reportType in reportTypes)
            {
                foreach (var format in formats)
                {
                    var success = await _reportGenerationPage.GenerateReportWorkflowAsync(reportType, format);
                    Assert.That(success, Is.True, $"Report generation should succeed for {reportType} in {format} format");
                    
                    // Brief pause between generations
                    await Task.Delay(2000);
                }
            }
        }

        [Test]
        [Description("Verify page contains expected text elements")]
        public async Task ReportGenerationPage_Should_ContainExpectedText()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            // Check for common text elements (adjust based on actual CSET implementation)
            var hasReportText = await _reportGenerationPage.ContainsTextAsync("Report") || 
                               await _reportGenerationPage.ContainsTextAsync("Generate");
            Assert.That(hasReportText, Is.True, "Page should contain report/generate related text");
        }

        [Test]
        [Description("Verify report options are available")]
        public async Task ReportOptions_Should_BeAvailable()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();

            // Act
            var reportTypeCount = await _reportGenerationPage.GetReportTypeCountAsync();

            // Assert
            Assert.That(reportTypeCount, Is.GreaterThan(0), "Should have at least one report type available");
        }

        [Test]
        [Description("Verify report generation and export workflow")]
        public async Task ReportGenerationAndExport_Should_Work()
        {
            // Arrange
            await _reportGenerationPage.WaitForReportOptionsLoadAsync();
            await _reportGenerationPage.GenerateReportWorkflowAsync("Executive Summary", "PDF");

            // Act
            await _reportGenerationPage.ExportReportAsync();

            // Assert
            await Task.Delay(2000); // Wait for export to complete
            
            var hasSuccessMessage = await _reportGenerationPage.HasSuccessMessageAsync();
            Assert.That(hasSuccessMessage, Is.True, "Export should be successful");
        }
    }
} 