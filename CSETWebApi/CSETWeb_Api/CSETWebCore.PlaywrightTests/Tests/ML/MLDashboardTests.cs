using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLDashboardTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task Dashboard_Page_Should_Load()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            await Expect(Page.Locator(".ml-dashboard")).ToBeVisibleAsync();
            await Expect(Page.Locator(".dashboard-header")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Key_Metrics()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify key metrics sections
            await Expect(Page.Locator(".ml-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".total-models")).ToBeVisibleAsync();
            await Expect(Page.Locator(".active-models")).ToBeVisibleAsync();
            await Expect(Page.Locator(".total-predictions")).ToBeVisibleAsync();
            await Expect(Page.Locator(".accuracy-score")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Performance_Charts()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify performance charts
            await Expect(Page.Locator(".ml-performance")).ToBeVisibleAsync();
            await Expect(Page.Locator(".accuracy-trend")).ToBeVisibleAsync();
            await Expect(Page.Locator(".prediction-volume")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-performance")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Model_Status()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify model status section
            await Expect(Page.Locator(".ml-models")).ToBeVisibleAsync();
            await Expect(Page.Locator(".model-status-grid")).ToBeVisibleAsync();
            await Expect(Page.Locator(".deployed-models")).ToBeVisibleAsync();
            await Expect(Page.Locator(".training-models")).ToBeVisibleAsync();
            await Expect(Page.Locator(".error-models")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Recent_Activity()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify recent activity section
            await Expect(Page.Locator(".recent-activity")).ToBeVisibleAsync();
            await Expect(Page.Locator(".activity-list")).ToBeVisibleAsync();
            await Expect(Page.Locator(".activity-item")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Quick_Actions()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify quick actions
            await Expect(Page.Locator(".quick-actions")).ToBeVisibleAsync();
            await Expect(Page.Locator(".action-button")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Quick_Model_Training()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click quick train button
            var quickTrainButton = Page.Locator(".quick-train-button");
            if (await quickTrainButton.IsVisibleAsync())
            {
                await quickTrainButton.ClickAsync();
                
                // Verify training dialog
                await Expect(Page.Locator(".quick-train-dialog")).ToBeVisibleAsync();
                
                // Select model type
                var modelTypeSelect = Page.Locator(".quick-model-type");
                if (await modelTypeSelect.IsVisibleAsync())
                {
                    await modelTypeSelect.SelectOptionAsync("security-assessment");
                    
                    // Start quick training
                    var startButton = Page.Locator(".start-quick-train");
                    if (await startButton.IsVisibleAsync())
                    {
                        await startButton.ClickAsync();
                        
                        // Verify training started
                        await Expect(Page.Locator(".quick-training-started")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Allow_Quick_Prediction()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click quick prediction button
            var quickPredictButton = Page.Locator(".quick-predict-button");
            if (await quickPredictButton.IsVisibleAsync())
            {
                await quickPredictButton.ClickAsync();
                
                // Verify prediction dialog
                await Expect(Page.Locator(".quick-predict-dialog")).ToBeVisibleAsync();
                
                // Enter prediction data
                var predictionInput = Page.Locator(".quick-prediction-input");
                if (await predictionInput.IsVisibleAsync())
                {
                    await predictionInput.FillAsync("Test assessment data");
                    
                    // Submit prediction
                    var submitButton = Page.Locator(".submit-quick-prediction");
                    if (await submitButton.IsVisibleAsync())
                    {
                        await submitButton.ClickAsync();
                        
                        // Verify prediction result
                        await Expect(Page.Locator(".quick-prediction-result")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_System_Health()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify system health section
            await Expect(Page.Locator(".system-health")).ToBeVisibleAsync();
            await Expect(Page.Locator(".health-indicators")).ToBeVisibleAsync();
            await Expect(Page.Locator(".cpu-usage")).ToBeVisibleAsync();
            await Expect(Page.Locator(".memory-usage")).ToBeVisibleAsync();
            await Expect(Page.Locator(".gpu-usage")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Alerts()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify alerts section
            await Expect(Page.Locator(".ml-alerts")).ToBeVisibleAsync();
            await Expect(Page.Locator(".alert-list")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Alert_Acknowledgment()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click on first alert
            var firstAlert = Page.Locator(".alert-item").First;
            if (await firstAlert.IsVisibleAsync())
            {
                await firstAlert.ClickAsync();
                
                // Acknowledge alert
                var acknowledgeButton = Page.Locator(".acknowledge-alert");
                if (await acknowledgeButton.IsVisibleAsync())
                {
                    await acknowledgeButton.ClickAsync();
                    
                    // Verify alert acknowledged
                    await Expect(Page.Locator(".alert-acknowledged")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Data_Quality_Metrics()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify data quality section
            await Expect(Page.Locator(".data-quality")).ToBeVisibleAsync();
            await Expect(Page.Locator(".quality-metrics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".data-completeness")).ToBeVisibleAsync();
            await Expect(Page.Locator(".data-accuracy")).ToBeVisibleAsync();
            await Expect(Page.Locator(".data-consistency")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Training_Queue()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify training queue section
            await Expect(Page.Locator(".training-queue")).ToBeVisibleAsync();
            await Expect(Page.Locator(".queue-list")).ToBeVisibleAsync();
            await Expect(Page.Locator(".queue-item")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Queue_Management()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click on queue item
            var queueItem = Page.Locator(".queue-item").First;
            if (await queueItem.IsVisibleAsync())
            {
                await queueItem.ClickAsync();
                
                // Verify queue management options
                await Expect(Page.Locator(".queue-actions")).ToBeVisibleAsync();
                
                // Pause training
                var pauseButton = Page.Locator(".pause-training");
                if (await pauseButton.IsVisibleAsync())
                {
                    await pauseButton.ClickAsync();
                    
                    // Verify training paused
                    await Expect(Page.Locator(".training-paused")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Model_Recommendations()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify recommendations section
            await Expect(Page.Locator(".model-recommendations")).ToBeVisibleAsync();
            await Expect(Page.Locator(".recommendation-list")).ToBeVisibleAsync();
            await Expect(Page.Locator(".recommendation-item")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Recommendation_Action()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click on recommendation
            var recommendation = Page.Locator(".recommendation-item").First;
            if (await recommendation.IsVisibleAsync())
            {
                await recommendation.ClickAsync();
                
                // Apply recommendation
                var applyButton = Page.Locator(".apply-recommendation");
                if (await applyButton.IsVisibleAsync())
                {
                    await applyButton.ClickAsync();
                    
                    // Verify recommendation applied
                    await Expect(Page.Locator(".recommendation-applied")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Usage_Statistics()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify usage statistics
            await Expect(Page.Locator(".usage-statistics")).ToBeVisibleAsync();
            await Expect(Page.Locator(".usage-chart")).ToBeVisibleAsync();
            await Expect(Page.Locator(".peak-usage")).ToBeVisibleAsync();
            await Expect(Page.Locator(".average-usage")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Time_Range_Selection()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Change time range
            var timeRangeSelect = Page.Locator(".time-range-select");
            if (await timeRangeSelect.IsVisibleAsync())
            {
                await timeRangeSelect.SelectOptionAsync("7d");
                
                // Verify data updated
                await Expect(Page.Locator(".data-updated")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Export_Options()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click export button
            var exportButton = Page.Locator(".export-dashboard");
            if (await exportButton.IsVisibleAsync())
            {
                await exportButton.ClickAsync();
                
                // Verify export options
                await Expect(Page.Locator(".export-options")).ToBeVisibleAsync();
                
                // Export as PDF
                var pdfExportButton = Page.Locator(".export-pdf");
                if (await pdfExportButton.IsVisibleAsync())
                {
                    await pdfExportButton.ClickAsync();
                    
                    // Verify export started
                    await Expect(Page.Locator(".export-started")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Refresh_Status()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click refresh button
            var refreshButton = Page.Locator(".refresh-dashboard");
            if (await refreshButton.IsVisibleAsync())
            {
                await refreshButton.ClickAsync();
                
                // Verify refresh status
                await Expect(Page.Locator(".refreshing")).ToBeVisibleAsync();
                
                // Wait for refresh completion
                await Expect(Page.Locator(".refresh-complete")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Loading_States()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify loading indicators are present during initial load
            await Expect(Page.Locator(".loading-indicator")).ToBeVisibleAsync();
            
            // Wait for content to load
            await Page.WaitForTimeoutAsync(2000);
            
            // Verify loading indicator is hidden
            await Expect(Page.Locator(".loading-indicator")).ToBeHiddenAsync();
        }

        [Test]
        public async Task Dashboard_Should_Handle_Error_States()
        {
            // Navigate to dashboard with error state
            await Page.GotoAsync("/ml/dashboard?error=true");
            await Page.WaitForLoadStateAsync();
            
            // Verify error handling
            await Expect(Page.Locator(".error-state")).ToBeVisibleAsync();
            await Expect(Page.Locator(".error-message")).ToBeVisibleAsync();
            
            // Retry button
            var retryButton = Page.Locator(".retry-button");
            if (await retryButton.IsVisibleAsync())
            {
                await retryButton.ClickAsync();
                
                // Verify retry attempt
                await Expect(Page.Locator(".retrying")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Dashboard_Should_Show_Responsive_Design()
        {
            // Test mobile viewport
            await Page.SetViewportSizeAsync(375, 667);
            await Page.GotoAsync("/ml/dashboard");
            
            // Verify mobile layout
            await Expect(Page.Locator(".mobile-layout")).ToBeVisibleAsync();
            
            // Test tablet viewport
            await Page.SetViewportSizeAsync(768, 1024);
            await Page.ReloadAsync();
            
            // Verify tablet layout
            await Expect(Page.Locator(".tablet-layout")).ToBeVisibleAsync();
            
            // Test desktop viewport
            await Page.SetViewportSizeAsync(1920, 1080);
            await Page.ReloadAsync();
            
            // Verify desktop layout
            await Expect(Page.Locator(".desktop-layout")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Show_Real_Time_Updates()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Verify real-time update indicators
            await Expect(Page.Locator(".real-time-indicator")).ToBeVisibleAsync();
            
            // Wait for potential updates
            await Page.WaitForTimeoutAsync(5000);
            
            // Verify data is being updated
            await Expect(Page.Locator(".data-updated")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Dashboard_Should_Allow_Customization()
        {
            await Page.GotoAsync("/ml/dashboard");
            await Page.WaitForLoadStateAsync();
            
            // Click customize button
            var customizeButton = Page.Locator(".customize-dashboard");
            if (await customizeButton.IsVisibleAsync())
            {
                await customizeButton.ClickAsync();
                
                // Verify customization panel
                await Expect(Page.Locator(".customization-panel")).ToBeVisibleAsync();
                
                // Toggle widget visibility
                var widgetToggle = Page.Locator(".widget-toggle").First;
                if (await widgetToggle.IsVisibleAsync())
                {
                    await widgetToggle.ClickAsync();
                    
                    // Save customization
                    var saveButton = Page.Locator(".save-customization");
                    if (await saveButton.IsVisibleAsync())
                    {
                        await saveButton.ClickAsync();
                        
                        // Verify customization saved
                        await Expect(Page.Locator(".customization-saved")).ToBeVisibleAsync();
                    }
                }
            }
        }
    }
} 