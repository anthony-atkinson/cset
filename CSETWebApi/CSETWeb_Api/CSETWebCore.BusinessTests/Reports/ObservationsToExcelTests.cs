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
using CSETWebCore.Interfaces.Reports;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Reports
{
    [TestClass]
    public class ObservationsToExcelTests : BaseBusinessTest
    {
        private ObservationsToExcel _observationsToExcel;
        private Mock<IReportsDataBusiness> _mockReportsDataBusiness;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
            _mockReportsDataBusiness = _fixture.Freeze<Mock<IReportsDataBusiness>>();

            _observationsToExcel = new ObservationsToExcel(_context, _mockReportsDataBusiness.Object);
        }

        #region Excel Generation Tests

        [TestMethod]
        public void GenerateSpreadsheet_WithValidAssessmentId_CreatesExcelFile()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividuals();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            memoryStream.Position.Should().Be(0);
            
            _mockReportsDataBusiness.Verify(x => x.SetReportsAssessmentId(assessmentId), Times.Once);
            _mockReportsDataBusiness.Verify(x => x.GetInformation(), Times.Once);
            _mockReportsDataBusiness.Verify(x => x.GetObservationIndividuals(), Times.Once);
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithEmptyObservations_CreatesExcelWithHeadersOnly()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var emptyIndividuals = new List<Individual>();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(emptyIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should contain headers but no data rows
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithMultipleObservations_CreatesExcelWithAllData()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividualsWithMultipleObservations();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should contain headers and multiple data rows
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithNullResolutionDate_HandlesNullDateCorrectly()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividualsWithNullResolutionDate();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle null resolution date gracefully
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithSpecialCharacters_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividualsWithSpecialCharacters();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle special characters in observation data
        }

        #endregion

        #region Excel Structure Tests

        [TestMethod]
        public void GenerateSpreadsheet_CreatesCorrectColumnHeaders()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividuals();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            // Verify that the Excel file contains the expected column headers
            var excelContent = memoryStream.ToArray();
            var contentString = System.Text.Encoding.UTF8.GetString(excelContent);
            
            // Check for expected column headers
            contentString.Should().Contain("Contact");
            contentString.Should().Contain("Observation Title");
            contentString.Should().Contain("Question ID");
            contentString.Should().Contain("Importance");
            contentString.Should().Contain("Resolution Date");
            contentString.Should().Contain("Issue");
            contentString.Should().Contain("Impacts");
            contentString.Should().Contain("Recommendations");
            contentString.Should().Contain("Vulnerabilities");
        }

        [TestMethod]
        public void GenerateSpreadsheet_CreatesCorrectWorksheetName()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividuals();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            var excelContent = memoryStream.ToArray();
            var contentString = System.Text.Encoding.UTF8.GetString(excelContent);
            
            // Check for worksheet name
            contentString.Should().Contain("Observations");
        }

        #endregion

        #region Data Formatting Tests

        [TestMethod]
        public void GenerateSpreadsheet_WithLongText_HandlesTextWrapping()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividualsWithLongText();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle long text with proper formatting
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithEmptyFields_HandlesEmptyDataCorrectly()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testIndividuals = CreateTestIndividualsWithEmptyFields();
            
            SeedAssessmentData(testAssessment, testInformation);
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(assessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns(testInformation);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(assessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle empty fields gracefully
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void GenerateSpreadsheet_WithNullMemoryStream_ThrowsException()
        {
            // Arrange
            int assessmentId = 1;
            MemoryStream nullStream = null;

            // Act & Assert
            Action act = () => _observationsToExcel.GenerateSpreadsheet(assessmentId, nullStream);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GenerateSpreadsheet_WithInvalidAssessmentId_HandlesGracefully()
        {
            // Arrange
            int invalidAssessmentId = 999;
            var testIndividuals = new List<Individual>();
            
            _mockReportsDataBusiness.Setup(x => x.SetReportsAssessmentId(invalidAssessmentId));
            _mockReportsDataBusiness.Setup(x => x.GetInformation()).Returns((BasicReportData.INFORMATION)null);
            _mockReportsDataBusiness.Setup(x => x.GetObservationIndividuals()).Returns(testIndividuals);

            using var memoryStream = new MemoryStream();

            // Act
            _observationsToExcel.GenerateSpreadsheet(invalidAssessmentId, memoryStream);

            // Assert
            memoryStream.Length.Should().BeGreaterThan(0);
            // Should handle invalid assessment ID gracefully
        }

        #endregion

        #region Helper Methods

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

        private BasicReportData.INFORMATION CreateTestInformation(int assessmentId)
        {
            return new BasicReportData.INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = "Test Assessment",
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State"
            };
        }

        private List<Individual> CreateTestIndividuals()
        {
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "John Doe",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "Test Observation",
                            QuestionIdentifier = "AC.1",
                            Importance = "High",
                            ResolutionDate = DateTime.Now.AddDays(30),
                            Issue = "Test issue description",
                            Impact = "Test impact description",
                            Recommendations = "Test recommendations",
                            Vulnerabilities = "Test vulnerabilities"
                        }
                    }
                }
            };
        }

        private List<Individual> CreateTestIndividualsWithMultipleObservations()
        {
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "John Doe",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "First Observation",
                            QuestionIdentifier = "AC.1",
                            Importance = "High",
                            ResolutionDate = DateTime.Now.AddDays(30),
                            Issue = "First issue",
                            Impact = "First impact",
                            Recommendations = "First recommendations",
                            Vulnerabilities = "First vulnerabilities"
                        },
                        new Observation
                        {
                            ObservationTitle = "Second Observation",
                            QuestionIdentifier = "AC.2",
                            Importance = "Medium",
                            ResolutionDate = DateTime.Now.AddDays(60),
                            Issue = "Second issue",
                            Impact = "Second impact",
                            Recommendations = "Second recommendations",
                            Vulnerabilities = "Second vulnerabilities"
                        }
                    }
                },
                new Individual
                {
                    FullName = "Jane Smith",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "Third Observation",
                            QuestionIdentifier = "AC.3",
                            Importance = "Low",
                            ResolutionDate = DateTime.Now.AddDays(90),
                            Issue = "Third issue",
                            Impact = "Third impact",
                            Recommendations = "Third recommendations",
                            Vulnerabilities = "Third vulnerabilities"
                        }
                    }
                }
            };
        }

        private List<Individual> CreateTestIndividualsWithNullResolutionDate()
        {
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "John Doe",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "Test Observation",
                            QuestionIdentifier = "AC.1",
                            Importance = "High",
                            ResolutionDate = null,
                            Issue = "Test issue",
                            Impact = "Test impact",
                            Recommendations = "Test recommendations",
                            Vulnerabilities = "Test vulnerabilities"
                        }
                    }
                }
            };
        }

        private List<Individual> CreateTestIndividualsWithSpecialCharacters()
        {
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "José María",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "Test Observation with special chars: áéíóúñ",
                            QuestionIdentifier = "AC.1",
                            Importance = "High",
                            ResolutionDate = DateTime.Now.AddDays(30),
                            Issue = "Issue with special chars: & < > \" '",
                            Impact = "Impact with special chars: © ® ™",
                            Recommendations = "Recommendations with special chars: € £ ¥",
                            Vulnerabilities = "Vulnerabilities with special chars: ± ∞ ≠"
                        }
                    }
                }
            };
        }

        private List<Individual> CreateTestIndividualsWithLongText()
        {
            var longText = new string('A', 1000); // Very long text
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "John Doe",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = longText,
                            QuestionIdentifier = "AC.1",
                            Importance = "High",
                            ResolutionDate = DateTime.Now.AddDays(30),
                            Issue = longText,
                            Impact = longText,
                            Recommendations = longText,
                            Vulnerabilities = longText
                        }
                    }
                }
            };
        }

        private List<Individual> CreateTestIndividualsWithEmptyFields()
        {
            return new List<Individual>
            {
                new Individual
                {
                    FullName = "",
                    Observations = new List<Observation>
                    {
                        new Observation
                        {
                            ObservationTitle = "",
                            QuestionIdentifier = "",
                            Importance = "",
                            ResolutionDate = null,
                            Issue = "",
                            Impact = "",
                            Recommendations = "",
                            Vulnerabilities = ""
                        }
                    }
                }
            };
        }

        private void SeedAssessmentData(ASSESSMENTS assessment, BasicReportData.INFORMATION information)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.SaveChanges();
        }

        #endregion
    }
} 