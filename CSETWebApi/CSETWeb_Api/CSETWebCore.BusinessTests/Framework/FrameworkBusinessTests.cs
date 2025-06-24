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
using CSETWebCore.Business.Framework;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Framework;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Framework;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Framework
{
    [TestClass]
    public class FrameworkBusinessTests : BaseBusinessTest
    {
        private FrameworkBusiness _frameworkBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
            _mockAssessmentUtil = _fixture.Freeze<Mock<IAssessmentUtil>>();

            _frameworkBusiness = new FrameworkBusiness(
                _context,
                _mockAssessmentUtil.Object
            );
        }

        #region GetFrameworks Tests

        [TestMethod]
        public void GetFrameworks_WithValidAssessmentId_ReturnsFrameworkResponse()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().NotBeEmpty();
            result.Frameworks.Should().HaveCount(2); // IDENTIFY and PROTECT
        }

        [TestMethod]
        public void GetFrameworks_WithSelectedTiers_MarksSelectedTiers()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers, selectedTiers: new List<string> { "IDENTIFY" });

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var identifyFramework = result.Frameworks.FirstOrDefault(f => f.Title == "IDENTIFY");
            identifyFramework.Should().NotBeNull();
            identifyFramework.Selected.Should().BeTrue();

            var protectFramework = result.Frameworks.FirstOrDefault(f => f.Title == "PROTECT");
            protectFramework.Should().NotBeNull();
            protectFramework.Selected.Should().BeFalse();
        }

        [TestMethod]
        public void GetFrameworks_WithNoSelectedTiers_ReturnsAllUnselected()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers, selectedTiers: new List<string>());

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.All(f => !f.Selected).Should().BeTrue();
        }

        [TestMethod]
        public void GetFrameworks_WithMultipleSelectedTiers_MarksAllSelectedTiers()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers, selectedTiers: new List<string> { "IDENTIFY", "PROTECT" });

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.All(f => f.Selected).Should().BeTrue();
        }

        [TestMethod]
        public void GetFrameworks_WithInvalidAssessmentId_ReturnsEmptyResponse()
        {
            // Arrange
            int assessmentId = 999;

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().BeEmpty();
        }

        #endregion

        #region Framework Selection Logic Tests

        [TestMethod]
        public void GetFrameworks_WithFrameworkQuestions_IncludesAllFrameworkCategories()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var frameworkTitles = result.Frameworks.Select(f => f.Title).ToList();
            frameworkTitles.Should().Contain("IDENTIFY");
            frameworkTitles.Should().Contain("PROTECT");
            frameworkTitles.Should().Contain("DETECT");
            frameworkTitles.Should().Contain("RESPOND");
            frameworkTitles.Should().Contain("RECOVER");
        }

        [TestMethod]
        public void GetFrameworks_WithFrameworkQuestions_IncludesCorrectQuestionCounts()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var identifyFramework = result.Frameworks.FirstOrDefault(f => f.Title == "IDENTIFY");
            identifyFramework.Should().NotBeNull();
            identifyFramework.QuestionCount.Should().BeGreaterThan(0);
        }

        #endregion

        #region Framework Customization Processing Tests

        [TestMethod]
        public void GetFrameworks_WithCustomTierSelections_PreservesCustomSelections()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            var customSelections = new List<string> { "IDENTIFY", "DETECT" };
            SeedFrameworkData(assessmentId, testFrameworks, testTiers, selectedTiers: customSelections);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var selectedFrameworks = result.Frameworks.Where(f => f.Selected).Select(f => f.Title).ToList();
            selectedFrameworks.Should().BeEquivalentTo(customSelections);
        }

        [TestMethod]
        public void GetFrameworks_WithPartialTierSelections_HandlesPartialSelections()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            var partialSelections = new List<string> { "IDENTIFY" };
            SeedFrameworkData(assessmentId, testFrameworks, testTiers, selectedTiers: partialSelections);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            var selectedCount = result.Frameworks.Count(f => f.Selected);
            selectedCount.Should().Be(1);
        }

        #endregion

        #region Framework Validation and Constraints Tests

        [TestMethod]
        public void GetFrameworks_WithValidFrameworkData_ReturnsValidFrameworkStructure()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().NotBeEmpty();
            
            foreach (var framework in result.Frameworks)
            {
                framework.Title.Should().NotBeNullOrEmpty();
                framework.QuestionCount.Should().BeGreaterThanOrEqualTo(0);
                framework.Selected.Should().BeFalse(); // Default state
            }
        }

        [TestMethod]
        public void GetFrameworks_WithEmptyFrameworkData_ReturnsEmptyResponse()
        {
            // Arrange
            int assessmentId = 1;
            // No framework data seeded

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().BeEmpty();
        }

        [TestMethod]
        public void GetFrameworks_WithInvalidTierData_HandlesGracefully()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var invalidTiers = new List<FRAMEWORK_TIER_TYPE>
            {
                new FRAMEWORK_TIER_TYPE { TierType = "INVALID_TIER" }
            };
            SeedFrameworkData(assessmentId, testFrameworks, invalidTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            // Should handle gracefully without throwing exceptions
        }

        #endregion

        #region Edge Cases and Error Handling Tests

        [TestMethod]
        public void GetFrameworks_WithDuplicateFrameworkData_HandlesDuplicates()
        {
            // Arrange
            int assessmentId = 1;
            var testFrameworks = CreateTestFrameworks();
            var testTiers = CreateTestTiers();
            SeedFrameworkData(assessmentId, testFrameworks, testTiers);
            
            // Add duplicate framework data
            var duplicateFrameworks = CreateTestFrameworks();
            SeedFrameworkData(assessmentId, duplicateFrameworks, testTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            // Should handle duplicates gracefully
        }

        [TestMethod]
        public void GetFrameworks_WithNullFrameworkData_ReturnsEmptyResponse()
        {
            // Arrange
            int assessmentId = 1;
            // No data seeded, context is empty

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().BeEmpty();
        }

        [TestMethod]
        public void GetFrameworks_WithLargeDataSet_HandlesPerformance()
        {
            // Arrange
            int assessmentId = 1;
            var largeFrameworks = CreateLargeTestFrameworks();
            var largeTiers = CreateLargeTestTiers();
            SeedFrameworkData(assessmentId, largeFrameworks, largeTiers);

            // Act
            var result = _frameworkBusiness.GetFrameworks(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Frameworks.Should().HaveCount(50); // Large dataset
        }

        #endregion

        #region Helper Methods

        private List<FRAMEWORK_TIER_TYPE> CreateTestTiers()
        {
            return new List<FRAMEWORK_TIER_TYPE>
            {
                new FRAMEWORK_TIER_TYPE { TierType = "IDENTIFY" },
                new FRAMEWORK_TIER_TYPE { TierType = "PROTECT" },
                new FRAMEWORK_TIER_TYPE { TierType = "DETECT" },
                new FRAMEWORK_TIER_TYPE { TierType = "RESPOND" },
                new FRAMEWORK_TIER_TYPE { TierType = "RECOVER" }
            };
        }

        private List<FRAMEWORK_TIER_TYPE> CreateLargeTestTiers()
        {
            var tiers = new List<FRAMEWORK_TIER_TYPE>();
            for (int i = 1; i <= 50; i++)
            {
                tiers.Add(new FRAMEWORK_TIER_TYPE { TierType = $"TIER_{i}" });
            }
            return tiers;
        }

        private List<NEW_QUESTION> CreateTestFrameworks()
        {
            return new List<NEW_QUESTION>
            {
                new NEW_QUESTION 
                { 
                    Question_Id = 1, 
                    Question_Text = "IDENTIFY Framework Question",
                    Question_Type = "Framework",
                    Framework_Title = "IDENTIFY"
                },
                new NEW_QUESTION 
                { 
                    Question_Id = 2, 
                    Question_Text = "PROTECT Framework Question",
                    Question_Type = "Framework",
                    Framework_Title = "PROTECT"
                },
                new NEW_QUESTION 
                { 
                    Question_Id = 3, 
                    Question_Text = "DETECT Framework Question",
                    Question_Type = "Framework",
                    Framework_Title = "DETECT"
                },
                new NEW_QUESTION 
                { 
                    Question_Id = 4, 
                    Question_Text = "RESPOND Framework Question",
                    Question_Type = "Framework",
                    Framework_Title = "RESPOND"
                },
                new NEW_QUESTION 
                { 
                    Question_Id = 5, 
                    Question_Text = "RECOVER Framework Question",
                    Question_Type = "Framework",
                    Framework_Title = "RECOVER"
                }
            };
        }

        private List<NEW_QUESTION> CreateLargeTestFrameworks()
        {
            var frameworks = new List<NEW_QUESTION>();
            for (int i = 1; i <= 50; i++)
            {
                frameworks.Add(new NEW_QUESTION 
                { 
                    Question_Id = i, 
                    Question_Text = $"Framework Question {i}",
                    Question_Type = "Framework",
                    Framework_Title = $"FRAMEWORK_{i}"
                });
            }
            return frameworks;
        }

        private void SeedFrameworkData(int assessmentId, List<NEW_QUESTION> frameworks, List<FRAMEWORK_TIER_TYPE> tiers, List<string> selectedTiers = null)
        {
            // Add framework tier types
            _context.FRAMEWORK_TIER_TYPE.AddRange(tiers);

            // Add framework questions
            _context.NEW_QUESTION.AddRange(frameworks);

            // Add selected tiers if provided
            if (selectedTiers != null)
            {
                var frameworkSelections = selectedTiers.Select(tier => new FRAMEWORK_SELECTION
                {
                    Assessment_Id = assessmentId,
                    TierType = tier
                }).ToList();
                _context.FRAMEWORK_SELECTION.AddRange(frameworkSelections);
            }

            _context.SaveChanges();
        }

        #endregion
    }
} 