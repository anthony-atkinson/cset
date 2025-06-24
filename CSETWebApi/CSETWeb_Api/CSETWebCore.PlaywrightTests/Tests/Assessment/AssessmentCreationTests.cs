using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;

namespace CSETWebCore.PlaywrightTests.Tests.Assessment
{
    /// <summary>
    /// Test class for assessment creation functionality
    /// </summary>
    [TestFixture]
    [Category("Assessment")]
    [Category("Critical")]
    public class AssessmentCreationTests : BaseTestFixture
    {
        private AssessmentCreationPage _assessmentCreationPage = null!;
        private LoginPage _loginPage = null!;

        protected override async Task SetUpAsync()
        {
            _assessmentCreationPage = new AssessmentCreationPage(Page);
            _loginPage = new LoginPage(Page);

            // Login before each test
            await _loginPage.NavigateAsync();
            await _loginPage.AcceptPrivacyWarningAsync();
            await _loginPage.LoginWithTestCredentialsAsync();
            
            // Wait for login to complete
            await Task.Delay(2000);
            
            // Navigate to assessment creation page
            await _assessmentCreationPage.NavigateAsync();
        }

        [Test]
        [Description("Verify that assessment creation page loads correctly")]
        public async Task AssessmentCreationPage_Should_LoadCorrectly()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _assessmentCreationPage.IsDisplayedAsync(), Is.True, "Assessment creation page should be displayed");
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            
            // Verify gallery has items
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();
            Assert.That(itemCount, Is.GreaterThan(0), "Gallery should contain at least one item");
        }

        [Test]
        [Description("Verify gallery items display correctly")]
        public async Task GalleryItems_Should_DisplayCorrectly()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();

            // Act
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            // Assert
            Assert.That(itemCount, Is.GreaterThan(0), "Gallery should contain items");

            // Verify first item has title and description
            var firstItemTitle = await _assessmentCreationPage.GetGalleryItemTitleAsync(0);
            Assert.That(string.IsNullOrEmpty(firstItemTitle), Is.False, "First gallery item should have a title");

            var firstItemDescription = await _assessmentCreationPage.GetGalleryItemDescriptionAsync(0);
            Assert.That(string.IsNullOrEmpty(firstItemDescription), Is.False, "First gallery item should have a description");
        }

        [Test]
        [Description("Verify gallery item selection works")]
        public async Task GalleryItemSelection_Should_Work()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            // Act & Assert
            if (itemCount > 0)
            {
                // Select first item
                await _assessmentCreationPage.SelectGalleryItemAsync(0);
                
                // Verify selection (this might need adjustment based on actual UI behavior)
                await Task.Delay(1000);
                
                // Check if create button becomes enabled
                var isCreateEnabled = await _assessmentCreationPage.IsCreateButtonEnabledAsync();
                Assert.That(isCreateEnabled, Is.True, "Create button should be enabled after selecting gallery item");
            }
        }

        [Test]
        [Description("Verify assessment name can be set")]
        public async Task AssessmentName_Should_BeSettable()
        {
            // Arrange
            var testName = "Test Assessment " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _assessmentCreationPage.SetAssessmentNameAsync(testName);

            // Assert
            var currentName = await _assessmentCreationPage.GetAssessmentNameAsync();
            Assert.That(currentName, Is.EqualTo(testName), "Assessment name should be set correctly");
        }

        [Test]
        [Description("Verify assessment creation with valid data")]
        public async Task AssessmentCreation_WithValidData_ShouldSucceed()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();
            
            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            var assessmentName = "E2E Test Assessment " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            var success = await _assessmentCreationPage.CreateAssessmentWorkflowAsync(assessmentName, 0);

            // Assert
            Assert.That(success, Is.True, "Assessment creation should succeed with valid data");
        }

        [Test]
        [Description("Verify assessment creation fails with empty name")]
        public async Task AssessmentCreation_WithEmptyName_ShouldFail()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();
            
            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            await _assessmentCreationPage.SelectGalleryItemAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync("");

            // Act
            var isCreateEnabled = await _assessmentCreationPage.IsCreateButtonEnabledAsync();

            // Assert
            Assert.That(isCreateEnabled, Is.False, "Create button should be disabled with empty name");
        }

        [Test]
        [Description("Verify assessment creation fails without selecting gallery item")]
        public async Task AssessmentCreation_WithoutGallerySelection_ShouldFail()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            await _assessmentCreationPage.SetAssessmentNameAsync("Test Assessment");

            // Act
            var isCreateEnabled = await _assessmentCreationPage.IsCreateButtonEnabledAsync();

            // Assert
            Assert.That(isCreateEnabled, Is.False, "Create button should be disabled without gallery selection");
        }

        [Test]
        [Description("Verify cancel button functionality")]
        public async Task CancelButton_Should_Work()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();

            // Act
            await _assessmentCreationPage.CancelCreationAsync();

            // Assert
            // Verify navigation away from creation page
            var currentUrl = await _assessmentCreationPage.GetCurrentUrlAsync();
            Assert.That(currentUrl, Does.Not.Contain("/assessment/create"), "Should navigate away from creation page");
        }

        [Test]
        [Description("Verify gallery item selection by title")]
        public async Task GalleryItemSelection_ByTitle_ShouldWork()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            var firstItemTitle = await _assessmentCreationPage.GetGalleryItemTitleAsync(0);

            // Act
            await _assessmentCreationPage.SelectGalleryItemByTitleAsync(firstItemTitle);

            // Assert
            var isCreateEnabled = await _assessmentCreationPage.IsCreateButtonEnabledAsync();
            Assert.That(isCreateEnabled, Is.True, "Create button should be enabled after selecting item by title");
        }

        [Test]
        [Description("Verify error handling for invalid gallery item selection")]
        public async Task GalleryItemSelection_WithInvalidIndex_ShouldHandleError()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            // Act & Assert
            var invalidIndex = itemCount + 10;
            
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _assessmentCreationPage.SelectGalleryItemAsync(invalidIndex);
            }, "Should throw exception for invalid gallery item index");
        }

        [Test]
        [Description("Verify error handling for invalid gallery item title")]
        public async Task GalleryItemSelection_WithInvalidTitle_ShouldHandleError()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _assessmentCreationPage.SelectGalleryItemByTitleAsync("Non-existent Title");
            }, "Should throw exception for invalid gallery item title");
        }

        [Test]
        [Description("Verify loading states during assessment creation")]
        public async Task AssessmentCreation_Should_ShowLoadingStates()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            var assessmentName = "Loading Test Assessment " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _assessmentCreationPage.SelectGalleryItemAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync(assessmentName);
            await _assessmentCreationPage.CreateAssessmentAsync();

            // Assert
            // Check for loading state (might be brief, so we check immediately)
            var isLoading = await _assessmentCreationPage.IsLoadingAsync();
            
            // Wait for loading to complete
            await _assessmentCreationPage.WaitForLoadingCompleteAsync();
            
            var isLoadingAfter = await _assessmentCreationPage.IsLoadingAsync();
            Assert.That(isLoadingAfter, Is.False, "Loading should complete after assessment creation");
        }

        [Test]
        [Description("Verify assessment creation with special characters in name")]
        public async Task AssessmentCreation_WithSpecialCharacters_ShouldWork()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            var assessmentName = "Test Assessment with Special Chars: !@#$%^&*() " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _assessmentCreationPage.SelectGalleryItemAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync(assessmentName);

            // Assert
            var currentName = await _assessmentCreationPage.GetAssessmentNameAsync();
            Assert.That(currentName, Is.EqualTo(assessmentName), "Assessment name with special characters should be set correctly");
        }

        [Test]
        [Description("Verify assessment creation with very long name")]
        public async Task AssessmentCreation_WithLongName_ShouldWork()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            if (itemCount == 0)
            {
                Assert.Ignore("No gallery items available for testing");
            }

            var longName = new string('A', 255) + " " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _assessmentCreationPage.SelectGalleryItemAsync(0);
            await _assessmentCreationPage.SetAssessmentNameAsync(longName);

            // Assert
            var currentName = await _assessmentCreationPage.GetAssessmentNameAsync();
            Assert.That(currentName, Is.EqualTo(longName), "Long assessment name should be set correctly");
        }

        [Test]
        [Description("Verify multiple gallery items can be browsed")]
        public async Task GalleryItems_Should_AllowBrowsing()
        {
            // Arrange
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();

            if (itemCount < 2)
            {
                Assert.Ignore("Need at least 2 gallery items for browsing test");
            }

            // Act & Assert
            for (int i = 0; i < Math.Min(itemCount, 3); i++)
            {
                var title = await _assessmentCreationPage.GetGalleryItemTitleAsync(i);
                Assert.That(string.IsNullOrEmpty(title), Is.False, $"Gallery item {i} should have a title");

                var description = await _assessmentCreationPage.GetGalleryItemDescriptionAsync(i);
                Assert.That(string.IsNullOrEmpty(description), Is.False, $"Gallery item {i} should have a description");
            }
        }

        [Test]
        [Description("Verify page contains expected text elements")]
        public async Task AssessmentCreationPage_Should_ContainExpectedText()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            // Check for common text elements (adjust based on actual CSET implementation)
            var hasCreateText = await _assessmentCreationPage.ContainsTextAsync("Create") || 
                               await _assessmentCreationPage.ContainsTextAsync("Start Assessment");
            Assert.That(hasCreateText, Is.True, "Page should contain create/start assessment text");
        }
    }
} 