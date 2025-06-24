//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSETWebCore.PlaywrightTests.PageObjects.FileRepository
{
    public class FileRepositoryPage
    {
        private readonly IPage _page;

        // Selectors
        private const string FileUploadInput = "input[type='file']";
        private const string UploadButton = "button[data-testid='upload-button']";
        private const string FileList = "[data-testid='file-list']";
        private const string FileItem = "[data-testid='file-item']";
        private const string FileName = "[data-testid='file-name']";
        private const string FileSize = "[data-testid='file-size']";
        private const string FileType = "[data-testid='file-type']";
        private const string UploadDate = "[data-testid='upload-date']";
        private const string DownloadButton = "[data-testid='download-button']";
        private const string DeleteButton = "[data-testid='delete-button']";
        private const string RenameButton = "[data-testid='rename-button']";
        private const string PreviewButton = "[data-testid='preview-button']";
        private const string SearchInput = "[data-testid='search-input']";
        private const string FilterDropdown = "[data-testid='filter-dropdown']";
        private const string SortDropdown = "[data-testid='sort-dropdown']";
        private const string ProgressBar = "[data-testid='upload-progress']";
        private const string ProgressPercentage = "[data-testid='progress-percentage']";
        private const string CancelUploadButton = "[data-testid='cancel-upload']";
        private const string SuccessMessage = "[data-testid='success-message']";
        private const string ErrorMessage = "[data-testid='error-message']";
        private const string FileCheckbox = "[data-testid='file-checkbox']";
        private const string BulkActionsMenu = "[data-testid='bulk-actions']";
        private const string BulkDeleteButton = "[data-testid='bulk-delete']";
        private const string FileDetailsModal = "[data-testid='file-details-modal']";
        private const string PreviewModal = "[data-testid='preview-modal']";
        private const string DragDropZone = "[data-testid='drag-drop-zone']";
        private const string VersionHistory = "[data-testid='version-history']";
        private const string RetryButton = "[data-testid='retry-button']";

        public FileRepositoryPage(IPage page)
        {
            _page = page;
        }

        /// <summary>
        /// Navigate to the file repository page
        /// </summary>
        public async Task NavigateToFileRepository()
        {
            await _page.GotoAsync("/file-repository");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await _page.WaitForSelectorAsync(FileList, new() { State = WaitForSelectorState.Visible });
        }

        /// <summary>
        /// Upload a single file
        /// </summary>
        public async Task UploadFile(TestFile file)
        {
            // Create a temporary file
            var tempFilePath = await CreateTempFile(file);
            
            // Set the file input
            await _page.SetInputFilesAsync(FileUploadInput, tempFilePath);
            
            // Click upload button if it exists
            var uploadButton = await _page.QuerySelectorAsync(UploadButton);
            if (uploadButton != null)
            {
                await uploadButton.ClickAsync();
            }
            
            // Wait for upload to start
            await _page.WaitForSelectorAsync(ProgressBar, new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        public async Task UploadMultipleFiles(TestFile[] files)
        {
            var tempFilePaths = new List<string>();
            
            foreach (var file in files)
            {
                var tempFilePath = await CreateTempFile(file);
                tempFilePaths.Add(tempFilePath);
            }
            
            // Set multiple files
            await _page.SetInputFilesAsync(FileUploadInput, tempFilePaths.ToArray());
            
            // Click upload button if it exists
            var uploadButton = await _page.QuerySelectorAsync(UploadButton);
            if (uploadButton != null)
            {
                await uploadButton.ClickAsync();
            }
        }

        /// <summary>
        /// Wait for upload to complete
        /// </summary>
        public async Task WaitForUploadComplete()
        {
            await _page.WaitForSelectorAsync(ProgressBar, new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
            await _page.WaitForSelectorAsync(SuccessMessage, new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        }

        /// <summary>
        /// Wait for all uploads to complete
        /// </summary>
        public async Task WaitForAllUploadsComplete()
        {
            await _page.WaitForSelectorAsync(ProgressBar, new() { State = WaitForSelectorState.Hidden, Timeout = 60000 });
            await _page.WaitForSelectorAsync(SuccessMessage, new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        }

        /// <summary>
        /// Verify file was uploaded successfully
        /// </summary>
        public async Task VerifyFileUploaded(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Not.Null, $"File {fileName} was not found in the file list");
        }

        /// <summary>
        /// Verify upload success message
        /// </summary>
        public async Task VerifyUploadSuccessMessage()
        {
            var successMessage = await _page.QuerySelectorAsync(SuccessMessage);
            Assert.That(successMessage, Is.Not.Null, "Upload success message was not displayed");
        }

        /// <summary>
        /// Verify upload progress is displayed
        /// </summary>
        public async Task VerifyUploadProgressDisplayed()
        {
            var progressBar = await _page.QuerySelectorAsync(ProgressBar);
            Assert.That(progressBar, Is.Not.Null, "Upload progress bar was not displayed");
        }

        /// <summary>
        /// Verify upload error message
        /// </summary>
        public async Task VerifyUploadErrorMessage(string expectedMessage)
        {
            var errorMessage = await _page.QuerySelectorAsync(ErrorMessage);
            Assert.That(errorMessage, Is.Not.Null, "Upload error message was not displayed");
            
            var messageText = await errorMessage.TextContentAsync();
            Assert.That(messageText, Does.Contain(expectedMessage), $"Error message should contain '{expectedMessage}'");
        }

        /// <summary>
        /// Download a file
        /// </summary>
        public async Task DownloadFile(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            var downloadButton = await fileItem.QuerySelectorAsync(DownloadButton);
            await downloadButton.ClickAsync();
        }

        /// <summary>
        /// Verify download started
        /// </summary>
        public async Task VerifyDownloadStarted()
        {
            // Wait for download to start (this might vary based on browser implementation)
            await Task.Delay(1000); // Brief delay to allow download to initiate
        }

        /// <summary>
        /// Attempt to download a non-existent file
        /// </summary>
        public async Task AttemptDownloadNonExistentFile(string fileName)
        {
            // This would typically involve trying to access a download link for a non-existent file
            await _page.GotoAsync($"/api/files/download/nonexistent");
        }

        /// <summary>
        /// Verify download error message
        /// </summary>
        public async Task VerifyDownloadErrorMessage(string expectedMessage)
        {
            // Check for error response or message
            var errorMessage = await _page.QuerySelectorAsync(ErrorMessage);
            if (errorMessage != null)
            {
                var messageText = await errorMessage.TextContentAsync();
                Assert.That(messageText, Does.Contain(expectedMessage), $"Download error message should contain '{expectedMessage}'");
            }
        }

        /// <summary>
        /// Delete a file
        /// </summary>
        public async Task DeleteFile(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            var deleteButton = await fileItem.QuerySelectorAsync(DeleteButton);
            await deleteButton.ClickAsync();
            
            // Confirm deletion if confirmation dialog appears
            await _page.WaitForSelectorAsync("[data-testid='confirm-dialog']", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
            await _page.ClickAsync("[data-testid='confirm-delete']");
        }

        /// <summary>
        /// Verify file was deleted
        /// </summary>
        public async Task VerifyFileDeleted(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Null, $"File {fileName} was not deleted");
        }

        /// <summary>
        /// Verify delete success message
        /// </summary>
        public async Task VerifyDeleteSuccessMessage()
        {
            var successMessage = await _page.QuerySelectorAsync(SuccessMessage);
            Assert.That(successMessage, Is.Not.Null, "Delete success message was not displayed");
        }

        /// <summary>
        /// Rename a file
        /// </summary>
        public async Task RenameFile(string oldFileName, string newFileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{oldFileName}')");
            var renameButton = await fileItem.QuerySelectorAsync(RenameButton);
            await renameButton.ClickAsync();
            
            // Enter new filename
            await _page.FillAsync("[data-testid='rename-input']", newFileName);
            await _page.ClickAsync("[data-testid='confirm-rename']");
        }

        /// <summary>
        /// Verify file was renamed
        /// </summary>
        public async Task VerifyFileRenamed(string oldFileName, string newFileName)
        {
            var oldFileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{oldFileName}')");
            Assert.That(oldFileItem, Is.Null, $"Old file name {oldFileName} still exists");
            
            var newFileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{newFileName}')");
            Assert.That(newFileItem, Is.Not.Null, $"New file name {newFileName} was not found");
        }

        /// <summary>
        /// Verify rename success message
        /// </summary>
        public async Task VerifyRenameSuccessMessage()
        {
            var successMessage = await _page.QuerySelectorAsync(SuccessMessage);
            Assert.That(successMessage, Is.Not.Null, "Rename success message was not displayed");
        }

        /// <summary>
        /// View file details
        /// </summary>
        public async Task ViewFileDetails(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            var detailsButton = await fileItem.QuerySelectorAsync("[data-testid='details-button']");
            await detailsButton.ClickAsync();
        }

        /// <summary>
        /// Verify file details are displayed
        /// </summary>
        public async Task VerifyFileDetailsDisplayed(string fileName)
        {
            var detailsModal = await _page.QuerySelectorAsync(FileDetailsModal);
            Assert.That(detailsModal, Is.Not.Null, "File details modal was not displayed");
            
            var fileNameElement = await detailsModal.QuerySelectorAsync($"[data-testid='detail-filename']:has-text('{fileName}')");
            Assert.That(fileNameElement, Is.Not.Null, $"File name {fileName} not found in details");
        }

        /// <summary>
        /// Verify file size is displayed
        /// </summary>
        public async Task VerifyFileSizeDisplayed()
        {
            var detailsModal = await _page.QuerySelectorAsync(FileDetailsModal);
            var fileSizeElement = await detailsModal.QuerySelectorAsync("[data-testid='detail-filesize']");
            Assert.That(fileSizeElement, Is.Not.Null, "File size not displayed in details");
        }

        /// <summary>
        /// Verify file type is displayed
        /// </summary>
        public async Task VerifyFileTypeDisplayed(string expectedType)
        {
            var detailsModal = await _page.QuerySelectorAsync(FileDetailsModal);
            var fileTypeElement = await detailsModal.QuerySelectorAsync($"[data-testid='detail-filetype']:has-text('{expectedType}')");
            Assert.That(fileTypeElement, Is.Not.Null, $"File type {expectedType} not found in details");
        }

        /// <summary>
        /// Verify upload date is displayed
        /// </summary>
        public async Task VerifyUploadDateDisplayed()
        {
            var detailsModal = await _page.QuerySelectorAsync(FileDetailsModal);
            var uploadDateElement = await detailsModal.QuerySelectorAsync("[data-testid='detail-uploaddate']");
            Assert.That(uploadDateElement, Is.Not.Null, "Upload date not displayed in details");
        }

        /// <summary>
        /// Search for files
        /// </summary>
        public async Task SearchFiles(string searchTerm)
        {
            await _page.FillAsync(SearchInput, searchTerm);
            await _page.Keyboard.PressAsync("Enter");
        }

        /// <summary>
        /// Verify search results contain expected file
        /// </summary>
        public async Task VerifySearchResultsContain(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Not.Null, $"Search results should contain {fileName}");
        }

        /// <summary>
        /// Verify search results do not contain unexpected file
        /// </summary>
        public async Task VerifySearchResultsDoNotContain(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Null, $"Search results should not contain {fileName}");
        }

        /// <summary>
        /// Filter by file type
        /// </summary>
        public async Task FilterByFileType(string fileType)
        {
            await _page.ClickAsync(FilterDropdown);
            await _page.ClickAsync($"[data-testid='filter-option-{fileType.ToLower()}']");
        }

        /// <summary>
        /// Verify filtered results contain expected file
        /// </summary>
        public async Task VerifyFilteredResultsContain(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Not.Null, $"Filtered results should contain {fileName}");
        }

        /// <summary>
        /// Verify filtered results do not contain unexpected file
        /// </summary>
        public async Task VerifyFilteredResultsDoNotContain(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Null, $"Filtered results should not contain {fileName}");
        }

        /// <summary>
        /// Sort by date
        /// </summary>
        public async Task SortByDate(string order)
        {
            await _page.ClickAsync(SortDropdown);
            await _page.ClickAsync($"[data-testid='sort-option-{order}']");
        }

        /// <summary>
        /// Verify file order
        /// </summary>
        public async Task VerifyFileOrder(params string[] expectedOrder)
        {
            var fileItems = await _page.QuerySelectorAllAsync(FileName);
            var actualOrder = await Task.WhenAll(fileItems.Select(async item => await item.TextContentAsync()));
            
            for (int i = 0; i < expectedOrder.Length && i < actualOrder.Length; i++)
            {
                Assert.That(actualOrder[i], Does.Contain(expectedOrder[i]), $"File order mismatch at position {i}");
            }
        }

        /// <summary>
        /// Select multiple files
        /// </summary>
        public async Task SelectMultipleFiles(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
                var checkbox = await fileItem.QuerySelectorAsync(FileCheckbox);
                await checkbox.CheckAsync();
            }
        }

        /// <summary>
        /// Verify files are selected
        /// </summary>
        public async Task VerifyFilesSelected(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
                var checkbox = await fileItem.QuerySelectorAsync(FileCheckbox);
                var isChecked = await checkbox.IsCheckedAsync();
                Assert.That(isChecked, Is.True, $"File {fileName} should be selected");
            }
        }

        /// <summary>
        /// Verify bulk actions are enabled
        /// </summary>
        public async Task VerifyBulkActionsEnabled()
        {
            var bulkActionsMenu = await _page.QuerySelectorAsync(BulkActionsMenu);
            Assert.That(bulkActionsMenu, Is.Not.Null, "Bulk actions menu should be enabled");
        }

        /// <summary>
        /// Bulk delete selected files
        /// </summary>
        public async Task BulkDeleteSelectedFiles()
        {
            await _page.ClickAsync(BulkDeleteButton);
            await _page.ClickAsync("[data-testid='confirm-bulk-delete']");
        }

        /// <summary>
        /// Verify files are deleted
        /// </summary>
        public async Task VerifyFilesDeleted(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                await VerifyFileDeleted(fileName);
            }
        }

        /// <summary>
        /// Verify file exists
        /// </summary>
        public async Task VerifyFileExists(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Not.Null, $"File {fileName} should exist");
        }

        /// <summary>
        /// Preview a file
        /// </summary>
        public async Task PreviewFile(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            var previewButton = await fileItem.QuerySelectorAsync(PreviewButton);
            await previewButton.ClickAsync();
        }

        /// <summary>
        /// Verify file preview is displayed
        /// </summary>
        public async Task VerifyFilePreviewDisplayed()
        {
            var previewModal = await _page.QuerySelectorAsync(PreviewModal);
            Assert.That(previewModal, Is.Not.Null, "File preview modal was not displayed");
        }

        /// <summary>
        /// Verify preview content
        /// </summary>
        public async Task VerifyPreviewContent(string expectedContent)
        {
            var previewModal = await _page.QuerySelectorAsync(PreviewModal);
            var contentElement = await previewModal.QuerySelectorAsync("[data-testid='preview-content']");
            var content = await contentElement.TextContentAsync();
            Assert.That(content, Does.Contain(expectedContent), "Preview content does not match expected content");
        }

        /// <summary>
        /// Verify upload progress bar is displayed
        /// </summary>
        public async Task VerifyUploadProgressBarDisplayed()
        {
            var progressBar = await _page.QuerySelectorAsync(ProgressBar);
            Assert.That(progressBar, Is.Not.Null, "Upload progress bar should be displayed");
        }

        /// <summary>
        /// Verify progress percentage updates
        /// </summary>
        public async Task VerifyProgressPercentageUpdates()
        {
            var progressPercentage = await _page.QuerySelectorAsync(ProgressPercentage);
            Assert.That(progressPercentage, Is.Not.Null, "Progress percentage should be displayed");
            
            // Wait for progress to update
            await Task.Delay(2000);
            var percentageText = await progressPercentage.TextContentAsync();
            Assert.That(percentageText, Does.Match(@"\d+%"), "Progress percentage should be in correct format");
        }

        /// <summary>
        /// Cancel upload
        /// </summary>
        public async Task CancelUpload()
        {
            await _page.ClickAsync(CancelUploadButton);
        }

        /// <summary>
        /// Verify upload is cancelled
        /// </summary>
        public async Task VerifyUploadCancelled()
        {
            var progressBar = await _page.QuerySelectorAsync(ProgressBar);
            Assert.That(progressBar, Is.Null, "Upload progress bar should be hidden after cancellation");
        }

        /// <summary>
        /// Verify file was not uploaded
        /// </summary>
        public async Task VerifyFileNotUploaded(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            Assert.That(fileItem, Is.Null, $"File {fileName} should not be uploaded after cancellation");
        }

        /// <summary>
        /// Drag and drop files
        /// </summary>
        public async Task DragAndDropFiles(TestFile[] files)
        {
            var tempFilePaths = new List<string>();
            
            foreach (var file in files)
            {
                var tempFilePath = await CreateTempFile(file);
                tempFilePaths.Add(tempFilePath);
            }
            
            var dragDropZone = await _page.QuerySelectorAsync(DragDropZone);
            await dragDropZone.SetInputFilesAsync(tempFilePaths.ToArray());
        }

        /// <summary>
        /// Start concurrent uploads
        /// </summary>
        public async Task StartConcurrentUploads(TestFile[] files)
        {
            foreach (var file in files)
            {
                await UploadFile(file);
            }
        }

        /// <summary>
        /// Simulate network interruption
        /// </summary>
        public async Task SimulateNetworkInterruption()
        {
            // This would typically involve network throttling or disconnection
            // For testing purposes, we'll just wait a bit
            await Task.Delay(1000);
        }

        /// <summary>
        /// Verify upload failure is handled
        /// </summary>
        public async Task VerifyUploadFailureHandled()
        {
            var errorMessage = await _page.QuerySelectorAsync(ErrorMessage);
            Assert.That(errorMessage, Is.Not.Null, "Upload failure should be handled with error message");
        }

        /// <summary>
        /// Verify retry option is available
        /// </summary>
        public async Task VerifyRetryOptionAvailable()
        {
            var retryButton = await _page.QuerySelectorAsync(RetryButton);
            Assert.That(retryButton, Is.Not.Null, "Retry option should be available after upload failure");
        }

        /// <summary>
        /// Verify file versioning is enabled
        /// </summary>
        public async Task VerifyFileVersioningEnabled()
        {
            var versionHistory = await _page.QuerySelectorAsync(VersionHistory);
            Assert.That(versionHistory, Is.Not.Null, "File versioning should be enabled");
        }

        /// <summary>
        /// Verify version history is available
        /// </summary>
        public async Task VerifyVersionHistoryAvailable(string fileName)
        {
            var fileItem = await _page.QuerySelectorAsync($"{FileItem}:has-text('{fileName}')");
            var versionButton = await fileItem.QuerySelectorAsync("[data-testid='version-button']");
            Assert.That(versionButton, Is.Not.Null, "Version history button should be available");
        }

        /// <summary>
        /// Verify ARIA labels are present
        /// </summary>
        public async Task VerifyAriaLabelsPresent()
        {
            var elementsWithAria = await _page.QuerySelectorAllAsync("[aria-label]");
            Assert.That(elementsWithAria.Length, Is.GreaterThan(0), "Elements should have ARIA labels for accessibility");
        }

        /// <summary>
        /// Verify keyboard navigation
        /// </summary>
        public async Task VerifyKeyboardNavigation()
        {
            // Test tab navigation
            await _page.Keyboard.PressAsync("Tab");
            var focusedElement = await _page.EvaluateAsync<string>("document.activeElement.tagName");
            Assert.That(focusedElement, Is.Not.Null, "Keyboard navigation should work");
        }

        /// <summary>
        /// Verify screen reader support
        /// </summary>
        public async Task VerifyScreenReaderSupport()
        {
            var screenReaderElements = await _page.QuerySelectorAllAsync("[role], [aria-label], [aria-describedby]");
            Assert.That(screenReaderElements.Length, Is.GreaterThan(0), "Elements should have screen reader support attributes");
        }

        /// <summary>
        /// Create a temporary file for testing
        /// </summary>
        private async Task<string> CreateTempFile(TestFile file)
        {
            var tempPath = Path.GetTempFileName();
            var extension = Path.GetExtension(file.Name);
            var newPath = Path.ChangeExtension(tempPath, extension);
            
            if (File.Exists(tempPath))
            {
                File.Move(tempPath, newPath);
            }
            
            await File.WriteAllTextAsync(newPath, file.Content);
            return newPath;
        }
    }
} 