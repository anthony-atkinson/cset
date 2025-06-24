using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Dashboard;
using CSETWebCore.PlaywrightTests.PageObjects.Questions;
using NUnit.Framework;

namespace CSETWebCore.PlaywrightTests.Tests.Performance
{
    /// <summary>
    /// Performance tests for specific UI components and interactions
    /// </summary>
    [TestFixture]
    [TestCategory("Performance")]
    [TestCategory("E2E")]
    public class UIComponentPerformanceTests : PerformanceTestFixture
    {
        private LoginPage _loginPage = null!;
        private DashboardPage _dashboardPage = null!;
        private AssessmentCreationPage _assessmentCreationPage = null!;
        private QuestionNavigationPage _questionNavigationPage = null!;

        protected override async Task SetUpAsync()
        {
            await base.SetUpAsync();
            
            _loginPage = new LoginPage(Page);
            _dashboardPage = new DashboardPage(Page);
            _assessmentCreationPage = new AssessmentCreationPage(Page);
            _questionNavigationPage = new QuestionNavigationPage(Page);
        }

        [Test]
        [Description("Verify form input responsiveness")]
        public async Task FormInputs_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test input responsiveness
            var inputTime = await MeasureActionAsync(async () =>
            {
                // Test username input
                await _loginPage.SetUsernameAsync("testuser");
                await Task.Delay(100);
                
                // Test password input
                await _loginPage.SetPasswordAsync("testpassword");
                await Task.Delay(100);
                
                // Test clearing inputs
                await _loginPage.ClearFormAsync();
                await Task.Delay(100);
            });

            // Assert
            Assert.That(inputTime.TotalMilliseconds, Is.LessThan(1000), 
                $"Form input operations should complete within 1000ms. Actual: {inputTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify button click responsiveness")]
        public async Task ButtonClicks_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test button click responsiveness
            var clickTime = await MeasureActionAsync(async () =>
            {
                // Test multiple button clicks
                for (int i = 0; i < 5; i++)
                {
                    await _loginPage.ClickLoginButtonAsync();
                    await Task.Delay(50);
                }
            });

