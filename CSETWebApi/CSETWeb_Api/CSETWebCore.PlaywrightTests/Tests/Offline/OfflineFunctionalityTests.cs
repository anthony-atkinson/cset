using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.Offline
{
    [TestFixture]
    public class OfflineFunctionalityTests : PlaywrightTestBase
    {
        [Test]
        [Description("Verify offline mode detection and status indicators")]
        public async Task OfflineMode_Should_ShowCorrectStatusIndicators()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify online status indicator is visible
            await Expect(Page.Locator(".online-status-indicator")).ToBeVisibleAsync();
            await Expect(Page.Locator(".online-status-indicator")).ToHaveClassAsync(new Regex("online"));

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Wait for offline detection
            await Page.WaitForTimeoutAsync(2000);

            // Verify offline status indicator
            await Expect(Page.Locator(".offline-status-indicator")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-status-indicator")).ToHaveClassAsync(new Regex("offline"));

            // Verify offline notification
            await Expect(Page.Locator(".offline-notification")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-notification")).ToContainTextAsync("You are currently offline");
        }

        [Test]
        [Description("Verify offline data storage and retrieval")]
        public async Task OfflineData_Should_BeStoredAndRetrieved()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Create assessment data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Offline Test Assessment");
            await Page.FillAsync("#assessment-description", "Test assessment for offline functionality");

            // Simulate offline mode before saving
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Try to save assessment (should queue for offline sync)
            await Page.ClickAsync("#save-assessment-btn");

            // Verify offline queue notification
            await Expect(Page.Locator(".offline-queue-notification")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-queue-notification")).ToContainTextAsync("Changes will be synced when online");

            // Verify data is stored locally
            await Expect(Page.Locator(".offline-data-indicator")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-data-indicator")).ToContainTextAsync("1 pending change");
        }

        [Test]
        [Description("Verify offline form submission and queuing")]
        public async Task OfflineFormSubmission_Should_QueueForSync()
        {
            // Navigate to questions page
            await Page.GotoAsync($"{BaseUrl}/assessment/123/questions");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Answer some questions
            await Page.ClickAsync("#question-1-yes");
            await Page.FillAsync("#question-1-notes", "Offline test notes");

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Submit answers
            await Page.ClickAsync("#submit-answers-btn");

            // Verify offline queue
            await Expect(Page.Locator(".offline-queue")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-queue-item")).ToHaveCountAsync(1);

            // Verify queue item details
            await Expect(Page.Locator(".offline-queue-item")).ToContainTextAsync("Question Answer Update");
            await Expect(Page.Locator(".offline-queue-item")).ToContainTextAsync("Pending");
        }

        [Test]
        [Description("Verify offline sync when connection is restored")]
        public async Task OfflineSync_Should_ProcessQueuedItems()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Create some offline data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Sync Test Assessment");

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Queue the change
            await Page.ClickAsync("#save-assessment-btn");

            // Verify offline queue
            await Expect(Page.Locator(".offline-queue-item")).ToHaveCountAsync(1);

            // Restore connection
            await Page.UnrouteAsync("**/*");

            // Wait for sync to start
            await Page.WaitForTimeoutAsync(1000);

            // Verify sync in progress
            await Expect(Page.Locator(".sync-progress")).ToBeVisibleAsync();
            await Expect(Page.Locator(".sync-progress")).ToContainTextAsync("Syncing...");

            // Wait for sync to complete
            await Page.WaitForTimeoutAsync(3000);

            // Verify sync completed
            await Expect(Page.Locator(".sync-complete")).ToBeVisibleAsync();
            await Expect(Page.Locator(".sync-complete")).ToContainTextAsync("Sync completed successfully");

            // Verify queue is empty
            await Expect(Page.Locator(".offline-queue-item")).ToHaveCountAsync(0);
        }

        [Test]
        [Description("Verify offline data persistence across browser sessions")]
        public async Task OfflineData_Should_PersistAcrossSessions()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Create offline data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Persistent Test Assessment");

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Queue the change
            await Page.ClickAsync("#save-assessment-btn");

            // Verify data is stored
            await Expect(Page.Locator(".offline-data-indicator")).ToContainTextAsync("1 pending change");

            // Reload page
            await Page.ReloadAsync();

            // Verify offline data persists
            await Expect(Page.Locator(".offline-data-indicator")).ToContainTextAsync("1 pending change");
            await Expect(Page.Locator(".offline-queue-item")).ToHaveCountAsync(1);
        }

        [Test]
        [Description("Verify offline conflict resolution")]
        public async Task OfflineConflict_Should_BeResolvedProperly()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment/123");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Make offline changes
            await Page.FillAsync("#assessment-name", "Offline Modified Name");

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Queue the change
            await Page.ClickAsync("#save-assessment-btn");

            // Restore connection with conflict
            await Page.UnrouteAsync("**/*");
            
            // Mock conflict response
            await Page.RouteAsync("**/api/assessment/123", async route =>
            {
                await route.FulfillAsync(new RouteFulfillOptions
                {
                    Status = 409,
                    ContentType = "application/json",
                    Body = "{\"error\":\"Conflict\",\"message\":\"Assessment has been modified by another user\"}"
                });
            });

            // Wait for sync attempt
            await Page.WaitForTimeoutAsync(2000);

            // Verify conflict resolution dialog
            await Expect(Page.Locator(".conflict-resolution-dialog")).ToBeVisibleAsync();
            await Expect(Page.Locator(".conflict-resolution-dialog")).ToContainTextAsync("Conflict detected");

            // Choose to keep local changes
            await Page.ClickAsync("#keep-local-changes-btn");

            // Verify conflict resolved
            await Expect(Page.Locator(".conflict-resolution-dialog")).Not.ToBeVisibleAsync();
            await Expect(Page.Locator(".sync-complete")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify offline storage limits and cleanup")]
        public async Task OfflineStorage_Should_HandleLimitsAndCleanup()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Create multiple offline items to test limits
            for (int i = 1; i <= 5; i++)
            {
                await Page.ClickAsync("#create-assessment-btn");
                await Page.FillAsync("#assessment-name", $"Test Assessment {i}");
                await Page.ClickAsync("#save-assessment-btn");
                await Page.WaitForTimeoutAsync(500);
            }

            // Verify storage limit warning
            await Expect(Page.Locator(".storage-limit-warning")).ToBeVisibleAsync();
            await Expect(Page.Locator(".storage-limit-warning")).ToContainTextAsync("Storage limit approaching");

            // Test cleanup functionality
            await Page.ClickAsync("#cleanup-offline-data-btn");

            // Verify cleanup confirmation
            await Expect(Page.Locator(".cleanup-confirmation")).ToBeVisibleAsync();
            await Page.ClickAsync("#confirm-cleanup-btn");

            // Verify data cleaned up
            await Expect(Page.Locator(".offline-data-indicator")).ToContainTextAsync("0 pending changes");
        }

        [Test]
        [Description("Verify offline error handling and recovery")]
        public async Task OfflineErrors_Should_BeHandledGracefully()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Simulate offline mode with storage errors
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Mock localStorage errors
            await Page.AddInitScriptAsync(@"
                const originalSetItem = localStorage.setItem;
                localStorage.setItem = function(key, value) {
                    if (key.includes('offline')) {
                        throw new Error('Storage quota exceeded');
                    }
                    return originalSetItem.call(this, key, value);
                };
            ");

            // Try to create offline data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Error Test Assessment");
            await Page.ClickAsync("#save-assessment-btn");

            // Verify error handling
            await Expect(Page.Locator(".offline-error-notification")).ToBeVisibleAsync();
            await Expect(Page.Locator(".offline-error-notification")).ToContainTextAsync("Unable to save offline data");

            // Verify recovery options
            await Expect(Page.Locator(".offline-recovery-options")).ToBeVisibleAsync();
            await Page.ClickAsync("#retry-offline-save-btn");

            // Verify retry attempt
            await Expect(Page.Locator(".offline-retry-indicator")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify offline performance and responsiveness")]
        public async Task OfflineMode_Should_RemainResponsive()
        {
            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Test UI responsiveness
            await Page.ClickAsync("#create-assessment-btn");
            await Expect(Page.Locator("#assessment-form")).ToBeVisibleAsync();

            // Fill form quickly
            await Page.FillAsync("#assessment-name", "Performance Test");
            await Page.FillAsync("#assessment-description", "Testing offline performance");

            // Verify immediate UI updates
            await Expect(Page.Locator("#assessment-name")).ToHaveValueAsync("Performance Test");

            // Test navigation while offline
            await Page.ClickAsync("#questions-tab");
            await Expect(Page.Locator(".questions-container")).ToBeVisibleAsync();

            // Verify no loading delays
            await Expect(Page.Locator(".loading-spinner")).Not.ToBeVisibleAsync();
        }
    }
} 