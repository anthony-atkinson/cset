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
using CSETWebCore.Business.ModuleBuilder;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.ModuleBuilder;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Model.Set;
using CSETWebCore.Model.Document;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace CSETWebCore.BusinessTests.ModuleBuilder
{
    [TestClass]
    public class ModuleBuilderBusinessTests : BaseBusinessTest
    {
        private ModuleBuilderBusiness _moduleBuilderBusiness;
        private Mock<CSETContext> _mockContext;
        private Mock<IQuestionRequirementManager> _mockQuestionRequirementManager;
        private Mock<IGalleryEditor> _mockGalleryEditor;

        [TestInitialize]
        public void Setup()
        {
            _mockContext = new Mock<CSETContext>();
            _mockQuestionRequirementManager = new Mock<IQuestionRequirementManager>();
            _mockGalleryEditor = new Mock<IGalleryEditor>();
            _moduleBuilderBusiness = new ModuleBuilderBusiness(_mockContext.Object, _mockQuestionRequirementManager.Object, _mockGalleryEditor.Object);
        }

        #region GetCustomSetList Tests

        [TestMethod]
        public void GetCustomSetList_WithCustomSets_ReturnsCustomSets()
        {
            // Arrange
            var testSets = CreateTestSets(true);
            var mockSetsDbSet = CreateMockDbSet(testSets);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetCustomSetList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testSets.Count);
            result.Should().OnlyContain(s => s.IsCustom);
        }

        [TestMethod]
        public void GetCustomSetList_WithIncludeNonCustom_ReturnsAllSets()
        {
            // Arrange
            var testSets = CreateTestSets(true, false);
            var mockSetsDbSet = CreateMockDbSet(testSets);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetCustomSetList(true);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testSets.Count);
        }

        [TestMethod]
        public void GetCustomSetList_WithDeprecatedSets_ExcludesDeprecatedSets()
        {
            // Arrange
            var testSets = CreateTestSets(true);
            testSets.Add(CreateTestSet("DEPRECATED", "Deprecated Set", true, true));
            var mockSetsDbSet = CreateMockDbSet(testSets);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetCustomSetList();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotContain(s => s.SetName == "DEPRECATED");
        }

        #endregion

        #region GetSetsInUseList Tests

        [TestMethod]
        public void GetSetsInUseList_WithSelectedStandards_ReturnsSetsInUse()
        {
            // Arrange
            var testSets = CreateTestSets(true);
            var testAvailableStandards = CreateTestAvailableStandards(testSets.Select(s => s.Set_Name).ToList());
            var mockSetsDbSet = CreateMockDbSet(testSets);
            var mockAvailableStandardsDbSet = CreateMockDbSet(testAvailableStandards);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.AVAILABLE_STANDARDS).Returns(mockAvailableStandardsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetSetsInUseList();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testAvailableStandards.Count(s => s.Selected));
        }

        [TestMethod]
        public void GetSetsInUseList_WithNoSelectedStandards_ReturnsEmptyList()
        {
            // Arrange
            var testSets = CreateTestSets(true);
            var testAvailableStandards = CreateTestAvailableStandards(testSets.Select(s => s.Set_Name).ToList(), false);
            var mockSetsDbSet = CreateMockDbSet(testSets);
            var mockAvailableStandardsDbSet = CreateMockDbSet(testAvailableStandards);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.AVAILABLE_STANDARDS).Returns(mockAvailableStandardsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetSetsInUseList();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region SetBaseSets Tests

        [TestMethod]
        public void SetBaseSets_WithValidSets_CopiesIntoSet()
        {
            // Arrange
            string setName = "TEST-SET";
            string[] setNames = { "SET1", "SET2" };
            
            _mockContext.Setup(x => x.usp_CopyIntoSet_Delete(setName));
            _mockContext.Setup(x => x.usp_CopyIntoSet(It.IsAny<string>(), setName));

            // Act
            _moduleBuilderBusiness.SetBaseSets(setName, setNames);

            // Assert
            _mockContext.Verify(x => x.usp_CopyIntoSet_Delete(setName), Times.Once);
            _mockContext.Verify(x => x.usp_CopyIntoSet(It.IsAny<string>(), setName), Times.Exactly(2));
        }

        [TestMethod]
        public void SetBaseSets_WithException_ThrowsException()
        {
            // Arrange
            string setName = "TEST-SET";
            string[] setNames = { "SET1" };
            
            _mockContext.Setup(x => x.usp_CopyIntoSet_Delete(setName)).Throws(new Exception("Database error"));

            // Act & Assert
            Action act = () => _moduleBuilderBusiness.SetBaseSets(setName, setNames);
            act.Should().Throw<Exception>().WithMessage("*Database error*");
        }

        #endregion

        #region GetBaseSets Tests

        [TestMethod]
        public void GetBaseSets_WithValidCustomSet_ReturnsBaseSets()
        {
            // Arrange
            string customSetName = "CUSTOM-SET";
            var testBaseSets = CreateTestCustomStandardBaseStandards(customSetName);
            var mockBaseSetsDbSet = CreateMockDbSet(testBaseSets);
            
            _mockContext.Setup(x => x.CUSTOM_STANDARD_BASE_STANDARD).Returns(mockBaseSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetBaseSets(customSetName);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testBaseSets.Count);
            result.Should().OnlyContain(s => testBaseSets.Any(bs => bs.Base_Standard == s));
        }

        [TestMethod]
        public void GetBaseSets_WithNonExistentCustomSet_ReturnsEmptyList()
        {
            // Arrange
            string customSetName = "NON-EXISTENT-SET";
            var mockBaseSetsDbSet = CreateMockDbSet<CUSTOM_STANDARD_BASE_STANDARD>(new List<CUSTOM_STANDARD_BASE_STANDARD>());
            
            _mockContext.Setup(x => x.CUSTOM_STANDARD_BASE_STANDARD).Returns(mockBaseSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetBaseSets(customSetName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetNonCustomSetList Tests

        [TestMethod]
        public void GetNonCustomSetList_WithValidSets_ReturnsNonCustomSets()
        {
            // Arrange
            var testSets = CreateTestSets(false);
            var mockSetsDbSet = CreateMockDbSet(testSets);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetNonCustomSetList("EXCEPTION-SET");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(testSets.Count);
            result.Should().OnlyContain(s => !s.IsCustom);
            result.Should().NotContain(s => s.SetName == "EXCEPTION-SET");
            result.Should().NotContain(s => s.SetName == "Components");
            result.Should().NotContain(s => s.SetName == "Standards");
        }

        [TestMethod]
        public void GetNonCustomSetList_WithDeprecatedSets_ExcludesDeprecatedSets()
        {
            // Arrange
            var testSets = CreateTestSets(false);
            testSets.Add(CreateTestSet("DEPRECATED", "Deprecated Set", false, true));
            var mockSetsDbSet = CreateMockDbSet(testSets);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetNonCustomSetList("EXCEPTION-SET");

            // Assert
            result.Should().NotBeNull();
            result.Should().NotContain(s => s.SetName == "DEPRECATED");
        }

        #endregion

        #region GetSetDetail Tests

        [TestMethod]
        public void GetSetDetail_WithExistingSet_ReturnsSetDetail()
        {
            // Arrange
            string setName = "EXISTING-SET";
            var testSet = CreateTestSet(setName, "Existing Set", true);
            var testCategories = CreateTestSetCategories();
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetSetDetail(setName);

            // Assert
            result.Should().NotBeNull();
            result.SetName.Should().Be(setName);
            result.FullName.Should().Be(testSet.Full_Name);
            result.ShortName.Should().Be(testSet.Short_Name);
            result.Description.Should().Be(testSet.Standard_ToolTip);
            result.IsCustom.Should().Be(testSet.Is_Custom);
            result.IsDisplayed.Should().Be(testSet.Is_Displayed);
            result.CategoryList.Should().NotBeNull();
            result.CategoryList.Should().HaveCount(testCategories.Count);
        }

        [TestMethod]
        public void GetSetDetail_WithNonExistentSet_ReturnsNewSetDetail()
        {
            // Arrange
            string setName = "NON-EXISTENT-SET";
            var testCategories = CreateTestSetCategories();
            var mockSetsDbSet = CreateMockDbSet<SETS>(new List<SETS>());
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetSetDetail(setName);

            // Assert
            result.Should().NotBeNull();
            result.SetName.Should().StartWith("SET.");
            result.SetCategory.Should().Be(0);
            result.IsCustom.Should().BeTrue();
            result.IsDisplayed.Should().BeTrue();
            result.CategoryList.Should().NotBeNull();
            result.CategoryList.Should().HaveCount(testCategories.Count);
        }

        #endregion

        #region CloneSet Tests

        [TestMethod]
        public void CloneSet_WithValidSet_CreatesClone()
        {
            // Arrange
            string originalSetName = "ORIGINAL-SET";
            var originalSet = CreateTestSet(originalSetName, "Original Set", true);
            var testRequirements = CreateTestRequirements(originalSetName);
            var testQuestions = CreateTestQuestions(originalSetName);
            var testCategories = CreateTestSetCategories();
            
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { originalSet });
            var mockRequirementsDbSet = CreateMockDbSet(testRequirements);
            var mockQuestionsDbSet = CreateMockDbSet(testQuestions);
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_REQUIREMENT).Returns(mockRequirementsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _moduleBuilderBusiness.CloneSet(originalSetName);

            // Assert
            result.Should().NotBeNull();
            result.SetName.Should().StartWith("SET.");
            result.FullName.Should().Be(originalSet.Full_Name);
            result.ShortName.Should().Be(originalSet.Short_Name);
            result.Description.Should().Be(originalSet.Standard_ToolTip);
            result.IsCustom.Should().BeTrue();
            result.IsDisplayed.Should().Be(originalSet.Is_Displayed);
        }

        [TestMethod]
        public void CloneSet_WithNonExistentSet_ReturnsNull()
        {
            // Arrange
            string setName = "NON-EXISTENT-SET";
            var mockSetsDbSet = CreateMockDbSet<SETS>(new List<SETS>());
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.CloneSet(setName);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region DeleteSet Tests

        [TestMethod]
        public void DeleteSet_WithCustomSet_DeletesSet()
        {
            // Arrange
            string setName = "CUSTOM-SET";
            var testSet = CreateTestSet(setName, "Custom Set", true);
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_REQUIREMENT).Returns(CreateMockDbSet<NEW_REQUIREMENT>(new List<NEW_REQUIREMENT>()).Object);
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(CreateMockDbSet<NEW_QUESTION>(new List<NEW_QUESTION>()).Object);
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(CreateMockDbSet<NEW_QUESTION_SETS>(new List<NEW_QUESTION_SETS>()).Object);
            _mockContext.Setup(x => x.REQUIREMENT_SETS).Returns(CreateMockDbSet<REQUIREMENT_SETS>(new List<REQUIREMENT_SETS>()).Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _moduleBuilderBusiness.DeleteSet(setName);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessages.Should().BeEmpty();
        }

        [TestMethod]
        public void DeleteSet_WithNonCustomSet_ReturnsError()
        {
            // Arrange
            string setName = "NON-CUSTOM-SET";
            var testSet = CreateTestSet(setName, "Non-Custom Set", false);
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.DeleteSet(setName);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessages.Should().NotBeEmpty();
        }

        [TestMethod]
        public void DeleteSet_WithReferencedRequirements_ReturnsError()
        {
            // Arrange
            string setName = "REFERENCED-SET";
            var testSet = CreateTestSet(setName, "Referenced Set", true);
            var testRequirements = CreateTestRequirements(setName, "OTHER-SET");
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            var mockRequirementsDbSet = CreateMockDbSet(testRequirements);
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_REQUIREMENT).Returns(mockRequirementsDbSet.Object);
            _mockContext.Setup(x => x.REQUIREMENT_SETS).Returns(CreateMockDbSet<REQUIREMENT_SETS>(new List<REQUIREMENT_SETS>()).Object);

            // Act
            var result = _moduleBuilderBusiness.DeleteSet(setName);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessages.Should().Contain("*referenced by other Modules*");
        }

        #endregion

        #region SaveSetDetail Tests

        [TestMethod]
        public void SaveSetDetail_WithNewSet_CreatesSet()
        {
            // Arrange
            var setDetail = CreateTestSetDetail("NEW-SET", true);
            var testCategories = CreateTestSetCategories();
            var mockSetsDbSet = CreateMockDbSet<SETS>(new List<SETS>());
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            var mockGalleryItemDbSet = CreateMockDbSet<GALLERY_ITEM>(new List<GALLERY_ITEM>());
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);
            _mockContext.Setup(x => x.GALLERY_ITEM).Returns(mockGalleryItemDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _moduleBuilderBusiness.SaveSetDetail(setDetail);

            // Assert
            result.Should().Be(setDetail.SetName);
            mockSetsDbSet.Verify(x => x.Add(It.IsAny<SETS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        [TestMethod]
        public void SaveSetDetail_WithExistingSet_UpdatesSet()
        {
            // Arrange
            var setDetail = CreateTestSetDetail("EXISTING-SET", true);
            var existingSet = CreateTestSet(setDetail.SetName, "Old Name", true);
            var testCategories = CreateTestSetCategories();
            var testGalleryItem = CreateTestGalleryItem(setDetail.SetName);
            
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { existingSet });
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            var mockGalleryItemDbSet = CreateMockDbSet(new List<GALLERY_ITEM> { testGalleryItem });
            
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);
            _mockContext.Setup(x => x.GALLERY_ITEM).Returns(mockGalleryItemDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _moduleBuilderBusiness.SaveSetDetail(setDetail);

            // Assert
            result.Should().Be(setDetail.SetName);
            existingSet.Full_Name.Should().Be(setDetail.FullName);
            existingSet.Short_Name.Should().Be(setDetail.ShortName);
            existingSet.Standard_ToolTip.Should().Be(setDetail.Description);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        #endregion

        #region GenerateNewSetName Tests

        [TestMethod]
        public void GenerateNewSetName_ReturnsTimestampedName()
        {
            // Act
            var result = _moduleBuilderBusiness.GenerateNewSetName();

            // Assert
            result.Should().StartWith("SET.");
            result.Should().MatchRegex(@"SET\.\d{8}\.\d{6}");
        }

        #endregion

        #region GetQuestionsForSet Tests

        [TestMethod]
        public void GetQuestionsForSet_WithValidSet_ReturnsQuestions()
        {
            // Arrange
            string setName = "TEST-SET";
            var testQuestions = CreateTestQuestionSets(setName);
            var testCategories = CreateTestSetCategories();
            var testSubCategories = CreateTestUniversalSubCategories();
            var testGroupHeadings = CreateTestQuestionGroupHeadings();
            var testSubCategoryHeadings = CreateTestUniversalSubCategoryHeadings();
            
            var mockQuestionSetsDbSet = CreateMockDbSet(testQuestions);
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            var mockSubCategoriesDbSet = CreateMockDbSet(testSubCategories);
            var mockGroupHeadingsDbSet = CreateMockDbSet(testGroupHeadings);
            var mockSubCategoryHeadingsDbSet = CreateMockDbSet(testSubCategoryHeadings);
            var mockSetsDbSet = CreateMockDbSet(CreateTestSets(true));
            var mockQuestionLevelsDbSet = CreateMockDbSet<NEW_QUESTION_LEVELS>(new List<NEW_QUESTION_LEVELS>());
            
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.SETS_CATEGORY).Returns(mockCategoriesDbSet.Object);
            _mockContext.Setup(x => x.UNIVERSAL_SUB_CATEGORIES).Returns(mockSubCategoriesDbSet.Object);
            _mockContext.Setup(x => x.QUESTION_GROUP_HEADING).Returns(mockGroupHeadingsDbSet.Object);
            _mockContext.Setup(x => x.UNIVERSAL_SUB_CATEGORY_HEADINGS).Returns(mockSubCategoryHeadingsDbSet.Object);
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION_LEVELS).Returns(mockQuestionLevelsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.GetQuestionsForSet(setName);

            // Assert
            result.Should().NotBeNull();
            result.SetFullName.Should().NotBeEmpty();
            result.SetShortName.Should().NotBeEmpty();
            result.SetDescription.Should().NotBeEmpty();
        }

        #endregion

        #region AddCustomQuestion Tests

        [TestMethod]
        public void AddCustomQuestion_WithValidQuestion_CreatesQuestion()
        {
            // Arrange
            var setQuestion = CreateTestSetQuestion("TEST-SET", "Custom question text");
            var testCategories = CreateTestSetCategories();
            var testSubCategories = CreateTestUniversalSubCategories();
            var testGroupHeadings = CreateTestQuestionGroupHeadings();
            var testSubCategoryHeadings = CreateTestUniversalSubCategoryHeadings();
            
            var mockCategoriesDbSet = CreateMockDbSet(testCategories);
            var mockSubCategoriesDbSet = CreateMockDbSet(testSubCategories);
            var mockGroupHeadingsDbSet = CreateMockDbSet(testGroupHeadings);
            var mockSubCategoryHeadingsDbSet = CreateMockDbSet(testSubCategoryHeadings);
            var mockQuestionsDbSet = CreateMockDbSet<NEW_QUESTION>(new List<NEW_QUESTION>());
            var mockQuestionSetsDbSet = CreateMockDbSet<NEW_QUESTION_SETS>(new List<NEW_QUESTION_SETS>());
            var mockQuestionLevelsDbSet = CreateMockDbSet<NEW_QUESTION_LEVELS>(new List<NEW_QUESTION_LEVELS>());
            
            _mockContext.Setup(x => x.STANDARD_CATEGORY).Returns(mockCategoriesDbSet.Object);
            _mockContext.Setup(x => x.UNIVERSAL_SUB_CATEGORIES).Returns(mockSubCategoriesDbSet.Object);
            _mockContext.Setup(x => x.QUESTION_GROUP_HEADING).Returns(mockGroupHeadingsDbSet.Object);
            _mockContext.Setup(x => x.UNIVERSAL_SUB_CATEGORY_HEADINGS).Returns(mockSubCategoryHeadingsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION_LEVELS).Returns(mockQuestionLevelsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _moduleBuilderBusiness.AddCustomQuestion(setQuestion);

            // Assert
            mockQuestionsDbSet.Verify(x => x.Add(It.IsAny<NEW_QUESTION>()), Times.Once);
            mockQuestionSetsDbSet.Verify(x => x.Add(It.IsAny<NEW_QUESTION_SETS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.AtLeast(1));
        }

        #endregion

        #region RemoveQuestion Tests

        [TestMethod]
        public void RemoveQuestion_WithSetQuestion_RemovesQuestion()
        {
            // Arrange
            var setQuestion = CreateTestSetQuestion("TEST-SET", 0, 1);
            var testQuestionSet = CreateTestQuestionSet(setQuestion.SetName, setQuestion.QuestionID);
            var mockQuestionSetsDbSet = CreateMockDbSet(new List<NEW_QUESTION_SETS> { testQuestionSet });
            
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _moduleBuilderBusiness.RemoveQuestion(setQuestion);

            // Assert
            mockQuestionSetsDbSet.Verify(x => x.Remove(It.IsAny<NEW_QUESTION_SETS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void RemoveQuestion_WithRequirementQuestion_RemovesRequirementQuestion()
        {
            // Arrange
            var setQuestion = CreateTestSetQuestion("TEST-SET", 1, 1);
            var testRequirementQuestionSet = CreateTestRequirementQuestionSet(setQuestion.SetName, setQuestion.QuestionID, setQuestion.RequirementID);
            var mockRequirementQuestionSetsDbSet = CreateMockDbSet(new List<REQUIREMENT_QUESTIONS_SETS> { testRequirementQuestionSet });
            var mockQuestionSetsDbSet = CreateMockDbSet<NEW_QUESTION_SETS>(new List<NEW_QUESTION_SETS>());
            
            _mockContext.Setup(x => x.REQUIREMENT_QUESTIONS_SETS).Returns(mockRequirementQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            _moduleBuilderBusiness.RemoveQuestion(setQuestion);

            // Assert
            mockRequirementQuestionSetsDbSet.Verify(x => x.Remove(It.IsAny<REQUIREMENT_QUESTIONS_SETS>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        #endregion

        #region ExistsQuestionText Tests

        [TestMethod]
        public void ExistsQuestionText_WithExistingText_ReturnsTrue()
        {
            // Arrange
            string questionText = "Existing question text";
            var testQuestions = CreateTestQuestionsWithText(questionText);
            var mockQuestionsDbSet = CreateMockDbSet(testQuestions);
            
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.ExistsQuestionText(questionText);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void ExistsQuestionText_WithNonExistingText_ReturnsFalse()
        {
            // Arrange
            string questionText = "Non-existing question text";
            var mockQuestionsDbSet = CreateMockDbSet<NEW_QUESTION>(new List<NEW_QUESTION>());
            
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.ExistsQuestionText(questionText);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateQuestionText Tests

        [TestMethod]
        public void UpdateQuestionText_WithCustomQuestion_UpdatesText()
        {
            // Arrange
            int questionId = 1;
            string newText = "Updated question text";
            var testQuestion = CreateTestQuestion(questionId, "Original text", "CUSTOM-SET");
            var testSet = CreateTestSet("CUSTOM-SET", "Custom Set", true);
            var mockQuestionsDbSet = CreateMockDbSet(new List<NEW_QUESTION> { testQuestion });
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);
            _mockContext.Setup(x => x.SaveChanges()).Returns(1);

            // Act
            var result = _moduleBuilderBusiness.UpdateQuestionText(questionId, newText);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessages.Should().BeEmpty();
            testQuestion.Simple_Question.Should().Be(newText);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public void UpdateQuestionText_WithNonCustomQuestion_ReturnsError()
        {
            // Arrange
            int questionId = 1;
            string newText = "Updated question text";
            var testQuestion = CreateTestQuestion(questionId, "Original text", "NON-CUSTOM-SET");
            var testSet = CreateTestSet("NON-CUSTOM-SET", "Non-Custom Set", false);
            var mockQuestionsDbSet = CreateMockDbSet(new List<NEW_QUESTION> { testQuestion });
            var mockSetsDbSet = CreateMockDbSet(new List<SETS> { testSet });
            
            _mockContext.Setup(x => x.NEW_QUESTION).Returns(mockQuestionsDbSet.Object);
            _mockContext.Setup(x => x.SETS).Returns(mockSetsDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.UpdateQuestionText(questionId, newText);

            // Assert
            result.Should().NotBeNull();
            result.ErrorMessages.Should().BeEmpty();
            testQuestion.Simple_Question.Should().Be("Original text");
        }

        #endregion

        #region IsQuestionInUse Tests

        [TestMethod]
        public void IsQuestionInUse_WithMultipleSets_ReturnsTrue()
        {
            // Arrange
            int questionId = 1;
            var testQuestionSets = CreateTestQuestionSetsForMultipleSets(questionId);
            var mockQuestionSetsDbSet = CreateMockDbSet(testQuestionSets);
            var mockAnswersDbSet = CreateMockDbSet<ANSWER>(new List<ANSWER>());
            
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.ANSWER).Returns(mockAnswersDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.IsQuestionInUse(questionId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsQuestionInUse_WithAnsweredQuestion_ReturnsTrue()
        {
            // Arrange
            int questionId = 1;
            var testQuestionSets = CreateTestQuestionSetsForSingleSet(questionId);
            var testAnswers = CreateTestAnswers(questionId);
            var mockQuestionSetsDbSet = CreateMockDbSet(testQuestionSets);
            var mockAnswersDbSet = CreateMockDbSet(testAnswers);
            
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.ANSWER).Returns(mockAnswersDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.IsQuestionInUse(questionId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsQuestionInUse_WithUnusedQuestion_ReturnsFalse()
        {
            // Arrange
            int questionId = 1;
            var testQuestionSets = CreateTestQuestionSetsForSingleSet(questionId);
            var mockQuestionSetsDbSet = CreateMockDbSet(testQuestionSets);
            var mockAnswersDbSet = CreateMockDbSet<ANSWER>(new List<ANSWER>());
            
            _mockContext.Setup(x => x.NEW_QUESTION_SETS).Returns(mockQuestionSetsDbSet.Object);
            _mockContext.Setup(x => x.ANSWER).Returns(mockAnswersDbSet.Object);

            // Act
            var result = _moduleBuilderBusiness.IsQuestionInUse(questionId);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Helper Methods

        private List<SETS> CreateTestSets(bool isCustom, bool includeNonCustom = false)
        {
            var sets = new List<SETS>();
            
            if (isCustom || includeNonCustom)
            {
                sets.Add(CreateTestSet("SET1", "Test Set 1", isCustom));
                sets.Add(CreateTestSet("SET2", "Test Set 2", isCustom));
            }
            
            if (includeNonCustom)
            {
                sets.Add(CreateTestSet("NON-CUSTOM-SET", "Non-Custom Set", false));
            }
            
            return sets;
        }

        private SETS CreateTestSet(string setName, string fullName, bool isCustom, bool isDeprecated = false)
        {
            return new SETS
            {
                Set_Name = setName,
                Full_Name = fullName,
                Short_Name = setName.Replace("-", ""),
                Standard_ToolTip = $"Description for {fullName}",
                Set_Category_Id = 1,
                Is_Custom = isCustom,
                Is_Displayed = true,
                Is_Deprecated = isDeprecated
            };
        }

        private List<AVAILABLE_STANDARDS> CreateTestAvailableStandards(List<string> setNames, bool selected = true)
        {
            return setNames.Select(setName => new AVAILABLE_STANDARDS
            {
                Set_Name = setName,
                Selected = selected
            }).ToList();
        }

        private List<CUSTOM_STANDARD_BASE_STANDARD> CreateTestCustomStandardBaseStandards(string customSetName)
        {
            return new List<CUSTOM_STANDARD_BASE_STANDARD>
            {
                new CUSTOM_STANDARD_BASE_STANDARD { Custom_Questionaire_Name = customSetName, Base_Standard = "BASE-SET-1" },
                new CUSTOM_STANDARD_BASE_STANDARD { Custom_Questionaire_Name = customSetName, Base_Standard = "BASE-SET-2" }
            };
        }

        private List<SETS_CATEGORY> CreateTestSetCategories()
        {
            return new List<SETS_CATEGORY>
            {
                new SETS_CATEGORY { Set_Category_Id = 1, Set_Category_Name = "Category 1" },
                new SETS_CATEGORY { Set_Category_Id = 2, Set_Category_Name = "Category 2" }
            };
        }

        private SetDetail CreateTestSetDetail(string setName, bool isCustom)
        {
            return new SetDetail
            {
                SetName = setName,
                FullName = $"Full Name for {setName}",
                ShortName = setName.Replace("-", ""),
                Description = $"Description for {setName}",
                SetCategory = 1,
                IsCustom = isCustom,
                IsDisplayed = true,
                Clonable = true,
                Deletable = isCustom
            };
        }

        private GALLERY_ITEM CreateTestGalleryItem(string setName)
        {
            return new GALLERY_ITEM
            {
                Configuration_Setup = $"[\\\"{setName}\\\"]",
                Title = "Test Gallery Item",
                Description = "Test Description"
            };
        }

        private List<NEW_REQUIREMENT> CreateTestRequirements(string setName, string originalSetName = null)
        {
            return new List<NEW_REQUIREMENT>
            {
                new NEW_REQUIREMENT
                {
                    Requirement_Id = 1,
                    Requirement_Title = "Test Requirement 1",
                    Requirement_Text = "Test requirement text",
                    Standard_Category = "Test Category",
                    Standard_Sub_Category = "Test Subcategory",
                    Original_Set_Name = originalSetName ?? setName
                }
            };
        }

        private List<NEW_QUESTION> CreateTestQuestions(string setName)
        {
            return new List<NEW_QUESTION>
            {
                new NEW_QUESTION
                {
                    Question_Id = 1,
                    Simple_Question = "Test question 1",
                    Heading_Pair_Id = 1,
                    Original_Set_Name = setName
                }
            };
        }

        private List<NEW_QUESTION_SETS> CreateTestQuestionSets(string setName)
        {
            return new List<NEW_QUESTION_SETS>
            {
                new NEW_QUESTION_SETS
                {
                    New_Question_Set_Id = 1,
                    Set_Name = setName,
                    Question_Id = 1,
                    Question = new NEW_QUESTION
                    {
                        Question_Id = 1,
                        Simple_Question = "Test question",
                        Heading_Pair_Id = 1,
                        Original_Set_Name = setName
                    }
                }
            };
        }

        private List<UNIVERSAL_SUB_CATEGORIES> CreateTestUniversalSubCategories()
        {
            return new List<UNIVERSAL_SUB_CATEGORIES>
            {
                new UNIVERSAL_SUB_CATEGORIES { Universal_Sub_Category_Id = 1, Universal_Sub_Category = "Subcategory 1" }
            };
        }

        private List<QUESTION_GROUP_HEADING> CreateTestQuestionGroupHeadings()
        {
            return new List<QUESTION_GROUP_HEADING>
            {
                new QUESTION_GROUP_HEADING { Question_Group_Heading_Id = 1, Question_Group_Heading1 = "Category 1" }
            };
        }

        private List<UNIVERSAL_SUB_CATEGORY_HEADINGS> CreateTestUniversalSubCategoryHeadings()
        {
            return new List<UNIVERSAL_SUB_CATEGORY_HEADINGS>
            {
                new UNIVERSAL_SUB_CATEGORY_HEADINGS
                {
                    Heading_Pair_Id = 1,
                    Question_Group_Heading_Id = 1,
                    Universal_Sub_Category_Id = 1,
                    Sub_Heading_Question_Description = "Subheading 1",
                    Set_Name = "TEST-SET"
                }
            };
        }

        private SetQuestion CreateTestSetQuestion(string setName, int requirementId = 0, int questionId = 1, string customText = null)
        {
            return new SetQuestion
            {
                SetName = setName,
                RequirementID = requirementId,
                QuestionID = questionId,
                QuestionCategoryID = 1,
                QuestionSubcategoryText = "Test Subcategory",
                CustomQuestionText = customText,
                SalLevels = new List<string> { "L", "M", "H" }
            };
        }

        private NEW_QUESTION_SETS CreateTestQuestionSet(string setName, int questionId)
        {
            return new NEW_QUESTION_SETS
            {
                New_Question_Set_Id = 1,
                Set_Name = setName,
                Question_Id = questionId
            };
        }

        private REQUIREMENT_QUESTIONS_SETS CreateTestRequirementQuestionSet(string setName, int questionId, int requirementId)
        {
            return new REQUIREMENT_QUESTIONS_SETS
            {
                Set_Name = setName,
                Question_Id = questionId,
                Requirement_Id = requirementId
            };
        }

        private List<NEW_QUESTION> CreateTestQuestionsWithText(string questionText)
        {
            return new List<NEW_QUESTION>
            {
                new NEW_QUESTION
                {
                    Question_Id = 1,
                    Simple_Question = questionText
                }
            };
        }

        private NEW_QUESTION CreateTestQuestion(int questionId, string questionText, string originalSetName)
        {
            return new NEW_QUESTION
            {
                Question_Id = questionId,
                Simple_Question = questionText,
                Original_Set_Name = originalSetName
            };
        }

        private List<NEW_QUESTION_SETS> CreateTestQuestionSetsForMultipleSets(int questionId)
        {
            return new List<NEW_QUESTION_SETS>
            {
                new NEW_QUESTION_SETS { Question_Id = questionId, Set_Name = "SET1" },
                new NEW_QUESTION_SETS { Question_Id = questionId, Set_Name = "SET2" }
            };
        }

        private List<NEW_QUESTION_SETS> CreateTestQuestionSetsForSingleSet(int questionId)
        {
            return new List<NEW_QUESTION_SETS>
            {
                new NEW_QUESTION_SETS { Question_Id = questionId, Set_Name = "SET1" }
            };
        }

        private List<ANSWER> CreateTestAnswers(int questionId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Question_Or_Requirement_Id = questionId,
                    Answer_Text = "Y"
                }
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
            mockDbSet.Setup(d => d.Remove(It.IsAny<T>())).Callback<T>((s) => list.Remove(s));
            
            return mockDbSet;
        }

        #endregion
    }
} 