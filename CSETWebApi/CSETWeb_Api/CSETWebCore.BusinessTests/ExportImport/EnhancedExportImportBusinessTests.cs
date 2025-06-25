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
using System.Threading.Tasks;
using CSETWebCore.Business.ExportImport;
using CSETWebCore.Interfaces.ExportImport;
using CSETWebCore.Model.ExportImport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MSTest;

namespace CSETWebCore.BusinessTests.ExportImport
{
    /// <summary>
    /// Tests for enhanced export/import business logic
    /// </summary>
    [TestClass]
    public class EnhancedExportImportBusinessTests
    {
        private Mock<IExportImportRepository> _mockRepository;
        private Mock<IAssessmentBusiness> _mockAssessmentBusiness;
        private Mock<IQuestionBusiness> _mockQuestionBusiness;
        private Mock<IDocumentBusiness> _mockDocumentBusiness;
        private Mock<ILogger<EnhancedExportImportBusiness>> _mockLogger;
        private EnhancedExportImportBusiness _exportImportBusiness;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IExportImportRepository>();
            _mockAssessmentBusiness = new Mock<IAssessmentBusiness>();
            _mockQuestionBusiness = new Mock<IQuestionBusiness>();
            _mockDocumentBusiness = new Mock<IDocumentBusiness>();
            _mockLogger = new Mock<ILogger<EnhancedExportImportBusiness>>();

            _exportImportBusiness = new EnhancedExportImportBusiness(
                _mockRepository.Object,
                _mockAssessmentBusiness.Object,
                _mockQuestionBusiness.Object,
                _mockDocumentBusiness.Object,
                _mockLogger.Object);
        }

        #region Export Tests

        [TestMethod]
        public async Task ExportAssessment_ValidAssessmentId_ReturnsExportData()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentData = CreateTestAssessmentData(assessmentId);
            var questionsData = CreateTestQuestionsData(assessmentId);
            var documentsData = CreateTestDocumentsData(assessmentId);

            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Returns(assessmentData);
            _mockQuestionBusiness.Setup(x => x.GetQuestionsByAssessment(assessmentId)).Returns(questionsData);
            _mockDocumentBusiness.Setup(x => x.GetDocumentsByAssessment(assessmentId)).Returns(documentsData);

