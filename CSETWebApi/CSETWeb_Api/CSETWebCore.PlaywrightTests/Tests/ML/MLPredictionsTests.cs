using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLPredictionsTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task Predictions_Page_Should_Load()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            await Expect(Page.Locator(".ml-predictions")).ToBeVisibleAsync();
            await Expect(Page.Locator(".prediction-form")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Predictions_Should_Show_Model_Selection()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Verify model selection dropdown
            await Expect(Page.Locator(".model-select")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-description")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Predictions_Should_Allow_Data_Input()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Test text input
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Sample assessment data for prediction");
                
                // Verify input is captured
                await Expect(textInput).ToHaveValueAsync("Sample assessment data for prediction");
            }
        }

        [Test]
        public async Task Predictions_Should_Allow_File_Upload()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Test file upload
            var fileInput = Page.Locator("input[type='file']");
            if (await fileInput.IsVisibleAsync())
            {
                await fileInput.SetInputFilesAsync("test-assessment-data.json");
                
                // Verify file is uploaded
                await Expect(Page.Locator(".uploaded-file")).ToBeVisibleAsync();
                await Expect(Page.Locator(".file-name")).ToContainTextAsync("test-assessment-data.json");
            }
        }

        [Test]
        public async Task Predictions_Should_Allow_JSON_Input()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Test JSON input
            var jsonInput = Page.Locator(".json-input");
            if (await jsonInput.IsVisibleAsync())
            {
                var testJson = @"{
                    ""assessment_id"": ""12345"",
                    ""facility_type"": ""critical_infrastructure"",
                    ""security_controls"": [""access_control"", ""network_security""]
                }";
                
                await jsonInput.FillAsync(testJson);
                
                // Verify JSON is valid
                await Expect(Page.Locator(".json-valid")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Generate_Results()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Fill in prediction form
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test assessment data");
                
                // Submit prediction
                var submitButton = Page.Locator(".prediction-submit");
                if (await submitButton.IsVisibleAsync())
                {
                    await submitButton.ClickAsync();
                    
                    // Wait for prediction results
                    await Expect(Page.Locator(".prediction-result")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".prediction-confidence")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Confidence_Scores()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Generate a prediction first
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test data");
                await Page.Locator(".prediction-submit").ClickAsync();
                
                // Verify confidence scores
                await Expect(Page.Locator(".confidence-score")).ToBeVisibleAsync();
                await Expect(Page.Locator(".confidence-bar")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Detailed_Analysis()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Generate a prediction
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test assessment");
                await Page.Locator(".prediction-submit").ClickAsync();
                
                // Verify detailed analysis
                await Expect(Page.Locator(".detailed-analysis")).ToBeVisibleAsync();
                await Expect(Page.Locator(".analysis-breakdown")).ToBeVisibleAsync();
                await Expect(Page.Locator(".feature-importance")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Allow_Result_Export()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Generate a prediction
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test data");
                await Page.Locator(".prediction-submit").ClickAsync();
                
                // Export results
                var exportButton = Page.Locator(".export-prediction");
                if (await exportButton.IsVisibleAsync())
                {
                    await exportButton.ClickAsync();
                    
                    // Verify export options
                    await Expect(Page.Locator(".export-options")).ToBeVisibleAsync();
                    
                    // Test PDF export
                    var pdfOption = Page.Locator(".export-pdf");
                    if (await pdfOption.IsVisibleAsync())
                    {
                        await pdfOption.ClickAsync();
                        await Expect(Page.Locator(".download-started")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Historical_Results()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to history tab
            var historyTab = Page.Locator(".history-tab");
            if (await historyTab.IsVisibleAsync())
            {
                await historyTab.ClickAsync();
                
                // Verify historical predictions
                await Expect(Page.Locator(".prediction-history")).ToBeVisibleAsync();
                await Expect(Page.Locator(".historical-item")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Allow_Batch_Processing()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Switch to batch mode
            var batchTab = Page.Locator(".batch-tab");
            if (await batchTab.IsVisibleAsync())
            {
                await batchTab.ClickAsync();
                
                // Upload batch file
                var batchFileInput = Page.Locator(".batch-file-input");
                if (await batchFileInput.IsVisibleAsync())
                {
                    await batchFileInput.SetInputFilesAsync("batch-assessments.csv");
                    
                    // Start batch processing
                    var startBatchButton = Page.Locator(".start-batch");
                    if (await startBatchButton.IsVisibleAsync())
                    {
                        await startBatchButton.ClickAsync();
                        
                        // Verify batch progress
                        await Expect(Page.Locator(".batch-progress")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".batch-status")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Model_Performance()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to performance tab
            var performanceTab = Page.Locator(".performance-tab");
            if (await performanceTab.IsVisibleAsync())
            {
                await performanceTab.ClickAsync();
                
                // Verify performance metrics
                await Expect(Page.Locator(".model-performance")).ToBeVisibleAsync();
                await Expect(Page.Locator(".accuracy-metric")).ToBeVisibleAsync();
                await Expect(Page.Locator(".precision-metric")).ToBeVisibleAsync();
                await Expect(Page.Locator(".recall-metric")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Handle_Invalid_Input()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Submit empty form
            var submitButton = Page.Locator(".prediction-submit");
            if (await submitButton.IsVisibleAsync())
            {
                await submitButton.ClickAsync();
                
                // Verify error message
                await Expect(Page.Locator(".error-message")).ToBeVisibleAsync();
                await Expect(Page.Locator(".validation-error")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Handle_Invalid_JSON()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Test invalid JSON input
            var jsonInput = Page.Locator(".json-input");
            if (await jsonInput.IsVisibleAsync())
            {
                await jsonInput.FillAsync("{ invalid json }");
                
                // Verify JSON validation error
                await Expect(Page.Locator(".json-error")).ToBeVisibleAsync();
                await Expect(Page.Locator(".json-invalid")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Loading_State()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Fill form and submit
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test data");
                await Page.Locator(".prediction-submit").ClickAsync();
                
                // Verify loading indicator
                await Expect(Page.Locator(".prediction-loading")).ToBeVisibleAsync();
                
                // Wait for results
                await Expect(Page.Locator(".prediction-result")).ToBeVisibleAsync();
                await Expect(Page.Locator(".prediction-loading")).ToBeHiddenAsync();
            }
        }

        [Test]
        public async Task Predictions_Should_Allow_Result_Comparison()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Generate multiple predictions
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                // First prediction
                await textInput.FillAsync("Test data 1");
                await Page.Locator(".prediction-submit").ClickAsync();
                await Expect(Page.Locator(".prediction-result")).ToBeVisibleAsync();
                
                // Second prediction
                await textInput.FillAsync("Test data 2");
                await Page.Locator(".prediction-submit").ClickAsync();
                await Expect(Page.Locator(".prediction-result")).ToBeVisibleAsync();
                
                // Compare results
                var compareButton = Page.Locator(".compare-results");
                if (await compareButton.IsVisibleAsync())
                {
                    await compareButton.ClickAsync();
                    
                    // Verify comparison view
                    await Expect(Page.Locator(".comparison-view")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".comparison-table")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Predictions_Should_Show_Model_Explanation()
        {
            await Page.GotoAsync("/ml/predictions");
            await Page.WaitForLoadStateAsync();
            
            // Generate a prediction
            var textInput = Page.Locator(".prediction-text-input");
            if (await textInput.IsVisibleAsync())
            {
                await textInput.FillAsync("Test assessment");
                await Page.Locator(".prediction-submit").ClickAsync();
                
                // View model explanation
                var explainButton = Page.Locator(".explain-prediction");
                if (await explainButton.IsVisibleAsync())
                {
                    await explainButton.ClickAsync();
                    
                    // Verify explanation
                    await Expect(Page.Locator(".model-explanation")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".explanation-chart")).ToBeVisibleAsync();
                }
            }
        }
    }
} 