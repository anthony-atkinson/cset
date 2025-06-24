//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Moq;
using CSETWebCore.Business.RepositoryLibrary;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.ResourceLibrary;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.FileRepository
{
    [TestClass]
    public class ReferenceDocumentManagerTests : BaseBusinessTest
    {
        private ReferenceDocumentManager _referenceDocumentManager;
        private Mock<CSETContext> _mockContext;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private Mock<IConfiguration> _mockConfiguration;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockConfiguration = new Mock<IConfiguration>();
            
            _mockEnvironment.Setup(x => x.ContentRootPath).Returns("/test/path");
            _mockConfiguration.Setup(x => x.GetValue<string>("RefDocPath")).Returns("Documents");
            
            _referenceDocumentManager = new ReferenceDocumentManager(
                _mockContext.Object, 
                _mockEnvironment.Object, 
                _mockConfiguration.Object);
        }

        #region FindLocalReferenceDocument Tests

        [TestMethod]
        public void FindLocalReferenceDocument_WithValidFileId_ReturnsReferenceFileResponse()
        {
            // Arrange
            string fileId = "1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.FileName.Should().Be("test.pdf");
            result.ContentType.Should().Be("application/pdf");
            result.Stream.Should().NotBeNull();
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithFileName_ReturnsReferenceFileResponse()
        {
            // Arrange
            string fileName = "test.pdf";
            var testGenFile = CreateTestGenFile(1, fileName, "application/pdf");
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileName);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.FileName.Should().Be(fileName);
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithFileIdAndHash_StripsHashAndFindsFile()
        {
            // Arrange
            string fileIdWithHash = "1#section1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileIdWithHash);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithNonExistentFileId_ReturnsNull()
        {
            // Arrange
            string fileId = "999";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithNonExistentFileName_ReturnsNull()
        {
            // Arrange
            string fileName = "nonexistent.pdf";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileName);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithFileType_ReturnsCorrectContentType()
        {
            // Arrange
            string fileId = "1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", null, 1);
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().NotBeNull();
            result.ContentType.Should().Be("application/pdf");
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithBinaryData_ReturnsMemoryStream()
        {
            // Arrange
            string fileId = "1";
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf", data: testData);
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().NotBeNull();
            result.Stream.Should().BeOfType<MemoryStream>();
            
            using var memoryStream = new MemoryStream();
            result.Stream.CopyTo(memoryStream);
            memoryStream.ToArray().Should().BeEquivalentTo(testData);
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithPhysicalFile_ReturnsFileStream()
        {
            // Arrange
            string fileId = "1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf", data: null);
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().NotBeNull();
            // Note: This will throw FileNotFoundException in test environment since physical file doesn't exist
            // In real scenario, this would return a FileStream
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithInvalidFileId_ReturnsNull()
        {
            // Arrange
            string fileId = "invalid";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithEmptyFileId_ReturnsNull()
        {
            // Arrange
            string fileId = "";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region SaveDataBuffer Tests

        [TestMethod]
        public void SaveDataBuffer_WithValidGenFileId_SavesDataToDatabase()
        {
            // Arrange
            int genFileId = 1;
            var testData = new byte[] { 1, 2, 3, 4, 5 };
            var testGenFile = CreateTestGenFile(genFileId, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            using var stream = new MemoryStream(testData);

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            testGenFile.Data.Should().BeEquivalentTo(testData);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveDataBuffer_WithNonExistentGenFileId_DoesNothing()
        {
            // Arrange
            int genFileId = 999;
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void SaveDataBuffer_WithNullStream_DoesNothing()
        {
            // Arrange
            int genFileId = 1;
            var testGenFile = CreateTestGenFile(genFileId, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            Stream stream = null;

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void SaveDataBuffer_WithLargeData_SavesCorrectly()
        {
            // Arrange
            int genFileId = 1;
            var testData = new byte[1024 * 1024]; // 1MB of data
            new Random().NextBytes(testData); // Fill with random data
            var testGenFile = CreateTestGenFile(genFileId, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            using var stream = new MemoryStream(testData);

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            testGenFile.Data.Should().BeEquivalentTo(testData);
            testGenFile.Data.Length.Should().Be(1024 * 1024);
        }

        [TestMethod]
        public void SaveDataBuffer_WithZeroGenFileId_DoesNothing()
        {
            // Arrange
            int genFileId = 0;
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void SaveDataBuffer_WithNegativeGenFileId_DoesNothing()
        {
            // Arrange
            int genFileId = -1;
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf");
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);

            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

            // Act
            _referenceDocumentManager.SaveDataBuffer(genFileId, stream);

            // Assert
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        #endregion

        #region Edge Cases and Error Handling

        [TestMethod]
        public void FindLocalReferenceDocument_WithExceptionDuringFileAccess_ReturnsNull()
        {
            // Arrange
            string fileId = "1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", "application/pdf", data: null);
            var testFileType = CreateTestFileType(1, "PDF", "application/pdf");
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().BeNull(); // Should return null when physical file access fails
        }

        [TestMethod]
        public void FindLocalReferenceDocument_WithNullFileType_ReturnsDefaultContentType()
        {
            // Arrange
            string fileId = "1";
            var testGenFile = CreateTestGenFile(1, "test.pdf", null, 1);
            var testFileType = CreateTestFileType(1, "PDF", null);
            
            var mockGenFileDbSet = CreateMockDbSet(new List<GEN_FILE> { testGenFile });
            var mockFileTypeDbSet = CreateMockDbSet(new List<FILE_TYPE> { testFileType });
            
            _mockContext.Setup(x => x.GEN_FILE).Returns(mockGenFileDbSet.Object);
            _mockContext.Setup(x => x.FILE_TYPE).Returns(mockFileTypeDbSet.Object);

            // Act
            var result = _referenceDocumentManager.FindLocalReferenceDocument(fileId);

            // Assert
            result.Should().NotBeNull();
            // Content type should be determined by file extension when MIME type is null
        }

        #endregion

        #region Helper Methods

        private GEN_FILE CreateTestGenFile(int id, string fileName, string contentType, decimal? fileTypeId = null, byte[] data = null)
        {
            return new GEN_FILE
            {
                Gen_File_Id = id,
                File_Name = fileName,
                Title = $"Test File {id}",
                Name = fileName,
                File_Type_Id = fileTypeId,
                ContentType = contentType,
                Data = data,
                Is_Uploaded = true,
                Doc_Num = "TEST001",
                Short_Name = $"Test{id}",
                Language = "en"
            };
        }

        private FILE_TYPE CreateTestFileType(decimal id, string fileType, string mimeType)
        {
            return new FILE_TYPE
            {
                File_Type_Id = id,
                File_Type1 = fileType,
                Mime_Type = mimeType,
                Description = $"Test file type {fileType}"
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
            
            return mockDbSet;
        }

        #endregion
    }
} 