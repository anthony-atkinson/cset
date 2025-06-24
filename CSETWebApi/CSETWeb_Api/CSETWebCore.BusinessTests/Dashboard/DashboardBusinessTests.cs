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
using CSETWebCore.Business.Dashboard;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Dashboard;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Dashboard
{
    [TestClass]
    public class DashboardBusinessTests : BaseBusinessTest
    {
        private DashboardBusiness _dashboardBusiness;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateInMemoryDbContext();
            _dashboardBusiness = new DashboardBusiness(_context);
        }

        #region GetMaturityDashboardData Tests

        [TestMethod]
        public void GetMaturityDashboardData_WithValidParameters_ReturnsMaturityData()
        {
            // Arrange
            var maturityModelId = 1;
            var sectorId = 1;
            var industryId = 1;

            // Create test data for analytics views
            var assessment = CreateTestAssessment(1);
            var demographics = CreateTestDemographics(1, sectorId, industryId);
            var maturityAnswer = CreateTestMaturityAnswer(1, 1, maturityModelId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.DEMOGRAPHICS.Add(demographics);
            _context.Answer_Maturity.Add(maturityAnswer);
            _context.SaveChanges();

            // Act
            var result = _dashboardBusiness.getMaturityDashboardData(maturityModelId, sectorId, industryId);

            // Assert
            result.Should().NotBeNull();
            // Note: This test may need adjustment based on the actual analytics view implementation
        }

        [TestMethod]
        public void GetMaturityDashboardData_WithNullSectorId_ReturnsMaturityData()
        {
            // Arrange
            var maturityModelId = 1;
            int? sectorId = null;
            var industryId = 1;

            // Act
            var result = _dashboardBusiness.getMaturityDashboardData(maturityModelId, sectorId, industryId);

            // Assert
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void GetMaturityDashboardData_WithNullIndustryId_ReturnsMaturityData()
        {
            // Arrange
            var maturityModelId = 1;
            var sectorId = 1;
            int? industryId = null;

            // Act
            var result = _dashboardBusiness.getMaturityDashboardData(maturityModelId, sectorId, industryId);

            // Assert
            result.Should().NotBeNull();
        }

        #endregion

        #region GetSectors Tests

        [TestMethod]
        public async Task GetSectors_WithValidData_ReturnsSectorsWithIndustries()
        {
            // Arrange
            var sector1 = CreateTestSector(1, "Energy");
            var sector2 = CreateTestSector(2, "Healthcare");
            var industry1 = CreateTestSectorIndustry(1, 1, "Oil & Gas");
            var industry2 = CreateTestSectorIndustry(2, 1, "Renewable Energy");
            var industry3 = CreateTestSectorIndustry(3, 2, "Hospitals");
            
            _context.SECTOR.AddRange(sector1, sector2);
            _context.SECTOR_INDUSTRY.AddRange(industry1, industry2, industry3);
            _context.SaveChanges();

            // Act
            var result = await _dashboardBusiness.GetSectors();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            
            var energySector = result.FirstOrDefault(s => s.SectorId == 1);
            energySector.Should().NotBeNull();
            energySector.SectorName.Should().Be("Energy");
            energySector.Industries.Should().HaveCount(2);
            energySector.Industries.Should().Contain("Oil & Gas");
            energySector.Industries.Should().Contain("Renewable Energy");
            
            var healthcareSector = result.FirstOrDefault(s => s.SectorId == 2);
            healthcareSector.Should().NotBeNull();
            healthcareSector.SectorName.Should().Be("Healthcare");
            healthcareSector.Industries.Should().HaveCount(1);
            healthcareSector.Industries.Should().Contain("Hospitals");
        }

        [TestMethod]
        public async Task GetSectors_WithNoSectors_ReturnsEmptyList()
        {
            // Arrange - No sectors in database

            // Act
            var result = await _dashboardBusiness.GetSectors();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task GetSectors_WithSectorHavingNoIndustries_ReturnsSectorWithEmptyIndustries()
        {
            // Arrange
            var sector = CreateTestSector(1, "Test Sector");
            
            _context.SECTOR.Add(sector);
            _context.SaveChanges();

            // Act
            var result = await _dashboardBusiness.GetSectors();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].SectorId.Should().Be(1);
            result[0].SectorName.Should().Be("Test Sector");
            result[0].Industries.Should().NotBeNull();
            result[0].Industries.Should().BeEmpty();
        }

        #endregion

        #region GetSelectedIndustryList Tests

        [TestMethod]
        public void GetSelectedIndustryList_WithValidSectorId_PopulatesIndustries()
        {
            // Arrange
            var sectorId = 1;
            var industry1 = CreateTestSectorIndustry(1, sectorId, "Industry 1");
            var industry2 = CreateTestSectorIndustry(2, sectorId, "Industry 2");
            
            _context.SECTOR_INDUSTRY.AddRange(industry1, industry2);
            _context.SaveChanges();

            var sectorIndustryVM = new SectorIndustryVM
            {
                SectorId = sectorId,
                SectorName = "Test Sector",
                Industries = new List<string>()
            };

            // Act
            _dashboardBusiness.GetSelectedIndustryList(ref sectorIndustryVM);

            // Assert
            sectorIndustryVM.Industries.Should().HaveCount(2);
            sectorIndustryVM.Industries.Should().Contain("Industry 1");
            sectorIndustryVM.Industries.Should().Contain("Industry 2");
        }

        [TestMethod]
        public void GetSelectedIndustryList_WithInvalidSectorId_LeavesIndustriesEmpty()
        {
            // Arrange
            var invalidSectorId = 999;
            var sectorIndustryVM = new SectorIndustryVM
            {
                SectorId = invalidSectorId,
                SectorName = "Test Sector",
                Industries = new List<string>()
            };

            // Act
            _dashboardBusiness.GetSelectedIndustryList(ref sectorIndustryVM);

            // Assert
            sectorIndustryVM.Industries.Should().BeEmpty();
        }

        #endregion

        #region GetCategoryPercentagesTSA Tests

        [TestMethod]
        public void GetCategoryPercentagesTSA_WithValidAssessmentId_ReturnsCategoryPercentages()
        {
            // Arrange
            var assessmentId = 1;
            var assessment = CreateTestAssessment(assessmentId);
            var answer = CreateTestAnswer(1, 1, assessmentId);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            // Act
            var result = _dashboardBusiness.GetCategoryPercentagesTSA(assessmentId);

            // Assert
            result.Should().NotBeNull();
            // Note: This test may need adjustment based on the actual stored procedure implementation
        }

        [TestMethod]
        public void GetCategoryPercentagesTSA_WithInvalidAssessmentId_ReturnsNull()
        {
            // Arrange
            var invalidAssessmentId = 999;

            // Act
            var result = _dashboardBusiness.GetCategoryPercentagesTSA(invalidAssessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetDashboardData Tests

        [TestMethod]
        public void GetDashboardData_WithValidSector_ReturnsDashboardGraphData()
        {
            // Arrange
            var selectedSector = "Energy|Oil & Gas";
            var assessment = CreateTestAssessment(1);
            var answer = CreateTestAnswer(1, 1, 1);
            
            _context.ASSESSMENTS.Add(assessment);
            _context.ANSWER.Add(answer);
            _context.SaveChanges();

            // Act
            var result = _dashboardBusiness.GetDashboardData(selectedSector);

            // Assert
            result.Should().NotBeNull();
            result.BarData.Should().NotBeNull();
            result.BarData.Values.Should().NotBeNull();
            result.BarData.Labels.Should().NotBeNull();
            result.Min.Should().NotBeNull();
            result.Max.Should().NotBeNull();
            result.Median.Should().NotBeNull();
            result.sampleSize.Should().BeGreaterThanOrEqualTo(0);
        }

        [TestMethod]
        public void GetDashboardData_WithEmptySector_ReturnsDashboardGraphData()
        {
            // Arrange
            var selectedSector = "";

            // Act
            var result = _dashboardBusiness.GetDashboardData(selectedSector);

            // Assert
            result.Should().NotBeNull();
            result.BarData.Should().NotBeNull();
            result.BarData.Values.Should().NotBeNull();
            result.BarData.Labels.Should().NotBeNull();
        }

        [TestMethod]
        public void GetDashboardData_WithNullSector_ReturnsDashboardGraphData()
        {
            // Arrange
            string selectedSector = null;

            // Act
            var result = _dashboardBusiness.GetDashboardData(selectedSector);

            // Assert
            result.Should().NotBeNull();
            result.BarData.Should().NotBeNull();
            result.BarData.Values.Should().NotBeNull();
            result.BarData.Labels.Should().NotBeNull();
        }

        #endregion

        #region GetMedian Tests

        [TestMethod]
        public void GetMedian_WithOddNumberOfValues_ReturnsMiddleValue()
        {
            // Arrange
            var values = new List<double> { 1.0, 3.0, 2.0, 5.0, 4.0 };

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(3.0);
        }

        [TestMethod]
        public void GetMedian_WithEvenNumberOfValues_ReturnsAverageOfMiddleValues()
        {
            // Arrange
            var values = new List<double> { 1.0, 2.0, 3.0, 4.0 };

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(2.5);
        }

        [TestMethod]
        public void GetMedian_WithSingleValue_ReturnsThatValue()
        {
            // Arrange
            var values = new List<double> { 5.0 };

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(5.0);
        }

        [TestMethod]
        public void GetMedian_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var values = new List<double>();

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(0.0);
        }

        [TestMethod]
        public void GetMedian_WithNullList_ReturnsZero()
        {
            // Arrange
            List<double> values = null;

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(0.0);
        }

        [TestMethod]
        public void GetMedian_WithDuplicateValues_ReturnsCorrectMedian()
        {
            // Arrange
            var values = new List<double> { 1.0, 1.0, 2.0, 2.0, 3.0 };

            // Act
            var result = _dashboardBusiness.GetMedian(values);

            // Assert
            result.Should().Be(2.0);
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

        private DEMOGRAPHICS CreateTestDemographics(int assessmentId, int sectorId, int industryId)
        {
            return new DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                SectorId = sectorId,
                IndustryId = industryId,
                OrganizationName = "Test Organization",
                Agency = "Test Agency"
            };
        }

        private Answer_Maturity CreateTestMaturityAnswer(int answerId, int questionId, int modelId)
        {
            return new Answer_Maturity
            {
                Answer_Id = answerId,
                Question_Or_Requirement_Id = questionId,
                Assessment_Id = 1,
                Answer_Text = "Y",
                ModelId = modelId
            };
        }

        private ANSWER CreateTestAnswer(int answerId, int questionId, int assessmentId)
        {
            return new ANSWER
            {
                Answer_Id = answerId,
                Question_Or_Requirement_Id = questionId,
                Assessment_Id = assessmentId,
                Answer_Text = "Y",
                Is_Requirement = true
            };
        }

        private SECTOR CreateTestSector(int sectorId, string sectorName)
        {
            return new SECTOR
            {
                SectorId = sectorId,
                SectorName = sectorName
            };
        }

        private SECTOR_INDUSTRY CreateTestSectorIndustry(int industryId, int sectorId, string industryName)
        {
            return new SECTOR_INDUSTRY
            {
                IndustryId = industryId,
                SectorId = sectorId,
                IndustryName = industryName
            };
        }

        #endregion
    }
} 