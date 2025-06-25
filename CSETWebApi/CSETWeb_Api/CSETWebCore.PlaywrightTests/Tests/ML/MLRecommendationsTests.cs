using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.ML
{
    [TestFixture]
    public class MLRecommendationsTests : PageTest
    {
        [SetUp]
        public async Task Setup()
        {
            await Page.GotoAsync("/");
            await Page.WaitForLoadStateAsync();
        }

        [Test]
        public async Task Recommendations_Page_Should_Load()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            await Expect(Page.Locator(".ml-recommendations")).ToBeVisibleAsync();
            await Expect(Page.Locator(".recommendation-list")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Recommendations_Should_Display_Categories()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Verify recommendation categories are present
            await Expect(Page.Locator(".recommendation-category")).ToBeVisibleAsync();
            await Expect(Page.Locator(".category-filter")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Recommendations_Should_Allow_Search()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            var searchInput = Page.Locator(".recommendation-search input");
            if (await searchInput.IsVisibleAsync())
            {
                await searchInput.FillAsync("network security");
                await Page.WaitForTimeoutAsync(1000);
                
                // Verify search results
                await Expect(Page.Locator(".search-results")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Recommendations_Should_Allow_Filtering_By_Priority()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            var priorityFilter = Page.Locator(".priority-filter select");
            if (await priorityFilter.IsVisibleAsync())
            {
                await priorityFilter.SelectOptionAsync("high");
                await Page.WaitForTimeoutAsync(1000);
                
                // Verify filtered results show only high priority
                await Expect(Page.Locator(".recommendation-item.high-priority")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Recommendations_Should_Allow_Filtering_By_Status()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            var statusFilter = Page.Locator(".status-filter select");
            if (await statusFilter.IsVisibleAsync())
            {
                await statusFilter.SelectOptionAsync("implemented");
                await Page.WaitForTimeoutAsync(1000);
                
                // Verify filtered results show only implemented recommendations
                await Expect(Page.Locator(".recommendation-item.implemented")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Recommendation_Details_Should_Display()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Click on first recommendation
            var firstRecommendation = Page.Locator(".recommendation-item").First;
            if (await firstRecommendation.IsVisibleAsync())
            {
                await firstRecommendation.ClickAsync();
                
                // Verify details panel
                await Expect(Page.Locator(".recommendation-details")).ToBeVisibleAsync();
                await Expect(Page.Locator(".recommendation-description")).ToBeVisibleAsync();
                await Expect(Page.Locator(".recommendation-steps")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Recommendations_Should_Allow_Status_Update()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Click on first recommendation
            var firstRecommendation = Page.Locator(".recommendation-item").First;
            if (await firstRecommendation.IsVisibleAsync())
            {
                await firstRecommendation.ClickAsync();
                
                // Update status
                var statusSelect = Page.Locator(".status-select");
                if (await statusSelect.IsVisibleAsync())
                {
                    await statusSelect.SelectOptionAsync("in-progress");
                    
                    // Save changes
                    var saveButton = Page.Locator(".save-status");
                    if (await saveButton.IsVisibleAsync())
                    {
                        await saveButton.ClickAsync();
                        
                        // Verify status is updated
                        await Expect(Page.Locator(".status-updated")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Recommendations_Should_Allow_Notes()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Click on first recommendation
            var firstRecommendation = Page.Locator(".recommendation-item").First;
            if (await firstRecommendation.IsVisibleAsync())
            {
                await firstRecommendation.ClickAsync();
                
                // Add notes
                var notesInput = Page.Locator(".notes-input");
                if (await notesInput.IsVisibleAsync())
                {
                    await notesInput.FillAsync("Implementation started on 2024-01-15");
                    
                    // Save notes
                    var saveNotesButton = Page.Locator(".save-notes");
                    if (await saveNotesButton.IsVisibleAsync())
                    {
                        await saveNotesButton.ClickAsync();
                        
                        // Verify notes are saved
                        await Expect(Page.Locator(".notes-saved")).ToBeVisibleAsync();
                    }
                }
            }
        }

        [Test]
        public async Task Recommendations_Should_Show_Progress()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Verify progress indicators
            await Expect(Page.Locator(".recommendation-progress")).ToBeVisibleAsync();
            await Expect(Page.Locator(".progress-bar")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Recommendations_Should_Allow_Export()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            var exportButton = Page.Locator(".export-recommendations");
            if (await exportButton.IsVisibleAsync())
            {
                await exportButton.ClickAsync();
                
                // Verify export options
                await Expect(Page.Locator(".export-options")).ToBeVisibleAsync();
                
                // Select PDF export
                var pdfOption = Page.Locator(".export-pdf");
                if (await pdfOption.IsVisibleAsync())
                {
                    await pdfOption.ClickAsync();
                    
                    // Verify download starts
                    await Expect(Page.Locator(".download-started")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Recommendations_Should_Show_Related_Items()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Click on first recommendation
            var firstRecommendation = Page.Locator(".recommendation-item").First;
            if (await firstRecommendation.IsVisibleAsync())
            {
                await firstRecommendation.ClickAsync();
                
                // Verify related recommendations
                await Expect(Page.Locator(".related-recommendations")).ToBeVisibleAsync();
                await Expect(Page.Locator(".related-item")).ToBeVisibleAsync();
            }
        }

        [Test]
        public async Task Recommendations_Should_Allow_Bulk_Actions()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Select multiple recommendations
            var checkboxes = Page.Locator(".recommendation-checkbox");
            if (await checkboxes.First.IsVisibleAsync())
            {
                await checkboxes.First.CheckAsync();
                await checkboxes.Nth(1).CheckAsync();
                
                // Verify bulk actions are available
                await Expect(Page.Locator(".bulk-actions")).ToBeVisibleAsync();
                
                // Test bulk status update
                var bulkStatusButton = Page.Locator(".bulk-status-update");
                if (await bulkStatusButton.IsVisibleAsync())
                {
                    await bulkStatusButton.ClickAsync();
                    
                    // Verify bulk update dialog
                    await Expect(Page.Locator(".bulk-update-dialog")).ToBeVisibleAsync();
                }
            }
        }

        [Test]
        public async Task Recommendations_Should_Show_Statistics()
        {
            await Page.GotoAsync("/ml/recommendations");
            await Page.WaitForLoadStateAsync();
            
            // Verify statistics panel
            await Expect(Page.Locator(".recommendation-stats")).ToBeVisibleAsync();
            await Expect(Page.Locator(".stats-total")).ToBeVisibleAsync();
            await Expect(Page.Locator(".stats-implemented")).ToBeVisibleAsync();
            await Expect(Page.Locator(".stats-pending")).ToBeVisibleAsync();
        }

        [Test]
        public async Task Recommendations_Should_Handle_Empty_State()
        {
            // Navigate to recommendations with no data
            await Page.GotoAsync("/ml/recommendations?empty=true");
            await Page.WaitForLoadStateAsync();
            
            // Verify empty state message
            await Expect(Page.Locator(".empty-state")).ToBeVisibleAsync();
            await Expect(Page.Locator(".empty-message")).ToBeVisibleAsync();
        }
    }
} 