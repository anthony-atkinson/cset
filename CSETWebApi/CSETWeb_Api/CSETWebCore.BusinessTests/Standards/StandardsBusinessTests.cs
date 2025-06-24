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
using CSETWebCore.Business.Standards;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.Standards;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Question;
using CSETWebCore.Model.Standards;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Standards
{
    [TestClass]
    public class StandardsBusinessTests : BaseBusinessTest
    {
        private StandardsBusiness _standardsBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private Mock<ITokenManager> _mockTokenManager;
        private Mock<IQuestionRequirementManager> _mockQuestionRequirement;
        private Mock<IDemographicBusiness> _mockDemographicBusiness;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
            _mockAssessmentUtil = _fixture.Freeze<Mock<IAssessmentUtil>>();
            _mockTokenManager = _fixture.Freeze<Mock<ITokenManager>>();
            _mockQuestionRequirement = _fixture.Freeze<Mock<IQuestionRequirementManager>>();
            _mockDemographicBusiness = _fixture.Freeze<Mock<IDemographicBusiness>>();

            _standardsBusiness = new StandardsBusiness(
                _context,
                _mockAssessmentUtil.Object,
                _mockQuestionRequirement.Object,
                _mockTokenManager.Object,
                _mockDemographicBusiness.Object
            );
        }

        #region GetStandards Tests

        [TestMethod]
        public void GetStandards_WithValidAssessmentId_ReturnsStandardsResponse()
        {
            // Arrange
            int assessmentId = 1;
            var testStandards = CreateTestStandards();
            var testCategories = CreateTestCategories();
            SeedTestData(assessmentId, testStandards, testCategories);

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(10);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(5);

            // Act
            var result = _standardsBusiness.GetStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
            result.QuestionCount.Should().Be(10);
            result.RequirementCount.Should().Be(5);
        }

        [TestMethod]
        public void GetStandards_WithNoSelectedStandards_ReturnsEmptySelectedList()
        {
            // Arrange
            int assessmentId = 1;
            var testStandards = CreateTestStandards();
            var testCategories = CreateTestCategories();
            SeedTestData(assessmentId, testStandards, testCategories, selectedStandards: new List<string>());

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(0);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(0);

            // Act
            var result = _standardsBusiness.GetStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
            result.Categories.SelectMany(c => c.Standards).All(s => !s.Selected).Should().BeTrue();
        }

        [TestMethod]
        public void GetStandards_WithDeprecatedStandards_ExcludesUnusedDeprecatedStandards()
        {
            // Arrange
            int assessmentId = 1;
            var testStandards = CreateTestStandards(includeDeprecated: true);
            var testCategories = CreateTestCategories();
            SeedTestData(assessmentId, testStandards, testCategories, selectedStandards: new List<string> { "NCSF_V1" });

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(5);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(3);

            // Act
            var result = _standardsBusiness.GetStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            // Should only include deprecated standards that are selected
            var deprecatedStandards = result.Categories
                .SelectMany(c => c.Standards)
                .Where(s => s.FullName.Contains("[DEPRECATED]"));
            deprecatedStandards.Should().NotBeEmpty();
            deprecatedStandards.All(s => s.Selected).Should().BeTrue();
        }

        [TestMethod]
        public void GetStandards_WithRecommendedStandards_MarksRecommendedStandards()
        {
            // Arrange
            int assessmentId = 1;
            var testStandards = CreateTestStandards();
            var testCategories = CreateTestCategories();
            SeedTestData(assessmentId, testStandards, testCategories);

            var demographics = new Demographics { IndustryId = 1, SectorId = 1 };
            _mockDemographicBusiness.Setup(x => x.GetDemographics(assessmentId)).Returns(demographics);

            var assetValues = new List<DEMOGRAPHICS_ASSET_VALUES> { new DEMOGRAPHICS_ASSET_VALUES { DemographicsAssetId = 1, AssetValue = "High" } };
            var assetSizes = new List<DEMOGRAPHICS_SIZE> { new DEMOGRAPHICS_SIZE { DemographicId = 1, Size = "Large" } };
            var recommendations = new List<SECTOR_STANDARD_RECOMMENDATIONS> 
            { 
                new SECTOR_STANDARD_RECOMMENDATIONS { Set_Name = "NCSF_V1" } 
            };

            _context.DEMOGRAPHICS_ASSET_VALUES.AddRange(assetValues);
            _context.DEMOGRAPHICS_SIZE.AddRange(assetSizes);
            _context.SECTOR_STANDARD_RECOMMENDATIONS.AddRange(recommendations);
            _context.SaveChanges();

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(5);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(3);

            // Act
            var result = _standardsBusiness.GetStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var recommendedStandard = result.Categories
                .SelectMany(c => c.Standards)
                .FirstOrDefault(s => s.Code == "NCSF_V1");
            recommendedStandard.Should().NotBeNull();
            recommendedStandard.Recommended.Should().BeTrue();
        }

        #endregion

        #region GetFramework Tests

        [TestMethod]
        public void GetFramework_WithNCSFSelected_ReturnsTrue()
        {
            // Arrange
            int assessmentId = 1;
            var availableStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "NCSF_V1", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.GetFramework(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetFramework_WithNCSFNotSelected_ReturnsFalse()
        {
            // Arrange
            int assessmentId = 1;
            var availableStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "NCSF_V1", Selected = false }
            };
            _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.GetFramework(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void GetFramework_WithNoNCSF_ReturnsFalse()
        {
            // Arrange
            int assessmentId = 1;
            var availableStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "Key", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.GetFramework(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region GetACET Tests

        [TestMethod]
        public void GetACET_WithACETSelected_ReturnsTrue()
        {
            // Arrange
            int assessmentId = 1;
            var availableStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "ACET_V1", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.GetACET(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetACET_WithACETNotSelected_ReturnsFalse()
        {
            // Arrange
            int assessmentId = 1;
            var availableStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "ACET_V1", Selected = false }
            };
            _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.GetACET(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region RecommendedStandards Tests

        [TestMethod]
        public void RecommendedStandards_WithValidDemographics_ReturnsRecommendedStandards()
        {
            // Arrange
            int assessmentId = 1;
            var demographics = new Demographics 
            { 
                IndustryId = 1, 
                SectorId = 1, 
                AssetValue = 1, 
                Size = 1 
            };
            _mockDemographicBusiness.Setup(x => x.GetDemographics(assessmentId)).Returns(demographics);

            var assetValues = new List<DEMOGRAPHICS_ASSET_VALUES> 
            { 
                new DEMOGRAPHICS_ASSET_VALUES { DemographicsAssetId = 1, AssetValue = "High" } 
            };
            var assetSizes = new List<DEMOGRAPHICS_SIZE> 
            { 
                new DEMOGRAPHICS_SIZE { DemographicId = 1, Size = "Large" } 
            };
            var recommendations = new List<SECTOR_STANDARD_RECOMMENDATIONS> 
            { 
                new SECTOR_STANDARD_RECOMMENDATIONS 
                { 
                    Industry_Id = 1, 
                    Sector_Id = 1, 
                    Organization_Size = "Large", 
                    Asset_Value = "High", 
                    Set_Name = "NCSF_V1" 
                } 
            };

            _context.DEMOGRAPHICS_ASSET_VALUES.AddRange(assetValues);
            _context.DEMOGRAPHICS_SIZE.AddRange(assetSizes);
            _context.SECTOR_STANDARD_RECOMMENDATIONS.AddRange(recommendations);
            _context.SaveChanges();

            // Act
            var result = _standardsBusiness.RecommendedStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("NCSF_V1");
        }

        [TestMethod]
        public void RecommendedStandards_WithNullDemographics_ReturnsEmptyList()
        {
            // Arrange
            int assessmentId = 1;
            _mockDemographicBusiness.Setup(x => x.GetDemographics(assessmentId)).Returns((Demographics)null);

            // Act
            var result = _standardsBusiness.RecommendedStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void RecommendedStandards_WithNoMatchingRecommendations_ReturnsEmptyList()
        {
            // Arrange
            int assessmentId = 1;
            var demographics = new Demographics 
            { 
                IndustryId = 999, 
                SectorId = 999, 
                AssetValue = 999, 
                Size = 999 
            };
            _mockDemographicBusiness.Setup(x => x.GetDemographics(assessmentId)).Returns(demographics);

            // Act
            var result = _standardsBusiness.RecommendedStandards(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region PersistSelectedStandards Tests

        [TestMethod]
        public void PersistSelectedStandards_WithValidStandards_SavesAndReturnsCounts()
        {
            // Arrange
            int assessmentId = 1;
            var selectedStandards = new List<string> { "NCSF_V1", "Key" };

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(15);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(8);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistSelectedStandards(assessmentId, selectedStandards);

            // Assert
            result.Should().NotBeNull();
            result.QuestionCount.Should().Be(15);
            result.RequirementCount.Should().Be(8);

            var savedStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .Select(x => x.Set_Name)
                .ToList();
            savedStandards.Should().BeEquivalentTo(selectedStandards);

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void PersistSelectedStandards_WithNullStandards_ClearsExistingStandards()
        {
            // Arrange
            int assessmentId = 1;
            var existingStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "NCSF_V1", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(existingStandards);
            _context.SaveChanges();

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(0);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(0);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistSelectedStandards(assessmentId, null);

            // Assert
            result.Should().NotBeNull();
            var remainingStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .ToList();
            remainingStandards.Should().BeEmpty();

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void PersistSelectedStandards_WithEmptyStandards_ClearsExistingStandards()
        {
            // Arrange
            int assessmentId = 1;
            var existingStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "NCSF_V1", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(existingStandards);
            _context.SaveChanges();

            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(0);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(0);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistSelectedStandards(assessmentId, new List<string>());

            // Assert
            result.Should().NotBeNull();
            var remainingStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .ToList();
            remainingStandards.Should().BeEmpty();

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        #endregion

        #region PersistDefaultSelectedStandard Tests

        [TestMethod]
        public void PersistDefaultSelectedStandard_WithCSETScope_SavesKeyStandard()
        {
            // Arrange
            int assessmentId = 1;
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("cset");
            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(10);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(5);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistDefaultSelectedStandard(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.QuestionCount.Should().Be(10);
            result.RequirementCount.Should().Be(5);

            var savedStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .Select(x => x.Set_Name)
                .ToList();
            savedStandards.Should().Contain("Key");

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void PersistDefaultSelectedStandard_WithACETScope_SavesACETStandard()
        {
            // Arrange
            int assessmentId = 1;
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("acet");
            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(10);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(5);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistDefaultSelectedStandard(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var savedStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .Select(x => x.Set_Name)
                .ToList();
            savedStandards.Should().Contain("ACET_V1");

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void PersistDefaultSelectedStandard_ClearsExistingStandards()
        {
            // Arrange
            int assessmentId = 1;
            var existingStandards = new List<AVAILABLE_STANDARDS>
            {
                new AVAILABLE_STANDARDS { Assessment_Id = assessmentId, Set_Name = "NCSF_V1", Selected = true }
            };
            _context.AVAILABLE_STANDARDS.AddRange(existingStandards);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("cset");
            _mockQuestionRequirement.Setup(x => x.NumberOfQuestions()).Returns(10);
            _mockQuestionRequirement.Setup(x => x.NumberOfRequirements()).Returns(5);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _standardsBusiness.PersistDefaultSelectedStandard(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var oldStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Set_Name == "NCSF_V1")
                .ToList();
            oldStandards.Should().BeEmpty();

            var newStandards = _context.AVAILABLE_STANDARDS
                .Where(x => x.Assessment_Id == assessmentId && x.Selected)
                .Select(x => x.Set_Name)
                .ToList();
            newStandards.Should().Contain("Key");
        }

        #endregion

        #region GetDefaultStandardsList Tests

        [TestMethod]
        public void GetDefaultStandardsList_WithCSETScope_ReturnsKeyStandard()
        {
            // Arrange
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("cset");

            // Act
            var result = _standardsBusiness.GetDefaultStandardsList();

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("Key");
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetDefaultStandardsList_WithACETScope_ReturnsACETStandard()
        {
            // Arrange
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("acet");

            // Act
            var result = _standardsBusiness.GetDefaultStandardsList();

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain("ACET_V1");
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public void GetDefaultStandardsList_WithUnknownScope_ReturnsEmptyList()
        {
            // Arrange
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns("unknown");

            // Act
            var result = _standardsBusiness.GetDefaultStandardsList();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetDefaultStandardsList_WithNullScope_ReturnsEmptyList()
        {
            // Arrange
            _mockTokenManager.Setup(x => x.Payload("scope")).Returns((string)null);

            // Act
            var result = _standardsBusiness.GetDefaultStandardsList();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Helper Methods

        private List<SETS> CreateTestStandards(bool includeDeprecated = false)
        {
            var standards = new List<SETS>
            {
                new SETS 
                { 
                    Set_Name = "NCSF_V1", 
                    Full_Name = "NIST Cybersecurity Framework", 
                    Standard_ToolTip = "NIST CSF Description",
                    Is_Displayed = true,
                    IsEncryptedModule = false,
                    IsEncryptedModuleOpen = true,
                    Is_Deprecated = false,
                    Order_In_Category = 1
                },
                new SETS 
                { 
                    Set_Name = "Key", 
                    Full_Name = "Key Controls", 
                    Standard_ToolTip = "Key Controls Description",
                    Is_Displayed = true,
                    IsEncryptedModule = false,
                    IsEncryptedModuleOpen = true,
                    Is_Deprecated = false,
                    Order_In_Category = 2
                }
            };

            if (includeDeprecated)
            {
                standards.Add(new SETS 
                { 
                    Set_Name = "DEPRECATED_STD", 
                    Full_Name = "Deprecated Standard", 
                    Standard_ToolTip = "Deprecated Standard Description",
                    Is_Displayed = true,
                    IsEncryptedModule = false,
                    IsEncryptedModuleOpen = true,
                    Is_Deprecated = true,
                    Order_In_Category = 3
                });
            }

            return standards;
        }

        private List<SETS_CATEGORY> CreateTestCategories()
        {
            return new List<SETS_CATEGORY>
            {
                new SETS_CATEGORY 
                { 
                    Set_Category_Id = 1, 
                    Set_Category_Name = "Framework Standards" 
                },
                new SETS_CATEGORY 
                { 
                    Set_Category_Id = 2, 
                    Set_Category_Name = "Industry Standards" 
                }
            };
        }

        private void SeedTestData(int assessmentId, List<SETS> standards, List<SETS_CATEGORY> categories, List<string> selectedStandards = null)
        {
            // Add categories
            _context.SETS_CATEGORY.AddRange(categories);

            // Add standards with category relationships
            foreach (var standard in standards)
            {
                standard.Set_Category_Id = categories.First().Set_Category_Id;
            }
            _context.SETS.AddRange(standards);

            // Add selected standards if provided
            if (selectedStandards != null)
            {
                var availableStandards = selectedStandards.Select(s => new AVAILABLE_STANDARDS
                {
                    Assessment_Id = assessmentId,
                    Set_Name = s,
                    Selected = true
                }).ToList();
                _context.AVAILABLE_STANDARDS.AddRange(availableStandards);
            }

            _context.SaveChanges();
        }

        #endregion
    }
} 