            // Act
            var result = await _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.JSON);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentData.Should().BeEquivalentTo(assessmentData);
            result.QuestionsData.Should().BeEquivalentTo(questionsData);
            result.DocumentsData.Should().BeEquivalentTo(documentsData);
            result.ExportFormat.Should().Be(ExportFormat.JSON);
            result.ExportedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public async Task ExportAssessment_InvalidAssessmentId_ThrowsException()
        {
            // Arrange
            var invalidAssessmentId = 999;
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(invalidAssessmentId)).Returns((AssessmentData)null);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
                _exportImportBusiness.ExportAssessment(invalidAssessmentId, ExportFormat.JSON));
        }

        [TestMethod]
        public async Task ExportAssessment_JSONFormat_ReturnsJsonString()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentData = CreateTestAssessmentData(assessmentId);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Returns(assessmentData);
            _mockQuestionBusiness.Setup(x => x.GetQuestionsByAssessment(assessmentId)).Returns(new List<QuestionData>());
            _mockDocumentBusiness.Setup(x => x.GetDocumentsByAssessment(assessmentId)).Returns(new List<DocumentData>());

            // Act
            var result = await _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.JSON);

            // Assert
            result.ExportContent.Should().NotBeNullOrEmpty();
            result.ExportContent.Should().Contain("assessmentId");
            result.ExportContent.Should().Contain("1");
        }

        [TestMethod]
        public async Task ExportAssessment_XMLFormat_ReturnsXmlString()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentData = CreateTestAssessmentData(assessmentId);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Returns(assessmentData);
            _mockQuestionBusiness.Setup(x => x.GetQuestionsByAssessment(assessmentId)).Returns(new List<QuestionData>());
            _mockDocumentBusiness.Setup(x => x.GetDocumentsByAssessment(assessmentId)).Returns(new List<DocumentData>());

            // Act
            var result = await _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.XML);

            // Assert
            result.ExportContent.Should().NotBeNullOrEmpty();
            result.ExportContent.Should().Contain("<?xml");
            result.ExportContent.Should().Contain("<AssessmentData>");
        }

        [TestMethod]
        public async Task ExportAssessment_CSVFormat_ReturnsCsvString()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentData = CreateTestAssessmentData(assessmentId);
            var questionsData = CreateTestQuestionsData(assessmentId);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Returns(assessmentData);
            _mockQuestionBusiness.Setup(x => x.GetQuestionsByAssessment(assessmentId)).Returns(questionsData);
            _mockDocumentBusiness.Setup(x => x.GetDocumentsByAssessment(assessmentId)).Returns(new List<DocumentData>());

            // Act
            var result = await _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.CSV);

            // Assert
            result.ExportContent.Should().NotBeNullOrEmpty();
            result.ExportContent.Should().Contain(",");
            result.ExportContent.Should().Contain("QuestionId");
        }

        [TestMethod]
        public async Task ExportAssessment_ExcelFormat_ReturnsExcelBytes()
        {
            // Arrange
            var assessmentId = 1;
            var assessmentData = CreateTestAssessmentData(assessmentId);
            var questionsData = CreateTestQuestionsData(assessmentId);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Returns(assessmentData);
            _mockQuestionBusiness.Setup(x => x.GetQuestionsByAssessment(assessmentId)).Returns(questionsData);
            _mockDocumentBusiness.Setup(x => x.GetDocumentsByAssessment(assessmentId)).Returns(new List<DocumentData>());

            // Act
            var result = await _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.EXCEL);

            // Assert
            result.ExportContent.Should().NotBeNullOrEmpty();
            result.ExportContent.Should().Contain("PK"); // Excel file signature
        }

        #endregion

        #region Import Tests

        [TestMethod]
        public async Task ImportAssessment_ValidJsonData_ImportsSuccessfully()
        {
            // Arrange
            var importData = CreateTestImportData();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(importData);
            var importRequest = new ImportRequest
            {
                Content = jsonContent,
                Format = ImportFormat.JSON,
                ValidateOnly = false
            };

            _mockRepository.Setup(x => x.ValidateImportData(It.IsAny<ImportData>())).ReturnsAsync(new ValidationResult { IsValid = true });

            // Act
            var result = await _exportImportBusiness.ImportAssessment(importRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ImportedAssessmentId.Should().BeGreaterThan(0);
            result.ValidationResult.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public async Task ImportAssessment_InvalidData_ReturnsValidationErrors()
        {
            // Arrange
            var importData = CreateTestImportData();
            importData.AssessmentData.AssessmentName = ""; // Invalid - empty name
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(importData);
            var importRequest = new ImportRequest
            {
                Content = jsonContent,
                Format = ImportFormat.JSON,
                ValidateOnly = true
            };

            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Assessment name is required" }
            };
            _mockRepository.Setup(x => x.ValidateImportData(It.IsAny<ImportData>())).ReturnsAsync(validationResult);

            // Act
            var result = await _exportImportBusiness.ImportAssessment(importRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.ValidationResult.IsValid.Should().BeFalse();
            result.ValidationResult.Errors.Should().Contain("Assessment name is required");
        }

        [TestMethod]
        public async Task ImportAssessment_UnsupportedFormat_ThrowsException()
        {
            // Arrange
            var importRequest = new ImportRequest
            {
                Content = "invalid content",
                Format = ImportFormat.UNKNOWN,
                ValidateOnly = false
            };

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotSupportedException>(() => 
                _exportImportBusiness.ImportAssessment(importRequest));
        }

        [TestMethod]
        public async Task ImportAssessment_ValidateOnly_DoesNotImportData()
        {
            // Arrange
            var importData = CreateTestImportData();
            var jsonContent = System.Text.Json.JsonSerializer.Serialize(importData);
            var importRequest = new ImportRequest
            {
                Content = jsonContent,
                Format = ImportFormat.JSON,
                ValidateOnly = true
            };

            _mockRepository.Setup(x => x.ValidateImportData(It.IsAny<ImportData>())).ReturnsAsync(new ValidationResult { IsValid = true });

            // Act
            var result = await _exportImportBusiness.ImportAssessment(importRequest);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.ImportedAssessmentId.Should().Be(0); // No import when validate only
            _mockAssessmentBusiness.Verify(x => x.CreateAssessment(It.IsAny<AssessmentData>()), Times.Never);
        }

        #endregion

        #region Validation Tests

        [TestMethod]
        public async Task ValidateImportData_ValidData_ReturnsValidResult()
        {
            // Arrange
            var importData = CreateTestImportData();
            _mockRepository.Setup(x => x.ValidateImportData(importData)).ReturnsAsync(new ValidationResult { IsValid = true });

            // Act
            var result = await _exportImportBusiness.ValidateImportData(importData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ValidateImportData_InvalidAssessment_ReturnsErrors()
        {
            // Arrange
            var importData = CreateTestImportData();
            importData.AssessmentData.AssessmentName = ""; // Invalid

            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Assessment name is required" }
            };
            _mockRepository.Setup(x => x.ValidateImportData(importData)).ReturnsAsync(validationResult);

            // Act
            var result = await _exportImportBusiness.ValidateImportData(importData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Assessment name is required");
        }

        [TestMethod]
        public async Task ValidateImportData_InvalidQuestions_ReturnsErrors()
        {
            // Arrange
            var importData = CreateTestImportData();
            importData.QuestionsData[0].QuestionText = ""; // Invalid

            var validationResult = new ValidationResult
            {
                IsValid = false,
                Errors = new List<string> { "Question text is required" }
            };
            _mockRepository.Setup(x => x.ValidateImportData(importData)).ReturnsAsync(validationResult);

            // Act
            var result = await _exportImportBusiness.ValidateImportData(importData);

            // Assert
            result.Should().NotBeNull();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain("Question text is required");
        }

        #endregion

        #region Format Conversion Tests

        [TestMethod]
        public async Task ConvertFormat_JsonToXml_ConvertsSuccessfully()
        {
            // Arrange
            var jsonContent = "{\"assessmentId\":1,\"name\":\"Test Assessment\"}";
            var sourceFormat = ExportFormat.JSON;
            var targetFormat = ExportFormat.XML;

            // Act
            var result = await _exportImportBusiness.ConvertFormat(jsonContent, sourceFormat, targetFormat);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("<?xml");
            result.Should().Contain("<assessmentId>1</assessmentId>");
        }

        [TestMethod]
        public async Task ConvertFormat_XmlToJson_ConvertsSuccessfully()
        {
            // Arrange
            var xmlContent = "<AssessmentData><assessmentId>1</assessmentId><name>Test Assessment</name></AssessmentData>";
            var sourceFormat = ExportFormat.XML;
            var targetFormat = ExportFormat.JSON;

            // Act
            var result = await _exportImportBusiness.ConvertFormat(xmlContent, sourceFormat, targetFormat);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("\"assessmentId\":1");
            result.Should().Contain("\"name\":\"Test Assessment\"");
        }

        [TestMethod]
        public async Task ConvertFormat_UnsupportedConversion_ThrowsException()
        {
            // Arrange
            var content = "test content";
            var sourceFormat = ExportFormat.EXCEL;
            var targetFormat = ExportFormat.XML;

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotSupportedException>(() => 
                _exportImportBusiness.ConvertFormat(content, sourceFormat, targetFormat));
        }

        #endregion

        #region Batch Operations Tests

        [TestMethod]
        public async Task ExportMultipleAssessments_ValidIds_ReturnsMultipleExports()
        {
            // Arrange
            var assessmentIds = new List<int> { 1, 2, 3 };
            var assessmentData1 = CreateTestAssessmentData(1);
            var assessmentData2 = CreateTestAssessmentData(2);
            var assessmentData3 = CreateTestAssessmentData(3);

            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(1)).Returns(assessmentData1);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(2)).Returns(assessmentData2);
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(3)).Returns(assessmentData3);

            // Act
            var result = await _exportImportBusiness.ExportMultipleAssessments(assessmentIds, ExportFormat.JSON);

            // Assert
            result.Should().NotBeNull();
            result.Exports.Should().HaveCount(3);
            result.Exports[0].AssessmentData.AssessmentId.Should().Be(1);
            result.Exports[1].AssessmentData.AssessmentId.Should().Be(2);
            result.Exports[2].AssessmentData.AssessmentId.Should().Be(3);
        }

        [TestMethod]
        public async Task ImportMultipleAssessments_ValidData_ImportsAll()
        {
            // Arrange
            var importRequests = new List<ImportRequest>
            {
                new ImportRequest { Content = System.Text.Json.JsonSerializer.Serialize(CreateTestImportData()), Format = ImportFormat.JSON },
                new ImportRequest { Content = System.Text.Json.JsonSerializer.Serialize(CreateTestImportData()), Format = ImportFormat.JSON }
            };

            _mockRepository.Setup(x => x.ValidateImportData(It.IsAny<ImportData>())).ReturnsAsync(new ValidationResult { IsValid = true });

            // Act
            var result = await _exportImportBusiness.ImportMultipleAssessments(importRequests);

            // Assert
            result.Should().NotBeNull();
            result.Results.Should().HaveCount(2);
            result.Results.All(r => r.Success).Should().BeTrue();
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public async Task ExportAssessment_RepositoryException_LogsErrorAndThrows()
        {
            // Arrange
            var assessmentId = 1;
            _mockAssessmentBusiness.Setup(x => x.GetAssessmentById(assessmentId)).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _exportImportBusiness.ExportAssessment(assessmentId, ExportFormat.JSON));

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error exporting assessment")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task ImportAssessment_RepositoryException_LogsErrorAndThrows()
        {
            // Arrange
            var importRequest = new ImportRequest
            {
                Content = "invalid json",
                Format = ImportFormat.JSON,
                ValidateOnly = false
            };

            _mockRepository.Setup(x => x.ValidateImportData(It.IsAny<ImportData>())).Throws(new Exception("Validation error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => 
                _exportImportBusiness.ImportAssessment(importRequest));

            _mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error importing assessment")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        #endregion

        #region Helper Methods

        private AssessmentData CreateTestAssessmentData(int assessmentId)
        {
            return new AssessmentData
            {
                AssessmentId = assessmentId,
                AssessmentName = $"Test Assessment {assessmentId}",
                AssessmentDate = DateTime.UtcNow,
                CreatorName = "Test User",
                AssessmentType = "Standard"
            };
        }

        private List<QuestionData> CreateTestQuestionsData(int assessmentId)
        {
            return new List<QuestionData>
            {
                new QuestionData
                {
                    QuestionId = 1,
                    AssessmentId = assessmentId,
                    QuestionText = "Test Question 1",
                    Answer = "Y",
                    Category = "Test Category"
                },
                new QuestionData
                {
                    QuestionId = 2,
                    AssessmentId = assessmentId,
                    QuestionText = "Test Question 2",
                    Answer = "N",
                    Category = "Test Category"
                }
            };
        }

        private List<DocumentData> CreateTestDocumentsData(int assessmentId)
        {
            return new List<DocumentData>
            {
                new DocumentData
                {
                    DocumentId = 1,
                    AssessmentId = assessmentId,
                    Title = "Test Document 1",
                    FileName = "test1.pdf",
                    ContentType = "application/pdf"
                }
            };
        }

        private ImportData CreateTestImportData()
        {
            return new ImportData
            {
                AssessmentData = CreateTestAssessmentData(1),
                QuestionsData = CreateTestQuestionsData(1),
                DocumentsData = CreateTestDocumentsData(1)
            };
        }

        #endregion
    }
} 