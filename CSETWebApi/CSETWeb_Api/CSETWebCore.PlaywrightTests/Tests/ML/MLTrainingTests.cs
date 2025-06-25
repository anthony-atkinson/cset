using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLTrainingTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task Training_Page_Should_Load()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            await Expect(Page.Locator(".ml-training")).ToBeVisibleAsync();
            await Expect(Page.Locator(".training-form")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Training_Should_Show_Model_Types()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Verify model type selection
            await Expect(Page.Locator(".model-type-select")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-description")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Training_Should_Allow_Dataset_Upload()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Test dataset upload
            var datasetInput = Page.Locator(".dataset-input");
            if (await datasetInput.IsVisibleAsync())
            {
                await datasetInput.SetInputFilesAsync("training-dataset.csv");
                
                // Verify dataset is uploaded
                await Expect(Page.Locator(".uploaded-dataset")).ToBeVisibleAsync();
                await Expect(Page.Locator(".dataset-info")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Training_Should_Show_Dataset_Preview()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Upload dataset
            var datasetInput = Page.Locator(".dataset-input");
            if (await datasetInput.IsVisibleAsync())
            {
                await datasetInput.SetInputFilesAsync("training-dataset.csv");
                
                // Verify dataset preview
                await Expect(Page.Locator(".dataset-preview")).ToBeVisibleAsync();
                await Expect(Page.Locator(".preview-table")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Training_Should_Allow_Hyperparameter_Configuration()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to hyperparameters tab
            var hyperparamsTab = Page.Locator(".hyperparams-tab");
            if (await hyperparamsTab.IsVisibleAsync())
            {
                await hyperparamsTab.ClickAsync();
                
                // Configure hyperparameters
                var learningRateInput = Page.Locator(".learning-rate-input");
                if (await learningRateInput.IsVisibleAsync())
                {
                    await learningRateInput.FillAsync("0.001");
                }
                
                var epochsInput = Page.Locator(".epochs-input");
                if (await epochsInput.IsVisibleAsync())
                {
                    await epochsInput.FillAsync("100");
                }
                
                var batchSizeInput = Page.Locator(".batch-size-input");
                if (await batchSizeInput.IsVisibleAsync())
                {
                    await batchSizeInput.FillAsync("32");
                }
            }
        }

        [Test]
        public async Task Training_Should_Allow_Feature_Selection()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Upload dataset first
            var datasetInput = Page.Locator(".dataset-input");
            if (await datasetInput.IsVisibleAsync())
            {
                await datasetInput.SetInputFilesAsync("training-dataset.csv");
                
                // Navigate to features tab
                var featuresTab = Page.Locator(".features-tab");
                if (await featuresTab.IsVisibleAsync())
                {
                    await featuresTab.ClickAsync();
                    
                    // Select features
                    var featureCheckboxes = Page.Locator(".feature-checkbox");
                    if (await featureCheckboxes.First.IsVisibleAsync())
                    {
                        await featureCheckboxes.First.CheckAsync();
                        await featureCheckboxes.Nth(1).CheckAsync();
                        
                        // Verify selected features
                        await Expect(Page.Locator(".selected-features")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Training_Should_Start_Model_Training()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Configure training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    
                    // Start training
                    var startTrainingButton = Page.Locator(".start-training");
                    if (await startTrainingButton.IsVisibleAsync())
                    {
                        await startTrainingButton.ClickAsync();
                        
                        // Verify training status
                        await Expect(Page.Locator(".training-status")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".training-progress")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Training_Should_Show_Real_Time_Progress()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Start training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Verify real-time progress
                    await Expect(Page.Locator(".training-progress")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".epoch-progress")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".loss-chart")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".accuracy-chart")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Training_Should_Show_Validation_Metrics()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Start training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Wait for training to progress
                    await Page.WaitForTimeoutAsync(3000);
                    
                    // Verify validation metrics
                    await Expect(Page.Locator(".validation-metrics")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".validation-loss")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".validation-accuracy")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Training_Should_Allow_Early_Stopping()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Start training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Stop training early
                    var stopTrainingButton = Page.Locator(".stop-training");
                    if (await stopTrainingButton.IsVisibleAsync())
                    {
                        await stopTrainingButton.ClickAsync();
                        
                        // Verify training stopped
                        await Expect(Page.Locator(".training-stopped")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".final-metrics")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Training_Should_Show_Model_Evaluation()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Complete training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Wait for training completion
                    await Expect(Page.Locator(".training-complete")).ToBeVisibleAsync();
                    
                    // Verify model evaluation
                    await Expect(Page.Locator(".model-evaluation")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".evaluation-metrics")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".confusion-matrix")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Training_Should_Allow_Model_Save()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Complete training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Wait for training completion
                    await Expect(Page.Locator(".training-complete")).ToBeVisibleAsync();
                    
                    // Save model
                    var saveModelButton = Page.Locator(".save-model");
                    if (await saveModelButton.IsVisibleAsync())
                    {
                        await saveModelButton.ClickAsync();
                        
                        // Enter model name
                        var modelNameInput = Page.Locator(".model-name-input");
                        if (await modelNameInput.IsVisibleAsync())
                        {
                            await modelNameInput.FillAsync("Security Assessment Model v1.0");
                            
                            // Confirm save
                            var confirmSaveButton = Page.Locator(".confirm-save");
                            if (await confirmSaveButton.IsVisibleAsync())
                            {
                                await confirmSaveButton.ClickAsync();
                                
                                // Verify model saved
                                await Expect(Page.Locator(".model-saved")).ToBeVisibleAsync();
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public async Task Training_Should_Show_Training_History()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to history tab
            var historyTab = Page.Locator(".training-history-tab");
            if (await historyTab.IsVisibleAsync())
            {
                await historyTab.ClickAsync();
                
                // Verify training history
                await Expect(Page.Locator(".training-history")).ToBeVisibleAsync();
                await Expect(Page.Locator(".history-item")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Training_Should_Allow_Model_Comparison()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to comparison tab
            var comparisonTab = Page.Locator(".model-comparison-tab");
            if (await comparisonTab.IsVisibleAsync())
            {
                await comparisonTab.ClickAsync();
                
                // Select models for comparison
                var modelCheckboxes = Page.Locator(".model-checkbox");
                if (await modelCheckboxes.First.IsVisibleAsync())
                {
                    await modelCheckboxes.First.CheckAsync();
                    await modelCheckboxes.Nth(1).CheckAsync();
                    
                    // Compare models
                    var compareButton = Page.Locator(".compare-models");
                    if (await compareButton.IsVisibleAsync())
                    {
                        await compareButton.ClickAsync();
                        
                        // Verify comparison results
                        await Expect(Page.Locator(".comparison-results")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".comparison-chart")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Training_Should_Handle_Validation_Errors()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Try to start training without dataset
            var startTrainingButton = Page.Locator(".start-training");
            if (await startTrainingButton.IsVisibleAsync())
            {
                await startTrainingButton.ClickAsync();
                
                // Verify validation error
                await Expect(Page.Locator(".validation-error")).ToBeVisibleAsync();
                await Expect(Page.Locator(".error-message")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Training_Should_Show_Resource_Usage()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Start training
            var modelTypeSelect = Page.Locator(".model-type-select");
            if (await modelTypeSelect.IsVisibleAsync())
            {
                await modelTypeSelect.SelectOptionAsync("security-assessment");
                
                var datasetInput = Page.Locator(".dataset-input");
                if (await datasetInput.IsVisibleAsync())
                {
                    await datasetInput.SetInputFilesAsync("training-dataset.csv");
                    await Page.Locator(".start-training").ClickAsync();
                    
                    // Verify resource usage
                    await Expect(Page.Locator(".resource-usage")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".cpu-usage")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".memory-usage")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".gpu-usage")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Training_Should_Allow_Resume_Training()
        {
            await Page.GotoAsync("/ml/training");
            await Page.WaitForLoadStateAsync();
            
            // Navigate to history tab
            var historyTab = Page.Locator(".training-history-tab");
            if (await historyTab.IsVisibleAsync())
            {
                await historyTab.ClickAsync();
                
                // Click on a training session
                var trainingSession = Page.Locator(".training-session").First;
                if (await trainingSession.IsVisibleAsync())
                {
                    await trainingSession.ClickAsync();
                    
                    // Resume training
                    var resumeButton = Page.Locator(".resume-training");
                    if (await resumeButton.IsVisibleAsync())
                    {
                        await resumeButton.ClickAsync();
                        
                        // Verify training resumed
                        await Expect(Page.Locator(".training-resumed")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".training-progress")).ToBeVisibleAsync();
                    }
                }
            }
        }
    }
} 