using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.UserManagement
{
    /// <summary>
    /// Page object for the CSET User Management functionality
    /// </summary>
    public class UserManagementPage : BasePageObject
    {
        public override string PageUrl => "/admin-settings";

        // Admin Settings Page Locators
        private ILocator AdminSettingsTitle => Page.Locator("h2:has-text('Admin Settings'), [data-testid='admin-settings-title']");
        private ILocator UsersTable => Page.Locator("table.table, [data-testid='users-table']");
        private ILocator UserRows => Page.Locator("table.table tbody tr, [data-testid='user-row']");
        private ILocator RoleSelects => Page.Locator("select, [data-testid='role-select']");
        
        // User Profile Dialog Locators
        private ILocator UserProfileDialog => Page.Locator(".edit-user-dialog, [data-testid='user-profile-dialog']");
        private ILocator FirstNameInput => Page.Locator("#firstName, [data-testid='first-name-input']");
        private ILocator LastNameInput => Page.Locator("#lastName, [data-testid='last-name-input']");
        private ILocator EmailInput => Page.Locator("input[name='email'], [data-testid='email-input']");
        private ILocator ConfirmEmailInput => Page.Locator("input[name='confirmEmail'], [data-testid='confirm-email-input']");
        private ILocator SaveButton => Page.Locator("button[type='submit'], button:has-text('Save'), [data-testid='save-button']");
        private ILocator CancelButton => Page.Locator("button:has-text('Cancel'), [data-testid='cancel-button']");
        
        // User Menu Locators
        private ILocator UserMenuButton => Page.Locator("button[matMenuTriggerFor='usermenu'], [data-testid='user-menu-button']");
        private ILocator UserProfileMenuItem => Page.Locator("button:has-text('User Profile'), [data-testid='user-profile-menu-item']");
        private ILocator ChangePasswordMenuItem => Page.Locator("button:has-text('Change Password'), [data-testid='change-password-menu-item']");
        private ILocator SettingsMenuItem => Page.Locator("button:has-text('Settings'), [data-testid='settings-menu-item']");
        private ILocator AdminMenuItem => Page.Locator("button:has-text('Admin'), [data-testid='admin-menu-item']");
        
        // Registration Page Locators
        private ILocator RegisterPage => Page.Locator("[data-testid='register-page']");
        private ILocator RegisterForm => Page.Locator("form, [data-testid='register-form']");
        private ILocator RegisterFirstNameInput => Page.Locator("input[name='firstName'], [data-testid='register-first-name']");
        private ILocator RegisterLastNameInput => Page.Locator("input[name='lastName'], [data-testid='register-last-name']");
        private ILocator RegisterEmailInput => Page.Locator("input[name='email'], [data-testid='register-email']");
        private ILocator RegisterPasswordInput => Page.Locator("input[name='password'], [data-testid='register-password']");
        private ILocator RegisterConfirmPasswordInput => Page.Locator("input[name='confirmPassword'], [data-testid='register-confirm-password']");
        private ILocator RegisterButton => Page.Locator("button:has-text('Register'), [data-testid='register-button']");
        
        // Change Password Dialog Locators
        private ILocator ChangePasswordDialog => Page.Locator("[data-testid='change-password-dialog']");
        private ILocator CurrentPasswordInput => Page.Locator("input[name='currentPassword'], [data-testid='current-password-input']");
        private ILocator NewPasswordInput => Page.Locator("input[name='newPassword'], [data-testid='new-password-input']");
        private ILocator ConfirmNewPasswordInput => Page.Locator("input[name='confirmNewPassword'], [data-testid='confirm-new-password-input']");
        private ILocator ChangePasswordSaveButton => Page.Locator("button:has-text('Change Password'), [data-testid='change-password-save-button']");
        
        // Error Messages
        private ILocator ErrorMessages => Page.Locator(".alert-danger, .error, [data-testid='error-message']");
        private ILocator SuccessMessages => Page.Locator(".alert-success, .success, [data-testid='success-message']");
        
        // Loading Indicators
        private ILocator LoadingSpinner => Page.Locator(".spinner-container, [data-testid='loading-spinner']");

        public UserManagementPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await AdminSettingsTitle.IsVisibleAsync() || await UserProfileDialog.IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to admin settings page
        /// </summary>
        public async Task NavigateToAdminSettingsAsync()
        {
            await NavigateToAsync("/admin-settings");
            await AdminSettingsTitle.WaitForAsync();
        }

        /// <summary>
        /// Check if admin settings page is displayed
        /// </summary>
        public async Task<bool> IsAdminSettingsDisplayedAsync()
        {
            return await AdminSettingsTitle.IsVisibleAsync();
        }

        /// <summary>
        /// Get number of users in the table
        /// </summary>
        public async Task<int> GetUserCountAsync()
        {
            if (await UserRows.IsVisibleAsync())
            {
                return await UserRows.CountAsync();
            }
            return 0;
        }

        /// <summary>
        /// Check if users table is displayed
        /// </summary>
        public async Task<bool> IsUsersTableDisplayedAsync()
        {
            return await UsersTable.IsVisibleAsync();
        }

        /// <summary>
        /// Get user information from a specific row
        /// </summary>
        public async Task<(string firstName, string lastName, string email, string role)> GetUserInfoAsync(int rowIndex)
        {
            var row = UserRows.Nth(rowIndex);
            var cells = row.Locator("td");
            
            var firstName = await cells.Nth(0).TextContentAsync() ?? string.Empty;
            var lastName = await cells.Nth(1).TextContentAsync() ?? string.Empty;
            var email = await cells.Nth(2).TextContentAsync() ?? string.Empty;
            var role = await cells.Nth(3).Locator("select").EvaluateAsync<string>("el => el.value") ?? string.Empty;
            
            return (firstName, lastName, email, role);
        }

        /// <summary>
        /// Change user role
        /// </summary>
        public async Task ChangeUserRoleAsync(int rowIndex, string newRole)
        {
            var row = UserRows.Nth(rowIndex);
            var roleSelect = row.Locator("select");
            await roleSelect.SelectOptionAsync(newRole);
        }

        /// <summary>
        /// Open user profile dialog
        /// </summary>
        public async Task OpenUserProfileAsync()
        {
            await UserMenuButton.ClickAsync();
            await UserProfileMenuItem.ClickAsync();
            await UserProfileDialog.WaitForAsync();
        }

        /// <summary>
        /// Check if user profile dialog is displayed
        /// </summary>
        public async Task<bool> IsUserProfileDialogDisplayedAsync()
        {
            return await UserProfileDialog.IsVisibleAsync();
        }

        /// <summary>
        /// Fill user profile form
        /// </summary>
        public async Task FillUserProfileAsync(string firstName, string lastName, string email, string confirmEmail)
        {
            await FirstNameInput.FillAsync(firstName);
            await LastNameInput.FillAsync(lastName);
            await EmailInput.FillAsync(email);
            await ConfirmEmailInput.FillAsync(confirmEmail);
        }

        /// <summary>
        /// Save user profile
        /// </summary>
        public async Task SaveUserProfileAsync()
        {
            await SaveButton.ClickAsync();
        }

        /// <summary>
        /// Cancel user profile changes
        /// </summary>
        public async Task CancelUserProfileAsync()
        {
            await CancelButton.ClickAsync();
        }

        /// <summary>
        /// Open change password dialog
        /// </summary>
        public async Task OpenChangePasswordAsync()
        {
            await UserMenuButton.ClickAsync();
            await ChangePasswordMenuItem.ClickAsync();
            await ChangePasswordDialog.WaitForAsync();
        }

        /// <summary>
        /// Check if change password dialog is displayed
        /// </summary>
        public async Task<bool> IsChangePasswordDialogDisplayedAsync()
        {
            return await ChangePasswordDialog.IsVisibleAsync();
        }

        /// <summary>
        /// Fill change password form
        /// </summary>
        public async Task FillChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword)
        {
            await CurrentPasswordInput.FillAsync(currentPassword);
            await NewPasswordInput.FillAsync(newPassword);
            await ConfirmNewPasswordInput.FillAsync(confirmNewPassword);
        }

        /// <summary>
        /// Save password change
        /// </summary>
        public async Task SavePasswordChangeAsync()
        {
            await ChangePasswordSaveButton.ClickAsync();
        }

        /// <summary>
        /// Open user settings
        /// </summary>
        public async Task OpenUserSettingsAsync()
        {
            await UserMenuButton.ClickAsync();
            await SettingsMenuItem.ClickAsync();
        }

        /// <summary>
        /// Open admin settings from menu
        /// </summary>
        public async Task OpenAdminSettingsFromMenuAsync()
        {
            await UserMenuButton.ClickAsync();
            await AdminMenuItem.ClickAsync();
            await AdminSettingsTitle.WaitForAsync();
        }

        /// <summary>
        /// Navigate to registration page
        /// </summary>
        public async Task NavigateToRegistrationAsync()
        {
            await NavigateToAsync("/register");
            await RegisterForm.WaitForAsync();
        }

        /// <summary>
        /// Check if registration page is displayed
        /// </summary>
        public async Task<bool> IsRegistrationPageDisplayedAsync()
        {
            return await RegisterForm.IsVisibleAsync();
        }

        /// <summary>
        /// Fill registration form
        /// </summary>
        public async Task FillRegistrationFormAsync(string firstName, string lastName, string email, string password, string confirmPassword)
        {
            await RegisterFirstNameInput.FillAsync(firstName);
            await RegisterLastNameInput.FillAsync(lastName);
            await RegisterEmailInput.FillAsync(email);
            await RegisterPasswordInput.FillAsync(password);
            await RegisterConfirmPasswordInput.FillAsync(confirmPassword);
        }

        /// <summary>
        /// Submit registration form
        /// </summary>
        public async Task SubmitRegistrationAsync()
        {
            await RegisterButton.ClickAsync();
        }

        /// <summary>
        /// Get error messages
        /// </summary>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await ErrorMessages.IsVisibleAsync())
            {
                return await ErrorMessages.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get success messages
        /// </summary>
        public async Task<string> GetSuccessMessageAsync()
        {
            if (await SuccessMessages.IsVisibleAsync())
            {
                return await SuccessMessages.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if loading spinner is visible
        /// </summary>
        public async Task<bool> IsLoadingSpinnerVisibleAsync()
        {
            return await LoadingSpinner.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for loading to complete
        /// </summary>
        public async Task WaitForLoadingToCompleteAsync()
        {
            try
            {
                await LoadingSpinner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 30000 });
            }
            catch
            {
                // Loading might already be complete
            }
        }

        /// <summary>
        /// Check if user menu is accessible
        /// </summary>
        public async Task<bool> IsUserMenuAccessibleAsync()
        {
            return await UserMenuButton.IsVisibleAsync();
        }

        /// <summary>
        /// Get current user name from menu
        /// </summary>
        public async Task<string> GetCurrentUserNameAsync()
        {
            if (await UserMenuButton.IsVisibleAsync())
            {
                return await UserMenuButton.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if admin menu item is visible (indicates admin role)
        /// </summary>
        public async Task<bool> IsAdminMenuItemVisibleAsync()
        {
            await UserMenuButton.ClickAsync();
            var isVisible = await AdminMenuItem.IsVisibleAsync();
            // Close menu by clicking outside
            await Page.ClickAsync("body");
            return isVisible;
        }
    }
} 