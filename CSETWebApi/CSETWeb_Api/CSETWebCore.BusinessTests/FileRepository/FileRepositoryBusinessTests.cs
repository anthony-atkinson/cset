//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CSETWebCore.Business.FileRepository;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.FileRepository;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.FileRepository
{
    [TestClass]
    public class FileRepositoryBusinessTests : BaseBusinessTest
    {
        private FileRepository _fileRepository;
        private Mock<CSETContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _fileRepository = new FileRepository(_mockContext.Object);
        }

        #region AddFileDescriptions Tests

        [TestMethod]
        public void AddFileDescriptions_WithValidFileResult_AddsFilesToDatabase()
        {
            // Arrange
            var fileResult = CreateTestFileResult();
            var mockDbSet = CreateMockDbSet<DOCUMENT_FILE>(new List<DOCUMENT_FILE>());
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _fileRepository.AddFileDescriptions(fileResult);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(fileResult.FileNames.Count);
            mockDbSet.Verify(x => x.Add(It.IsAny<DOCUMENT_FILE>()), Times.Exactly(fileResult.FileNames.Count));
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void AddFileDescriptions_WithSingleFile_CreatesCorrectFileDescription()
        {
            // Arrange
            var fileResult = CreateTestFileResult(1);
            var mockDbSet = CreateMockDbSet<DOCUMENT_FILE>(new List<DOCUMENT_FILE>());
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _fileRepository.AddFileDescriptions(fileResult);

            // Assert
            result.Should().HaveCount(1);
            var fileDesc = result.First();
            fileDesc.Name.Should().Be(fileResult.Names[0]);
            fileDesc.Path.Should().Be(fileResult.FileNames[0]);
            fileDesc.Title.Should().Be(fileResult.Tiitle);
        }

        [TestMethod]
        public void AddFileDescriptions_WithMultipleFiles_CreatesAllFileDescriptions()
        {
            // Arrange
            var fileResult = CreateTestFileResult(3);
            var mockDbSet = CreateMockDbSet<DOCUMENT_FILE>(new List<DOCUMENT_FILE>());
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _fileRepository.AddFileDescriptions(fileResult);

            // Assert
            result.Should().HaveCount(3);
            result.Should().OnlyContain(f => f.Title == fileResult.Tiitle);
        }

        [TestMethod]
        public void AddFileDescriptions_WithEmptyFileResult_ReturnsEmptyList()
        {
            // Arrange
            var fileResult = CreateTestFileResult(0);
            var mockDbSet = CreateMockDbSet<DOCUMENT_FILE>(new List<DOCUMENT_FILE>());
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(0);

            // Act
            var result = _fileRepository.AddFileDescriptions(fileResult);

            // Assert
            result.Should().BeEmpty();
            mockDbSet.Verify(x => x.Add(It.IsAny<DOCUMENT_FILE>()), Times.Never);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void AddFileDescriptions_WithNullFileResult_ThrowsArgumentNullException()
        {
            // Arrange
            FileResult fileResult = null;

            // Act & Assert
            Action act = () => _fileRepository.AddFileDescriptions(fileResult);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddFileDescriptions_WithNullFileNames_ThrowsNullReferenceException()
        {
            // Arrange
            var fileResult = CreateTestFileResult();
            fileResult.FileNames = null;

            // Act & Assert
            Action act = () => _fileRepository.AddFileDescriptions(fileResult);
            act.Should().Throw<NullReferenceException>();
        }

        [TestMethod]
        public void AddFileDescriptions_WithMismatchedArrays_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            var fileResult = CreateTestFileResult();
            fileResult.ContentTypes = new List<string> { "application/pdf" }; // Only one content type for multiple files

            // Act & Assert
            Action act = () => _fileRepository.AddFileDescriptions(fileResult);
            act.Should().Throw<IndexOutOfRangeException>();
        }

        #endregion

        #region GetAllFiles Tests

        [TestMethod]
        public void GetAllFiles_WithValidAssessmentId_ReturnsFilesForAssessment()
        {
            // Arrange
            int assessmentId = 1;
            var testFiles = CreateTestDocumentFiles(assessmentId, 3);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetAllFiles(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(f => f.Id > 0);
        }

        [TestMethod]
        public void GetAllFiles_WithNonExistentAssessmentId_ReturnsEmptyList()
        {
            // Arrange
            int assessmentId = 999;
            var testFiles = CreateTestDocumentFiles(1, 2); // Files for different assessment
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetAllFiles(assessmentId);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAllFiles_WithZeroAssessmentId_ReturnsEmptyList()
        {
            // Arrange
            int assessmentId = 0;
            var testFiles = CreateTestDocumentFiles(1, 2);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetAllFiles(assessmentId);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAllFiles_WithNegativeAssessmentId_ReturnsEmptyList()
        {
            // Arrange
            int assessmentId = -1;
            var testFiles = CreateTestDocumentFiles(1, 2);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetAllFiles(assessmentId);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAllFiles_WithMultipleAssessments_ReturnsOnlyRequestedAssessmentFiles()
        {
            // Arrange
            int assessmentId = 1;
            var filesForAssessment1 = CreateTestDocumentFiles(1, 2);
            var filesForAssessment2 = CreateTestDocumentFiles(2, 3);
            var allFiles = filesForAssessment1.Concat(filesForAssessment2).ToList();
            var mockDbSet = CreateMockDbSet(allFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetAllFiles(assessmentId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(f => f.Id <= 2); // First two files belong to assessment 1
        }

        #endregion

        #region GetFileDescription Tests

        [TestMethod]
        public void GetFileDescription_WithValidId_ReturnsFileDescription()
        {
            // Arrange
            int fileId = 1;
            var testFile = CreateTestDocumentFile(fileId, 1);
            var mockDbSet = CreateMockDbSet(new List<DOCUMENT_FILE> { testFile });
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act
            var result = _fileRepository.GetFileDescription(fileId);

            // Assert
            result.Should().NotBeNull();
            result.Document_Id.Should().Be(fileId);
            result.Title.Should().Be(testFile.Title);
            result.Path.Should().Be(testFile.Path);
        }

        [TestMethod]
        public void GetFileDescription_WithNonExistentId_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = 999;
            var testFiles = CreateTestDocumentFiles(1, 2);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act & Assert
            Action act = () => _fileRepository.GetFileDescription(fileId);
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void GetFileDescription_WithZeroId_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = 0;
            var testFiles = CreateTestDocumentFiles(1, 2);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act & Assert
            Action act = () => _fileRepository.GetFileDescription(fileId);
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void GetFileDescription_WithNegativeId_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = -1;
            var testFiles = CreateTestDocumentFiles(1, 2);
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act & Assert
            Action act = () => _fileRepository.GetFileDescription(fileId);
            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void GetFileDescription_WithMultipleMatchingFiles_ThrowsInvalidOperationException()
        {
            // Arrange
            int fileId = 1;
            var testFiles = new List<DOCUMENT_FILE>
            {
                CreateTestDocumentFile(1, 1),
                CreateTestDocumentFile(1, 1) // Duplicate ID
            };
            var mockDbSet = CreateMockDbSet(testFiles);
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);

            // Act & Assert
            Action act = () => _fileRepository.GetFileDescription(fileId);
            act.Should().Throw<InvalidOperationException>();
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void FileRepository_CompleteWorkflow_AddAndRetrieveFiles()
        {
            // Arrange
            var fileResult = CreateTestFileResult(2);
            var mockDbSet = CreateMockDbSet<DOCUMENT_FILE>(new List<DOCUMENT_FILE>());
            
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(mockDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act - Add files
            var addedFiles = _fileRepository.AddFileDescriptions(fileResult);

            // Assert - Files were added
            addedFiles.Should().HaveCount(2);

            // Arrange - Setup for retrieval
            var testFiles = CreateTestDocumentFiles(1, 2);
            var retrievalMockDbSet = CreateMockDbSet(testFiles);
            _mockContext.Setup(x => x.DOCUMENT_FILE).Returns(retrievalMockDbSet.Object);

            // Act - Retrieve files
            var retrievedFiles = _fileRepository.GetAllFiles(1);

            // Assert - Files can be retrieved
            retrievedFiles.Should().HaveCount(2);
        }

        #endregion

        #region Helper Methods

        private FileResult CreateTestFileResult(int fileCount = 2)
        {
            var fileNames = new List<string>();
            var contentTypes = new List<string>();
            var names = new List<string>();

            for (int i = 0; i < fileCount; i++)
            {
                fileNames.Add($"test-file-{i + 1}.pdf");
                contentTypes.Add("application/pdf");
                names.Add($"Test File {i + 1}");
            }

            return new FileResult
            {
                FileNames = fileNames,
                ContentTypes = contentTypes,
                Names = names,
                Tiitle = "Test Document",
                CreatedTimestamp = DateTime.Now,
                UpdatedTimestamp = DateTime.Now
            };
        }

        private List<DOCUMENT_FILE> CreateTestDocumentFiles(int assessmentId, int count)
        {
            var files = new List<DOCUMENT_FILE>();
            for (int i = 0; i < count; i++)
            {
                files.Add(CreateTestDocumentFile(i + 1, assessmentId));
            }
            return files;
        }

        private DOCUMENT_FILE CreateTestDocumentFile(int documentId, int assessmentId)
        {
            return new DOCUMENT_FILE
            {
                Document_Id = documentId,
                Assessment_Id = assessmentId,
                Title = $"Test Document {documentId}",
                Path = $"test-file-{documentId}.pdf",
                Name = $"Test File {documentId}",
                ContentType = "application/pdf",
                CreatedTimestamp = DateTime.Now,
                UpdatedTimestamp = DateTime.Now,
                FileMd5 = $"hash-{documentId}",
                Data = new byte[] { 1, 2, 3, 4 },
                IsGlobal = false
            };
        }

        private Mock<Microsoft.EntityFrameworkCore.DbSet<T>> CreateMockDbSet<T>(List<T> list) where T : class
        {
            var queryable = list.AsQueryable();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            mockDbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => list.Add(s));
            
            return mockDbSet;
        }

        #endregion
    }
} 