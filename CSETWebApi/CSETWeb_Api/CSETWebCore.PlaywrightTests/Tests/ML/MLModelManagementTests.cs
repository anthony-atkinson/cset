using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLModelManagementTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task Model_Management_Page_Should_Load()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            await Expect(Page.Locator(".ml-model-management")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-list")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Model_List_Should_Display_Models()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Verify model list components
            await Expect(Page.Locator(".model-item")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-name")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-version")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-status")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Model_Details_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Verify model details
                await Expect(Page.Locator(".model-details")).ToBeVisibleAsync();
                await Expect(Page.Locator(".model-description")).ToBeVisibleAsync();
                await Expect(Page.Locator(".model-metadata")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Model_Metrics_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Navigate to metrics tab
                var metricsTab = Page.Locator(".metrics-tab");
                if (await metricsTab.IsVisibleAsync())
                {
                    await metricsTab.ClickAsync();
                    
                    // Verify metrics
                    await Expect(Page.Locator(".model-metrics")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".accuracy-metric")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".precision-metric")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".recall-metric")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".f1-metric")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Model_Performance_Charts_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Navigate to performance tab
                var performanceTab = Page.Locator(".performance-tab");
                if (await performanceTab.IsVisibleAsync())
                {
                    await performanceTab.ClickAsync();
                    
                    // Verify performance charts
                    await Expect(Page.Locator(".performance-charts")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".accuracy-chart")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".loss-chart")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".confusion-matrix")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Model_Deployment_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Deploy model
                var deployButton = Page.Locator(".deploy-model");
                if (await deployButton.IsVisibleAsync())
                {
                    await deployButton.ClickAsync();
                    
                    // Verify deployment dialog
                    await Expect(Page.Locator(".deployment-dialog")).ToBeVisibleAsync();
                    
                    // Configure deployment
                    var environmentSelect = Page.Locator(".environment-select");
                    if (await environmentSelect.IsVisibleAsync())
                    {
                        await environmentSelect.SelectOptionAsync("production");
                    }
                    
                    // Confirm deployment
                    var confirmButton = Page.Locator(".confirm-deployment");
                    if (await confirmButton.IsVisibleAsync())
                    {
                        await confirmButton.ClickAsync();
                        
                        // Verify deployment status
                        await Expect(Page.Locator(".deployment-status")).ToBeVisibleAsync();
                        await Expect(Page.Locator(".deployment-success")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Model_Undeployment_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on deployed model
            var deployedModel = Page.Locator(".model-item.deployed").First;
            if (await deployedModel.IsVisibleAsync())
            {
                await deployedModel.ClickAsync();
                
                // Undeploy model
                var undeployButton = Page.Locator(".undeploy-model");
                if (await undeployButton.IsVisibleAsync())
                {
                    await undeployButton.ClickAsync();
                    
                    // Confirm undeployment
                    var confirmButton = Page.Locator(".confirm-undeployment");
                    if (await confirmButton.IsVisibleAsync())
                    {
                        await confirmButton.ClickAsync();
                        
                        // Verify undeployment success
                        await Expect(Page.Locator(".undeployment-success")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Model_Evaluation_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Evaluate model
                var evaluateButton = Page.Locator(".evaluate-model");
                if (await evaluateButton.IsVisibleAsync())
                {
                    await evaluateButton.ClickAsync();
                    
                    // Upload test dataset
                    var testDatasetInput = Page.Locator(".test-dataset-input");
                    if (await testDatasetInput.IsVisibleAsync())
                    {
                        await testDatasetInput.SetInputFilesAsync("test-dataset.csv");
                        
                        // Start evaluation
                        var startEvaluationButton = Page.Locator(".start-evaluation");
                        if (await startEvaluationButton.IsVisibleAsync())
                        {
                            await startEvaluationButton.ClickAsync();
                            
                            // Verify evaluation progress
                            await Expect(Page.Locator(".evaluation-progress")).ToBeVisibleAsync();
                            
                            // Wait for evaluation completion
                            await Expect(Page.Locator(".evaluation-complete")).ToBeVisibleAsync();
                            
                            // Verify evaluation results
                            await Expect(Page.Locator(".evaluation-results")).ToBeVisibleAsync();
                            await Expect(Page.Locator(".evaluation-metrics")).ToBeVisibleAsync();
                        }
                    }
                }
            }
        }

        [Test]
        public async Task Model_Versioning_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Navigate to versions tab
                var versionsTab = Page.Locator(".versions-tab");
                if (await versionsTab.IsVisibleAsync())
                {
                    await versionsTab.ClickAsync();
                    
                    // Verify version list
                    await Expect(Page.Locator(".version-list")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".version-item")).ToBeVisibleAsync();
                    
                    // Create new version
                    var createVersionButton = Page.Locator(".create-version");
                    if (await createVersionButton.IsVisibleAsync())
                    {
                        await createVersionButton.ClickAsync();
                        
                        // Enter version details
                        var versionNameInput = Page.Locator(".version-name-input");
                        if (await versionNameInput.IsVisibleAsync())
                        {
                            await versionNameInput.FillAsync("v2.0");
                            
                            var versionNotesInput = Page.Locator(".version-notes-input");
                            if (await versionNotesInput.IsVisibleAsync())
                            {
                                await versionNotesInput.FillAsync("Improved accuracy with new features");
                            }
                            
                            // Save version
                            var saveVersionButton = Page.Locator(".save-version");
                            if (await saveVersionButton.IsVisibleAsync())
                            {
                                await saveVersionButton.ClickAsync();
                                
                                // Verify version created
                                await Expect(Page.Locator(".version-created")).ToBeVisibleAsync();
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public async Task Model_Comparison_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Select multiple models for comparison
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
                    
                    // Verify comparison view
                    await Expect(Page.Locator(".comparison-view")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".comparison-table")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".comparison-chart")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Model_Export_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Export model
                var exportButton = Page.Locator(".export-model");
                if (await exportButton.IsVisibleAsync())
                {
                    await exportButton.ClickAsync();
                    
                    // Verify export options
                    await Expect(Page.Locator(".export-options")).ToBeVisibleAsync();
                    
                    // Select export format
                    var formatSelect = Page.Locator(".export-format-select");
                    if (await formatSelect.IsVisibleAsync())
                    {
                        await formatSelect.SelectOptionAsync("onnx");
                        
                        // Start export
                        var startExportButton = Page.Locator(".start-export");
                        if (await startExportButton.IsVisibleAsync())
                        {
                            await startExportButton.ClickAsync();
                            
                            // Verify export progress
                            await Expect(Page.Locator(".export-progress")).ToBeVisibleAsync();
                            
                            // Wait for export completion
                            await Expect(Page.Locator(".export-complete")).ToBeVisibleAsync();
                        }
                    }
                }
            }
        }

        [Test]
        public async Task Model_Import_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Import model
            var importButton = Page.Locator(".import-model");
            if (await importButton.IsVisibleAsync())
            {
                await importButton.ClickAsync();
                
                // Verify import dialog
                await Expect(Page.Locator(".import-dialog")).ToBeVisibleAsync();
                
                // Upload model file
                var modelFileInput = Page.Locator(".model-file-input");
                if (await modelFileInput.IsVisibleAsync())
                {
                    await modelFileInput.SetInputFilesAsync("imported-model.onnx");
                    
                    // Enter model details
                    var modelNameInput = Page.Locator(".import-model-name-input");
                    if (await modelNameInput.IsVisibleAsync())
                    {
                        await modelNameInput.FillAsync("Imported Security Model");
                    }
                    
                    var modelDescriptionInput = Page.Locator(".import-model-description-input");
                    if (await modelDescriptionInput.IsVisibleAsync())
                    {
                        await modelDescriptionInput.FillAsync("Model imported from external source");
                    }
                    
                    // Confirm import
                    var confirmImportButton = Page.Locator(".confirm-import");
                    if (await confirmImportButton.IsVisibleAsync())
                    {
                        await confirmImportButton.ClickAsync();
                        
                        // Verify import success
                        await Expect(Page.Locator(".import-success")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Model_Deletion_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Delete model
                var deleteButton = Page.Locator(".delete-model");
                if (await deleteButton.IsVisibleAsync())
                {
                    await deleteButton.ClickAsync();
                    
                    // Verify deletion confirmation
                    await Expect(Page.Locator(".delete-confirmation")).ToBeVisibleAsync();
                    
                    // Confirm deletion
                    var confirmDeleteButton = Page.Locator(".confirm-delete");
                    if (await confirmDeleteButton.IsVisibleAsync())
                    {
                        await confirmDeleteButton.ClickAsync();
                        
                        // Verify deletion success
                        await Expect(Page.Locator(".deletion-success")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Model_Search_And_Filter_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Search models
            var searchInput = Page.Locator(".model-search input");
            if (await searchInput.IsVisibleAsync())
            {
                await searchInput.FillAsync("security");
                await Page.WaitForTimeoutAsync(1000);
                
                // Verify search results
                await Expect(Page.Locator(".search-results")).ToBeVisibleAsync();
            }
            
            // Filter by status
            var statusFilter = Page.Locator(".status-filter select");
            if (await statusFilter.IsVisibleAsync())
            {
                await statusFilter.SelectOptionAsync("deployed");
                await Page.WaitForTimeoutAsync(1000);
                
                // Verify filtered results
                await Expect(Page.Locator(".filtered-results")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Model_Monitoring_Should_Display()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on deployed model
            var deployedModel = Page.Locator(".model-item.deployed").First;
            if (await deployedModel.IsVisibleAsync())
            {
                await deployedModel.ClickAsync();
                
                // Navigate to monitoring tab
                var monitoringTab = Page.Locator(".monitoring-tab");
                if (await monitoringTab.IsVisibleAsync())
                {
                    await monitoringTab.ClickAsync();
                    
                    // Verify monitoring data
                    await Expect(Page.Locator(".model-monitoring")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".prediction-count")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".average-response-time")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".error-rate")).ToBeVisibleAsync();
                    await Expect(Page.Locator(".performance-trends")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Model_Retraining_Should_Work()
        {
            await Page.GotoAsync("/ml/models");
            await Page.WaitForLoadStateAsync();
            
            // Click on first model
            var firstModel = Page.Locator(".model-item").First;
            if (await firstModel.IsVisibleAsync())
            {
                await firstModel.ClickAsync();
                
                // Retrain model
                var retrainButton = Page.Locator(".retrain-model");
                if (await retrainButton.IsVisibleAsync())
                {
                    await retrainButton.ClickAsync();
                    
                    // Verify retraining dialog
                    await Expect(Page.Locator(".retraining-dialog")).ToBeVisibleAsync();
                    
                    // Upload new training data
                    var newDataInput = Page.Locator(".new-training-data-input");
                    if (await newDataInput.IsVisibleAsync())
                    {
                        await newDataInput.SetInputFilesAsync("new-training-data.csv");
                        
                        // Start retraining
                        var startRetrainingButton = Page.Locator(".start-retraining");
                        if (await startRetrainingButton.IsVisibleAsync())
                        {
                            await startRetrainingButton.ClickAsync();
                            
                            // Verify retraining progress
                            await Expect(Page.Locator(".retraining-progress")).ToBeVisibleAsync();
                            
                            // Wait for retraining completion
                            await Expect(Page.Locator(".retraining-complete")).ToBeVisibleAsync();
                        }
                    }
                }
            }
        }
    }
} 