using System;
using System.Threading.Tasks;
using NUnit.Framework;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.Questions;
using CSETWebCore.PlaywrightTests.PageObjects.Authentication;
using CSETWebCore.PlaywrightTests.PageObjects.Assessment;

namespace CSETWebCore.PlaywrightTests.Tests.Questions
{
    /// <summary>
    /// Test class for question navigation and answering functionality
    /// </summary>
    [TestFixture]
    [Category("Questions")]
    [Category("Critical")]
    public class QuestionNavigationTests : BaseTestFixture
    {
        private QuestionNavigationPage _questionNavigationPage = null!;
        private LoginPage _loginPage = null!;
        private AssessmentCreationPage _assessmentCreationPage = null!;

        protected override async Task SetUpAsync()
        {
            _questionNavigationPage = new QuestionNavigationPage(Page);
            _loginPage = new LoginPage(Page);
            _assessmentCreationPage = new AssessmentCreationPage(Page);

            // Login and create assessment before each test
            await _loginPage.NavigateAsync();
            await _loginPage.AcceptPrivacyWarningAsync();
            await _loginPage.LoginWithTestCredentialsAsync();
            
            // Wait for login to complete
            await Task.Delay(2000);
            
            // Create a test assessment
            await _assessmentCreationPage.NavigateAsync();
            await _assessmentCreationPage.WaitForGalleryLoadAsync();
            
            var itemCount = await _assessmentCreationPage.GetGalleryItemCountAsync();
            if (itemCount > 0)
            {
                var assessmentName = "E2E Question Test " + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await _assessmentCreationPage.CreateAssessmentWorkflowAsync(assessmentName, 0);
                
                // Wait for assessment to be created and navigate to questions
                await Task.Delay(3000);
            }
            
            // Navigate to questions page
            await _questionNavigationPage.NavigateAsync();
        }

        [Test]
        [Description("Verify that question navigation page loads correctly")]
        public async Task QuestionNavigationPage_Should_LoadCorrectly()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            Assert.That(await _questionNavigationPage.IsDisplayedAsync(), Is.True, "Question navigation page should be displayed");
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            
            // Verify question elements are present
            var questionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
            Assert.That(string.IsNullOrEmpty(questionText), Is.False, "Question text should be displayed");
        }

        [Test]
        [Description("Verify question text and title display correctly")]
        public async Task QuestionText_Should_DisplayCorrectly()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            var questionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
            var questionTitle = await _questionNavigationPage.GetCurrentQuestionTitleAsync();

