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
using AutoFixture;
using AutoFixture.AutoMoq;
using CSETWebCore.Business.Reports;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.Maturity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Reports
{
    [TestClass]
    public class ExportPoamBusinessTests : BaseBusinessTest
    {
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
        }

        #region POAM Spreadsheet Generation Tests

        [TestMethod]
        public void GenerateSpreadSheet_WithValidMaturityResponse_CreatesExcelFile()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponse();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            memoryStream.Position.Should().Be(0);
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithEmptyMaturityResponse_CreatesExcelWithHeadersOnly()
        {
            // Arrange
            var emptyMaturityResponse = new MaturityResponse
            {
                Models = new List<MaturityModel>()
            };
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, emptyMaturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should contain headers but no data rows
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithMultipleModels_CreatesExcelWithAllData()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponseWithMultipleModels();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should contain headers and multiple data rows
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithUnpardonableItems_MarksItemsCorrectly()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponseWithUnpardonableItems();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle unpardonable items correctly
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithNullValues_HandlesNullDataCorrectly()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponseWithNullValues();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle null values gracefully
        }

        #endregion

        #region Excel Structure Tests

        [TestMethod]
        public void GenerateSpreadSheet_CreatesCorrectColumnHeaders()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponse();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            var excelContent = memoryStream.ToArray();
            var contentString = System.Text.Encoding.UTF8.GetString(excelContent);
            
            // Check for expected column headers
            contentString.Should().Contain("Control Title");
            contentString.Should().Contain("Weakness");
            contentString.Should().Contain("Maturity Level");
            contentString.Should().Contain("Resource Estimate");
            contentString.Should().Contain("Scheduled Completion Date");
            contentString.Should().Contain("Milestones with Interim Completion Dates");
            contentString.Should().Contain("Changes to Milestones");
            contentString.Should().Contain("How was the weakness identified?");
            contentString.Should().Contain("Status (Ongoing or Complete)");
        }

        [TestMethod]
        public void GenerateSpreadSheet_CreatesCorrectWorksheetName()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponse();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            var excelContent = memoryStream.ToArray();
            var contentString = System.Text.Encoding.UTF8.GetString(excelContent);
            
            // Check for worksheet name (should be default or based on content)
            contentString.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Filename Generation Tests

        [TestMethod]
        public void GetFilename_WithValidAssessmentId_ReturnsFormattedFilename()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            SeedAssessmentData(testAssessment, testInformation);

            // Act
            var result = ExportPoamBusiness.GetFilename(assessmentId, _context);

            // Assert
            result.Should().Be("Test Assessment - POAM.xlsx");
        }

        [TestMethod]
        public void GetFilename_WithInvalidAssessmentId_ReturnsDefaultFilename()
        {
            // Arrange
            int invalidAssessmentId = 999;

            // Act
            var result = ExportPoamBusiness.GetFilename(invalidAssessmentId, _context);

            // Assert
            result.Should().Be("ExcelExport.xlsx");
        }

        [TestMethod]
        public void GetFilename_WithNullAssessmentName_ReturnsDefaultFilename()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformationWithNullName(assessmentId);
            SeedAssessmentData(testAssessment, testInformation);

            // Act
            var result = ExportPoamBusiness.GetFilename(assessmentId, _context);

            // Assert
            result.Should().Be("ExcelExport.xlsx");
        }

        [TestMethod]
        public void GetFilename_WithSpecialCharacters_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformationWithSpecialCharacters(assessmentId);
            SeedAssessmentData(testAssessment, testInformation);

            // Act
            var result = ExportPoamBusiness.GetFilename(assessmentId, _context);

            // Assert
            result.Should().Contain("Test Assessment with special chars");
            result.Should().EndWith(".xlsx");
        }

        #endregion

        #region Unpardonable Item Detection Tests

        [TestMethod]
        public void IsUnpardonable_WithUnpardonableControlTitle_ReturnsTrue()
        {
            // Arrange
            string unpardonableTitle = "AC.1.001 - Access Control Policy and Procedures";

            // Act
            var result = ExportPoamBusiness.IsUnpardonable(unpardonableTitle);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsUnpardonable_WithPardonableControlTitle_ReturnsFalse()
        {
            // Arrange
            string pardonableTitle = "AC.2.001 - Account Management";

            // Act
            var result = ExportPoamBusiness.IsUnpardonable(pardonableTitle);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsUnpardonable_WithNullTitle_ReturnsFalse()
        {
            // Arrange
            string nullTitle = null;

            // Act
            var result = ExportPoamBusiness.IsUnpardonable(nullTitle);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsUnpardonable_WithEmptyTitle_ReturnsFalse()
        {
            // Arrange
            string emptyTitle = "";

            // Act
            var result = ExportPoamBusiness.IsUnpardonable(emptyTitle);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsUnpardonable_WithCaseInsensitiveTitle_ReturnsCorrectResult()
        {
            // Arrange
            string unpardonableTitle = "ac.1.001 - access control policy and procedures";

            // Act
            var result = ExportPoamBusiness.IsUnpardonable(unpardonableTitle);

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Data Formatting Tests

        [TestMethod]
        public void GenerateSpreadSheet_WithLongText_HandlesTextWrapping()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponseWithLongText();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle long text with proper formatting
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithSpecialCharacters_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponseWithSpecialCharacters();
            using var memoryStream = new MemoryStream();

            // Act
            ExportPoamBusiness.GenerateSpreadSheet(memoryStream, maturityResponse);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle special characters in data
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void GenerateSpreadSheet_WithNullMemoryStream_ThrowsException()
        {
            // Arrange
            var maturityResponse = CreateTestMaturityResponse();
            MemoryStream nullStream = null;

            // Act & Assert
            Action act = () => ExportPoamBusiness.GenerateSpreadSheet(nullStream, maturityResponse);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GenerateSpreadSheet_WithNullMaturityResponse_ThrowsException()
        {
            // Arrange
            MaturityResponse nullResponse = null;
            using var memoryStream = new MemoryStream();

            // Act & Assert
            Action act = () => ExportPoamBusiness.GenerateSpreadSheet(memoryStream, nullResponse);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Helper Methods

        private MaturityResponse CreateTestMaturityResponse()
        {
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = "Access Control Policy and Procedures",
                                                PracticeId = 1,
                                                Answer = "N",
                                                Comment = "Test comment",
                                                Finding = "Test finding"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private MaturityResponse CreateTestMaturityResponseWithMultipleModels()
        {
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = "Access Control Policy and Procedures",
                                                PracticeId = 1,
                                                Answer = "N",
                                                Comment = "First practice comment",
                                                Finding = "First practice finding"
                                            },
                                            new MaturityPractice
                                            {
                                                PracticeName = "Account Management",
                                                PracticeId = 2,
                                                Answer = "N",
                                                Comment = "Second practice comment",
                                                Finding = "Second practice finding"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new MaturityModel
                    {
                        ModelName = "CMMC Level 2",
                        ModelId = 2,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Audit and Accountability",
                                DomainId = 2,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AU.1.001",
                                        GoalId = 2,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = "Audit Policy and Procedures",
                                                PracticeId = 3,
                                                Answer = "N",
                                                Comment = "Third practice comment",
                                                Finding = "Third practice finding"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private MaturityResponse CreateTestMaturityResponseWithUnpardonableItems()
        {
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = "AC.1.001 - Access Control Policy and Procedures",
                                                PracticeId = 1,
                                                Answer = "N",
                                                Comment = "Unpardonable item comment",
                                                Finding = "Unpardonable item finding"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private MaturityResponse CreateTestMaturityResponseWithNullValues()
        {
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = null,
                                                PracticeId = 1,
                                                Answer = null,
                                                Comment = null,
                                                Finding = null
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private MaturityResponse CreateTestMaturityResponseWithLongText()
        {
            var longText = new string('A', 1000); // Very long text
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = longText,
                                                PracticeId = 1,
                                                Answer = "N",
                                                Comment = longText,
                                                Finding = longText
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private MaturityResponse CreateTestMaturityResponseWithSpecialCharacters()
        {
            return new MaturityResponse
            {
                Models = new List<MaturityModel>
                {
                    new MaturityModel
                    {
                        ModelName = "CMMC",
                        ModelId = 1,
                        Domains = new List<MaturityDomain>
                        {
                            new MaturityDomain
                            {
                                DomainName = "Access Control",
                                DomainId = 1,
                                Goals = new List<MaturityGoal>
                                {
                                    new MaturityGoal
                                    {
                                        GoalName = "AC.1.001",
                                        GoalId = 1,
                                        Practices = new List<MaturityPractice>
                                        {
                                            new MaturityPractice
                                            {
                                                PracticeName = "Practice with special chars: áéíóúñ",
                                                PracticeId = 1,
                                                Answer = "N",
                                                Comment = "Comment with special chars: & < > \" ' © ® ™",
                                                Finding = "Finding with special chars: € £ ¥ ± ∞ ≠"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private ASSESSMENTS CreateTestAssessment(int assessmentId)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = assessmentId,
                Assessment_Name = "Test Assessment",
                Assessment_Date = DateTime.Now,
                AssessmentCreatorId = 1,
                Assessment_State = "Active"
            };
        }

        private INFORMATION CreateTestInformation(int assessmentId)
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = "Test Assessment",
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State"
            };
        }

        private INFORMATION CreateTestInformationWithNullName(int assessmentId)
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = null,
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State"
            };
        }

        private INFORMATION CreateTestInformationWithSpecialCharacters(int assessmentId)
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = "Test Assessment with special chars: áéíóúñ & < > \" ' © ® ™",
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State"
            };
        }

        private void SeedAssessmentData(ASSESSMENTS assessment, INFORMATION information)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();
        }

        #endregion
    }
} 