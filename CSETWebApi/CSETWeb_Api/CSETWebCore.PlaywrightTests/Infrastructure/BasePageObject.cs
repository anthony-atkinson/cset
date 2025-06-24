using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace CSETWebCore.PlaywrightTests.Infrastructure
{
    /// <summary>
    /// Base class for all page objects, providing common functionality and patterns
    /// </summary>
    public abstract class BasePageObject
    {
        protected readonly IPage Page;
        protected readonly TestConfiguration Config;

        protected BasePageObject(IPage page)
        {
            Page = page;
            Config = TestConfiguration.Instance;
        }

        /// <summary>
        /// Navigate to the specific page URL
        /// </summary>
        public abstract string PageUrl { get; }

        /// <summary>
        /// Navigate to this page
        /// </summary>
        public virtual async Task NavigateAsync()
        {
            await Page.GotoAsync($"{Config.BaseUrl}{PageUrl}");
            await WaitForPageLoadAsync();
        }

        /// <summary>
        /// Wait for the page to be fully loaded
        /// </summary>
        public virtual async Task WaitForPageLoadAsync()
        {
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Check if the page is currently displayed
        /// </summary>
        public abstract Task<bool> IsDisplayedAsync();

        // Common UI Element Helpers

        /// <summary>
        /// Click an element with retry logic
        /// </summary>
        protected async Task ClickAsync(string selector, int timeout = 10000)
        {
            await Page.Locator(selector).ClickAsync(new LocatorClickOptions { Timeout = timeout });
        }

        /// <summary>
        /// Fill a text input field
        /// </summary>
        protected async Task FillAsync(string selector, string value, int timeout = 10000)
        {
            await Page.Locator(selector).FillAsync(value, new LocatorFillOptions { Timeout = timeout });
        }

        /// <summary>
        /// Select from a dropdown
        /// </summary>
        protected async Task SelectAsync(string selector, string value, int timeout = 10000)
        {
            await Page.Locator(selector).SelectOptionAsync(value, new LocatorSelectOptionOptions { Timeout = timeout });
        }

        /// <summary>
        /// Wait for element to be visible
        /// </summary>
        protected async Task WaitForVisibleAsync(string selector, int timeout = 10000)
        {
            await Page.Locator(selector).WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible, 
                Timeout = timeout 
            });
        }

        /// <summary>
        /// Wait for element to be hidden
        /// </summary>
        protected async Task WaitForHiddenAsync(string selector, int timeout = 10000)
        {
            await Page.Locator(selector).WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Hidden, 
                Timeout = timeout 
            });
        }

        /// <summary>
        /// Check if element exists and is visible
        /// </summary>
        protected async Task<bool> IsVisibleAsync(string selector)
        {
            try
            {
                return await Page.Locator(selector).IsVisibleAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get text content of an element
        /// </summary>
        protected async Task<string> GetTextAsync(string selector)
        {
            return await Page.Locator(selector).TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Get attribute value from an element
        /// </summary>
        protected async Task<string> GetAttributeAsync(string selector, string attributeName)
        {
            return await Page.Locator(selector).GetAttributeAsync(attributeName) ?? string.Empty;
        }

        /// <summary>
        /// Wait for a specific URL or URL pattern
        /// </summary>
        protected async Task WaitForUrlAsync(string urlPattern, int timeout = 10000)
        {
            await Page.WaitForURLAsync(urlPattern, new PageWaitForURLOptions { Timeout = timeout });
        }

        /// <summary>
        /// Take a screenshot for debugging
        /// </summary>
        protected async Task TakeScreenshotAsync(string name)
        {
            var screenshotPath = Path.Combine(Config.ScreenshotPath, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
        }

        /// <summary>
        /// Scroll element into view
        /// </summary>
        protected async Task ScrollIntoViewAsync(string selector)
        {
            await Page.Locator(selector).ScrollIntoViewIfNeededAsync();
        }

        /// <summary>
        /// Wait for element to contain specific text
        /// </summary>
        protected async Task WaitForTextAsync(string selector, string expectedText, int timeout = 10000)
        {
            await Page.Locator(selector).Filter(new LocatorFilterOptions { HasText = expectedText })
                .WaitForAsync(new LocatorWaitForOptions { Timeout = timeout });
        }

        /// <summary>
        /// Handle confirmation dialogs
        /// </summary>
        protected async Task HandleDialogAsync(bool accept = true)
        {
            Page.Dialog += async (_, dialog) =>
            {
                if (accept)
                    await dialog.AcceptAsync();
                else
                    await dialog.DismissAsync();
            };
        }

        /// <summary>
        /// Upload file to input element
        /// </summary>
        protected async Task UploadFileAsync(string selector, string filePath)
        {
            await Page.Locator(selector).SetInputFilesAsync(filePath);
        }

        /// <summary>
        /// Wait for network request to complete
        /// </summary>
        protected async Task WaitForResponseAsync(string urlPattern, int timeout = 10000)
        {
            await Page.WaitForResponseAsync(urlPattern, new PageWaitForResponseOptions { Timeout = timeout });
        }

        /// <summary>
        /// Check if page contains specific text
        /// </summary>
        protected async Task<bool> ContainsTextAsync(string text)
        {
            return await Page.GetByText(text).IsVisibleAsync();
        }

        /// <summary>
        /// Hover over an element
        /// </summary>
        protected async Task HoverAsync(string selector)
        {
            await Page.Locator(selector).HoverAsync();
        }
    }
} 