            // Assert
            Assert.That(string.IsNullOrEmpty(questionText), Is.False, "Question text should not be empty");
            Assert.That(string.IsNullOrEmpty(questionTitle), Is.False, "Question title should not be empty");
        }

        [Test]
        [Description("Verify Yes answer selection works")]
        public async Task YesAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            await _questionNavigationPage.SelectYesAsync();

            // Assert
            // Verify answer was selected (this might need adjustment based on actual UI behavior)
            await Task.Delay(1000);
            
            // Check if save button is enabled or answer is marked as selected
            var isAnswered = await _questionNavigationPage.IsCurrentQuestionAnsweredAsync();
            Assert.That(isAnswered, Is.True, "Question should be marked as answered after selecting Yes");
        }

        [Test]
        [Description("Verify No answer selection works")]
        public async Task NoAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            await _questionNavigationPage.SelectNoAsync();

            // Assert
            await Task.Delay(1000);
            
            var isAnswered = await _questionNavigationPage.IsCurrentQuestionAnsweredAsync();
            Assert.That(isAnswered, Is.True, "Question should be marked as answered after selecting No");
        }

        [Test]
        [Description("Verify Unanswered selection works")]
        public async Task UnansweredAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            await _questionNavigationPage.SelectUnansweredAsync();

            // Assert
            await Task.Delay(1000);
            
            var isAnswered = await _questionNavigationPage.IsCurrentQuestionAnsweredAsync();
            Assert.That(isAnswered, Is.True, "Question should be marked as answered after selecting Unanswered");
        }

        [Test]
        [Description("Verify text answer input works")]
        public async Task TextAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var testAnswer = "Test answer " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _questionNavigationPage.EnterTextAnswerAsync(testAnswer);

            // Assert
            var currentAnswer = await _questionNavigationPage.GetCurrentTextAnswerAsync();
            Assert.That(currentAnswer, Is.EqualTo(testAnswer), "Text answer should be entered correctly");
        }

        [Test]
        [Description("Verify comment addition works")]
        public async Task Comment_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var testComment = "Test comment " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _questionNavigationPage.AddCommentAsync(testComment);

            // Assert
            var currentComment = await _questionNavigationPage.GetCurrentCommentAsync();
            Assert.That(currentComment, Is.EqualTo(testComment), "Comment should be added correctly");
        }

        [Test]
        [Description("Verify answer saving works")]
        public async Task SaveAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            await _questionNavigationPage.SelectYesAsync();

            // Act
            await _questionNavigationPage.SaveAnswerAsync();

            // Assert
            // Check for success message or other indication of save
            var hasSuccessMessage = await _questionNavigationPage.HasSuccessMessageAsync();
            Assert.That(hasSuccessMessage, Is.True, "Success message should be displayed after saving answer");
        }

        [Test]
        [Description("Verify navigation to next question works")]
        public async Task NextQuestion_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            await _questionNavigationPage.CompleteQuestionWorkflowAsync("Yes");

            // Act
            if (await _questionNavigationPage.IsNextButtonEnabledAsync())
            {
                var initialQuestionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
                await _questionNavigationPage.GoToNextQuestionAsync();

                // Assert
                await Task.Delay(1000);
                var newQuestionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
                Assert.That(newQuestionText, Is.Not.EqualTo(initialQuestionText), "Question text should change after navigation");
            }
            else
            {
                Assert.Ignore("Next button not enabled - no more questions available");
            }
        }

        [Test]
        [Description("Verify navigation to previous question works")]
        public async Task PreviousQuestion_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            
            // First go to next question if possible
            if (await _questionNavigationPage.IsNextButtonEnabledAsync())
            {
                var initialQuestionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
                await _questionNavigationPage.GoToNextQuestionAsync();
                await Task.Delay(1000);

                // Act - go back to previous question
                if (await _questionNavigationPage.IsPreviousButtonEnabledAsync())
                {
                    await _questionNavigationPage.GoToPreviousQuestionAsync();

                    // Assert
                    await Task.Delay(1000);
                    var previousQuestionText = await _questionNavigationPage.GetCurrentQuestionTextAsync();
                    Assert.That(previousQuestionText, Is.EqualTo(initialQuestionText), "Should return to the same question");
                }
                else
                {
                    Assert.Ignore("Previous button not enabled");
                }
            }
            else
            {
                Assert.Ignore("Next button not enabled - cannot test previous navigation");
            }
        }

        [Test]
        [Description("Verify progress indicator displays correctly")]
        public async Task ProgressIndicator_Should_Display()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            var progress = await _questionNavigationPage.GetProgressAsync();

            // Assert
            Assert.That(string.IsNullOrEmpty(progress), Is.False, "Progress indicator should display progress");
        }

        [Test]
        [Description("Verify category navigation works")]
        public async Task CategoryNavigation_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var categories = await _questionNavigationPage.GetAvailableCategoriesAsync();

            // Act & Assert
            if (categories.Length > 0)
            {
                var firstCategory = categories[0];
                await _questionNavigationPage.NavigateToCategoryAsync(firstCategory);
                
                await Task.Delay(1000);
                
                // Verify navigation occurred (this might need adjustment based on actual UI behavior)
                var currentUrl = await _questionNavigationPage.GetCurrentUrlAsync();
                Assert.That(currentUrl, Does.Contain(firstCategory.ToLower().Replace(" ", "-")), 
                    "URL should reflect category navigation");
            }
            else
            {
                Assert.Ignore("No categories available for testing");
            }
        }

        [Test]
        [Description("Verify complete question workflow")]
        public async Task CompleteQuestionWorkflow_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            var success = await _questionNavigationPage.CompleteQuestionWorkflowAsync("Yes");

            // Assert
            Assert.That(success, Is.True, "Complete question workflow should succeed");
        }

        [Test]
        [Description("Verify navigation through multiple questions")]
        public async Task MultipleQuestionNavigation_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            var success = await _questionNavigationPage.NavigateThroughQuestionsAsync(3);

            // Assert
            Assert.That(success, Is.True, "Navigation through multiple questions should succeed");
        }

        [Test]
        [Description("Verify error handling for invalid answer option selection")]
        public async Task InvalidAnswerOption_Should_HandleError()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var optionCount = await _questionNavigationPage.GetAnswerOptionCountAsync();

            // Act & Assert
            var invalidIndex = optionCount + 10;
            
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _questionNavigationPage.SelectAnswerOptionAsync(invalidIndex);
            }, "Should throw exception for invalid answer option index");
        }

        [Test]
        [Description("Verify error handling for invalid category navigation")]
        public async Task InvalidCategoryNavigation_Should_HandleError()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _questionNavigationPage.NavigateToCategoryAsync("Non-existent Category");
            }, "Should throw exception for invalid category");
        }

        [Test]
        [Description("Verify loading states during question operations")]
        public async Task QuestionOperations_Should_ShowLoadingStates()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            await _questionNavigationPage.SelectYesAsync();
            await _questionNavigationPage.SaveAnswerAsync();

            // Assert
            // Check for loading state (might be brief, so we check immediately)
            var isLoading = await _questionNavigationPage.IsLoadingAsync();
            
            // Wait for loading to complete
            await _questionNavigationPage.WaitForLoadingCompleteAsync();
            
            var isLoadingAfter = await _questionNavigationPage.IsLoadingAsync();
            Assert.That(isLoadingAfter, Is.False, "Loading should complete after question operations");
        }

        [Test]
        [Description("Verify question with special characters in answer")]
        public async Task QuestionWithSpecialCharacters_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var specialAnswer = "Test answer with special chars: !@#$%^&*() " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _questionNavigationPage.EnterTextAnswerAsync(specialAnswer);

            // Assert
            var currentAnswer = await _questionNavigationPage.GetCurrentTextAnswerAsync();
            Assert.That(currentAnswer, Is.EqualTo(specialAnswer), "Answer with special characters should be handled correctly");
        }

        [Test]
        [Description("Verify question with very long answer")]
        public async Task QuestionWithLongAnswer_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var longAnswer = new string('A', 500) + " " + DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Act
            await _questionNavigationPage.EnterTextAnswerAsync(longAnswer);

            // Assert
            var currentAnswer = await _questionNavigationPage.GetCurrentTextAnswerAsync();
            Assert.That(currentAnswer, Is.EqualTo(longAnswer), "Long answer should be handled correctly");
        }

        [Test]
        [Description("Verify page contains expected text elements")]
        public async Task QuestionNavigationPage_Should_ContainExpectedText()
        {
            // Arrange & Act - page loaded in SetUp

            // Assert
            // Check for common text elements (adjust based on actual CSET implementation)
            var hasQuestionText = await _questionNavigationPage.ContainsTextAsync("Question") || 
                                 await _questionNavigationPage.ContainsTextAsync("Answer");
            Assert.That(hasQuestionText, Is.True, "Page should contain question/answer related text");
        }

        [Test]
        [Description("Verify answer options are available")]
        public async Task AnswerOptions_Should_BeAvailable()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();

            // Act
            var optionCount = await _questionNavigationPage.GetAnswerOptionCountAsync();

            // Assert
            Assert.That(optionCount, Is.GreaterThan(0), "Should have at least one answer option available");
        }

        [Test]
        [Description("Verify save and resume functionality")]
        public async Task SaveAndResume_Should_Work()
        {
            // Arrange
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            await _questionNavigationPage.CompleteQuestionWorkflowAsync("Yes");

            // Act - simulate save and resume by navigating away and back
            await NavigateToAsync("/assessment");
            await Task.Delay(1000);
            await _questionNavigationPage.NavigateAsync();

            // Assert
            await _questionNavigationPage.WaitForQuestionsLoadAsync();
            var isAnswered = await _questionNavigationPage.IsCurrentQuestionAnsweredAsync();
            Assert.That(isAnswered, Is.True, "Question should remain answered after save and resume");
        }
    }
} 