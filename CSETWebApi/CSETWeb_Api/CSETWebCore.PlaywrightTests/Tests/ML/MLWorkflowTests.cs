using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLWorkflowTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            // Navigate to the base URL before each test
            await Page.GotoAsync("/");
            
            // Wait for the application to load
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task ML_Dashboard_Should_Display()
        {
            // Navigate to the ML dashboard route
            await Page.GotoAsync("/ml/dashboard");

            // Wait for the dashboard main container or title to be visible
            var dashboardTitle = await Page.Locator("h1, h2, .ml-dashboard-title").First.OrNullAsync();
            Assert.IsNotNull(dashboardTitle, "ML Dashboard title or main container should be present");
            
            // Verify dashboard components are present
            await Expect(Page.Locator(".ml-dashboard")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Dashboard_Should_Show_Key_Metrics()
        {
            await Page.GotoAsync("/ml/dashboard");
            
            // Wait for dashboard to load
            await Page.WaitForLoadStateAsync();
            
            // Verify key metrics sections are present
            await Expect(Page.Locator(".ml-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".ml-performance")).ToBeVisibleAsync();
            await Expect(Page.Locator(".ml-models")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Recommendations_Should_Display()
        {
            await Page.GotoAsync("/ml/recommendations");
            
            // Wait for recommendations page to load
            await Page.WaitForLoadStateAsync();
            
            // Verify recommendations components
            await Expect(Page.Locator(".ml-recommendations")).ToBeVisibleAsync();
            await Expect(Page.Locator(".recommendation-list")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Recommendations_Should_Allow_Filtering()
        {
            await Page.GotoAsync("/ml/recommendations");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Test filtering functionality
            var filterInput = Page.Locator(".recommendation-filter input");
            if (await filterInput.IsVisibleAsync())
            {
                await filterInput.FillAsync("security");
                await Page.WaitForTimeoutAsync(1000); // Wait for filter to apply
                
                // Verify filtered results
                await Expect(Page.Locator(".recommendation-item")).ToHaveCountAsync(1);
            }
        }

        [Test]
        public async Task ML_Predictions_Should_Display()
        {
            await Page.GotoAsync("/ml/predictions");
            
            // Wait for predictions page to load
            await Page.WaitForLoadStateAsync();
            
            // Verify predictions components
            await Expect(Page.Locator(".ml-predictions")).ToBeVisibleAsync();
            await Expect(Page.Locator(".prediction-form")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Predictions_Should_Allow_Input()
        {
            await Page.GotoAsync("/ml/predictions");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Test input form
            var inputField = Page.Locator(".prediction-input");
            if (await inputField.IsVisibleAsync())
            {
                await inputField.FillAsync("test assessment data");
                
                // Test submit button
                var submitButton = Page.Locator(".prediction-submit");
                if (await submitButton.IsVisibleAsync())
                {
                    await submitButton.ClickAsync();
                    
                    // Wait for prediction result
                    await Expect(Page.Locator(".prediction-result")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task ML_Model_Management_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            
            // Wait for model management page to load
            await Page.WaitForLoadStateAsync();
            
            // Verify model management components
            await Expect(Page.Locator(".ml-model-management")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-list")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Model_Management_Should_Show_Model_Details()
        {
            await Page.GotoAsync("/ml/models");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Click on first model if available
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Verify model details are shown
                await Expect(Page.Locator(".model-details")).ToBeVisibleAsync();
                await Expect(Page.Locator(".model-metrics")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task ML_Model_Training_Should_Display()
        {
            await Page.GotoAsync("/ml/training");
            
            // Wait for training page to load
            await Page.WaitForLoadStateAsync();
            
            // Verify training components
            await Expect(Page.Locator(".ml-training")).ToBeVisibleAsync();
            await Expect(Page.Locator(".training-form")).ToBeVisibleAsync();
        }

        [Test]
        public async Task ML_Model_Training_Should_Allow_Configuration()
        {
            await Page.GotoAsync("/ml/training");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Test training configuration
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.FillAsync("test-dataset.csv");
                }
                
                // Test start training button
                var startTrainingButton = Page.Locator(".start-training");
                if (await startTrainingButton.IsVisibleAsync())
                {
                    await startTrainingButton.ClickAsync();
                    
                    // Wait for training status
                    await Expect(Page.Locator(".training-status")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task ML_Workflow_Navigation_Should_Work()
        {
            // Test navigation between ML pages
            await Page.GotoAsync("/ml/dashboard");
            
            // Navigate to recommendations
            await Page.ClickAsync("text=Recommendations");
            await Expect(Page).ToHaveURLAsync("**/ml/recommendations");
            
            // Navigate to predictions
            await Page.ClickAsync("text=Predictions");
            await Expect(Page).ToHaveURLAsync("**/ml/predictions");
            
            // Navigate to models
            await Page.ClickAsync("text=Models");
            await Expect(Page).ToHaveURLAsync("**/ml/models");
            
            // Navigate to training
            await Page.ClickAsync("text=Training");
            await Expect(Page).ToHaveURLAsync("**/ml/training");
        }

        [Test]
        public async Task ML_Data_Upload_Should_Work()
        {
            await Page.GotoAsync("/ml/training");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Test file upload functionality
            var fileInput = Page.Locator("input[type='file']");
            if (await fileInput.IsVisibleAsync())
            {
                // Create a test file for upload
                await fileInput.SetInputFilesAsync("test-data.csv");
                
                // Verify file is uploaded
                await Expect(Page.Locator(".uploaded-file")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task ML_Model_Evaluation_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Click on evaluate button for first model
            var evaluateButton = Page.Locator(".evaluate-model").First;
            if (await evaluateButton.IsVisibleAsync())
            {
                await evaluateButton.ClickAsync();
                
                // Verify evaluation results
                await Expect(Page.Locator(".evaluation-results")).ToBeVisibleAsync();
                await Expect(Page.Locator(".evaluation-metrics")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task ML_Model_Deployment_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Click on deploy button for first model
            var deployButton = Page.Locator(".deploy-model").First;
            if (await deployButton.IsVisibleAsync())
            {
                await deployButton.ClickAsync();
                
                // Verify deployment dialog
                await Expect(Page.Locator(".deployment-dialog")).ToBeVisibleAsync();
                
                // Confirm deployment
                var confirmButton = Page.Locator(".confirm-deployment");
                if (await confirmButton.IsVisibleAsync())
                {
                    await confirmButton.ClickAsync();
                    
                    // Verify deployment status
                    await Expect(Page.Locator(".deployment-status")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task ML_Error_Handling_Should_Work()
        {
            // Test error handling for invalid inputs
            await Page.GotoAsync("/ml/predictions");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Submit empty form
            var submitButton = Page.Locator(".prediction-submit");
            if (await submitButton.IsVisibleAsync())
            {
                await submitButton.ClickAsync();
                
                // Verify error message is displayed
                await Expect(Page.Locator(".error-message")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task ML_Loading_States_Should_Display()
        {
            await Page.GotoAsync("/ml/dashboard");
            
            // Wait for page to load
            await Page.WaitForLoadStateAsync();
            
            // Verify loading indicators are present during data fetch
            await Expect(Page.Locator(".loading-indicator")).ToBeVisibleAsync();
            
            // Wait for content to load
            await Page.WaitForTimeoutAsync(2000);
            
            // Verify loading indicator is hidden
            await Expect(Page.Locator(".loading-indicator")).ToBeHiddenAsync();
        }

        [Test]
        public async Task ML_Responsive_Design_Should_Work()
        {
            // Test responsive design on different screen sizes
            await Page.SetViewportSizeAsync(375, 667); // Mobile size
            await Page.GotoAsync("/ml/dashboard");
            
            // Verify mobile layout
            await Expect(Page.Locator(".mobile-menu")).ToBeVisibleAsync();
            
            // Test tablet size
            await Page.SetViewportSizeAsync(768, 1024);
            await Page.ReloadAsync();
            
            // Verify tablet layout
            await Expect(Page.Locator(".tablet-layout")).ToBeVisibleAsync();
            
            // Test desktop size
            await Page.SetViewportSizeAsync(1920, 1080);
            await Page.ReloadAsync();
            
            // Verify desktop layout
            await Expect(Page.Locator(".desktop-layout")).ToBeVisibleAsync();
        }
    }
} 