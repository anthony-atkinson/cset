using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using CSETWebCore.PlaywrightTests.Infrastructure;

namespace CSETWebCore.PlaywrightTests.PageObjects.Questions
{
    /// <summary>
    /// Page object for the CSET question navigation and answering page
    /// </summary>
    public class QuestionNavigationPage : BasePageObject
    {
        public override string PageUrl => "/assessment/questions";

        // Locators
        private ILocator QuestionContainer => Page.Locator(".question-container, [data-testid='question-container']");
        private ILocator QuestionText => Page.Locator(".question-text, [data-testid='question-text']");
        private ILocator QuestionTitle => Page.Locator(".question-title, [data-testid='question-title']");
        private ILocator AnswerOptions => Page.Locator(".answer-option, [data-testid='answer-option']");
        private ILocator AnswerInput => Page.Locator("input[type='text'], textarea, [data-testid='answer-input']");
        private ILocator YesButton => Page.Locator("button:has-text('Yes'), [data-testid='answer-yes']");
        private ILocator NoButton => Page.Locator("button:has-text('No'), [data-testid='answer-no']");
        private ILocator UnansweredButton => Page.Locator("button:has-text('Unanswered'), [data-testid='answer-unanswered']");
        private ILocator CommentInput => Page.Locator(".comment-input, [data-testid='comment-input']");
        private ILocator SaveButton => Page.Locator("button:has-text('Save'), [data-testid='save-button']");
        private ILocator NextButton => Page.Locator("button:has-text('Next'), [data-testid='next-button']");
        private ILocator PreviousButton => Page.Locator("button:has-text('Previous'), [data-testid='previous-button']");
        private ILocator ProgressIndicator => Page.Locator(".progress-indicator, [data-testid='progress']");
        private ILocator CategoryNavigation => Page.Locator(".category-nav, [data-testid='category-navigation']");
        private ILocator LoadingSpinner => Page.Locator(".loading, .spinner, [data-testid='loading']");
        private ILocator ErrorMessage => Page.Locator(".error, .alert-danger, [data-testid='error-message']");
        private ILocator SuccessMessage => Page.Locator(".success, .alert-success, [data-testid='success-message']");

        public QuestionNavigationPage(IPage page) : base(page) { }

