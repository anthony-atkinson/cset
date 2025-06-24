using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.Assessment
{
    /// <summary>
    /// Page object for the CSET assessment creation page
    /// </summary>
    public class AssessmentCreationPage : BasePageObject
    {
        public override string PageUrl => "/assessment/create";

        // Locators
        private ILocator GalleryContainer => Page.Locator(".gallery-container, [data-testid='gallery']");
        private ILocator GalleryItems => Page.Locator(".gallery-item, [data-testid='gallery-item']");
        private ILocator AssessmentNameInput => Page.Locator("[data-testid='assessment-name'], input[name='assessmentName'], #assessmentName");
        private ILocator CreateButton => Page.Locator("[data-testid='create-assessment'], button:has-text('Create'), button:has-text('Start Assessment')");
        private ILocator CancelButton => Page.Locator("[data-testid='cancel'], button:has-text('Cancel')");
        private ILocator LoadingSpinner => Page.Locator(".loading, .spinner, [data-testid='loading']");
        private ILocator ErrorMessage => Page.Locator(".error, .alert-danger, [data-testid='error-message']");
        private ILocator SuccessMessage => Page.Locator(".success, .alert-success, [data-testid='success-message']");

        // Gallery item specific locators
        private ILocator GalleryItemTitle => Page.Locator(".gallery-item-title, [data-testid='gallery-item-title']");
        private ILocator GalleryItemDescription => Page.Locator(".gallery-item-description, [data-testid='gallery-item-description']");
        private ILocator GalleryItemImage => Page.Locator(".gallery-item-image, [data-testid='gallery-item-image']");

        public AssessmentCreationPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await GalleryContainer.IsVisibleAsync() && await CreateButton.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for gallery items to load
        /// </summary>
        public async Task WaitForGalleryLoadAsync()
        {
            await WaitForVisibleAsync(GalleryContainer);
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Get the number of available gallery items
        /// </summary>
        public async Task<int> GetGalleryItemCountAsync()
        {
            return await GalleryItems.CountAsync();
        }

        /// <summary>
        /// Select a gallery item by index
        /// </summary>
        public async Task SelectGalleryItemAsync(int index)
        {
            var items = GalleryItems;
            if (await items.CountAsync() > index)
            {
                await items.Nth(index).ClickAsync();
                await WaitForLoadingAsync();
            }
            else
            {
                throw new ArgumentException($"Gallery item at index {index} does not exist");
            }
        }

        /// <summary>
        /// Select a gallery item by title
        /// </summary>
        public async Task SelectGalleryItemByTitleAsync(string title)
        {
            var item = GalleryItems.Filter(new LocatorFilterOptions { HasText = title });
            if (await item.CountAsync() > 0)
            {
                await item.First.ClickAsync();
                await WaitForLoadingAsync();
            }
            else
            {
                throw new ArgumentException($"Gallery item with title '{title}' not found");
            }
        }

        /// <summary>
        /// Get the title of a gallery item by index
        /// </summary>
        public async Task<string> GetGalleryItemTitleAsync(int index)
        {
            var items = GalleryItems;
            if (await items.CountAsync() > index)
            {
                var titleElement = items.Nth(index).Locator(GalleryItemTitle);
                return await titleElement.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get the description of a gallery item by index
        /// </summary>
        public async Task<string> GetGalleryItemDescriptionAsync(int index)
        {
            var items = GalleryItems;
            if (await items.CountAsync() > index)
            {
                var descElement = items.Nth(index).Locator(GalleryItemDescription);
                return await descElement.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Set the assessment name
        /// </summary>
        public async Task SetAssessmentNameAsync(string name)
        {
            await FillAsync(AssessmentNameInput, name);
        }

        /// <summary>
        /// Get the current assessment name
        /// </summary>
        public async Task<string> GetAssessmentNameAsync()
        {
            return await GetAttributeAsync(AssessmentNameInput, "value") ?? string.Empty;
        }

        /// <summary>
        /// Check if the create button is enabled
        /// </summary>
        public async Task<bool> IsCreateButtonEnabledAsync()
        {
            return await CreateButton.IsEnabledAsync();
        }

        /// <summary>
        /// Click the create assessment button
        /// </summary>
        public async Task CreateAssessmentAsync()
        {
            await CreateButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Click the cancel button
        /// </summary>
        public async Task CancelCreationAsync()
        {
            await CancelButton.ClickAsync();
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
        /// Wait for assessment creation to complete and redirect
        /// </summary>
        public async Task<bool> WaitForAssessmentCreationAsync()
        {
            try
            {
                await Page.WaitForURLAsync("**/assessment/**", new PageWaitForURLOptions { Timeout = 30000 });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Complete the full assessment creation workflow
        /// </summary>
        public async Task<bool> CreateAssessmentWorkflowAsync(string assessmentName, int galleryItemIndex = 0)
        {
            try
            {
                // Wait for gallery to load
                await WaitForGalleryLoadAsync();

                // Select gallery item
                await SelectGalleryItemAsync(galleryItemIndex);

                // Set assessment name
                await SetAssessmentNameAsync(assessmentName);

                // Verify create button is enabled
                if (!await IsCreateButtonEnabledAsync())
                {
                    return false;
                }

                // Create assessment
                await CreateAssessmentAsync();

                // Wait for creation to complete
                return await WaitForAssessmentCreationAsync();
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
        public async Task TakeScreenshotAsync(string name = "assessment-creation")
        {
            await TakeScreenshotAsync(name);
        }
    }
} 