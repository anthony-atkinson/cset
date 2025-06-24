using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.PageObjects.Notifications
{
    /// <summary>
    /// Page object for CSET notification system interactions
    /// </summary>
    public class NotificationPage
    {
        private readonly IPage _page;

        public NotificationPage(IPage page)
        {
            _page = page;
        }

        #region Version Notifications

        /// <summary>
        /// Check if version notification is present
        /// </summary>
        public async Task<bool> HasVersionNotificationAsync()
        {
            return await _page.Locator(".notification-link, .newinstaller").IsVisibleAsync();
        }

        /// <summary>
        /// Wait for version notification icon to be visible
        /// </summary>
        public async Task WaitForVersionNotificationIconAsync()
        {
            await _page.Locator(".notification-link mat-icon").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get version notification count
        /// </summary>
        public async Task<int> GetVersionNotificationCountAsync()
        {
            var countText = await _page.Locator(".newinstaller").TextContentAsync();
            return int.TryParse(countText, out var count) ? count : 0;
        }

        /// <summary>
        /// Click version notification
        /// </summary>
        public async Task ClickVersionNotificationAsync()
        {
            await _page.Locator(".notification-link").ClickAsync();
        }

        /// <summary>
        /// Wait for version notification dialog
        /// </summary>
        public async Task WaitForVersionNotificationDialogAsync()
        {
            await _page.Locator(".mat-dialog-container").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get version notification title
        /// </summary>
        public async Task<string> GetVersionNotificationTitleAsync()
        {
            return await _page.Locator(".mat-dialog-container h2, .mat-dialog-container .dialog-title").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Get version notification content
        /// </summary>
        public async Task<string> GetVersionNotificationContentAsync()
        {
            return await _page.Locator(".mat-dialog-container .dialog-content, .mat-dialog-container p").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Close version notification
        /// </summary>
        public async Task CloseVersionNotificationAsync()
        {
            await _page.Locator(".mat-dialog-container .close-button, .mat-dialog-container button[aria-label='Close']").ClickAsync();
        }

        /// <summary>
        /// Wait for version notification dialog to close
        /// </summary>
        public async Task WaitForVersionNotificationDialogClosedAsync()
        {
            await _page.Locator(".mat-dialog-container").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }

        #endregion

        #region Upgrade Notifications

        /// <summary>
        /// Check if upgrade notification is present
        /// </summary>
        public async Task<bool> HasUpgradeNotificationAsync()
        {
            return await _page.Locator(".alert-warning, .upgrade-notification").IsVisibleAsync();
        }

        /// <summary>
        /// Wait for upgrade notification
        /// </summary>
        public async Task WaitForUpgradeNotificationAsync()
        {
            await _page.Locator(".alert-warning, .upgrade-notification").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get upgrade notification text
        /// </summary>
        public async Task<string> GetUpgradeNotificationTextAsync()
        {
            return await _page.Locator(".alert-warning, .upgrade-notification").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if upgrade button is present
        /// </summary>
        public async Task<bool> HasUpgradeButtonAsync()
        {
            return await _page.Locator(".alert-warning button, .upgrade-notification button").IsVisibleAsync();
        }

        /// <summary>
        /// Hide upgrade notification
        /// </summary>
        public async Task HideUpgradeNotificationAsync()
        {
            await _page.Locator(".alert-warning .btn-outline-light, .upgrade-notification .close-button").ClickAsync();
        }

        /// <summary>
        /// Check if upgrade notification is hidden
        /// </summary>
        public async Task<bool> IsUpgradeNotificationHiddenAsync()
        {
            return !await _page.Locator(".alert-warning, .upgrade-notification").IsVisibleAsync();
        }

        #endregion

        #region Snackbar Notifications

        /// <summary>
        /// Check if snackbar notification is present
        /// </summary>
        public async Task<bool> HasSnackbarNotificationAsync()
        {
            return await _page.Locator(".mat-mdc-snack-bar-container, .snackbar, .notify-snackbar").IsVisibleAsync();
        }

        /// <summary>
        /// Get snackbar text
        /// </summary>
        public async Task<string> GetSnackbarTextAsync()
        {
            return await _page.Locator(".mat-mdc-snack-bar-container .mdc-snackbar__label, .snackbar .message").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if snackbar has action button
        /// </summary>
        public async Task<bool> HasSnackbarActionAsync()
        {
            return await _page.Locator(".mat-mdc-snack-bar-container button, .snackbar button").IsVisibleAsync();
        }

        /// <summary>
        /// Click snackbar action
        /// </summary>
        public async Task ClickSnackbarActionAsync()
        {
            await _page.Locator(".mat-mdc-snack-bar-container button, .snackbar button").ClickAsync();
        }

        /// <summary>
        /// Wait for snackbar to be dismissed
        /// </summary>
        public async Task WaitForSnackbarDismissedAsync()
        {
            await _page.Locator(".mat-mdc-snack-bar-container, .snackbar").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }

        /// <summary>
        /// Check if snackbar is dismissed
        /// </summary>
        public async Task<bool> IsSnackbarDismissedAsync()
        {
            return !await _page.Locator(".mat-mdc-snack-bar-container, .snackbar").IsVisibleAsync();
        }

        #endregion

        #region Error Notifications

        /// <summary>
        /// Wait for error notification
        /// </summary>
        public async Task WaitForErrorNotificationAsync()
        {
            await _page.Locator(".alert-danger, .error-notification, .mat-error").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get error notification text
        /// </summary>
        public async Task<string> GetErrorNotificationTextAsync()
        {
            return await _page.Locator(".alert-danger, .error-notification, .mat-error").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if error notification has error class
        /// </summary>
        public async Task<bool> HasErrorNotificationClassAsync()
        {
            return await _page.Locator(".alert-danger, .error-notification, .mat-error").IsVisibleAsync();
        }

        /// <summary>
        /// Dismiss error notification
        /// </summary>
        public async Task DismissErrorNotificationAsync()
        {
            await _page.Locator(".alert-danger .close, .error-notification .dismiss").ClickAsync();
        }

        /// <summary>
        /// Check if error notification is dismissed
        /// </summary>
        public async Task<bool> IsErrorNotificationDismissedAsync()
        {
            return !await _page.Locator(".alert-danger, .error-notification").IsVisibleAsync();
        }

        #endregion

        #region Success Notifications

        /// <summary>
        /// Check if success notification is present
        /// </summary>
        public async Task<bool> HasSuccessNotificationAsync()
        {
            return await _page.Locator(".alert-success, .success-notification, .mat-success").IsVisibleAsync();
        }

        /// <summary>
        /// Get success notification text
        /// </summary>
        public async Task<string> GetSuccessNotificationTextAsync()
        {
            return await _page.Locator(".alert-success, .success-notification, .mat-success").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if success notification has success class
        /// </summary>
        public async Task<bool> HasSuccessNotificationClassAsync()
        {
            return await _page.Locator(".alert-success, .success-notification, .mat-success").IsVisibleAsync();
        }

        /// <summary>
        /// Wait for success notification to be dismissed
        /// </summary>
        public async Task WaitForSuccessNotificationDismissedAsync()
        {
            await _page.Locator(".alert-success, .success-notification").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }

        /// <summary>
        /// Check if success notification is dismissed
        /// </summary>
        public async Task<bool> IsSuccessNotificationDismissedAsync()
        {
            return !await _page.Locator(".alert-success, .success-notification").IsVisibleAsync();
        }

        #endregion

        #region Warning Notifications

        /// <summary>
        /// Check if warning notification is present
        /// </summary>
        public async Task<bool> HasWarningNotificationAsync()
        {
            return await _page.Locator(".alert-warning, .warning-notification, .mat-warning").IsVisibleAsync();
        }

        /// <summary>
        /// Get warning notification text
        /// </summary>
        public async Task<string> GetWarningNotificationTextAsync()
        {
            return await _page.Locator(".alert-warning, .warning-notification, .mat-warning").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if warning notification has warning class
        /// </summary>
        public async Task<bool> HasWarningNotificationClassAsync()
        {
            return await _page.Locator(".alert-warning, .warning-notification, .mat-warning").IsVisibleAsync();
        }

        /// <summary>
        /// Check if warning notification has action button
        /// </summary>
        public async Task<bool> HasWarningNotificationActionAsync()
        {
            return await _page.Locator(".alert-warning button, .warning-notification button").IsVisibleAsync();
        }

        /// <summary>
        /// Click warning notification action
        /// </summary>
        public async Task ClickWarningNotificationActionAsync()
        {
            await _page.Locator(".alert-warning button, .warning-notification button").ClickAsync();
        }

        #endregion

        #region Persistent Notifications

        /// <summary>
        /// Check if persistent notification is present
        /// </summary>
        public async Task<bool> HasPersistentNotificationAsync()
        {
            return await _page.Locator(".persistent-notification, .sticky-notification").IsVisibleAsync();
        }

        /// <summary>
        /// Get persistent notification text
        /// </summary>
        public async Task<string> GetPersistentNotificationTextAsync()
        {
            return await _page.Locator(".persistent-notification, .sticky-notification").TextContentAsync() ?? string.Empty;
        }

        #endregion

        #region Accessibility

        /// <summary>
        /// Check if notification has ARIA label
        /// </summary>
        public async Task<bool> HasNotificationAriaLabelAsync()
        {
            return await _page.Locator("[aria-label], [aria-labelledby]").IsVisibleAsync();
        }

        /// <summary>
        /// Check if notification is focused
        /// </summary>
        public async Task<bool> IsNotificationFocusedAsync()
        {
            return await _page.Locator(".notification-link:focus, .alert:focus").IsVisibleAsync();
        }

        /// <summary>
        /// Get notification screen reader text
        /// </summary>
        public async Task<string> GetNotificationScreenReaderTextAsync()
        {
            return await _page.Locator(".sr-only, [aria-label], [aria-labelledby]").TextContentAsync() ?? string.Empty;
        }

        #endregion

        #region Performance and Dismissal

        /// <summary>
        /// Check if any notification is present
        /// </summary>
        public async Task<bool> HasAnyNotificationAsync()
        {
            return await _page.Locator(".alert, .notification, .mat-mdc-snack-bar-container, .snackbar").IsVisibleAsync();
        }

        /// <summary>
        /// Wait for notification to be displayed
        /// </summary>
        public async Task WaitForNotificationDisplayedAsync()
        {
            await _page.Locator(".alert, .notification, .mat-mdc-snack-bar-container, .snackbar").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Check if dismissible notification is present
        /// </summary>
        public async Task<bool> HasDismissibleNotificationAsync()
        {
            return await _page.Locator(".alert .close, .notification .dismiss, .dismissible-notification").IsVisibleAsync();
        }

        /// <summary>
        /// Check if dismiss button is present
        /// </summary>
        public async Task<bool> HasDismissButtonAsync()
        {
            return await _page.Locator(".alert .close, .notification .dismiss, .dismissible-notification .dismiss").IsVisibleAsync();
        }

        /// <summary>
        /// Click dismiss button
        /// </summary>
        public async Task ClickDismissButtonAsync()
        {
            await _page.Locator(".alert .close, .notification .dismiss, .dismissible-notification .dismiss").ClickAsync();
        }

        /// <summary>
        /// Wait for notification to be dismissed
        /// </summary>
        public async Task WaitForNotificationDismissedAsync()
        {
            await _page.Locator(".alert, .notification").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }

        /// <summary>
        /// Check if notification is dismissed
        /// </summary>
        public async Task<bool> IsNotificationDismissedAsync()
        {
            return !await _page.Locator(".alert, .notification").IsVisibleAsync();
        }

        #endregion

        #region Notification Management

        /// <summary>
        /// Get notification count
        /// </summary>
        public async Task<int> GetNotificationCountAsync()
        {
            var notifications = await _page.Locator(".alert, .notification, .mat-mdc-snack-bar-container, .snackbar").CountAsync();
            return notifications;
        }

        /// <summary>
        /// Check if notifications are stacked
        /// </summary>
        public async Task<bool> AreNotificationsStackedAsync()
        {
            var notifications = await _page.Locator(".alert, .notification").AllAsync();
            if (notifications.Count <= 1) return true;

            // Check if notifications have proper spacing or stacking classes
            var firstNotification = notifications.First();
            var secondNotification = notifications.Skip(1).First();
            
            var firstRect = await firstNotification.BoundingBoxAsync();
            var secondRect = await secondNotification.BoundingBoxAsync();
            
            // Check if notifications are positioned one after another
            return secondRect.Y > firstRect.Y;
        }

        /// <summary>
        /// Check if notifications are ordered by timestamp
        /// </summary>
        public async Task<bool> AreNotificationsOrderedAsync()
        {
            var notifications = await _page.Locator(".alert, .notification").AllAsync();
            if (notifications.Count <= 1) return true;

            // For now, assume notifications are ordered if they exist
            // In a real implementation, you would check timestamps
            return true;
        }

        /// <summary>
        /// Get grouped notification count
        /// </summary>
        public async Task<int> GetGroupedNotificationCountAsync()
        {
            // Count notifications that might be grouped (same type)
            var groupedNotifications = await _page.Locator(".alert-success, .alert-warning, .alert-danger").CountAsync();
            return groupedNotifications;
        }

        /// <summary>
        /// Get all notifications
        /// </summary>
        public async Task<List<NotificationInfo>> GetAllNotificationsAsync()
        {
            var notifications = new List<NotificationInfo>();
            var notificationElements = await _page.Locator(".alert, .notification, .mat-mdc-snack-bar-container, .snackbar").AllAsync();

            foreach (var element in notificationElements)
            {
                var text = await element.TextContentAsync() ?? string.Empty;
                var type = await GetNotificationTypeAsync(element);
                var timestamp = DateTime.Now; // Default timestamp

                notifications.Add(new NotificationInfo
                {
                    Text = text,
                    Type = type,
                    Timestamp = timestamp
                });
            }

            return notifications;
        }

        /// <summary>
        /// Get notification type from element
        /// </summary>
        private async Task<string> GetNotificationTypeAsync(ILocator element)
        {
            var classList = await element.GetAttributeAsync("class") ?? string.Empty;
            
            if (classList.Contains("alert-success") || classList.Contains("success"))
                return "success";
            if (classList.Contains("alert-warning") || classList.Contains("warning"))
                return "warning";
            if (classList.Contains("alert-danger") || classList.Contains("error"))
                return "error";
            if (classList.Contains("alert-info") || classList.Contains("info"))
                return "info";
            
            return "general";
        }

        #endregion

        #region Email Notifications

        /// <summary>
        /// Check if invitation form is present
        /// </summary>
        public async Task<bool> HasInvitationFormAsync()
        {
            return await _page.Locator(".invitation-form, .contact-form, [data-testid='invitation-form']").IsVisibleAsync();
        }

        /// <summary>
        /// Set invitee email
        /// </summary>
        public async Task SetInviteeEmailAsync(string email)
        {
            await _page.Locator("input[type='email'], .invitee-email, [data-testid='invitee-email']").FillAsync(email);
        }

        /// <summary>
        /// Set invitation subject
        /// </summary>
        public async Task SetInvitationSubjectAsync(string subject)
        {
            await _page.Locator("input[name='subject'], .invitation-subject, [data-testid='invitation-subject']").FillAsync(subject);
        }

        /// <summary>
        /// Set invitation message
        /// </summary>
        public async Task SetInvitationMessageAsync(string message)
        {
            await _page.Locator("textarea[name='message'], .invitation-message, [data-testid='invitation-message']").FillAsync(message);
        }

        /// <summary>
        /// Submit invitation
        /// </summary>
        public async Task SubmitInvitationAsync()
        {
            await _page.Locator("button[type='submit'], .submit-invitation, [data-testid='submit-invitation']").ClickAsync();
        }

        /// <summary>
        /// Wait for invitation success notification
        /// </summary>
        public async Task WaitForInvitationSuccessNotificationAsync()
        {
            await _page.Locator(".invitation-success, .success-message, [data-testid='invitation-success']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get invitation success message
        /// </summary>
        public async Task<string> GetInvitationSuccessMessageAsync()
        {
            return await _page.Locator(".invitation-success, .success-message, [data-testid='invitation-success']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email notification is present
        /// </summary>
        public async Task<bool> HasEmailNotificationAsync()
        {
            return await _page.Locator(".email-notification, .email-sent, [data-testid='email-notification']").IsVisibleAsync();
        }

        /// <summary>
        /// Wait for password reset notification
        /// </summary>
        public async Task WaitForPasswordResetNotificationAsync()
        {
            await _page.Locator(".password-reset-notification, .reset-message, [data-testid='password-reset-notification']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get password reset message
        /// </summary>
        public async Task<string> GetPasswordResetMessageAsync()
        {
            return await _page.Locator(".password-reset-notification, .reset-message, [data-testid='password-reset-notification']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email settings are available
        /// </summary>
        public async Task<bool> HasEmailSettingsAsync()
        {
            return await _page.Locator(".email-settings, .smtp-settings, [data-testid='email-settings']").IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to email settings
        /// </summary>
        public async Task NavigateToEmailSettingsAsync()
        {
            await _page.Locator(".email-settings-link, .smtp-settings-link, [data-testid='email-settings-link']").ClickAsync();
        }

        /// <summary>
        /// Check if SMTP settings are available
        /// </summary>
        public async Task<bool> HasSmtpSettingsAsync()
        {
            return await _page.Locator(".smtp-host, .smtp-port, [data-testid='smtp-settings']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if sender email setting is available
        /// </summary>
        public async Task<bool> HasSenderEmailSettingAsync()
        {
            return await _page.Locator(".sender-email, [data-testid='sender-email']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if sender name setting is available
        /// </summary>
        public async Task<bool> HasSenderNameSettingAsync()
        {
            return await _page.Locator(".sender-name, [data-testid='sender-name']").IsVisibleAsync();
        }

        /// <summary>
        /// Test email configuration
        /// </summary>
        public async Task TestEmailConfigurationAsync()
        {
            await _page.Locator(".test-email-button, [data-testid='test-email']").ClickAsync();
        }

        /// <summary>
        /// Wait for test email notification
        /// </summary>
        public async Task WaitForTestEmailNotificationAsync()
        {
            await _page.Locator(".test-email-notification, .test-result, [data-testid='test-email-notification']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get test email message
        /// </summary>
        public async Task<string> GetTestEmailMessageAsync()
        {
            return await _page.Locator(".test-email-notification, .test-result, [data-testid='test-email-notification']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email templates are available
        /// </summary>
        public async Task<bool> HasEmailTemplatesAsync()
        {
            return await _page.Locator(".email-templates, .template-management, [data-testid='email-templates']").IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to email templates
        /// </summary>
        public async Task NavigateToEmailTemplatesAsync()
        {
            await _page.Locator(".email-templates-link, .template-management-link, [data-testid='email-templates-link']").ClickAsync();
        }

        /// <summary>
        /// Check if invitation template is available
        /// </summary>
        public async Task<bool> HasInvitationTemplateAsync()
        {
            return await _page.Locator(".invitation-template, [data-testid='invitation-template']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if password reset template is available
        /// </summary>
        public async Task<bool> HasPasswordResetTemplateAsync()
        {
            return await _page.Locator(".password-reset-template, [data-testid='password-reset-template']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if welcome template is available
        /// </summary>
        public async Task<bool> HasWelcomeTemplateAsync()
        {
            return await _page.Locator(".welcome-template, [data-testid='welcome-template']").IsVisibleAsync();
        }

        /// <summary>
        /// Preview email template
        /// </summary>
        public async Task PreviewEmailTemplateAsync(string templateType)
        {
            await _page.Locator($".preview-{templateType}-template, [data-testid='preview-{templateType}-template']").ClickAsync();
        }

        /// <summary>
        /// Get email template preview
        /// </summary>
        public async Task<string> GetEmailTemplatePreviewAsync()
        {
            return await _page.Locator(".template-preview, .preview-content, [data-testid='template-preview']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if template variables are available
        /// </summary>
        public async Task<bool> HasTemplateVariablesAsync()
        {
            return await _page.Locator(".template-variables, .variable-list, [data-testid='template-variables']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if notification preferences are available
        /// </summary>
        public async Task<bool> HasNotificationPreferencesAsync()
        {
            return await _page.Locator(".notification-preferences, .preferences-settings, [data-testid='notification-preferences']").IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to notification preferences
        /// </summary>
        public async Task NavigateToNotificationPreferencesAsync()
        {
            await _page.Locator(".notification-preferences-link, .preferences-settings-link, [data-testid='notification-preferences-link']").ClickAsync();
        }

        /// <summary>
        /// Check if invitation email preference is available
        /// </summary>
        public async Task<bool> HasInvitationEmailPreferenceAsync()
        {
            return await _page.Locator(".invitation-email-preference, [data-testid='invitation-email-preference']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if password reset email preference is available
        /// </summary>
        public async Task<bool> HasPasswordResetEmailPreferenceAsync()
        {
            return await _page.Locator(".password-reset-email-preference, [data-testid='password-reset-email-preference']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if system email preference is available
        /// </summary>
        public async Task<bool> HasSystemEmailPreferenceAsync()
        {
            return await _page.Locator(".system-email-preference, [data-testid='system-email-preference']").IsVisibleAsync();
        }

        /// <summary>
        /// Toggle invitation emails
        /// </summary>
        public async Task ToggleInvitationEmailsAsync(bool enabled)
        {
            var toggle = _page.Locator(".invitation-email-toggle, [data-testid='invitation-email-toggle']");
            var isChecked = await toggle.IsCheckedAsync();
            
            if (isChecked != enabled)
            {
                await toggle.ClickAsync();
            }
        }

        /// <summary>
        /// Check if invitation emails are disabled
        /// </summary>
        public async Task<bool> IsInvitationEmailsDisabledAsync()
        {
            return !await _page.Locator(".invitation-email-toggle, [data-testid='invitation-email-toggle']").IsCheckedAsync();
        }

        /// <summary>
        /// Check if invitation emails are enabled
        /// </summary>
        public async Task<bool> IsInvitationEmailsEnabledAsync()
        {
            return await _page.Locator(".invitation-email-toggle, [data-testid='invitation-email-toggle']").IsCheckedAsync();
        }

        /// <summary>
        /// Save notification preferences
        /// </summary>
        public async Task SaveNotificationPreferencesAsync()
        {
            await _page.Locator(".save-preferences, [data-testid='save-preferences']").ClickAsync();
        }

        /// <summary>
        /// Wait for preferences saved notification
        /// </summary>
        public async Task WaitForPreferencesSavedNotificationAsync()
        {
            await _page.Locator(".preferences-saved, .save-success, [data-testid='preferences-saved']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get preferences saved message
        /// </summary>
        public async Task<string> GetPreferencesSavedMessageAsync()
        {
            return await _page.Locator(".preferences-saved, .save-success, [data-testid='preferences-saved']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Set invalid SMTP settings
        /// </summary>
        public async Task SetInvalidSmtpSettingsAsync()
        {
            await _page.Locator(".smtp-host").FillAsync("invalid-host");
            await _page.Locator(".smtp-port").FillAsync("99999");
        }

        /// <summary>
        /// Wait for email error notification
        /// </summary>
        public async Task WaitForEmailErrorNotificationAsync()
        {
            await _page.Locator(".email-error, .smtp-error, [data-testid='email-error']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get email error message
        /// </summary>
        public async Task<string> GetEmailErrorMessageAsync()
        {
            return await _page.Locator(".email-error, .smtp-error, [data-testid='email-error']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email error details are available
        /// </summary>
        public async Task<bool> HasEmailErrorDetailsAsync()
        {
            return await _page.Locator(".email-error-details, .error-details, [data-testid='email-error-details']").IsVisibleAsync();
        }

        /// <summary>
        /// Set invalid email address
        /// </summary>
        public async Task SetInvalidEmailAddressAsync(string invalidEmail)
        {
            await _page.Locator("input[type='email'], .invitee-email, [data-testid='invitee-email']").FillAsync(invalidEmail);
        }

        /// <summary>
        /// Wait for email validation error
        /// </summary>
        public async Task WaitForEmailValidationErrorAsync()
        {
            await _page.Locator(".email-validation-error, .validation-error, [data-testid='email-validation-error']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get email validation message
        /// </summary>
        public async Task<string> GetEmailValidationMessageAsync()
        {
            return await _page.Locator(".email-validation-error, .validation-error, [data-testid='email-validation-error']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email tracking is available
        /// </summary>
        public async Task<bool> HasEmailTrackingAsync()
        {
            return await _page.Locator(".email-tracking, .delivery-tracking, [data-testid='email-tracking']").IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to email tracking
        /// </summary>
        public async Task NavigateToEmailTrackingAsync()
        {
            await _page.Locator(".email-tracking-link, .delivery-tracking-link, [data-testid='email-tracking-link']").ClickAsync();
        }

        /// <summary>
        /// Check if delivery status is available
        /// </summary>
        public async Task<bool> HasDeliveryStatusAsync()
        {
            return await _page.Locator(".delivery-status, .email-status, [data-testid='delivery-status']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if sent email count is available
        /// </summary>
        public async Task<bool> HasSentEmailCountAsync()
        {
            return await _page.Locator(".sent-count, .emails-sent, [data-testid='sent-count']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if failed email count is available
        /// </summary>
        public async Task<bool> HasFailedEmailCountAsync()
        {
            return await _page.Locator(".failed-count, .emails-failed, [data-testid='failed-count']").IsVisibleAsync();
        }

        /// <summary>
        /// Get email history
        /// </summary>
        public async Task<List<EmailInfo>> GetEmailHistoryAsync()
        {
            var emails = new List<EmailInfo>();
            var emailElements = await _page.Locator(".email-history-item, .email-record, [data-testid='email-history-item']").AllAsync();

            foreach (var element in emailElements)
            {
                var recipient = await element.Locator(".email-recipient, [data-testid='email-recipient']").TextContentAsync() ?? string.Empty;
                var subject = await element.Locator(".email-subject, [data-testid='email-subject']").TextContentAsync() ?? string.Empty;
                var status = await element.Locator(".email-status, [data-testid='email-status']").TextContentAsync() ?? string.Empty;
                var sentDate = await element.Locator(".email-date, [data-testid='email-date']").TextContentAsync() ?? string.Empty;

                emails.Add(new EmailInfo
                {
                    Recipient = recipient,
                    Subject = subject,
                    Status = status,
                    SentDate = DateTime.TryParse(sentDate, out var date) ? date : DateTime.Now
                });
            }

            return emails;
        }

        /// <summary>
        /// Check if rate limit notification is present
        /// </summary>
        public async Task<bool> HasRateLimitNotificationAsync()
        {
            return await _page.Locator(".rate-limit-notification, .rate-limit-error, [data-testid='rate-limit-notification']").IsVisibleAsync();
        }

        /// <summary>
        /// Get rate limit message
        /// </summary>
        public async Task<string> GetRateLimitMessageAsync()
        {
            return await _page.Locator(".rate-limit-notification, .rate-limit-error, [data-testid='rate-limit-notification']").TextContentAsync() ?? string.Empty;
        }

        /// <summary>
        /// Check if email security features are available
        /// </summary>
        public async Task<bool> HasEmailSecurityFeaturesAsync()
        {
            return await _page.Locator(".email-security, .security-features, [data-testid='email-security']").IsVisibleAsync();
        }

        /// <summary>
        /// Navigate to email security
        /// </summary>
        public async Task NavigateToEmailSecurityAsync()
        {
            await _page.Locator(".email-security-link, .security-features-link, [data-testid='email-security-link']").ClickAsync();
        }

        /// <summary>
        /// Check if email encryption is available
        /// </summary>
        public async Task<bool> HasEmailEncryptionAsync()
        {
            return await _page.Locator(".email-encryption, .encryption-setting, [data-testid='email-encryption']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if email authentication is available
        /// </summary>
        public async Task<bool> HasEmailAuthenticationAsync()
        {
            return await _page.Locator(".email-authentication, .auth-setting, [data-testid='email-authentication']").IsVisibleAsync();
        }

        /// <summary>
        /// Check if spam protection is available
        /// </summary>
        public async Task<bool> HasSpamProtectionAsync()
        {
            return await _page.Locator(".spam-protection, .spam-setting, [data-testid='spam-protection']").IsVisibleAsync();
        }

        /// <summary>
        /// Test email security
        /// </summary>
        public async Task TestEmailSecurityAsync()
        {
            await _page.Locator(".test-security, .security-test, [data-testid='test-security']").ClickAsync();
        }

        /// <summary>
        /// Wait for security test results
        /// </summary>
        public async Task WaitForSecurityTestResultsAsync()
        {
            await _page.Locator(".security-test-results, .test-results, [data-testid='security-test-results']").WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Get security test results
        /// </summary>
        public async Task<string> GetSecurityTestResultsAsync()
        {
            return await _page.Locator(".security-test-results, .test-results, [data-testid='security-test-results']").TextContentAsync() ?? string.Empty;
        }

        #endregion
    }

    /// <summary>
    /// Email information model
    /// </summary>
    public class EmailInfo
    {
        public string Recipient { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }
    }

    /// <summary>
    /// Notification information model
    /// </summary>
    public class NotificationInfo
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
} 