        public override async Task<bool> IsDisplayedAsync()
        {
            return await QuestionContainer.IsVisibleAsync() && await QuestionText.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for questions to load
        /// </summary>
        public async Task WaitForQuestionsLoadAsync()
        {
            await WaitForVisibleAsync(QuestionContainer);
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Get the current question text
        /// </summary>
        public async Task<string> GetCurrentQuestionTextAsync()
        {
            return await GetTextAsync(QuestionText);
        }

        /// <summary>
        /// Get the current question title
        /// </summary>
        public async Task<string> GetCurrentQuestionTitleAsync()
        {
            return await GetTextAsync(QuestionTitle);
        }

        /// <summary>
        /// Get the number of answer options available
        /// </summary>
        public async Task<int> GetAnswerOptionCountAsync()
        {
            return await AnswerOptions.CountAsync();
        }

        /// <summary>
        /// Select an answer option by index
        /// </summary>
        public async Task SelectAnswerOptionAsync(int index)
        {
            var options = AnswerOptions;
            if (await options.CountAsync() > index)
            {
                await options.Nth(index).ClickAsync();
                await WaitForLoadingAsync();
            }
            else
            {
                throw new ArgumentException($"Answer option at index {index} does not exist");
            }
        }

        /// <summary>
        /// Select Yes answer
        /// </summary>
        public async Task SelectYesAsync()
        {
            await YesButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select No answer
        /// </summary>
        public async Task SelectNoAsync()
        {
            await NoButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Select Unanswered
        /// </summary>
        public async Task SelectUnansweredAsync()
        {
            await UnansweredButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Enter text answer
        /// </summary>
        public async Task EnterTextAnswerAsync(string answer)
        {
            await FillAsync(AnswerInput, answer);
        }

        /// <summary>
        /// Get the current text answer
        /// </summary>
        public async Task<string> GetCurrentTextAnswerAsync()
        {
            return await GetAttributeAsync(AnswerInput, "value") ?? string.Empty;
        }

        /// <summary>
        /// Add a comment to the current question
        /// </summary>
        public async Task AddCommentAsync(string comment)
        {
            await FillAsync(CommentInput, comment);
        }

        /// <summary>
        /// Get the current comment text
        /// </summary>
        public async Task<string> GetCurrentCommentAsync()
        {
            return await GetAttributeAsync(CommentInput, "value") ?? string.Empty;
        }

        /// <summary>
        /// Save the current answer
        /// </summary>
        public async Task SaveAnswerAsync()
        {
            await SaveButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Navigate to the next question
        /// </summary>
        public async Task GoToNextQuestionAsync()
        {
            await NextButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Navigate to the previous question
        /// </summary>
        public async Task GoToPreviousQuestionAsync()
        {
            await PreviousButton.ClickAsync();
            await WaitForLoadingAsync();
        }

        /// <summary>
        /// Check if next button is enabled
        /// </summary>
        public async Task<bool> IsNextButtonEnabledAsync()
        {
            return await NextButton.IsEnabledAsync();
        }

        /// <summary>
        /// Check if previous button is enabled
        /// </summary>
        public async Task<bool> IsPreviousButtonEnabledAsync()
        {
            return await PreviousButton.IsEnabledAsync();
        }

        /// <summary>
        /// Get the current progress percentage
        /// </summary>
        public async Task<string> GetProgressAsync()
        {
            return await GetTextAsync(ProgressIndicator);
        }

        /// <summary>
        /// Navigate to a specific category
        /// </summary>
        public async Task NavigateToCategoryAsync(string categoryName)
        {
            var categoryLink = CategoryNavigation.GetByText(categoryName);
            if (await categoryLink.IsVisibleAsync())
            {
                await categoryLink.ClickAsync();
                await WaitForLoadingAsync();
            }
            else
            {
                throw new ArgumentException($"Category '{categoryName}' not found");
            }
        }

        /// <summary>
        /// Get the list of available categories
        /// </summary>
        public async Task<string[]> GetAvailableCategoriesAsync()
        {
            var categories = await CategoryNavigation.Locator("a, button").AllTextContentsAsync();
            return categories.ToArray();
        }

        /// <summary>
        /// Check if the current question is answered
        /// </summary>
        public async Task<bool> IsCurrentQuestionAnsweredAsync()
        {
            // This would need to be implemented based on actual UI indicators
            // For now, we'll check if any answer option is selected
            var selectedOptions = AnswerOptions.Filter(new LocatorFilterOptions { HasAttribute = "data-selected", Value = "true" });
            return await selectedOptions.CountAsync() > 0;
        }

        /// <summary>
        /// Check if there's an error message displayed
        /// </summary>
        public async Task<bool> HasErrorMessageAsync()
        {
            return await ErrorMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Get the error message text
        /// </summary>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await HasErrorMessageAsync())
            {
                return await ErrorMessage.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Check if there's a success message displayed
        /// </summary>
        public async Task<bool> HasSuccessMessageAsync()
        {
            return await SuccessMessage.IsVisibleAsync();
        }

        /// <summary>
        /// Get the success message text
        /// </summary>
        public async Task<string> GetSuccessMessageAsync()
        {
            if (await HasSuccessMessageAsync())
            {
                return await SuccessMessage.TextContentAsync() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Complete a full question workflow (answer and save)
        /// </summary>
        public async Task<bool> CompleteQuestionWorkflowAsync(string answer = "Yes")
        {
            try
            {
                // Wait for question to load
                await WaitForQuestionsLoadAsync();

                // Select answer based on type
                switch (answer.ToLower())
                {
                    case "yes":
                        await SelectYesAsync();
                        break;
                    case "no":
                        await SelectNoAsync();
                        break;
                    case "unanswered":
                        await SelectUnansweredAsync();
                        break;
                    default:
                        await EnterTextAnswerAsync(answer);
                        break;
                }

                // Save the answer
                await SaveAnswerAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Navigate through multiple questions
        /// </summary>
        public async Task<bool> NavigateThroughQuestionsAsync(int questionCount)
        {
            try
            {
                for (int i = 0; i < questionCount; i++)
                {
                    // Answer current question
                    await CompleteQuestionWorkflowAsync("Yes");

                    // Navigate to next question if available
                    if (await IsNextButtonEnabledAsync())
                    {
                        await GoToNextQuestionAsync();
                        await Task.Delay(1000); // Brief pause between questions
                    }
                    else
                    {
                        break; // No more questions
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if the page is in a loading state
        /// </summary>
        public async Task<bool> IsLoadingAsync()
        {
            return await LoadingSpinner.IsVisibleAsync();
        }

        /// <summary>
        /// Wait for any loading operations to complete
        /// </summary>
        public async Task WaitForLoadingCompleteAsync()
        {
            await WaitForHiddenAsync(LoadingSpinner);
        }

        /// <summary>
        /// Get the current URL to verify navigation
        /// </summary>
        public async Task<string> GetCurrentUrlAsync()
        {
            return Page.Url;
        }

        /// <summary>
        /// Check if the page contains specific text
        /// </summary>
        public async Task<bool> ContainsTextAsync(string text)
        {
            return await Page.GetByText(text).IsVisibleAsync();
        }

        /// <summary>
        /// Take a screenshot for debugging
        /// </summary>
        public async Task TakeScreenshotAsync(string name = "question-navigation")
        {
            await TakeScreenshotAsync(name);
        }
    }
} 