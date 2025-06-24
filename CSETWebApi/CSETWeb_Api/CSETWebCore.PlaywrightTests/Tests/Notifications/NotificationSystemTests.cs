using System;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Notifications;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Tests.Notifications
{
    /// <summary>
    /// Comprehensive tests for CSET notification system functionality
    /// </summary>
    [TestFixture]
    [TestCategory("Notifications")]
    [TestCategory("E2E")]
    public class NotificationSystemTests : BaseTestFixture
    {
        private LoginPage _loginPage = null!;
        private DashboardPage _dashboardPage = null!;
        private NotificationPage _notificationPage = null!;

        protected override async Task SetUpAsync()
        {
            await base.SetUpAsync();
            
            _loginPage = new LoginPage(Page);
            _dashboardPage = new DashboardPage(Page);
            _notificationPage = new NotificationPage(Page);
        }

        [Test]
        [Description("Verify version notification display and interaction")]
        public async Task VersionNotification_Should_DisplayAndBeInteractive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for version notification
            var hasVersionNotification = await _notificationPage.HasVersionNotificationAsync();

            if (hasVersionNotification)
            {
                // Verify notification icon and badge
                await _notificationPage.WaitForVersionNotificationIconAsync();
                var notificationCount = await _notificationPage.GetVersionNotificationCountAsync();
                Assert.That(notificationCount, Is.GreaterThan(0), "Version notification count should be greater than 0");

                // Click notification to open details
                await _notificationPage.ClickVersionNotificationAsync();
                await _notificationPage.WaitForVersionNotificationDialogAsync();

                // Verify notification content
                var notificationTitle = await _notificationPage.GetVersionNotificationTitleAsync();
                Assert.That(notificationTitle, Is.Not.Empty, "Version notification should have a title");

                var notificationContent = await _notificationPage.GetVersionNotificationContentAsync();
                Assert.That(notificationContent, Is.Not.Empty, "Version notification should have content");

                // Close notification
                await _notificationPage.CloseVersionNotificationAsync();
                await _notificationPage.WaitForVersionNotificationDialogClosedAsync();
            }
            else
            {
                TestContext.WriteLine("No version notification present - this is expected if running latest version");
            }
        }

        [Test]
        [Description("Verify upgrade notification functionality")]
        public async Task UpgradeNotification_Should_DisplayAndAllowUpgrade()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Create an assessment that might need upgrading
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _dashboardPage.WaitForAssessmentCreationPageAsync();

            // Act - Check for upgrade notification
            var hasUpgradeNotification = await _notificationPage.HasUpgradeNotificationAsync();

            if (hasUpgradeNotification)
            {
                // Verify upgrade notification display
                await _notificationPage.WaitForUpgradeNotificationAsync();
                var upgradeText = await _notificationPage.GetUpgradeNotificationTextAsync();
                Assert.That(upgradeText, Is.Not.Empty, "Upgrade notification should display text");

                // Verify upgrade button is present
                var hasUpgradeButton = await _notificationPage.HasUpgradeButtonAsync();
                Assert.That(hasUpgradeButton, Is.True, "Upgrade notification should have an upgrade button");

                // Test hiding the notification
                await _notificationPage.HideUpgradeNotificationAsync();
                var isHidden = await _notificationPage.IsUpgradeNotificationHiddenAsync();
                Assert.That(isHidden, Is.True, "Upgrade notification should be hidden after clicking hide");
            }
            else
            {
                TestContext.WriteLine("No upgrade notification present - this is expected if no upgrade is available");
            }
        }

        [Test]
        [Description("Verify snackbar notification system")]
        public async Task SnackbarNotifications_Should_DisplayAndAutoDismiss()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Trigger an action that should show a snackbar notification
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _dashboardPage.WaitForAssessmentCreationPageAsync();

            // Check for snackbar notifications
            var hasSnackbar = await _notificationPage.HasSnackbarNotificationAsync();
            
            if (hasSnackbar)
            {
                // Verify snackbar content
                var snackbarText = await _notificationPage.GetSnackbarTextAsync();
                Assert.That(snackbarText, Is.Not.Empty, "Snackbar should display text");

                // Verify snackbar actions (if any)
                var hasSnackbarAction = await _notificationPage.HasSnackbarActionAsync();
                if (hasSnackbarAction)
                {
                    await _notificationPage.ClickSnackbarActionAsync();
                }

                // Wait for auto-dismiss
                await _notificationPage.WaitForSnackbarDismissedAsync();
                var isDismissed = await _notificationPage.IsSnackbarDismissedAsync();
                Assert.That(isDismissed, Is.True, "Snackbar should auto-dismiss");
            }
        }

        [Test]
        [Description("Verify error notification handling")]
        public async Task ErrorNotifications_Should_DisplayCorrectly()
        {
            // Arrange
            await _loginPage.NavigateAsync();

            // Act - Try to login with invalid credentials to trigger error notification
            await _loginPage.SetUsernameAsync("invalid@example.com");
            await _loginPage.SetPasswordAsync("invalidpassword");
            await _loginPage.ClickLoginButtonAsync();

            // Verify error notification appears
            await _notificationPage.WaitForErrorNotificationAsync();
            var errorText = await _notificationPage.GetErrorNotificationTextAsync();
            Assert.That(errorText, Is.Not.Empty, "Error notification should display error text");

            // Verify error notification styling
            var hasErrorClass = await _notificationPage.HasErrorNotificationClassAsync();
            Assert.That(hasErrorClass, Is.True, "Error notification should have error styling");

            // Verify error notification can be dismissed
            await _notificationPage.DismissErrorNotificationAsync();
            var isDismissed = await _notificationPage.IsErrorNotificationDismissedAsync();
            Assert.That(isDismissed, Is.True, "Error notification should be dismissible");
        }

        [Test]
        [Description("Verify success notification handling")]
        public async Task SuccessNotifications_Should_DisplayCorrectly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Perform an action that should trigger a success notification
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _dashboardPage.WaitForAssessmentCreationPageAsync();

            // Check for success notifications
            var hasSuccessNotification = await _notificationPage.HasSuccessNotificationAsync();
            
            if (hasSuccessNotification)
            {
                // Verify success notification content
                var successText = await _notificationPage.GetSuccessNotificationTextAsync();
                Assert.That(successText, Is.Not.Empty, "Success notification should display text");

                // Verify success notification styling
                var hasSuccessClass = await _notificationPage.HasSuccessNotificationClassAsync();
                Assert.That(hasSuccessClass, Is.True, "Success notification should have success styling");

                // Verify success notification auto-dismisses
                await _notificationPage.WaitForSuccessNotificationDismissedAsync();
                var isDismissed = await _notificationPage.IsSuccessNotificationDismissedAsync();
                Assert.That(isDismissed, Is.True, "Success notification should auto-dismiss");
            }
        }

        [Test]
        [Description("Verify warning notification handling")]
        public async Task WarningNotifications_Should_DisplayCorrectly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for warning notifications (like incomplete assessments)
            var hasWarningNotification = await _notificationPage.HasWarningNotificationAsync();
            
            if (hasWarningNotification)
            {
                // Verify warning notification content
                var warningText = await _notificationPage.GetWarningNotificationTextAsync();
                Assert.That(warningText, Is.Not.Empty, "Warning notification should display text");

                // Verify warning notification styling
                var hasWarningClass = await _notificationPage.HasWarningNotificationClassAsync();
                Assert.That(hasWarningClass, Is.True, "Warning notification should have warning styling");

                // Verify warning notification actions
                var hasWarningAction = await _notificationPage.HasWarningNotificationActionAsync();
                if (hasWarningAction)
                {
                    await _notificationPage.ClickWarningNotificationActionAsync();
                }
            }
        }

        [Test]
        [Description("Verify notification persistence across page navigation")]
        public async Task Notifications_Should_PersistAcrossNavigation()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for any persistent notifications
            var hasPersistentNotification = await _notificationPage.HasPersistentNotificationAsync();
            
            if (hasPersistentNotification)
            {
                // Get notification content before navigation
                var notificationText = await _notificationPage.GetPersistentNotificationTextAsync();
                
                // Navigate to another page
                await _dashboardPage.ClickCreateAssessmentAsync();
                await _dashboardPage.WaitForAssessmentCreationPageAsync();

                // Verify notification still exists
                var stillHasNotification = await _notificationPage.HasPersistentNotificationAsync();
                Assert.That(stillHasNotification, Is.True, "Persistent notification should remain after navigation");

                var newNotificationText = await _notificationPage.GetPersistentNotificationTextAsync();
                Assert.That(newNotificationText, Is.EqualTo(notificationText), "Persistent notification text should remain the same");
            }
        }

        [Test]
        [Description("Verify notification accessibility features")]
        public async Task Notifications_Should_BeAccessible()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for version notification accessibility
            var hasVersionNotification = await _notificationPage.HasVersionNotificationAsync();
            
            if (hasVersionNotification)
            {
                // Verify ARIA labels
                var hasAriaLabel = await _notificationPage.HasNotificationAriaLabelAsync();
                Assert.That(hasAriaLabel, Is.True, "Notification should have ARIA label");

                // Verify keyboard navigation
                await Page.Keyboard.PressAsync("Tab");
                var isFocused = await _notificationPage.IsNotificationFocusedAsync();
                Assert.That(isFocused, Is.True, "Notification should be keyboard accessible");

                // Verify screen reader text
                var screenReaderText = await _notificationPage.GetNotificationScreenReaderTextAsync();
                Assert.That(screenReaderText, Is.Not.Empty, "Notification should have screen reader text");
            }
        }

        [Test]
        [Description("Verify notification performance and responsiveness")]
        public async Task Notifications_Should_BePerformant()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Measure notification display time
            var startTime = DateTime.Now;
            
            // Trigger an action that might show notifications
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _dashboardPage.WaitForAssessmentCreationPageAsync();

            // Check for any notifications
            var hasNotification = await _notificationPage.HasAnyNotificationAsync();
            
            if (hasNotification)
            {
                await _notificationPage.WaitForNotificationDisplayedAsync();
                var displayTime = DateTime.Now - startTime;
                
                // Assert notification displays quickly
                Assert.That(displayTime.TotalMilliseconds, Is.LessThan(1000), 
                    $"Notification should display within 1000ms. Actual: {displayTime.TotalMilliseconds:F2}ms");
            }
        }

        [Test]
        [Description("Verify notification dismissal functionality")]
        public async Task Notifications_Should_BeDismissible()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for dismissible notifications
            var hasDismissibleNotification = await _notificationPage.HasDismissibleNotificationAsync();
            
            if (hasDismissibleNotification)
            {
                // Verify dismiss button is present
                var hasDismissButton = await _notificationPage.HasDismissButtonAsync();
                Assert.That(hasDismissButton, Is.True, "Dismissible notification should have dismiss button");

                // Click dismiss button
                await _notificationPage.ClickDismissButtonAsync();
                
                // Verify notification is dismissed
                await _notificationPage.WaitForNotificationDismissedAsync();
                var isDismissed = await _notificationPage.IsNotificationDismissedAsync();
                Assert.That(isDismissed, Is.True, "Notification should be dismissed after clicking dismiss button");
            }
        }

        [Test]
        [Description("Verify notification grouping and stacking")]
        public async Task Notifications_Should_GroupAndStackCorrectly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check for multiple notifications
            var notificationCount = await _notificationPage.GetNotificationCountAsync();
            
            if (notificationCount > 1)
            {
                // Verify notifications are properly stacked
                var isStacked = await _notificationPage.AreNotificationsStackedAsync();
                Assert.That(isStacked, Is.True, "Multiple notifications should be stacked properly");

                // Verify notification order (newest first)
                var isOrdered = await _notificationPage.AreNotificationsOrderedAsync();
                Assert.That(isOrdered, Is.True, "Notifications should be ordered by timestamp");

                // Verify notification grouping
                var groupedCount = await _notificationPage.GetGroupedNotificationCountAsync();
                Assert.That(groupedCount, Is.LessThanOrEqualTo(notificationCount), 
                    "Grouped notification count should not exceed total count");
            }
        }

        [Test]
        [Description("Verify notification content validation")]
        public async Task NotificationContent_Should_BeValid()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Check all visible notifications
            var notifications = await _notificationPage.GetAllNotificationsAsync();
            
            foreach (var notification in notifications)
            {
                // Verify notification has required fields
                Assert.That(notification.Text, Is.Not.Empty, "Notification should have text content");
                Assert.That(notification.Type, Is.Not.Empty, "Notification should have a type");
                
                // Verify notification text is not too long
                Assert.That(notification.Text.Length, Is.LessThan(500), 
                    "Notification text should not be excessively long");

                // Verify notification has valid timestamp
                Assert.That(notification.Timestamp, Is.Not.Null, "Notification should have a timestamp");
                Assert.That(notification.Timestamp, Is.LessThanOrEqualTo(DateTime.Now), 
                    "Notification timestamp should not be in the future");
            }
        }
    }
} 