//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using CSETWebCore.Business.Document;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Document;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Document
{
    [TestClass]
    public class DocumentBusinessTests : BaseBusinessTest
    {
        private DocumentBusiness _documentBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateInMemoryDbContext();
            _mockAssessmentUtil = Fixture.Freeze<Mock<IAssessmentUtil>>();

            _documentBusiness = new DocumentBusiness(_context, _mockAssessmentUtil.Object);
        }

        #region GetDocumentsForAnswer Tests

        [TestMethod]
        public void GetDocumentsForAnswer_WithValidAnswerId_ReturnsDocuments()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            var document = CreateTestDocument(1, assessmentId);
            var documentAnswer = CreateTestDocumentAnswer(document.Document_Id, answerId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.Add(documentAnswer);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetDocumentsForAnswer(answerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Document_Id.Should().Be(document.Document_Id);
            result[0].Title.Should().Be(document.Title);
            result[0].FileName.Should().Be(document.Name);
            result[0].IsGlobal.Should().Be(document.IsGlobal);
        }

        [TestMethod]
        public void GetDocumentsForAnswer_WithInvalidAnswerId_ReturnsEmptyList()
        {
            // Arrange
            var invalidAnswerId = 999;

            // Act
            var result = _documentBusiness.GetDocumentsForAnswer(invalidAnswerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetDocumentsForAnswer_WithAnswerHavingNoDocuments_ReturnsEmptyList()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetDocumentsForAnswer(answerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region RenameDocument Tests

        [TestMethod]
        public void RenameDocument_WithValidDocumentId_UpdatesTitle()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var newTitle = "Updated Document Title";
            
            var assessment = CreateTestAssessment(assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(document);
            _context.SaveChanges();

            var originalTimestamp = document.UpdatedTimestamp;

            // Act
            _documentBusiness.RenameDocument(documentId, newTitle);

            // Assert
            var updatedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == documentId);
            updatedDocument.Should().NotBeNull();
            updatedDocument.Title.Should().Be(newTitle);
            updatedDocument.UpdatedTimestamp.Should().BeAfter(originalTimestamp);
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void RenameDocument_WithInvalidDocumentId_DoesNothing()
        {
            // Arrange
            var invalidDocumentId = 999;
            var newTitle = "Updated Document Title";

            // Act
            _documentBusiness.RenameDocument(invalidDocumentId, newTitle);

            // Assert
            var document = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == invalidDocumentId);
            document.Should().BeNull();
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void RenameDocument_WithEmptyTitle_UpdatesTitle()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var emptyTitle = "";
            
            var assessment = CreateTestAssessment(assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(document);
            _context.SaveChanges();

            // Act
            _documentBusiness.RenameDocument(documentId, emptyTitle);

            // Assert
            var updatedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == documentId);
            updatedDocument.Should().NotBeNull();
            updatedDocument.Title.Should().Be(emptyTitle);
        }

        #endregion

        #region ChangeGlobal Tests

        [TestMethod]
        public void ChangeGlobal_WithValidDocumentId_UpdatesGlobalFlag()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var isGlobal = true;
            
            var assessment = CreateTestAssessment(assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            document.IsGlobal = false;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(document);
            _context.SaveChanges();

            var originalTimestamp = document.UpdatedTimestamp;

            // Act
            _documentBusiness.ChangeGlobal(documentId, isGlobal);

            // Assert
            var updatedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == documentId);
            updatedDocument.Should().NotBeNull();
            updatedDocument.IsGlobal.Should().Be(isGlobal);
            updatedDocument.UpdatedTimestamp.Should().BeAfter(originalTimestamp);
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void ChangeGlobal_WithInvalidDocumentId_DoesNothing()
        {
            // Arrange
            var invalidDocumentId = 999;
            var isGlobal = true;

            // Act
            _documentBusiness.ChangeGlobal(invalidDocumentId, isGlobal);

            // Assert
            var document = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == invalidDocumentId);
            document.Should().BeNull();
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(It.IsAny<int>()), Times.Never);
        }

        [TestMethod]
        public void ChangeGlobal_WithFalseValue_UpdatesGlobalFlag()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var isGlobal = false;
            
            var assessment = CreateTestAssessment(assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            document.IsGlobal = true;
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(document);
            _context.SaveChanges();

            // Act
            _documentBusiness.ChangeGlobal(documentId, isGlobal);

            // Assert
            var updatedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => d.Document_Id == documentId);
            updatedDocument.Should().NotBeNull();
            updatedDocument.IsGlobal.Should().Be(isGlobal);
        }

        #endregion

        #region DeleteDocument Tests

        [TestMethod]
        public void DeleteDocument_WithValidDocumentId_RemovesDocumentFromAnswer()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            var documentAnswer = CreateTestDocumentAnswer(documentId, answerId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.Add(documentAnswer);
            _context.SaveChanges();

            // Act
            _documentBusiness.DeleteDocument(documentId, questionId, assessmentId);

            // Assert
            var remainingDocumentAnswer = _context.DOCUMENT_ANSWERS
                .FirstOrDefault(da => da.Document_Id == documentId && da.Answer_Id == answerId);
            remainingDocumentAnswer.Should().BeNull();
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void DeleteDocument_WithDocumentUsedByMultipleAnswers_RemovesOnlyFromSpecifiedAnswer()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var answerId1 = 1;
            var answerId2 = 2;
            var questionId1 = 1;
            var questionId2 = 2;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer1 = CreateTestAnswer(answerId1, questionId1, assessmentId);
            var answer2 = CreateTestAnswer(answerId2, questionId2, assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            var documentAnswer1 = CreateTestDocumentAnswer(documentId, answerId1);
            var documentAnswer2 = CreateTestDocumentAnswer(documentId, answerId2);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.AddRange(answer1, answer2);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.AddRange(documentAnswer1, documentAnswer2);
            _context.SaveChanges();

            // Act
            _documentBusiness.DeleteDocument(documentId, questionId1, assessmentId);

            // Assert
            var remainingDocumentAnswer1 = _context.DOCUMENT_ANSWERS
                .FirstOrDefault(da => da.Document_Id == documentId && da.Answer_Id == answerId1);
            var remainingDocumentAnswer2 = _context.DOCUMENT_ANSWERS
                .FirstOrDefault(da => da.Document_Id == documentId && da.Answer_Id == answerId2);
            
            remainingDocumentAnswer1.Should().BeNull();
            remainingDocumentAnswer2.Should().NotBeNull();
            
            var documentStillExists = _context.DOCUMENT_FILE.Any(d => d.Document_Id == documentId);
            documentStillExists.Should().BeTrue();
        }

        [TestMethod]
        public void DeleteDocument_WithDocumentUsedBySingleAnswer_DeletesDocumentCompletely()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            var documentAnswer = CreateTestDocumentAnswer(documentId, answerId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.Add(documentAnswer);
            _context.SaveChanges();

            // Act
            _documentBusiness.DeleteDocument(documentId, questionId, assessmentId);

            // Assert
            var documentStillExists = _context.DOCUMENT_FILE.Any(d => d.Document_Id == documentId);
            documentStillExists.Should().BeFalse();
        }

        [TestMethod]
        public void DeleteDocument_WithInvalidDocumentId_DoesNothing()
        {
            // Arrange
            var invalidDocumentId = 999;
            var questionId = 1;
            var assessmentId = 1;

            // Act
            _documentBusiness.DeleteDocument(invalidDocumentId, questionId, assessmentId);

            // Assert
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region GetQuestionsForDocument Tests

        [TestMethod]
        public void GetQuestionsForDocument_WithValidDocumentId_ReturnsQuestionIds()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            var answerId1 = 1;
            var answerId2 = 2;
            var questionId1 = 1;
            var questionId2 = 2;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer1 = CreateTestAnswer(answerId1, questionId1, assessmentId);
            var answer2 = CreateTestAnswer(answerId2, questionId2, assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            var documentAnswer1 = CreateTestDocumentAnswer(documentId, answerId1);
            var documentAnswer2 = CreateTestDocumentAnswer(documentId, answerId2);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.AddRange(answer1, answer2);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.AddRange(documentAnswer1, documentAnswer2);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetQuestionsForDocument(documentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(questionId1);
            result.Should().Contain(questionId2);
        }

        [TestMethod]
        public void GetQuestionsForDocument_WithInvalidDocumentId_ReturnsEmptyList()
        {
            // Arrange
            var invalidDocumentId = 999;

            // Act
            var result = _documentBusiness.GetQuestionsForDocument(invalidDocumentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetQuestionsForDocument_WithDocumentNotLinkedToAnswers_ReturnsEmptyList()
        {
            // Arrange
            var assessmentId = 1;
            var documentId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var document = CreateTestDocument(documentId, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(document);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetQuestionsForDocument(documentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region AddDocument Tests

        [TestMethod]
        public void AddDocument_WithNewDocument_CreatesDocumentAndLinksToAnswer()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var title = "Test Document";
            var fileName = "test.pdf";
            var fileHash = "abc123";
            var contentType = "application/pdf";
            var fileBytes = new byte[] { 1, 2, 3, 4 };
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, 1, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            var fileUploadResult = new FileUploadStreamResult
            {
                FileResultList = new List<FileUploadResult>
                {
                    new FileUploadResult
                    {
                        FileName = fileName,
                        FileHash = fileHash,
                        ContentType = contentType,
                        FileBytes = fileBytes
                    }
                }
            };

            _documentBusiness.SetUserAssessmentId(assessmentId);

            // Act
            _documentBusiness.AddDocument(title, answerId, fileUploadResult);

            // Assert
            var createdDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => 
                d.Assessment_Id == assessmentId && 
                d.Name == fileName && 
                d.FileMd5 == fileHash);
            
            createdDocument.Should().NotBeNull();
            createdDocument.Title.Should().Be(title);
            createdDocument.ContentType.Should().Be(contentType);
            createdDocument.Data.Should().BeEquivalentTo(fileBytes);
            
            var documentAnswer = _context.DOCUMENT_ANSWERS.FirstOrDefault(da => 
                da.Document_Id == createdDocument.Document_Id && 
                da.Answer_Id == answerId);
            documentAnswer.Should().NotBeNull();
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void AddDocument_WithExistingDocument_ReusesDocumentAndLinksToAnswer()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var title = "Updated Title";
            var fileName = "test.pdf";
            var fileHash = "abc123";
            var contentType = "application/pdf";
            var fileBytes = new byte[] { 1, 2, 3, 4 };
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, 1, assessmentId);
            var existingDocument = CreateTestDocument(1, assessmentId);
            existingDocument.Name = fileName;
            existingDocument.FileMd5 = fileHash;
            existingDocument.Title = "Original Title";
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(existingDocument);
            _context.SaveChanges();

            var fileUploadResult = new FileUploadStreamResult
            {
                FileResultList = new List<FileUploadResult>
                {
                    new FileUploadResult
                    {
                        FileName = fileName,
                        FileHash = fileHash,
                        ContentType = contentType,
                        FileBytes = fileBytes
                    }
                }
            };

            _documentBusiness.SetUserAssessmentId(assessmentId);

            // Act
            _documentBusiness.AddDocument(title, answerId, fileUploadResult);

            // Assert
            var documentCount = _context.DOCUMENT_FILE.Count(d => 
                d.Assessment_Id == assessmentId && 
                d.Name == fileName && 
                d.FileMd5 == fileHash);
            documentCount.Should().Be(1);
            
            var updatedDocument = _context.DOCUMENT_FILE.First(d => 
                d.Assessment_Id == assessmentId && 
                d.Name == fileName && 
                d.FileMd5 == fileHash);
            updatedDocument.Title.Should().Be(title);
            
            var documentAnswer = _context.DOCUMENT_ANSWERS.FirstOrDefault(da => 
                da.Document_Id == updatedDocument.Document_Id && 
                da.Answer_Id == answerId);
            documentAnswer.Should().NotBeNull();
        }

        [TestMethod]
        public void AddDocument_WithEmptyTitle_UsesDefaultTitle()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var emptyTitle = "";
            var fileName = "test.pdf";
            var fileHash = "abc123";
            var contentType = "application/pdf";
            var fileBytes = new byte[] { 1, 2, 3, 4 };
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, 1, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            var fileUploadResult = new FileUploadStreamResult
            {
                FileResultList = new List<FileUploadResult>
                {
                    new FileUploadResult
                    {
                        FileName = fileName,
                        FileHash = fileHash,
                        ContentType = contentType,
                        FileBytes = fileBytes
                    }
                }
            };

            _documentBusiness.SetUserAssessmentId(assessmentId);

            // Act
            _documentBusiness.AddDocument(emptyTitle, answerId, fileUploadResult);

            // Assert
            var createdDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => 
                d.Assessment_Id == assessmentId && 
                d.Name == fileName);
            
            createdDocument.Should().NotBeNull();
            createdDocument.Title.Should().Be("click to edit title");
        }

        [TestMethod]
        public void AddDocument_WithMultipleFiles_ProcessesAllFiles()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var title = "Test Documents";
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, 1, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            var fileUploadResult = new FileUploadStreamResult
            {
                FileResultList = new List<FileUploadResult>
                {
                    new FileUploadResult
                    {
                        FileName = "file1.pdf",
                        FileHash = "hash1",
                        ContentType = "application/pdf",
                        FileBytes = new byte[] { 1, 2, 3 }
                    },
                    new FileUploadResult
                    {
                        FileName = "file2.docx",
                        FileHash = "hash2",
                        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        FileBytes = new byte[] { 4, 5, 6 }
                    }
                }
            };

            _documentBusiness.SetUserAssessmentId(assessmentId);

            // Act
            _documentBusiness.AddDocument(title, answerId, fileUploadResult);

            // Assert
            var documentCount = _context.DOCUMENT_FILE.Count(d => d.Assessment_Id == assessmentId);
            documentCount.Should().Be(2);
            
            var documentAnswerCount = _context.DOCUMENT_ANSWERS.Count(da => da.Answer_Id == answerId);
            documentAnswerCount.Should().Be(2);
        }

        #endregion

        #region GetDocumentsForAssessment Tests

        [TestMethod]
        public void GetDocumentsForAssessment_WithValidAssessmentId_ReturnsDocuments()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            var document = CreateTestDocument(1, assessmentId);
            var documentAnswer = CreateTestDocumentAnswer(document.Document_Id, answerId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.Add(documentAnswer);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetDocumentsForAssessment(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Document_Id.Should().Be(document.Document_Id);
            result[0].Title.Should().Be(document.Title);
            result[0].FileName.Should().Be(document.Name);
        }

        [TestMethod]
        public void GetDocumentsForAssessment_WithGlobalDocuments_IncludesGlobalDocuments()
        {
            // Arrange
            var assessmentId = 1;
            var globalDocument = CreateTestDocument(1, assessmentId);
            globalDocument.IsGlobal = true;
            
            var assessment = CreateTestAssessment(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(globalDocument);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetDocumentsForAssessment(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Document_Id.Should().Be(globalDocument.Document_Id);
        }

        [TestMethod]
        public void GetDocumentsForAssessment_WithClickToEditTitle_ReplacesWithUntitled()
        {
            // Arrange
            var assessmentId = 1;
            var answerId = 1;
            var questionId = 1;
            
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(answerId, questionId, assessmentId);
            var document = CreateTestDocument(1, assessmentId);
            document.Title = "click to edit title";
            var documentAnswer = CreateTestDocumentAnswer(document.Document_Id, answerId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.DOCUMENT_FILE.Add(document);
            _context.DOCUMENT_ANSWERS.Add(documentAnswer);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetDocumentsForAssessment(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("(untitled)");
        }

        [TestMethod]
        public void GetDocumentsForAssessment_WithInvalidAssessmentId_ReturnsEmptyList()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _documentBusiness.GetDocumentsForAssessment(invalidAssessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetGlobalDocuments Tests

        [TestMethod]
        public void GetGlobalDocuments_WithGlobalDocuments_ReturnsGlobalDocuments()
        {
            // Arrange
            var assessmentId = 1;
            var globalDocument1 = CreateTestDocument(1, assessmentId);
            globalDocument1.IsGlobal = true;
            var globalDocument2 = CreateTestDocument(2, assessmentId);
            globalDocument2.IsGlobal = true;
            var nonGlobalDocument = CreateTestDocument(3, assessmentId);
            nonGlobalDocument.IsGlobal = false;
            
            var assessment = CreateTestAssessment(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.AddRange(globalDocument1, globalDocument2, nonGlobalDocument);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetGlobalDocuments();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(d => d.Document_Id == globalDocument1.Document_Id);
            result.Should().Contain(d => d.Document_Id == globalDocument2.Document_Id);
            result.Should().NotContain(d => d.Document_Id == nonGlobalDocument.Document_Id);
        }

        [TestMethod]
        public void GetGlobalDocuments_WithNoGlobalDocuments_ReturnsEmptyList()
        {
            // Arrange
            var assessmentId = 1;
            var nonGlobalDocument = CreateTestDocument(1, assessmentId);
            nonGlobalDocument.IsGlobal = false;
            
            var assessment = CreateTestAssessment(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(nonGlobalDocument);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetGlobalDocuments();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetGlobalDocuments_WithClickToEditTitle_ReplacesWithUntitled()
        {
            // Arrange
            var assessmentId = 1;
            var globalDocument = CreateTestDocument(1, assessmentId);
            globalDocument.IsGlobal = true;
            globalDocument.Title = "click to edit title";
            
            var assessment = CreateTestAssessment(assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DOCUMENT_FILE.Add(globalDocument);
            _context.SaveChanges();

            // Act
            var result = _documentBusiness.GetGlobalDocuments();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Title.Should().Be("(untitled)");
        }

        #endregion

        #region CopyFilesForMerge Tests

        [TestMethod]
        public void CopyFilesForMerge_WithValidDocuments_CopiesDocumentsToNewAssessment()
        {
            // Arrange
            var sourceAssessmentId = 1;
            var targetAssessmentId = 2;
            var sourceDocumentId = 1;
            var sourceAnswerId = 1;
            var targetAnswerId = 2;
            
            var sourceAssessment = CreateTestAssessment(sourceAssessmentId);
            var targetAssessment = CreateTestAssessment(targetAssessmentId);
            var sourceAnswer = CreateTestAnswer(sourceAnswerId, 1, sourceAssessmentId);
            var targetAnswer = CreateTestAnswer(targetAnswerId, 1, targetAssessmentId);
            var sourceDocument = CreateTestDocument(sourceDocumentId, sourceAssessmentId);
            
            _context.ASSESSMENTS.AddRange(sourceAssessment, targetAssessment);
            _context.ANSWER.AddRange(sourceAnswer, targetAnswer);
            _context.DOCUMENT_FILE.Add(sourceDocument);
            _context.SaveChanges();

            var documentsToMerge = new List<DocumentWithAnswerId>
            {
                new DocumentWithAnswerId
                {
                    Document_Id = sourceDocumentId,
                    Answer_Id = targetAnswerId,
                    Title = sourceDocument.Title,
                    FileName = sourceDocument.Name
                }
            };

            // Act
            _documentBusiness.CopyFilesForMerge(documentsToMerge);

            // Assert
            var copiedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => 
                d.Assessment_Id == targetAssessmentId && 
                d.Name == sourceDocument.Name && 
                d.FileMd5 == sourceDocument.FileMd5);
            
            copiedDocument.Should().NotBeNull();
            copiedDocument.Title.Should().Be(sourceDocument.Title);
            copiedDocument.ContentType.Should().Be(sourceDocument.ContentType);
            copiedDocument.Data.Should().BeEquivalentTo(sourceDocument.Data);
            
            var documentAnswer = _context.DOCUMENT_ANSWERS.FirstOrDefault(da => 
                da.Document_Id == copiedDocument.Document_Id && 
                da.Answer_Id == targetAnswerId);
            documentAnswer.Should().NotBeNull();
            
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(targetAssessmentId), Times.Once);
        }

        [TestMethod]
        public void CopyFilesForMerge_WithEmptyTitle_UsesDefaultTitle()
        {
            // Arrange
            var sourceAssessmentId = 1;
            var targetAssessmentId = 2;
            var sourceDocumentId = 1;
            var sourceAnswerId = 1;
            var targetAnswerId = 2;
            
            var sourceAssessment = CreateTestAssessment(sourceAssessmentId);
            var targetAssessment = CreateTestAssessment(targetAssessmentId);
            var sourceAnswer = CreateTestAnswer(sourceAnswerId, 1, sourceAssessmentId);
            var targetAnswer = CreateTestAnswer(targetAnswerId, 1, targetAssessmentId);
            var sourceDocument = CreateTestDocument(sourceDocumentId, sourceAssessmentId);
            sourceDocument.Title = "";
            
            _context.ASSESSMENTS.AddRange(sourceAssessment, targetAssessment);
            _context.ANSWER.AddRange(sourceAnswer, targetAnswer);
            _context.DOCUMENT_FILE.Add(sourceDocument);
            _context.SaveChanges();

            var documentsToMerge = new List<DocumentWithAnswerId>
            {
                new DocumentWithAnswerId
                {
                    Document_Id = sourceDocumentId,
                    Answer_Id = targetAnswerId,
                    Title = sourceDocument.Title,
                    FileName = sourceDocument.Name
                }
            };

            // Act
            _documentBusiness.CopyFilesForMerge(documentsToMerge);

            // Assert
            var copiedDocument = _context.DOCUMENT_FILE.FirstOrDefault(d => 
                d.Assessment_Id == targetAssessmentId);
            
            copiedDocument.Should().NotBeNull();
            copiedDocument.Title.Should().Be("click to edit title");
        }

        #endregion

        #region Helper Methods

        private ASSESSMENTS CreateTestAssessment(int assessmentId)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = assessmentId,
                Assessment_Name = $"Test Assessment {assessmentId}",
                Assessment_Date = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };
        }

        private ANSWER CreateTestAnswer(int answerId, int questionId, int assessmentId)
        {
            return new ANSWER
            {
                Answer_Id = answerId,
                Question_Or_Requirement_Id = questionId,
                Assessment_Id = assessmentId,
                Answer_Text = "Yes",
                Is_Requirement = true
            };
        }

        private DOCUMENT_FILE CreateTestDocument(int documentId, int assessmentId)
        {
            return new DOCUMENT_FILE
            {
                Document_Id = documentId,
                Assessment_Id = assessmentId,
                Title = $"Test Document {documentId}",
                Name = $"test_document_{documentId}.pdf",
                Path = $"/documents/test_document_{documentId}.pdf",
                FileMd5 = $"hash_{documentId}",
                ContentType = "application/pdf",
                CreatedTimestamp = DateTime.UtcNow,
                UpdatedTimestamp = DateTime.UtcNow,
                Data = new byte[] { 1, 2, 3, 4 },
                IsGlobal = false
            };
        }

        private DOCUMENT_ANSWERS CreateTestDocumentAnswer(int documentId, int answerId)
        {
            return new DOCUMENT_ANSWERS
            {
                Document_Id = documentId,
                Answer_Id = answerId
            };
        }

        #endregion
    }
} 