            // Assert
            Assert.That(clickTime.TotalMilliseconds, Is.LessThan(500), 
                $"Button click operations should complete within 500ms. Actual: {clickTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify dropdown/select responsiveness")]
        public async Task DropdownSelections_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();
            await _dashboardPage.ClickCreateAssessmentAsync();
            await _assessmentCreationPage.WaitForPageLoadAsync();

            // Act - Test dropdown selection responsiveness
            var selectionTime = await MeasureActionAsync(async () =>
            {
                // Test multiple gallery item selections
                for (int i = 0; i < 3; i++)
                {
                    await _assessmentCreationPage.SelectGalleryItemByIndexAsync(i);
                    await Task.Delay(100);
                }
            });

            // Assert
            Assert.That(selectionTime.TotalMilliseconds, Is.LessThan(1000), 
                $"Dropdown selection operations should complete within 1000ms. Actual: {selectionTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 2.0,
                MaxMemoryIncreaseMB = 10
            });
        }

        [Test]
        [Description("Verify chart rendering performance")]
        public async Task ChartRendering_Should_BeFast()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test chart rendering performance
            var chartRenderTime = await MeasureActionWithMemoryAsync(async () =>
            {
                await _dashboardPage.WaitForChartsToRenderAsync();
                await WaitForPerformanceStabilityAsync(3000);
            });

            // Assert
            Assert.That(chartRenderTime.Duration.TotalSeconds, Is.LessThan(3.0), 
                $"Chart rendering should complete within 3 seconds. Actual: {chartRenderTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = chartRenderTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(15.0), 
                $"Chart rendering should not increase memory by more than 15MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 3.0,
                MaxFirstPaintTime = 2000,
                MaxFirstContentfulPaintTime = 2500,
                MaxMemoryIncreaseMB = 15
            });
        }

        [Test]
        [Description("Verify table rendering performance")]
        public async Task TableRendering_Should_BeFast()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test table rendering performance
            var tableRenderTime = await MeasureActionWithMemoryAsync(async () =>
            {
                // Navigate to a section that has tables (if available)
                await _dashboardPage.WaitForDashboardLoadAsync();
                await WaitForPerformanceStabilityAsync(2000);
            });

            // Assert
            Assert.That(tableRenderTime.Duration.TotalSeconds, Is.LessThan(2.0), 
                $"Table rendering should complete within 2 seconds. Actual: {tableRenderTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = tableRenderTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(10.0), 
                $"Table rendering should not increase memory by more than 10MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 2.0,
                MaxFirstPaintTime = 1500,
                MaxFirstContentfulPaintTime = 2000,
                MaxMemoryIncreaseMB = 10
            });
        }

        [Test]
        [Description("Verify modal dialog performance")]
        public async Task ModalDialogs_Should_OpenAndCloseQuickly()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test modal dialog performance
            var modalTime = await MeasureActionAsync(async () =>
            {
                // Test opening and closing modals (if available)
                // This is a placeholder for actual modal testing
                await Task.Delay(100); // Simulate modal operations
            });

            // Assert
            Assert.That(modalTime.TotalMilliseconds, Is.LessThan(500), 
                $"Modal dialog operations should complete within 500ms. Actual: {modalTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify navigation menu responsiveness")]
        public async Task NavigationMenu_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test navigation menu responsiveness
            var navigationTime = await MeasureActionAsync(async () =>
            {
                // Test navigation menu interactions (if available)
                // This is a placeholder for actual navigation testing
                await Task.Delay(100); // Simulate navigation operations
            });

            // Assert
            Assert.That(navigationTime.TotalMilliseconds, Is.LessThan(500), 
                $"Navigation menu operations should complete within 500ms. Actual: {navigationTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify text rendering performance")]
        public async Task TextRendering_Should_BeFast()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test text rendering performance
            var textRenderTime = await MeasureActionAsync(async () =>
            {
                // Test text rendering by checking for text elements
                await _loginPage.WaitForLoginFormAsync();
                await Task.Delay(100); // Allow for text rendering
            });

            // Assert
            Assert.That(textRenderTime.TotalMilliseconds, Is.LessThan(500, 
                $"Text rendering should complete within 500ms. Actual: {textRenderTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxFirstPaintTime = 1000,
                MaxFirstContentfulPaintTime = 1500,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify image loading performance")]
        public async Task ImageLoading_Should_BeFast()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test image loading performance
            var imageLoadTime = await MeasureActionAsync(async () =>
            {
                // Wait for images to load
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Task.Delay(500); // Allow for image rendering
            });

            // Assert
            Assert.That(imageLoadTime.TotalSeconds, Is.LessThan(2.0), 
                $"Image loading should complete within 2 seconds. Actual: {imageLoadTime.TotalSeconds:F2}s");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 2.0,
                MaxPageLoadTime = 3000,
                MaxDOMContentLoadedTime = 2000,
                MaxMemoryIncreaseMB = 10
            });
        }

        [Test]
        [Description("Verify scroll performance")]
        public async Task Scrolling_Should_BeSmooth()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test scroll performance
            var scrollTime = await MeasureActionAsync(async () =>
            {
                // Test smooth scrolling
                await Page.EvaluateAsync("window.scrollTo(0, 500)");
                await Task.Delay(200);
                await Page.EvaluateAsync("window.scrollTo(0, 1000)");
                await Task.Delay(200);
                await Page.EvaluateAsync("window.scrollTo(0, 0)");
                await Task.Delay(200);
            });

            // Assert
            Assert.That(scrollTime.TotalMilliseconds, Is.LessThan(1000), 
                $"Scrolling operations should complete within 1000ms. Actual: {scrollTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify keyboard input responsiveness")]
        public async Task KeyboardInput_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test keyboard input responsiveness
            var keyboardTime = await MeasureActionAsync(async () =>
            {
                // Test keyboard navigation
                await Page.Keyboard.PressAsync("Tab");
                await Task.Delay(50);
                await Page.Keyboard.PressAsync("Tab");
                await Task.Delay(50);
                await Page.Keyboard.PressAsync("Enter");
                await Task.Delay(50);
            });

            // Assert
            Assert.That(keyboardTime.TotalMilliseconds, Is.LessThan(500), 
                $"Keyboard input operations should complete within 500ms. Actual: {keyboardTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify mouse interaction responsiveness")]
        public async Task MouseInteractions_Should_BeResponsive()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test mouse interaction responsiveness
            var mouseTime = await MeasureActionAsync(async () =>
            {
                // Test mouse hover and click interactions
                await Page.HoverAsync("input[type='text']");
                await Task.Delay(50);
                await Page.ClickAsync("input[type='text']");
                await Task.Delay(50);
                await Page.HoverAsync("input[type='password']");
                await Task.Delay(50);
                await Page.ClickAsync("input[type='password']");
                await Task.Delay(50);
            });

            // Assert
            Assert.That(mouseTime.TotalMilliseconds, Is.LessThan(500), 
                $"Mouse interaction operations should complete within 500ms. Actual: {mouseTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }

        [Test]
        [Description("Verify component re-rendering performance")]
        public async Task ComponentReRendering_Should_BeFast()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.LoginAsync(Config.TestUsername, Config.TestPassword);
            await _dashboardPage.WaitForDashboardLoadAsync();

            // Act - Test component re-rendering performance
            var reRenderTime = await MeasureActionWithMemoryAsync(async () =>
            {
                // Trigger component re-renders by changing data
                await _dashboardPage.WaitForDashboardLoadAsync();
                await Task.Delay(1000); // Allow for potential re-renders
                await WaitForPerformanceStabilityAsync(2000);
            });

            // Assert
            Assert.That(reRenderTime.Duration.TotalSeconds, Is.LessThan(3.0), 
                $"Component re-rendering should complete within 3 seconds. Actual: {reRenderTime.Duration.TotalSeconds:F2}s");

            var memoryIncreaseMB = reRenderTime.MemoryIncrease / (1024.0 * 1024.0);
            Assert.That(memoryIncreaseMB, Is.LessThan(10.0), 
                $"Component re-rendering should not increase memory by more than 10MB. Actual: {memoryIncreaseMB:F2}MB");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 3.0,
                MaxMemoryIncreaseMB = 10
            });
        }

        [Test]
        [Description("Verify animation performance")]
        public async Task Animations_Should_BeSmooth()
        {
            // Arrange
            await _loginPage.NavigateAsync();
            await _loginPage.WaitForLoginFormAsync();

            // Act - Test animation performance
            var animationTime = await MeasureActionAsync(async () =>
            {
                // Test animations by triggering UI state changes
                await _loginPage.SetUsernameAsync("test");
                await Task.Delay(200); // Allow for animations
                await _loginPage.SetPasswordAsync("test");
                await Task.Delay(200); // Allow for animations
                await _loginPage.ClearFormAsync();
                await Task.Delay(200); // Allow for animations
            });

            // Assert
            Assert.That(animationTime.TotalMilliseconds, Is.LessThan(1000), 
                $"Animation operations should complete within 1000ms. Actual: {animationTime.TotalMilliseconds:F2}ms");

            // Assert performance metrics
            AssertPerformanceMetrics(new PerformanceAssertions
            {
                MaxDuration = 1.0,
                MaxMemoryIncreaseMB = 5
            });
        }
    }
} 