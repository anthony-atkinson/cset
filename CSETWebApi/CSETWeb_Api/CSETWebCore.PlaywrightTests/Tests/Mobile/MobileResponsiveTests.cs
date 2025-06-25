using Microsoft.Playwright;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.Tests.Mobile
{
    [TestFixture]
    public class MobileResponsiveTests : PlaywrightTestBase
    {
        [Test]
        [Description("Verify mobile navigation and menu functionality")]
        public async Task MobileNavigation_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to main page
            await Page.GotoAsync($"{BaseUrl}/");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify mobile menu button is visible
            await Expect(Page.Locator(".mobile-menu-toggle")).ToBeVisibleAsync();

            // Open mobile menu
            await Page.ClickAsync(".mobile-menu-toggle");

            // Verify mobile menu is visible
            await Expect(Page.Locator(".mobile-menu")).ToBeVisibleAsync();
            await Expect(Page.Locator(".mobile-menu")).ToHaveClassAsync(new Regex("open"));

            // Verify menu items are accessible
            await Expect(Page.Locator(".mobile-menu .nav-item")).ToHaveCountAsync(5);

            // Navigate using mobile menu
            await Page.ClickAsync(".mobile-menu .nav-item[href='/assessment']");

            // Verify navigation occurred
            await Expect(Page).ToHaveURLAsync(new Regex(".*/assessment"));

            // Verify mobile menu closes after navigation
            await Expect(Page.Locator(".mobile-menu")).Not.ToHaveClassAsync(new Regex("open"));
        }

        [Test]
        [Description("Verify mobile assessment creation workflow")]
        public async Task MobileAssessmentCreation_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify mobile layout is active
            await Expect(Page.Locator(".mobile-layout")).ToBeVisibleAsync();

            // Create new assessment
            await Page.ClickAsync("#create-assessment-btn");

            // Verify mobile form layout
            await Expect(Page.Locator(".mobile-form-container")).ToBeVisibleAsync();

            // Fill assessment details
            await Page.FillAsync("#assessment-name", "Mobile Test Assessment");
            await Page.FillAsync("#assessment-description", "Testing mobile assessment creation");

            // Verify mobile-friendly input fields
            await Expect(Page.Locator("#assessment-name")).ToHaveAttributeAsync("type", "text");
            await Expect(Page.Locator("#assessment-description")).ToHaveAttributeAsync("type", "text");

            // Submit form
            await Page.ClickAsync("#save-assessment-btn");

            // Verify success message
            await Expect(Page.Locator(".success-message")).ToBeVisibleAsync();
            await Expect(Page.Locator(".success-message")).ToContainTextAsync("Assessment created successfully");
        }

        [Test]
        [Description("Verify mobile question answering interface")]
        public async Task MobileQuestionInterface_Should_BeUserFriendly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to questions page
            await Page.GotoAsync($"{BaseUrl}/assessment/123/questions");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify mobile question layout
            await Expect(Page.Locator(".mobile-question-container")).ToBeVisibleAsync();

            // Verify touch-friendly buttons
            await Expect(Page.Locator(".question-answer-btn")).ToHaveCountAsync(3); // Yes, No, Maybe
            await Expect(Page.Locator(".question-answer-btn")).ToHaveCSSAsync("min-height", "44px");

            // Answer a question
            await Page.ClickAsync("#question-1-yes");

            // Verify answer is selected
            await Expect(Page.Locator("#question-1-yes")).ToHaveClassAsync(new Regex("selected"));

            // Add notes
            await Page.FillAsync("#question-1-notes", "Mobile test notes");

            // Verify mobile-friendly textarea
            await Expect(Page.Locator("#question-1-notes")).ToHaveCSSAsync("font-size", "16px");

            // Navigate to next question
            await Page.ClickAsync("#next-question-btn");

            // Verify smooth navigation
            await Expect(Page.Locator("#question-2")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify mobile dashboard responsiveness")]
        public async Task MobileDashboard_Should_BeResponsive()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to dashboard
            await Page.GotoAsync($"{BaseUrl}/dashboard");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify mobile dashboard layout
            await Expect(Page.Locator(".mobile-dashboard")).ToBeVisibleAsync();

            // Verify charts are mobile-optimized
            await Expect(Page.Locator(".mobile-chart-container")).ToBeVisibleAsync();
            await Expect(Page.Locator(".chart-canvas")).ToHaveCSSAsync("max-width", "100%");

            // Verify mobile-friendly widgets
            await Expect(Page.Locator(".mobile-widget")).ToHaveCountAsync(4);

            // Test widget interaction
            await Page.ClickAsync(".mobile-widget:first-child");

            // Verify widget expands properly
            await Expect(Page.Locator(".mobile-widget:first-child")).ToHaveClassAsync(new Regex("expanded"));

            // Verify touch-friendly controls
            await Expect(Page.Locator(".mobile-control")).ToHaveCSSAsync("min-height", "44px");
        }

        [Test]
        [Description("Verify mobile form validation and error handling")]
        public async Task MobileFormValidation_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment creation
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Try to submit empty form
            await Page.ClickAsync("#create-assessment-btn");
            await Page.ClickAsync("#save-assessment-btn");

            // Verify mobile-friendly error messages
            await Expect(Page.Locator(".mobile-error-message")).ToBeVisibleAsync();
            await Expect(Page.Locator(".mobile-error-message")).ToContainTextAsync("Assessment name is required");

            // Verify error message is touch-friendly
            await Expect(Page.Locator(".mobile-error-message")).ToHaveCSSAsync("font-size", "14px");

            // Fill required fields
            await Page.FillAsync("#assessment-name", "Valid Assessment");

            // Submit again
            await Page.ClickAsync("#save-assessment-btn");

            // Verify no errors
            await Expect(Page.Locator(".mobile-error-message")).Not.ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify mobile touch interactions")]
        public async Task MobileTouchInteractions_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Test swipe navigation
            await Page.Touchscreen.SwipeAsync(300, 400, 100, 400);

            // Verify swipe gesture is detected
            await Expect(Page.Locator(".swipe-indicator")).ToBeVisibleAsync();

            // Test pinch zoom
            await Page.Touchscreen.PinchAsync(200, 300, 1.5);

            // Verify zoom is handled appropriately
            await Expect(Page.Locator(".zoom-indicator")).ToBeVisibleAsync();

            // Test long press
            await Page.Locator(".assessment-item").First.PressAsync("TouchStart");
            await Page.WaitForTimeoutAsync(1000);
            await Page.Locator(".assessment-item").First.PressAsync("TouchEnd");

            // Verify context menu appears
            await Expect(Page.Locator(".mobile-context-menu")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify mobile performance and loading states")]
        public async Task MobilePerformance_Should_BeOptimized()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify mobile loading indicators
            await Expect(Page.Locator(".mobile-loading-spinner")).ToBeVisibleAsync();

            // Wait for content to load
            await Page.WaitForTimeoutAsync(2000);

            // Verify loading spinner disappears
            await Expect(Page.Locator(".mobile-loading-spinner")).Not.ToBeVisibleAsync();

            // Test mobile-optimized images
            await Expect(Page.Locator(".mobile-image")).ToHaveAttributeAsync("loading", "lazy");

            // Verify mobile-friendly animations
            await Expect(Page.Locator(".mobile-animation")).ToHaveCSSAsync("transition-duration", "0.3s");
        }

        [Test]
        [Description("Verify mobile accessibility features")]
        public async Task MobileAccessibility_Should_BeCompliant()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify ARIA labels
            await Expect(Page.Locator("#create-assessment-btn")).ToHaveAttributeAsync("aria-label", "Create new assessment");

            // Verify focus indicators
            await Page.ClickAsync("#create-assessment-btn");
            await Expect(Page.Locator("#assessment-name")).ToHaveCSSAsync("outline", "2px solid rgb(0, 123, 255)");

            // Verify screen reader support
            await Expect(Page.Locator(".sr-only")).ToBeVisibleAsync();

            // Verify color contrast
            await Expect(Page.Locator(".mobile-text")).ToHaveCSSAsync("color", "rgb(33, 37, 41)");
        }

        [Test]
        [Description("Verify mobile orientation handling")]
        public async Task MobileOrientation_Should_AdaptCorrectly()
        {
            // Start in portrait mode
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify portrait layout
            await Expect(Page.Locator(".portrait-layout")).ToBeVisibleAsync();

            // Switch to landscape
            await Page.SetViewportSizeAsync(667, 375);

            // Wait for layout adjustment
            await Page.WaitForTimeoutAsync(1000);

            // Verify landscape layout
            await Expect(Page.Locator(".landscape-layout")).ToBeVisibleAsync();

            // Verify content adapts
            await Expect(Page.Locator(".mobile-content")).ToHaveCSSAsync("flex-direction", "row");

            // Switch back to portrait
            await Page.SetViewportSizeAsync(375, 667);

            // Wait for layout adjustment
            await Page.WaitForTimeoutAsync(1000);

            // Verify portrait layout restored
            await Expect(Page.Locator(".portrait-layout")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify mobile offline functionality")]
        public async Task MobileOffline_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Simulate offline mode
            await Page.RouteAsync("**/*", async route =>
            {
                await route.AbortAsync();
            });

            // Wait for offline detection
            await Page.WaitForTimeoutAsync(2000);

            // Verify mobile offline indicator
            await Expect(Page.Locator(".mobile-offline-indicator")).ToBeVisibleAsync();

            // Create offline data
            await Page.ClickAsync("#create-assessment-btn");
            await Page.FillAsync("#assessment-name", "Mobile Offline Test");
            await Page.ClickAsync("#save-assessment-btn");

            // Verify mobile offline queue
            await Expect(Page.Locator(".mobile-offline-queue")).ToBeVisibleAsync();
            await Expect(Page.Locator(".mobile-offline-queue")).ToContainTextAsync("1 pending change");

            // Restore connection
            await Page.UnrouteAsync("**/*");

            // Wait for sync
            await Page.WaitForTimeoutAsync(3000);

            // Verify sync completed
            await Expect(Page.Locator(".mobile-sync-complete")).ToBeVisibleAsync();
        }

        [Test]
        [Description("Verify mobile keyboard handling")]
        public async Task MobileKeyboard_Should_WorkCorrectly()
        {
            // Set mobile viewport
            await Page.SetViewportSizeAsync(375, 667);

            // Navigate to assessment page
            await Page.GotoAsync($"{BaseUrl}/assessment");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Create new assessment
            await Page.ClickAsync("#create-assessment-btn");

            // Focus on name field
            await Page.ClickAsync("#assessment-name");

            // Verify mobile keyboard appears
            await Expect(Page.Locator(".mobile-keyboard-indicator")).ToBeVisibleAsync();

            // Type with mobile keyboard
            await Page.Keyboard.TypeAsync("Mobile Keyboard Test");

            // Verify input works
            await Expect(Page.Locator("#assessment-name")).ToHaveValueAsync("Mobile Keyboard Test");

            // Test keyboard navigation
            await Page.Keyboard.PressAsync("Tab");

            // Verify focus moves to next field
            await Expect(Page.Locator("#assessment-description")).ToHaveCSSAsync("outline", "2px solid rgb(0, 123, 255)");

            // Test keyboard submission
            await Page.Keyboard.PressAsync("Enter");

            // Verify form submission
            await Expect(Page.Locator(".success-message")).ToBeVisibleAsync();
        }
    }
} 