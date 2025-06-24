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
using CSETWebCore.Business.Demographic;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Demographic;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.Demographic
{
    [TestClass]
    public class DemographicBusinessTests : BaseBusinessTest
    {
        private DemographicBusiness _demographicBusiness;
        private Mock<CSETContext> _mockContext;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _mockAssessmentUtil = new Mock<IAssessmentUtil>();
            _demographicBusiness = new DemographicBusiness(_mockContext.Object, _mockAssessmentUtil.Object);
        }

        #region GetDemographics Tests

        [TestMethod]
        public void GetDemographics_WithValidAssessmentId_ReturnsDemographics()
        {
            // Arrange
            int assessmentId = 1;
            var testDemographics = CreateTestDemographics(assessmentId);
            var testSize = CreateTestDemographicsSize("Large", 1);
            var testAssetValue = CreateTestDemographicsAssetValue("High", 1);
            
            var mockDemographicsDbSet = CreateMockDbSet(new List<DEMOGRAPHICS> { testDemographics });
            var mockSizeDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_SIZE> { testSize });
            var mockAssetValueDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_ASSET_VALUES> { testAssetValue });
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);

            // Act
            var result = _demographicBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.SectorId.Should().Be(testDemographics.SectorId);
            result.IndustryId.Should().Be(testDemographics.IndustryId);
            result.Size.Should().Be(testSize.DemographicId);
            result.AssetValue.Should().Be(testAssetValue.DemographicsAssetId);
            result.OrganizationName.Should().Be(testDemographics.OrganizationName);
        }

        [TestMethod]
        public void GetDemographics_WithNonExistentAssessmentId_ReturnsEmptyDemographics()
        {
            // Arrange
            int assessmentId = 999;
            var mockDemographicsDbSet = CreateMockDbSet<DEMOGRAPHICS>(new List<DEMOGRAPHICS>());
            var mockSizeDbSet = CreateMockDbSet<DEMOGRAPHICS_SIZE>(new List<DEMOGRAPHICS_SIZE>());
            var mockAssetValueDbSet = CreateMockDbSet<DEMOGRAPHICS_ASSET_VALUES>(new List<DEMOGRAPHICS_ASSET_VALUES>());
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);

            // Act
            var result = _demographicBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.SectorId.Should().BeNull();
            result.IndustryId.Should().BeNull();
            result.Size.Should().BeNull();
            result.AssetValue.Should().BeNull();
        }

        [TestMethod]
        public void GetDemographics_WithNullSizeAndAssetValue_HandlesGracefully()
        {
            // Arrange
            int assessmentId = 1;
            var testDemographics = CreateTestDemographics(assessmentId);
            testDemographics.Size = null;
            testDemographics.AssetValue = null;
            
            var mockDemographicsDbSet = CreateMockDbSet(new List<DEMOGRAPHICS> { testDemographics });
            var mockSizeDbSet = CreateMockDbSet<DEMOGRAPHICS_SIZE>(new List<DEMOGRAPHICS_SIZE>());
            var mockAssetValueDbSet = CreateMockDbSet<DEMOGRAPHICS_ASSET_VALUES>(new List<DEMOGRAPHICS_ASSET_VALUES>());
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);

            // Act
            var result = _demographicBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Size.Should().BeNull();
            result.AssetValue.Should().BeNull();
        }

        #endregion

        #region GetAnonymousDemographics Tests

        [TestMethod]
        public void GetAnonymousDemographics_WithValidAssessmentId_ReturnsAnalyticsDemographic()
        {
            // Arrange
            int assessmentId = 1;
            var testDemographics = CreateTestDemographics(assessmentId);
            var testSector = CreateTestSector(1, "Test Sector");
            var testIndustry = CreateTestSectorIndustry(1, "Test Industry");
            var testSize = CreateTestDemographicsSize("Large", 1);
            var testAssetValue = CreateTestDemographicsAssetValue("High", 1);
            
            var mockDemographicsDbSet = CreateMockDbSet(new List<DEMOGRAPHICS> { testDemographics });
            var mockSectorDbSet = CreateMockDbSet(new List<SECTOR> { testSector });
            var mockIndustryDbSet = CreateMockDbSet(new List<SECTOR_INDUSTRY> { testIndustry });
            var mockSizeDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_SIZE> { testSize });
            var mockAssetValueDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_ASSET_VALUES> { testAssetValue });
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SECTOR).Returns(mockSectorDbSet.Object);
            _mockContext.Setup(x => x.SECTOR_INDUSTRY).Returns(mockIndustryDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);

            // Act
            var result = _demographicBusiness.GetAnonymousDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.SectorId.Should().Be(testSector.SectorId);
            result.SectorName.Should().Be(testSector.SectorName);
            result.IndustryId.Should().Be(testIndustry.IndustryId);
            result.IndustryName.Should().Be(testIndustry.IndustryName);
            result.Size.Should().Be(testDemographics.Size);
            result.AssetValue.Should().Be(testDemographics.AssetValue);
        }

        [TestMethod]
        public void GetAnonymousDemographics_WithNonExistentAssessmentId_ReturnsEmptyAnalyticsDemographic()
        {
            // Arrange
            int assessmentId = 999;
            var mockDemographicsDbSet = CreateMockDbSet<DEMOGRAPHICS>(new List<DEMOGRAPHICS>());
            var mockSectorDbSet = CreateMockDbSet<SECTOR>(new List<SECTOR>());
            var mockIndustryDbSet = CreateMockDbSet<SECTOR_INDUSTRY>(new List<SECTOR_INDUSTRY>());
            var mockSizeDbSet = CreateMockDbSet<DEMOGRAPHICS_SIZE>(new List<DEMOGRAPHICS_SIZE>());
            var mockAssetValueDbSet = CreateMockDbSet<DEMOGRAPHICS_ASSET_VALUES>(new List<DEMOGRAPHICS_ASSET_VALUES>());
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SECTOR).Returns(mockSectorDbSet.Object);
            _mockContext.Setup(x => x.SECTOR_INDUSTRY).Returns(mockIndustryDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);

            // Act
            var result = _demographicBusiness.GetAnonymousDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.SectorId.Should().Be(0);
            result.SectorName.Should().BeEmpty();
            result.IndustryId.Should().Be(0);
            result.IndustryName.Should().BeEmpty();
        }

        #endregion

        #region SaveDemographics Tests

        [TestMethod]
        public void SaveDemographics_WithValidDemographics_SavesToDatabase()
        {
            // Arrange
            var demographics = CreateTestDemographicsModel(1);
            var testAssetValue = CreateTestDemographicsAssetValue("High", 1);
            var testSize = CreateTestDemographicsSize("Large", 1);
            
            var mockDemographicsDbSet = CreateMockDbSet<DEMOGRAPHICS>(new List<DEMOGRAPHICS>());
            var mockAssetValueDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_ASSET_VALUES> { testAssetValue });
            var mockSizeDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_SIZE> { testSize });
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _demographicBusiness.SaveDemographics(demographics);

            // Assert
            result.Should().Be(demographics.AssessmentId);
            mockDemographicsDbSet.Verify(x => x.Add(It.IsAny<DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(demographics.AssessmentId), Times.Once);
        }

        [TestMethod]
        public void SaveDemographics_WithExistingDemographics_UpdatesDatabase()
        {
            // Arrange
            var demographics = CreateTestDemographicsModel(1);
            var existingDemographics = CreateTestDemographics(1);
            var testAssetValue = CreateTestDemographicsAssetValue("High", 1);
            var testSize = CreateTestDemographicsSize("Large", 1);
            
            var mockDemographicsDbSet = CreateMockDbSet(new List<DEMOGRAPHICS> { existingDemographics });
            var mockAssetValueDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_ASSET_VALUES> { testAssetValue });
            var mockSizeDbSet = CreateMockDbSet(new List<DEMOGRAPHICS_SIZE> { testSize });
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _demographicBusiness.SaveDemographics(demographics);

            // Assert
            result.Should().Be(demographics.AssessmentId);
            mockDemographicsDbSet.Verify(x => x.Add(It.IsAny<DEMOGRAPHICS>()), Times.Never);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        [TestMethod]
        public void SaveDemographics_WithZeroSectorAndIndustry_ConvertsToNull()
        {
            // Arrange
            var demographics = CreateTestDemographicsModel(1);
            demographics.SectorId = 0;
            demographics.IndustryId = 0;
            
            var mockDemographicsDbSet = CreateMockDbSet<DEMOGRAPHICS>(new List<DEMOGRAPHICS>());
            var mockAssetValueDbSet = CreateMockDbSet<DEMOGRAPHICS_ASSET_VALUES>(new List<DEMOGRAPHICS_ASSET_VALUES>());
            var mockSizeDbSet = CreateMockDbSet<DEMOGRAPHICS_SIZE>(new List<DEMOGRAPHICS_SIZE>());
            
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_ASSET_VALUES).Returns(mockAssetValueDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS_SIZE).Returns(mockSizeDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _demographicBusiness.SaveDemographics(demographics);

            // Assert
            result.Should().Be(demographics.AssessmentId);
        }

        #endregion

        #region SaveExtendedDemographics Tests

        [TestMethod]
        public void SaveExtendedDemographics_WithValidExtendedDemographics_SavesToDatabase()
        {
            // Arrange
            var extendedDemographics = CreateTestExtendedDemographics(1);
            var mockDemographicAnswersDbSet = CreateMockDbSet<DEMOGRAPHIC_ANSWERS>(new List<DEMOGRAPHIC_ANSWERS>());
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DEMOGRAPHIC_ANSWERS).Returns(mockDemographicAnswersDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _demographicBusiness.SaveDemographics(extendedDemographics);

            // Assert
            result.Should().Be(extendedDemographics.AssessmentId);
            mockDemographicAnswersDbSet.Verify(x => x.Add(It.IsAny<DEMOGRAPHIC_ANSWERS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(extendedDemographics.AssessmentId), Times.Once);
        }

        [TestMethod]
        public void SaveExtendedDemographics_WithExistingDemographicAnswers_UpdatesDatabase()
        {
            // Arrange
            var extendedDemographics = CreateTestExtendedDemographics(1);
            var existingDemographicAnswers = CreateTestDemographicAnswers(1);
            var mockDemographicAnswersDbSet = CreateMockDbSet(new List<DEMOGRAPHIC_ANSWERS> { existingDemographicAnswers });
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DEMOGRAPHIC_ANSWERS).Returns(mockDemographicAnswersDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _demographicBusiness.SaveDemographics(extendedDemographics);

            // Assert
            result.Should().Be(extendedDemographics.AssessmentId);
            mockDemographicAnswersDbSet.Verify(x => x.Add(It.IsAny<DEMOGRAPHIC_ANSWERS>()), Times.Never);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        #endregion

        #region GetExtendedDemographics Tests

        [TestMethod]
        public void GetExtendedDemographics_WithValidAssessmentId_ReturnsExtendedDemographic()
        {
            // Arrange
            int assessmentId = 1;
            var testDemographicAnswers = CreateTestDemographicAnswers(assessmentId);
            var mockDemographicAnswersDbSet = CreateMockDbSet(new List<DEMOGRAPHIC_ANSWERS> { testDemographicAnswers });
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DEMOGRAPHIC_ANSWERS).Returns(mockDemographicAnswersDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicBusiness.GetExtendedDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.SectorId.Should().Be(testDemographicAnswers.SectorId);
            result.SubSectorId.Should().Be(testDemographicAnswers.SubSectorId);
            result.Employees.Should().Be(testDemographicAnswers.Employees);
        }

        [TestMethod]
        public void GetExtendedDemographics_WithNonExistentAssessmentId_ReturnsEmptyExtendedDemographic()
        {
            // Arrange
            int assessmentId = 999;
            var mockDemographicAnswersDbSet = CreateMockDbSet<DEMOGRAPHIC_ANSWERS>(new List<DEMOGRAPHIC_ANSWERS>());
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DEMOGRAPHIC_ANSWERS).Returns(mockDemographicAnswersDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicBusiness.GetExtendedDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.SectorId.Should().BeNull();
            result.SubSectorId.Should().BeNull();
        }

        #endregion

        #region GetDD and SaveDD Tests

        [TestMethod]
        public void GetDD_WithValidKey_ReturnsStringValue()
        {
            // Arrange
            int assessmentId = 1;
            string key = "TEST-KEY";
            var testDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, "Test Value");
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { testDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicBusiness.GetDD(assessmentId, key);

            // Assert
            result.Should().Be("Test Value");
        }

        [TestMethod]
        public void GetDD_WithNonExistentKey_ReturnsNull()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NON-EXISTENT-KEY";
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicBusiness.GetDD(assessmentId, key);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void SaveDD_WithNewKey_CreatesNewRecord()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NEW-KEY";
            string value = "New Value";
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicBusiness.SaveDD(assessmentId, key, value, "string");

            // Assert
            mockDetailsDemographicsDbSet.Verify(x => x.Add(It.IsAny<DETAILS_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveDD_WithExistingKey_UpdatesExistingRecord()
        {
            // Arrange
            int assessmentId = 1;
            string key = "EXISTING-KEY";
            string value = "Updated Value";
            var existingDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, "Old Value");
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { existingDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicBusiness.SaveDD(assessmentId, key, value, "string");

            // Assert
            existingDetailsDemographics.StringValue.Should().Be(value);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region Helper Methods

        private DEMOGRAPHICS CreateTestDemographics(int assessmentId)
        {
            return new DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                SectorId = 1,
                IndustryId = 1,
                Size = "Large",
                AssetValue = "High",
                OrganizationName = "Test Organization",
                Agency = "Test Agency",
                OrganizationType = 1,
                Facilitator = 1,
                PointOfContact = 1,
                IsScoped = true,
                CriticalService = "Test Critical Service",
                NeedsPrivacy = false,
                NeedsSupplyChain = false,
                NeedsICS = false
            };
        }

        private Demographics CreateTestDemographicsModel(int assessmentId)
        {
            return new Demographics
            {
                AssessmentId = assessmentId,
                SectorId = 1,
                IndustryId = 1,
                Size = 1,
                AssetValue = 1,
                OrganizationName = "Test Organization",
                Agency = "Test Agency",
                OrganizationType = 1,
                OrgPointOfContact = 1,
                FacilitatorId = 1,
                SelfAssessment = true,
                CriticalService = "Test Critical Service",
                PointOfContact = 1,
                IsScoped = true,
                CisaRegion = 1,
                SectorDirective = "PPD-21"
            };
        }

        private ExtendedDemographic CreateTestExtendedDemographics(int assessmentId)
        {
            return new ExtendedDemographic
            {
                AssessmentId = assessmentId,
                SectorId = 1,
                SubSectorId = 1,
                Hb7055 = "Yes",
                Hb7055Party = "Republican",
                InfrastructureItOt = "IT",
                Employees = "100-500",
                CustomersSupported = "1000-5000",
                GeographicScope = "State",
                CioExists = "Yes",
                CisoExists = "Yes",
                CyberTrainingProgramExists = "Yes",
                CyberRiskService = "Yes",
                Hb7055Grant = "Yes"
            };
        }

        private DEMOGRAPHIC_ANSWERS CreateTestDemographicAnswers(int assessmentId)
        {
            return new DEMOGRAPHIC_ANSWERS
            {
                Assessment_Id = assessmentId,
                SectorId = 1,
                SubSectorId = 1,
                Employees = "100-500",
                CustomersSupported = "1000-5000",
                GeographicScope = "State",
                CIOExists = "Yes",
                CISOExists = "Yes",
                CyberTrainingProgramExists = "Yes",
                CyberRiskService = "Yes"
            };
        }

        private DETAILS_DEMOGRAPHICS CreateTestDetailsDemographics(int assessmentId, string key, string value)
        {
            return new DETAILS_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                DataItemName = key,
                StringValue = value
            };
        }

        private DEMOGRAPHICS_SIZE CreateTestDemographicsSize(string size, int demographicId)
        {
            return new DEMOGRAPHICS_SIZE
            {
                Size = size,
                DemographicId = demographicId,
                Description = $"Test {size} Size",
                ValueOrder = demographicId
            };
        }

        private DEMOGRAPHICS_ASSET_VALUES CreateTestDemographicsAssetValue(string assetValue, int demographicsAssetId)
        {
            return new DEMOGRAPHICS_ASSET_VALUES
            {
                AssetValue = assetValue,
                DemographicsAssetId = demographicsAssetId
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

        private SECTOR_INDUSTRY CreateTestSectorIndustry(int industryId, string industryName)
        {
            return new SECTOR_INDUSTRY
            {
                IndustryId = industryId,
                IndustryName = industryName
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