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
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Assessment;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.Demographic
{
    [TestClass]
    public class CisDemographicBusinessTests : BaseBusinessTest
    {
        private CisDemographicBusiness _cisDemographicBusiness;
        private Mock<CSETContext> _mockContext;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _mockAssessmentUtil = new Mock<IAssessmentUtil>();
            _cisDemographicBusiness = new CisDemographicBusiness(_mockContext.Object, _mockAssessmentUtil.Object);
        }

        #region SaveOrgDemographics Tests

        [TestMethod]
        public void SaveOrgDemographics_WithValidData_SavesToDatabase()
        {
            // Arrange
            var orgDemographics = CreateTestCisOrganizationDemographics(1);
            var mockOrgDemographicsDbSet = CreateMockDbSet<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>(new List<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_ORGANIZATION_DEMOGRAPHICS).Returns(mockOrgDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveOrgDemographics(orgDemographics);

            // Assert
            result.Should().Be(orgDemographics.AssessmentId);
            mockOrgDemographicsDbSet.Verify(x => x.Add(It.IsAny<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveOrgDemographics_WithExistingData_UpdatesDatabase()
        {
            // Arrange
            var orgDemographics = CreateTestCisOrganizationDemographics(1);
            var existingOrgDemographics = CreateTestCisOrganizationDemographicsEntity(1);
            var mockOrgDemographicsDbSet = CreateMockDbSet(new List<CIS_CSI_ORGANIZATION_DEMOGRAPHICS> { existingOrgDemographics });
            
            _mockContext.Setup(x => x.CIS_CSI_ORGANIZATION_DEMOGRAPHICS).Returns(mockOrgDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveOrgDemographics(orgDemographics);

            // Assert
            result.Should().Be(orgDemographics.AssessmentId);
            existingOrgDemographics.MotivationCrr.Should().Be(orgDemographics.MotivationCrr);
            existingOrgDemographics.MotivationCrrDescription.Should().Be(orgDemographics.MotivationCrrDescription);
            existingOrgDemographics.OrganizationName.Should().Be(orgDemographics.OrganizationName);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region GetOrgDemographics Tests

        [TestMethod]
        public void GetOrgDemographics_WithValidAssessmentId_ReturnsCisOrganizationDemographics()
        {
            // Arrange
            int assessmentId = 1;
            var testOrgDemographics = CreateTestCisOrganizationDemographicsEntity(assessmentId);
            var mockOrgDemographicsDbSet = CreateMockDbSet(new List<CIS_CSI_ORGANIZATION_DEMOGRAPHICS> { testOrgDemographics });
            
            _mockContext.Setup(x => x.CIS_CSI_ORGANIZATION_DEMOGRAPHICS).Returns(mockOrgDemographicsDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetOrgDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.MotivationCrr.Should().Be(testOrgDemographics.MotivationCrr);
            result.MotivationCrrDescription.Should().Be(testOrgDemographics.MotivationCrrDescription);
            result.OrganizationName.Should().Be(testOrgDemographics.OrganizationName);
            result.SiteName.Should().Be(testOrgDemographics.SiteName);
            result.StreetAddress.Should().Be(testOrgDemographics.StreetAddress);
            result.VisitDate.Should().Be(testOrgDemographics.VisitDate);
            result.CompletedForSltt.Should().Be(testOrgDemographics.CompletedForSltt);
            result.CompletedForFederal.Should().Be(testOrgDemographics.CompletedForFederal);
            result.CompletedForNationalSpecialEvent.Should().Be(testOrgDemographics.CompletedForNationalSpecialEvent);
            result.CikrSector.Should().Be(testOrgDemographics.CikrSector);
            result.SubSector.Should().Be(testOrgDemographics.SubSector);
            result.CustomersCount.Should().Be(testOrgDemographics.CustomersCount);
            result.ItIcsStaffCount.Should().Be(testOrgDemographics.ItIcsStaffCount);
        }

        [TestMethod]
        public void GetOrgDemographics_WithNonExistentAssessmentId_ReturnsNull()
        {
            // Arrange
            int assessmentId = 999;
            var mockOrgDemographicsDbSet = CreateMockDbSet<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>(new List<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_ORGANIZATION_DEMOGRAPHICS).Returns(mockOrgDemographicsDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetOrgDemographics(assessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region SaveServiceDemographics Tests

        [TestMethod]
        public void SaveServiceDemographics_WithValidData_SavesToDatabase()
        {
            // Arrange
            var serviceDemographics = CreateTestCisServiceDemographics(1);
            var mockServiceDemographicsDbSet = CreateMockDbSet<CIS_CSI_SERVICE_DEMOGRAPHICS>(new List<CIS_CSI_SERVICE_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_DEMOGRAPHICS).Returns(mockServiceDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveServiceDemographics(serviceDemographics, 1);

            // Assert
            result.Should().Be(serviceDemographics.AssessmentId);
            mockServiceDemographicsDbSet.Verify(x => x.Add(It.IsAny<CIS_CSI_SERVICE_DEMOGRAPHICS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(serviceDemographics.AssessmentId), Times.Once);
        }

        [TestMethod]
        public void SaveServiceDemographics_WithExistingData_UpdatesDatabase()
        {
            // Arrange
            var serviceDemographics = CreateTestCisServiceDemographics(1);
            var existingServiceDemographics = CreateTestCisServiceDemographicsEntity(1);
            var mockServiceDemographicsDbSet = CreateMockDbSet(new List<CIS_CSI_SERVICE_DEMOGRAPHICS> { existingServiceDemographics });
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_DEMOGRAPHICS).Returns(mockServiceDemographicsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveServiceDemographics(serviceDemographics, 1);

            // Assert
            result.Should().Be(serviceDemographics.AssessmentId);
            existingServiceDemographics.CriticalService.Should().Be(serviceDemographics.CriticalService);
            existingServiceDemographics.CriticalServiceDescription.Should().Be(serviceDemographics.CriticalServiceDescription);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region GetServiceDemographics Tests

        [TestMethod]
        public void GetServiceDemographics_WithValidAssessmentId_ReturnsCisServiceDemographics()
        {
            // Arrange
            int assessmentId = 1;
            var testServiceDemographics = CreateTestCisServiceDemographicsEntity(assessmentId);
            var mockServiceDemographicsDbSet = CreateMockDbSet(new List<CIS_CSI_SERVICE_DEMOGRAPHICS> { testServiceDemographics });
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_DEMOGRAPHICS).Returns(mockServiceDemographicsDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetServiceDemographics(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.CriticalService.Should().Be(testServiceDemographics.CriticalService);
            result.CriticalServiceDescription.Should().Be(testServiceDemographics.CriticalServiceDescription);
            result.CustomersCount.Should().Be(testServiceDemographics.CustomersCount);
            result.CustomersCountDescription.Should().Be(testServiceDemographics.CustomersCountDescription);
            result.ItIcsStaffCount.Should().Be(testServiceDemographics.ItIcsStaffCount);
            result.ItIcsStaffCountDescription.Should().Be(testServiceDemographics.ItIcsStaffCountDescription);
        }

        [TestMethod]
        public void GetServiceDemographics_WithNonExistentAssessmentId_ReturnsNull()
        {
            // Arrange
            int assessmentId = 999;
            var mockServiceDemographicsDbSet = CreateMockDbSet<CIS_CSI_SERVICE_DEMOGRAPHICS>(new List<CIS_CSI_SERVICE_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_DEMOGRAPHICS).Returns(mockServiceDemographicsDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetServiceDemographics(assessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region SaveServiceComposition Tests

        [TestMethod]
        public void SaveServiceComposition_WithValidData_SavesToDatabase()
        {
            // Arrange
            var serviceComposition = CreateTestCisServiceComposition(1);
            var mockServiceCompositionDbSet = CreateMockDbSet<CIS_CSI_SERVICE_COMPOSITION>(new List<CIS_CSI_SERVICE_COMPOSITION>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_COMPOSITION).Returns(mockServiceCompositionDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveServiceComposition(serviceComposition);

            // Assert
            result.Should().Be(serviceComposition.AssessmentId);
            mockServiceCompositionDbSet.Verify(x => x.Add(It.IsAny<CIS_CSI_SERVICE_COMPOSITION>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void SaveServiceComposition_WithExistingData_UpdatesDatabase()
        {
            // Arrange
            var serviceComposition = CreateTestCisServiceComposition(1);
            var existingServiceComposition = CreateTestCisServiceCompositionEntity(1);
            var mockServiceCompositionDbSet = CreateMockDbSet(new List<CIS_CSI_SERVICE_COMPOSITION> { existingServiceComposition });
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_COMPOSITION).Returns(mockServiceCompositionDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _cisDemographicBusiness.SaveServiceComposition(serviceComposition);

            // Assert
            result.Should().Be(serviceComposition.AssessmentId);
            existingServiceComposition.CriticalService.Should().Be(serviceComposition.CriticalService);
            existingServiceComposition.CriticalServiceDescription.Should().Be(serviceComposition.CriticalServiceDescription);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region GetServiceComposition Tests

        [TestMethod]
        public void GetServiceComposition_WithValidAssessmentId_ReturnsCisServiceComposition()
        {
            // Arrange
            int assessmentId = 1;
            var testServiceComposition = CreateTestCisServiceCompositionEntity(assessmentId);
            var mockServiceCompositionDbSet = CreateMockDbSet(new List<CIS_CSI_SERVICE_COMPOSITION> { testServiceComposition });
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_COMPOSITION).Returns(mockServiceCompositionDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetServiceComposition(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.AssessmentId.Should().Be(assessmentId);
            result.CriticalService.Should().Be(testServiceComposition.CriticalService);
            result.CriticalServiceDescription.Should().Be(testServiceComposition.CriticalServiceDescription);
        }

        [TestMethod]
        public void GetServiceComposition_WithNonExistentAssessmentId_ReturnsNull()
        {
            // Arrange
            int assessmentId = 999;
            var mockServiceCompositionDbSet = CreateMockDbSet<CIS_CSI_SERVICE_COMPOSITION>(new List<CIS_CSI_SERVICE_COMPOSITION>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_COMPOSITION).Returns(mockServiceCompositionDbSet.Object);

            // Act
            var result = _cisDemographicBusiness.GetServiceComposition(assessmentId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Edge Cases and Error Handling

        [TestMethod]
        public void SaveOrgDemographics_WithNullData_HandlesGracefully()
        {
            // Arrange
            CisOrganizationDemographics orgDemographics = null;
            var mockOrgDemographicsDbSet = CreateMockDbSet<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>(new List<CIS_CSI_ORGANIZATION_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_ORGANIZATION_DEMOGRAPHICS).Returns(mockOrgDemographicsDbSet.Object);

            // Act & Assert
            Action act = () => _cisDemographicBusiness.SaveOrgDemographics(orgDemographics);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SaveServiceDemographics_WithNullData_HandlesGracefully()
        {
            // Arrange
            CisServiceDemographics serviceDemographics = null;
            var mockServiceDemographicsDbSet = CreateMockDbSet<CIS_CSI_SERVICE_DEMOGRAPHICS>(new List<CIS_CSI_SERVICE_DEMOGRAPHICS>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_DEMOGRAPHICS).Returns(mockServiceDemographicsDbSet.Object);

            // Act & Assert
            Action act = () => _cisDemographicBusiness.SaveServiceDemographics(serviceDemographics, 1);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void SaveServiceComposition_WithNullData_HandlesGracefully()
        {
            // Arrange
            CisServiceComposition serviceComposition = null;
            var mockServiceCompositionDbSet = CreateMockDbSet<CIS_CSI_SERVICE_COMPOSITION>(new List<CIS_CSI_SERVICE_COMPOSITION>());
            
            _mockContext.Setup(x => x.CIS_CSI_SERVICE_COMPOSITION).Returns(mockServiceCompositionDbSet.Object);

            // Act & Assert
            Action act = () => _cisDemographicBusiness.SaveServiceComposition(serviceComposition);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Helper Methods

        private CisOrganizationDemographics CreateTestCisOrganizationDemographics(int assessmentId)
        {
            return new CisOrganizationDemographics
            {
                AssessmentId = assessmentId,
                MotivationCrr = true,
                MotivationCrrDescription = "Test CRR motivation",
                MotivationRrap = false,
                MotivationRrapDescription = "",
                MotivationOrganizationRequest = true,
                MotivationOrganizationRequestDescription = "Test organization request",
                MotivationLawEnforcementRequest = false,
                MotivationLawEnforcementRequestDescription = "",
                MotivationDirectThreats = true,
                MotivationDirectThreatsDescription = "Test direct threats",
                MotivationSpecialEvent = false,
                MotivationSpecialEventDescription = "",
                MotivationOther = false,
                MotivationOtherDescription = "",
                ParentOrganization = "Test Parent Org",
                OrganizationName = "Test Organization",
                SiteName = "Test Site",
                StreetAddress = "123 Test Street",
                VisitDate = DateTime.Now,
                CompletedForSltt = true,
                CompletedForFederal = false,
                CompletedForNationalSpecialEvent = false,
                CikrSector = "Test Sector",
                SubSector = "Test SubSector",
                CustomersCount = "1000-5000",
                ItIcsStaffCount = "10-25"
            };
        }

        private CIS_CSI_ORGANIZATION_DEMOGRAPHICS CreateTestCisOrganizationDemographicsEntity(int assessmentId)
        {
            return new CIS_CSI_ORGANIZATION_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                MotivationCrr = true,
                MotivationCrrDescription = "Test CRR motivation",
                MotivationRrap = false,
                MotivationRrapDescription = "",
                MotivationOrganizationRequest = true,
                MotivationOrganizationRequestDescription = "Test organization request",
                MotivationLawEnforcementRequest = false,
                MotivationLawEnforcementRequestDescription = "",
                MotivationDirectThreats = true,
                MotivationDirectThreatsDescription = "Test direct threats",
                MotivationSpecialEvent = false,
                MotivationSpecialEventDescription = "",
                MotivationOther = false,
                MotivationOtherDescription = "",
                ParentOrganization = "Test Parent Org",
                OrganizationName = "Test Organization",
                SiteName = "Test Site",
                StreetAddress = "123 Test Street",
                VisitDate = DateTime.Now,
                CompletedForSltt = true,
                CompletedForFederal = false,
                CompletedForNationalSpecialEvent = false,
                CikrSector = "Test Sector",
                SubSector = "Test SubSector",
                CustomersCount = "1000-5000",
                ItIcsStaffCount = "10-25"
            };
        }

        private CisServiceDemographics CreateTestCisServiceDemographics(int assessmentId)
        {
            return new CisServiceDemographics
            {
                AssessmentId = assessmentId,
                CriticalService = "Test Critical Service",
                CriticalServiceDescription = "Test critical service description",
                CustomersCount = "1000-5000",
                CustomersCountDescription = "Test customers count description",
                ItIcsStaffCount = "10-25",
                ItIcsStaffCountDescription = "Test IT/ICS staff count description"
            };
        }

        private CIS_CSI_SERVICE_DEMOGRAPHICS CreateTestCisServiceDemographicsEntity(int assessmentId)
        {
            return new CIS_CSI_SERVICE_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                CriticalService = "Test Critical Service",
                CriticalServiceDescription = "Test critical service description",
                CustomersCount = "1000-5000",
                CustomersCountDescription = "Test customers count description",
                ItIcsStaffCount = "10-25",
                ItIcsStaffCountDescription = "Test IT/ICS staff count description"
            };
        }

        private CisServiceComposition CreateTestCisServiceComposition(int assessmentId)
        {
            return new CisServiceComposition
            {
                AssessmentId = assessmentId,
                CriticalService = "Test Critical Service",
                CriticalServiceDescription = "Test critical service description"
            };
        }

        private CIS_CSI_SERVICE_COMPOSITION CreateTestCisServiceCompositionEntity(int assessmentId)
        {
            return new CIS_CSI_SERVICE_COMPOSITION
            {
                Assessment_Id = assessmentId,
                CriticalService = "Test Critical Service",
                CriticalServiceDescription = "Test critical service description"
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