using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.Authentication
{
    /// <summary>
    /// Page object for the CSET login page
    /// </summary>
    public class LoginPage : BasePageObject
    {
        public override string PageUrl => "/login";

        // Locators
        private ILocator UsernameInput => Page.Locator("[data-testid='username'], [name='username'], #username, input[type='text']:first-of-type");
        private ILocator PasswordInput => Page.Locator("[data-testid='password'], [name='password'], #password, input[type='password']");
        private ILocator LoginButton => Page.Locator("[data-testid='login-button'], button[type='submit'], button:has-text('Login'), button:has-text('Sign In')");
        private ILocator ErrorMessage => Page.Locator(".error, .alert-danger, [data-testid='error-message']");
        private ILocator ForgotPasswordLink => Page.Locator("[data-testid='forgot-password'], a:has-text('Forgot'), a[href*='forgot']");
        private ILocator RememberMeCheckbox => Page.Locator("[data-testid='remember-me'], input[type='checkbox']");
        private ILocator PrivacyWarning => Page.Locator(".privacy-warning, [data-testid='privacy-warning']");
        private ILocator AcceptPrivacyButton => Page.Locator("[data-testid='accept-privacy'], button:has-text('Accept'), button:has-text('I Agree')");

        public LoginPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await UsernameInput.IsVisibleAsync() && await PasswordInput.IsVisibleAsync();
        }

        /// <summary>
        /// Perform login with provided credentials
        /// </summary>
        public async Task LoginAsync(string username, string password, bool rememberMe = false)
        {
            await FillAsync(UsernameInput, username);
            await FillAsync(PasswordInput, password);
            
            if (rememberMe && await RememberMeCheckbox.IsVisibleAsync())
            {
                await RememberMeCheckbox.CheckAsync();
            }
            
            await LoginButton.ClickAsync();
        }

        /// <summary>
        /// Perform login with test credentials from configuration
        /// </summary>
        public async Task LoginWithTestCredentialsAsync()
        {
            await LoginAsync(Config.TestUsername, Config.TestPassword);
        }

        /// <summary>
        /// Check if login was successful by waiting for redirect
        /// </summary>
        public async Task<bool> IsLoginSuccessfulAsync()
        {
            try
            {
                await Page.WaitForURLAsync("**/assessment/**", new PageWaitForURLOptions { Timeout = 10000 });
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get error message text if login failed
        /// </summary>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await ErrorMessage.IsVisibleAsync())
            {
                return await ErrorMessage.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Click forgot password link
        /// </summary>
        public async Task ClickForgotPasswordAsync()
        {
            await ForgotPasswordLink.ClickAsync();
        }

        /// <summary>
        /// Accept privacy warning if present
        /// </summary>
        public async Task AcceptPrivacyWarningAsync()
        {
            if (await PrivacyWarning.IsVisibleAsync())
            {
                await AcceptPrivacyButton.ClickAsync();
            }
        }

        /// <summary>
        /// Check if privacy warning is displayed
        /// </summary>
        public async Task<bool> IsPrivacyWarningDisplayedAsync()
        {
            return await PrivacyWarning.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for login form to be ready
        /// </summary>
        public async Task WaitForLoginFormAsync()
        {
            await UsernameInput.WaitForAsync();
            await PasswordInput.WaitForAsync();
            await LoginButton.WaitForAsync();
        }

        /// <summary>
        /// Clear login form
        /// </summary>
        public async Task ClearFormAsync()
        {
            await UsernameInput.FillAsync("");
            await PasswordInput.FillAsync("");
        }

        /// <summary>
        /// Check if login button is enabled
        /// </summary>
        public async Task<bool> IsLoginButtonEnabledAsync()
        {
            return await LoginButton.IsEnabledAsync();
        }

        private async Task FillAsync(ILocator locator, string value)
        {
            await locator.WaitForAsync();
            await locator.FillAsync(value);
        }
    }
} 