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
using CSETWebCore.Model.Demographic;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.Demographic
{
    [TestClass]
    public class DemographicExtBusinessTests : BaseBusinessTest
    {
        private DemographicExtBusiness _demographicExtBusiness;
        private Mock<CSETContext> _mockContext;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _demographicExtBusiness = new DemographicExtBusiness(_mockContext.Object);
        }

        #region GetDemographics Tests

        [TestMethod]
        public void GetDemographics_WithValidAssessmentId_ReturnsDemographicExt()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testDetailsDemographics = CreateTestDetailsDemographicsList(assessmentId);
            var testOptions = CreateTestDetailsDemographicsOptions();
            
            var mockAssessmentDbSet = CreateMockDbSet(new List<ASSESSMENTS> { testAssessment });
            var mockInformationDbSet = CreateMockDbSet(new List<INFORMATION> { testInformation });
            var mockDetailsDemographicsDbSet = CreateMockDbSet(testDetailsDemographics);
            var mockOptionsDbSet = CreateMockDbSet(testOptions);
            
            _mockContext.Setup(x => x.ASSESSMENTS).Returns(mockAssessmentDbSet.Object);
            _mockContext.Setup(x => x.INFORMATION).Returns(mockInformationDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS_OPTIONS).Returns(mockOptionsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.AssessmentDate.Should().Be(testAssessment.Assessment_Date);
            result.OrganizationName.Should().Be(testInformation.Facility_Name);
            result.OrganizationType.Should().Be(1);
            result.Sector.Should().Be(1);
            result.Subsector.Should().Be(1);
            result.CisaRegion.Should().Be(1);
        }

        [TestMethod]
        public void GetDemographics_WithNonExistentAssessmentId_ReturnsEmptyDemographicExt()
        {
            // Arrange
            int assessmentId = 999;
            var mockAssessmentDbSet = CreateMockDbSet<ASSESSMENTS>(new List<ASSESSMENTS>());
            var mockInformationDbSet = CreateMockDbSet<INFORMATION>(new List<INFORMATION>());
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            var mockOptionsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS_OPTIONS>(new List<DETAILS_DEMOGRAPHICS_OPTIONS>());
            
            _mockContext.Setup(x => x.ASSESSMENTS).Returns(mockAssessmentDbSet.Object);
            _mockContext.Setup(x => x.INFORMATION).Returns(mockInformationDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS_OPTIONS).Returns(mockOptionsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.OrganizationType.Should().BeNull();
            result.Sector.Should().BeNull();
            result.Subsector.Should().BeNull();
        }

        [TestMethod]
        public void GetDemographics_WithSector_ReturnsSubsectors()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            var testDetailsDemographics = CreateTestDetailsDemographicsList(assessmentId);
            var testOptions = CreateTestDetailsDemographicsOptions();
            var testSubsectors = CreateTestSubsectors(1);
            
            var mockAssessmentDbSet = CreateMockDbSet(new List<ASSESSMENTS> { testAssessment });
            var mockInformationDbSet = CreateMockDbSet(new List<INFORMATION> { testInformation });
            var mockDetailsDemographicsDbSet = CreateMockDbSet(testDetailsDemographics);
            var mockOptionsDbSet = CreateMockDbSet(testOptions);
            var mockSubsectorsDbSet = CreateMockDbSet(testSubsectors);
            
            _mockContext.Setup(x => x.ASSESSMENTS).Returns(mockAssessmentDbSet.Object);
            _mockContext.Setup(x => x.INFORMATION).Returns(mockInformationDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS_OPTIONS).Returns(mockOptionsDbSet.Object);
            _mockContext.Setup(x => x.SECTOR_INDUSTRY).Returns(mockSubsectorsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.ListSubsectors.Should().NotBeNull();
            result.ListSubsectors.Should().HaveCount(testSubsectors.Count);
        }

        #endregion

        #region GetX Tests

        [TestMethod]
        public void GetX_WithValidKey_ReturnsStringValue()
        {
            // Arrange
            int assessmentId = 1;
            string key = "TEST-KEY";
            var testDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, "Test Value");
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { testDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetX(assessmentId, key);

            // Assert
            result.Should().Be("Test Value");
        }

        [TestMethod]
        public void GetX_WithValidKey_ReturnsIntValue()
        {
            // Arrange
            int assessmentId = 1;
            string key = "TEST-KEY";
            var testDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, intValue: 42);
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { testDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetX(assessmentId, key);

            // Assert
            result.Should().Be(42);
        }

        [TestMethod]
        public void GetX_WithValidKey_ReturnsBoolValue()
        {
            // Arrange
            int assessmentId = 1;
            string key = "TEST-KEY";
            var testDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, boolValue: true);
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { testDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetX(assessmentId, key);

            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        public void GetX_WithNonExistentKey_ReturnsNull()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NON-EXISTENT-KEY";
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetX(assessmentId, key);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region SaveX Tests

        [TestMethod]
        public void SaveX_WithStringValue_CreatesNewRecord()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NEW-KEY";
            string value = "New Value";
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveX(assessmentId, key, value);

            // Assert
            mockDetailsDemographicsDbSet.Verify(x => x.Add(It.IsAny<DETAILS_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveX_WithIntValue_CreatesNewRecord()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NEW-KEY";
            int value = 42;
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveX(assessmentId, key, value);

            // Assert
            mockDetailsDemographicsDbSet.Verify(x => x.Add(It.IsAny<DETAILS_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveX_WithBoolValue_CreatesNewRecord()
        {
            // Arrange
            int assessmentId = 1;
            string key = "NEW-KEY";
            bool value = true;
            var mockDetailsDemographicsDbSet = CreateMockDbSet<DETAILS_DEMOGRAPHICS>(new List<DETAILS_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveX(assessmentId, key, value);

            // Assert
            mockDetailsDemographicsDbSet.Verify(x => x.Add(It.IsAny<DETAILS_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveX_WithExistingKey_UpdatesExistingRecord()
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
            _demographicExtBusiness.SaveX(assessmentId, key, value);

            // Assert
            existingDetailsDemographics.StringValue.Should().Be(value);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveX_WithNullValue_ClearsAllFields()
        {
            // Arrange
            int assessmentId = 1;
            string key = "EXISTING-KEY";
            var existingDetailsDemographics = CreateTestDetailsDemographics(assessmentId, key, "Old Value", 42, true);
            var mockDetailsDemographicsDbSet = CreateMockDbSet(new List<DETAILS_DEMOGRAPHICS> { existingDetailsDemographics });
            
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveX(assessmentId, key, null);

            // Assert
            existingDetailsDemographics.StringValue.Should().BeNull();
            existingDetailsDemographics.IntValue.Should().BeNull();
            existingDetailsDemographics.BoolValue.Should().BeNull();
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region SaveDemographics Tests

        [TestMethod]
        public void SaveDemographics_WithValidDemographicExt_SavesToDatabase()
        {
            // Arrange
            var demographicExt = CreateTestDemographicExt(1);
            var testInformation = CreateTestInformation(1);
            var existingDetailsDemographics = new List<DETAILS_DEMOGRAPHICS>();
            
            var mockInformationDbSet = CreateMockDbSet(new List<INFORMATION> { testInformation });
            var mockDetailsDemographicsDbSet = CreateMockDbSet(existingDetailsDemographics);
            var mockDemographicsDbSet = CreateMockDbSet<DEMOGRAPHICS>(new List<DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.INFORMATION).Returns(mockInformationDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveDemographics(demographicExt, 1);

            // Assert
            testInformation.Facility_Name.Should().Be(demographicExt.OrganizationName);
            mockDetailsDemographicsDbSet.Verify(x => x.Add(It.IsAny<DETAILS_DEMOGRAPHICS>()), Times.AtLeast(1));
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        [TestMethod]
        public void SaveDemographics_WithExistingDemographics_UpdatesDatabase()
        {
            // Arrange
            var demographicExt = CreateTestDemographicExt(1);
            var testInformation = CreateTestInformation(1);
            var existingDemographics = CreateTestDemographics(1);
            var existingDetailsDemographics = new List<DETAILS_DEMOGRAPHICS>();
            
            var mockInformationDbSet = CreateMockDbSet(new List<INFORMATION> { testInformation });
            var mockDetailsDemographicsDbSet = CreateMockDbSet(existingDetailsDemographics);
            var mockDemographicsDbSet = CreateMockDbSet(new List<DEMOGRAPHICS> { existingDemographics });
            
            _mockContext.Setup(x => x.INFORMATION).Returns(mockInformationDbSet.Object);
            _mockContext.Setup(x => x.DETAILS_DEMOGRAPHICS).Returns(mockDetailsDemographicsDbSet.Object);
            _mockContext.Setup(x => x.DEMOGRAPHICS).Returns(mockDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _demographicExtBusiness.SaveDemographics(demographicExt, 1);

            // Assert
            existingDemographics.Agency.Should().Be(demographicExt.BusinessUnit);
            existingDemographics.SectorId.Should().Be(demographicExt.Sector);
            existingDemographics.IndustryId.Should().Be(demographicExt.Subsector);
            existingDemographics.OrganizationName.Should().Be(demographicExt.OrganizationName);
            existingDemographics.OrganizationType.Should().Be(demographicExt.OrganizationType);
        }

        #endregion

        #region GetSubsectors Tests

        [TestMethod]
        public void GetSubsectors_WithValidSectorId_ReturnsSubsectors()
        {
            // Arrange
            int sectorId = 1;
            var testSubsectors = CreateTestSubsectors(sectorId);
            var mockSubsectorsDbSet = CreateMockDbSet(testSubsectors);
            
            _mockContext.Setup(x => x.SECTOR_INDUSTRY).Returns(mockSubsectorsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetSubsectors(sectorId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testSubsectors.Count);
            result.Should().OnlyContain(s => s.OptionValue == sectorId);
        }

        [TestMethod]
        public void GetSubsectors_WithNonExistentSectorId_ReturnsEmptyList()
        {
            // Arrange
            int sectorId = 999;
            var mockSubsectorsDbSet = CreateMockDbSet<SECTOR_INDUSTRY>(new List<SECTOR_INDUSTRY>());
            
            _mockContext.Setup(x => x.SECTOR_INDUSTRY).Returns(mockSubsectorsDbSet.Object);

            // Act
            var result = _demographicExtBusiness.GetSubsectors(sectorId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Helper Methods

        private ASSESSMENTS CreateTestAssessment(int assessmentId)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = assessmentId,
                Assessment_Date = DateTime.Now,
                AssessmentCreatedDate = DateTime.Now,
                CreatorId = 1,
                LastModifiedDate = DateTime.Now
            };
        }

        private INFORMATION CreateTestInformation(int assessmentId)
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State",
                Assessor_Name = "Test Assessor",
                Assessor_Email = "test@example.com",
                Assessor_Phone = "555-1234",
                Assessment_Description = "Test Assessment",
                Additional_Notes_And_Comments = "Test Notes"
            };
        }

        private List<DETAILS_DEMOGRAPHICS> CreateTestDetailsDemographicsList(int assessmentId)
        {
            return new List<DETAILS_DEMOGRAPHICS>
            {
                CreateTestDetailsDemographics(assessmentId, "ORG-TYPE", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "SECTOR", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "SUBSECTOR", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "CISA-REGION", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "NUM-EMP-TOTAL", intValue: 100),
                CreateTestDetailsDemographics(assessmentId, "NUM-EMP-UNIT", intValue: 50),
                CreateTestDetailsDemographics(assessmentId, "ANN-REVENUE", intValue: 1000000),
                CreateTestDetailsDemographics(assessmentId, "ANN-REVENUE-PERCENT", intValue: 25),
                CreateTestDetailsDemographics(assessmentId, "NUM-PEOPLE-SERVED", intValue: 10000),
                CreateTestDetailsDemographics(assessmentId, "DISRUPTED-SECTOR1", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "DISRUPTED-SECTOR2", intValue: 2),
                CreateTestDetailsDemographics(assessmentId, "CRIT-DEPEND-INCIDENT-RESPONSE", "Yes"),
                CreateTestDetailsDemographics(assessmentId, "ORG-POC", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "STANDARD-USED", boolValue: true),
                CreateTestDetailsDemographics(assessmentId, "STANDARD1", "NIST CSF"),
                CreateTestDetailsDemographics(assessmentId, "STANDARD2", "ISO 27001"),
                CreateTestDetailsDemographics(assessmentId, "REGULATION-REQD", boolValue: true),
                CreateTestDetailsDemographics(assessmentId, "REG-TYPE1", intValue: 1),
                CreateTestDetailsDemographics(assessmentId, "REG-1-OTHER", "Test Regulation"),
                CreateTestDetailsDemographics(assessmentId, "REG-TYPE2", intValue: 2),
                CreateTestDetailsDemographics(assessmentId, "REG-2-OTHER", "Test Regulation 2"),
                CreateTestDetailsDemographics(assessmentId, "SHARE-OTHER", "Test Share"),
                CreateTestDetailsDemographics(assessmentId, "BARRIER1", "Test Barrier 1"),
                CreateTestDetailsDemographics(assessmentId, "BARRIER2", "Test Barrier 2"),
                CreateTestDetailsDemographics(assessmentId, "BUSINESS-UNIT", "Test Business Unit")
            };
        }

        private DETAILS_DEMOGRAPHICS CreateTestDetailsDemographics(int assessmentId, string key, string stringValue = null, int? intValue = null, bool? boolValue = null)
        {
            return new DETAILS_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                DataItemName = key,
                StringValue = stringValue,
                IntValue = intValue,
                BoolValue = boolValue
            };
        }

        private List<DETAILS_DEMOGRAPHICS_OPTIONS> CreateTestDetailsDemographicsOptions()
        {
            return new List<DETAILS_DEMOGRAPHICS_OPTIONS>
            {
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ORG-TYPE", OptionValue = 1, OptionText = "Government" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ORG-TYPE", OptionValue = 2, OptionText = "Private Sector" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "CISA-REGION", OptionValue = 1, OptionText = "Region 1" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "CISA-REGION", OptionValue = 2, OptionText = "Region 2" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-EMP-TOTAL", OptionValue = 1, OptionText = "1-50" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-EMP-TOTAL", OptionValue = 2, OptionText = "51-100" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-EMP-UNIT", OptionValue = 1, OptionText = "1-10" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-EMP-UNIT", OptionValue = 2, OptionText = "11-25" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ANN-REVENUE", OptionValue = 1, OptionText = "< $1M" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ANN-REVENUE", OptionValue = 2, OptionText = "$1M - $10M" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ANN-REVENUE-PERCENT", OptionValue = 1, OptionText = "< 10%" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "ANN-REVENUE-PERCENT", OptionValue = 2, OptionText = "10-25%" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-PEOPLE-SERVED", OptionValue = 1, OptionText = "< 1K" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "NUM-PEOPLE-SERVED", OptionValue = 2, OptionText = "1K - 10K" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "SHARE-ORG-1", OptionValue = 1, OptionText = "Share Org 1" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "SHARE-ORG-2", OptionValue = 2, OptionText = "Share Org 2" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "STANDARD", OptionValue = 1, OptionText = "NIST CSF" },
                new DETAILS_DEMOGRAPHICS_OPTIONS { DataItemName = "STANDARD", OptionValue = 2, OptionText = "ISO 27001" }
            };
        }

        private List<SECTOR_INDUSTRY> CreateTestSubsectors(int sectorId)
        {
            return new List<SECTOR_INDUSTRY>
            {
                new SECTOR_INDUSTRY { IndustryId = 1, SectorId = sectorId, IndustryName = "Subsector 1" },
                new SECTOR_INDUSTRY { IndustryId = 2, SectorId = sectorId, IndustryName = "Subsector 2" },
                new SECTOR_INDUSTRY { IndustryId = 3, SectorId = sectorId, IndustryName = "Subsector 3" }
            };
        }

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
                CriticalService = "Test Critical Service"
            };
        }

        private DemographicExt CreateTestDemographicExt(int assessmentId)
        {
            return new DemographicExt
            {
                AssessmentId = assessmentId,
                AssessmentDate = DateTime.Now,
                OrganizationType = 1,
                OrganizationName = "Test Organization",
                SectorDirective = "PPD-21",
                Sector = 1,
                Subsector = 1,
                OrgPointOfContact = 1,
                CisaRegion = 1,
                NumberEmployeesTotal = 100,
                NumberEmployeesUnit = 50,
                AnnualRevenue = 1000000,
                CriticalServiceRevenuePercent = 25,
                CriticalDependencyIncidentResponseSupport = "Yes",
                NumberPeopleServedByCritSvc = 10000,
                DisruptedSector1 = 1,
                DisruptedSector2 = 2,
                UsesStandard = true,
                Standard1 = "NIST CSF",
                Standard2 = "ISO 27001",
                RequiredToComply = true,
                RegulationType1 = 1,
                Reg1Other = "Test Regulation",
                RegulationType2 = 2,
                Reg2Other = "Test Regulation 2",
                ShareOrgs = new List<int> { 1, 2 },
                ShareOther = "Test Share",
                Barrier1 = "Test Barrier 1",
                Barrier2 = "Test Barrier 2",
                BusinessUnit = "Test Business Unit"
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