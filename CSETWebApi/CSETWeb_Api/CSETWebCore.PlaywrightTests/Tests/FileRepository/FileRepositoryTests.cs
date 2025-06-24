//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using NUnit.Framework;
using System.Threading.Tasks;
using CSETWebCore.PlaywrightTests.Infrastructure;
using CSETWebCore.PlaywrightTests.PageObjects.FileRepository;

namespace CSETWebCore.PlaywrightTests.Tests.FileRepository
{
    [TestFixture]
    [TestCategory("E2E")]
    [TestCategory("FileRepository")]
    public class FileRepositoryTests : PlaywrightTestBase
    {
        private FileRepositoryPage _fileRepositoryPage;

        [SetUp]
        public async Task Setup()
        {
            _fileRepositoryPage = new FileRepositoryPage(Page);
            await LoginAsDefaultUser();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileUpload_ValidFile_SuccessfulUpload()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a valid file
            var testFile = CreateTestFile("test-document.pdf", "application/pdf", "Test PDF content");
            await _fileRepositoryPage.UploadFile(testFile);

            // Verify upload success
            await _fileRepositoryPage.WaitForUploadComplete();
            await _fileRepositoryPage.VerifyFileUploaded("test-document.pdf");
            await _fileRepositoryPage.VerifyUploadSuccessMessage();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileUpload_MultipleFiles_SuccessfulBatchUpload()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload multiple files
            var files = new[]
            {
                CreateTestFile("document1.pdf", "application/pdf", "PDF content 1"),
                CreateTestFile("document2.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Word content"),
                CreateTestFile("image1.png", "image/png", "PNG image content")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);

            // Verify all files uploaded successfully
            await _fileRepositoryPage.WaitForAllUploadsComplete();
            foreach (var file in files)
            {
                await _fileRepositoryPage.VerifyFileUploaded(file.Name);
            }
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileUpload_LargeFile_HandlesLargeFileUpload()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Create a large test file (5MB)
            var largeFile = CreateLargeTestFile("large-document.pdf", "application/pdf", 5 * 1024 * 1024);
            await _fileRepositoryPage.UploadFile(largeFile);

            // Verify upload progress is shown
            await _fileRepositoryPage.VerifyUploadProgressDisplayed();
            await _fileRepositoryPage.WaitForUploadComplete();
            await _fileRepositoryPage.VerifyFileUploaded("large-document.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileUpload_InvalidFileType_ShowsError()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload an invalid file type
            var invalidFile = CreateTestFile("script.exe", "application/x-msdownload", "Executable content");
            await _fileRepositoryPage.UploadFile(invalidFile);

            // Verify error message is displayed
            await _fileRepositoryPage.VerifyUploadErrorMessage("File type not allowed");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileUpload_EmptyFile_ShowsError()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload an empty file
            var emptyFile = CreateTestFile("empty.txt", "text/plain", "");
            await _fileRepositoryPage.UploadFile(emptyFile);

            // Verify error message is displayed
            await _fileRepositoryPage.VerifyUploadErrorMessage("File cannot be empty");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileDownload_ValidFile_SuccessfulDownload()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a file first
            var testFile = CreateTestFile("download-test.pdf", "application/pdf", "Download test content");
            await _fileRepositoryPage.UploadFile(testFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Download the file
            await _fileRepositoryPage.DownloadFile("download-test.pdf");

            // Verify download started
            await _fileRepositoryPage.VerifyDownloadStarted();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileDownload_NonExistentFile_ShowsError()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Try to download a non-existent file
            await _fileRepositoryPage.AttemptDownloadNonExistentFile("non-existent-file.pdf");

            // Verify error message is displayed
            await _fileRepositoryPage.VerifyDownloadErrorMessage("File not found");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileManagement_DeleteFile_SuccessfulDeletion()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a file first
            var testFile = CreateTestFile("delete-test.pdf", "application/pdf", "Delete test content");
            await _fileRepositoryPage.UploadFile(testFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Delete the file
            await _fileRepositoryPage.DeleteFile("delete-test.pdf");

            // Verify file is deleted
            await _fileRepositoryPage.VerifyFileDeleted("delete-test.pdf");
            await _fileRepositoryPage.VerifyDeleteSuccessMessage();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileManagement_RenameFile_SuccessfulRename()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a file first
            var testFile = CreateTestFile("rename-test.pdf", "application/pdf", "Rename test content");
            await _fileRepositoryPage.UploadFile(testFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Rename the file
            await _fileRepositoryPage.RenameFile("rename-test.pdf", "renamed-document.pdf");

            // Verify file is renamed
            await _fileRepositoryPage.VerifyFileRenamed("rename-test.pdf", "renamed-document.pdf");
            await _fileRepositoryPage.VerifyRenameSuccessMessage();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileManagement_FileDetails_DisplaysCorrectInformation()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a file first
            var testFile = CreateTestFile("details-test.pdf", "application/pdf", "Details test content");
            await _fileRepositoryPage.UploadFile(testFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // View file details
            await _fileRepositoryPage.ViewFileDetails("details-test.pdf");

            // Verify file details are displayed correctly
            await _fileRepositoryPage.VerifyFileDetailsDisplayed("details-test.pdf");
            await _fileRepositoryPage.VerifyFileSizeDisplayed();
            await _fileRepositoryPage.VerifyFileTypeDisplayed("application/pdf");
            await _fileRepositoryPage.VerifyUploadDateDisplayed();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_SearchFiles_FindsMatchingFiles()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload multiple files with different names
            var files = new[]
            {
                CreateTestFile("report-2024.pdf", "application/pdf", "Report content"),
                CreateTestFile("manual-v2.pdf", "application/pdf", "Manual content"),
                CreateTestFile("data-analysis.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Excel content")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);
            await _fileRepositoryPage.WaitForAllUploadsComplete();

            // Search for files containing "report"
            await _fileRepositoryPage.SearchFiles("report");

            // Verify search results
            await _fileRepositoryPage.VerifySearchResultsContain("report-2024.pdf");
            await _fileRepositoryPage.VerifySearchResultsDoNotContain("manual-v2.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_FilterByType_ShowsCorrectFiles()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload files of different types
            var files = new[]
            {
                CreateTestFile("document1.pdf", "application/pdf", "PDF content"),
                CreateTestFile("document2.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Word content"),
                CreateTestFile("image1.png", "image/png", "PNG content")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);
            await _fileRepositoryPage.WaitForAllUploadsComplete();

            // Filter by PDF type
            await _fileRepositoryPage.FilterByFileType("PDF");

            // Verify only PDF files are shown
            await _fileRepositoryPage.VerifyFilteredResultsContain("document1.pdf");
            await _fileRepositoryPage.VerifyFilteredResultsDoNotContain("document2.docx");
            await _fileRepositoryPage.VerifyFilteredResultsDoNotContain("image1.png");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_SortByDate_OrdersFilesCorrectly()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload multiple files
            var files = new[]
            {
                CreateTestFile("old-file.pdf", "application/pdf", "Old content"),
                CreateTestFile("new-file.pdf", "application/pdf", "New content"),
                CreateTestFile("middle-file.pdf", "application/pdf", "Middle content")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);
            await _fileRepositoryPage.WaitForAllUploadsComplete();

            // Sort by date (newest first)
            await _fileRepositoryPage.SortByDate("newest");

            // Verify files are sorted correctly
            await _fileRepositoryPage.VerifyFileOrder("new-file.pdf", "middle-file.pdf", "old-file.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_BulkOperations_SelectMultipleFiles()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload multiple files
            var files = new[]
            {
                CreateTestFile("bulk1.pdf", "application/pdf", "Bulk content 1"),
                CreateTestFile("bulk2.pdf", "application/pdf", "Bulk content 2"),
                CreateTestFile("bulk3.pdf", "application/pdf", "Bulk content 3")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);
            await _fileRepositoryPage.WaitForAllUploadsComplete();

            // Select multiple files
            await _fileRepositoryPage.SelectMultipleFiles(new[] { "bulk1.pdf", "bulk2.pdf" });

            // Verify selection
            await _fileRepositoryPage.VerifyFilesSelected(new[] { "bulk1.pdf", "bulk2.pdf" });
            await _fileRepositoryPage.VerifyBulkActionsEnabled();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_BulkDelete_DeletesSelectedFiles()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload multiple files
            var files = new[]
            {
                CreateTestFile("bulk-delete1.pdf", "application/pdf", "Bulk delete content 1"),
                CreateTestFile("bulk-delete2.pdf", "application/pdf", "Bulk delete content 2"),
                CreateTestFile("bulk-delete3.pdf", "application/pdf", "Bulk delete content 3")
            };

            await _fileRepositoryPage.UploadMultipleFiles(files);
            await _fileRepositoryPage.WaitForAllUploadsComplete();

            // Select and delete multiple files
            await _fileRepositoryPage.SelectMultipleFiles(new[] { "bulk-delete1.pdf", "bulk-delete2.pdf" });
            await _fileRepositoryPage.BulkDeleteSelectedFiles();

            // Verify files are deleted
            await _fileRepositoryPage.VerifyFilesDeleted(new[] { "bulk-delete1.pdf", "bulk-delete2.pdf" });
            await _fileRepositoryPage.VerifyFileExists("bulk-delete3.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_FilePreview_DisplaysFileContent()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a text file for preview
            var textFile = CreateTestFile("preview-test.txt", "text/plain", "This is preview content for testing");
            await _fileRepositoryPage.UploadFile(textFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Preview the file
            await _fileRepositoryPage.PreviewFile("preview-test.txt");

            // Verify preview is displayed
            await _fileRepositoryPage.VerifyFilePreviewDisplayed();
            await _fileRepositoryPage.VerifyPreviewContent("This is preview content for testing");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_UploadProgress_ShowsProgressBar()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload a large file to see progress
            var largeFile = CreateLargeTestFile("progress-test.pdf", "application/pdf", 2 * 1024 * 1024);
            await _fileRepositoryPage.UploadFile(largeFile);

            // Verify progress bar is displayed
            await _fileRepositoryPage.VerifyUploadProgressBarDisplayed();
            await _fileRepositoryPage.VerifyProgressPercentageUpdates();
            await _fileRepositoryPage.WaitForUploadComplete();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_UploadCancel_CancelsUpload()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Start uploading a large file
            var largeFile = CreateLargeTestFile("cancel-test.pdf", "application/pdf", 3 * 1024 * 1024);
            await _fileRepositoryPage.UploadFile(largeFile);

            // Cancel the upload
            await _fileRepositoryPage.CancelUpload();

            // Verify upload is cancelled
            await _fileRepositoryPage.VerifyUploadCancelled();
            await _fileRepositoryPage.VerifyFileNotUploaded("cancel-test.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_DragAndDrop_UploadsFiles()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Create test files for drag and drop
            var files = new[]
            {
                CreateTestFile("drag1.pdf", "application/pdf", "Drag content 1"),
                CreateTestFile("drag2.pdf", "application/pdf", "Drag content 2")
            };

            // Perform drag and drop upload
            await _fileRepositoryPage.DragAndDropFiles(files);

            // Verify files are uploaded
            await _fileRepositoryPage.WaitForAllUploadsComplete();
            foreach (var file in files)
            {
                await _fileRepositoryPage.VerifyFileUploaded(file.Name);
            }
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_FileSizeLimit_EnforcesSizeRestrictions()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Try to upload a file that exceeds size limit (assuming 10MB limit)
            var oversizedFile = CreateLargeTestFile("oversized.pdf", "application/pdf", 15 * 1024 * 1024);
            await _fileRepositoryPage.UploadFile(oversizedFile);

            // Verify error message for oversized file
            await _fileRepositoryPage.VerifyUploadErrorMessage("File size exceeds maximum allowed size");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_ConcurrentUploads_HandlesMultipleUploads()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Start multiple uploads simultaneously
            var files = new[]
            {
                CreateTestFile("concurrent1.pdf", "application/pdf", "Concurrent content 1"),
                CreateTestFile("concurrent2.pdf", "application/pdf", "Concurrent content 2"),
                CreateTestFile("concurrent3.pdf", "application/pdf", "Concurrent content 3")
            };

            await _fileRepositoryPage.StartConcurrentUploads(files);

            // Verify all uploads complete successfully
            await _fileRepositoryPage.WaitForAllUploadsComplete();
            foreach (var file in files)
            {
                await _fileRepositoryPage.VerifyFileUploaded(file.Name);
            }
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_NetworkInterruption_HandlesUploadFailure()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Start uploading a file
            var testFile = CreateTestFile("network-test.pdf", "application/pdf", "Network test content");
            await _fileRepositoryPage.UploadFile(testFile);

            // Simulate network interruption
            await _fileRepositoryPage.SimulateNetworkInterruption();

            // Verify upload failure handling
            await _fileRepositoryPage.VerifyUploadFailureHandled();
            await _fileRepositoryPage.VerifyRetryOptionAvailable();
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_FileVersioning_ManagesFileVersions()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Upload initial file
            var initialFile = CreateTestFile("version-test.pdf", "application/pdf", "Version 1 content");
            await _fileRepositoryPage.UploadFile(initialFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Upload updated version of the same file
            var updatedFile = CreateTestFile("version-test.pdf", "application/pdf", "Version 2 content");
            await _fileRepositoryPage.UploadFile(updatedFile);
            await _fileRepositoryPage.WaitForUploadComplete();

            // Verify version management
            await _fileRepositoryPage.VerifyFileVersioningEnabled();
            await _fileRepositoryPage.VerifyVersionHistoryAvailable("version-test.pdf");
        }

        [Test]
        [TestCategory("Slow")]
        public async Task FileRepository_Accessibility_SupportsScreenReaders()
        {
            // Navigate to file repository
            await _fileRepositoryPage.NavigateToFileRepository();

            // Verify accessibility features
            await _fileRepositoryPage.VerifyAriaLabelsPresent();
            await _fileRepositoryPage.VerifyKeyboardNavigation();
            await _fileRepositoryPage.VerifyScreenReaderSupport();
        }

        private TestFile CreateTestFile(string name, string contentType, string content)
        {
            return new TestFile
            {
                Name = name,
                ContentType = contentType,
                Content = content,
                Size = content.Length
            };
        }

        private TestFile CreateLargeTestFile(string name, string contentType, int sizeInBytes)
        {
            var content = new string('A', sizeInBytes);
            return new TestFile
            {
                Name = name,
                ContentType = contentType,
                Content = content,
                Size = sizeInBytes
            };
        }
    }

    public class TestFile
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public int Size { get; set; }
    }
} 