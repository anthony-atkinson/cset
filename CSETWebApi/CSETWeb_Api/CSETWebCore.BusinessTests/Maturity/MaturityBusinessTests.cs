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
using CSETWebCore.Business.Maturity;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Maturity;
using CSETWebCore.Model.Question;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Maturity
{
    [TestClass]
    [TestCategory("Unit")]
    public class MaturityBusinessTests : BaseBusinessTest
    {
        private MaturityBusiness _maturityBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private Mock<IAdminTabBusiness> _mockAdminTabBusiness;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateInMemoryDbContext();
            _mockAssessmentUtil = Fixture.Freeze<Mock<IAssessmentUtil>>();
            _mockAdminTabBusiness = Fixture.Freeze<Mock<IAdminTabBusiness>>();

            _maturityBusiness = new MaturityBusiness(
                _context,
                _mockAssessmentUtil.Object,
                _mockAdminTabBusiness.Object
            );
        }

        #region Maturity Model Retrieval Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityModel_WithValidAssessmentId_ReturnsMaturityModel()
        {
            // Arrange
            int assessmentId = 1;
            var assessment = CreateTestAssessment("Test Assessment");
            var maturityModel = CreateTestMaturityModel(1, "EDM", "Enterprise Defense Model");
            var availableModel = CreateTestAvailableMaturityModel(assessmentId, 1);
            
            SeedMaturityModelData(assessment, maturityModel, availableModel);

            // Act
            var result = _maturityBusiness.GetMaturityModel(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.ModelId.Should().Be(1);
            result.ModelName.Should().Be("EDM");
            result.ModelTitle.Should().Be("Enterprise Defense Model");
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityModel_WithInvalidAssessmentId_ReturnsNull()
        {
            // Arrange
            int assessmentId = 999;

            // Act
            var result = _maturityBusiness.GetMaturityModel(assessmentId);

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityModel_WithGalleryItem_IncludesDescription()
        {
            // Arrange
            int assessmentId = 1;
            var assessment = CreateTestAssessment("Test Assessment");
            var maturityModel = CreateTestMaturityModel(1, "EDM", "Enterprise Defense Model");
            var availableModel = CreateTestAvailableMaturityModel(assessmentId, 1);
            var galleryItem = CreateTestGalleryItem(assessment.GalleryItemGuid, "Test Description");
            
            SeedMaturityModelData(assessment, maturityModel, availableModel, galleryItem);

            // Act
            var result = _maturityBusiness.GetMaturityModel(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.ModelDescription.Should().Be("Test Description");
        }

        #endregion

        #region Maturity Level Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityLevelsForModel_WithValidModelId_ReturnsLevels()
        {
            // Arrange
            int modelId = 1;
            int targetLevel = 3;
            var maturityLevels = CreateTestMaturityLevels(modelId, 5);

            SeedMaturityLevelsData(maturityLevels);

            // Act
            var result = _maturityBusiness.GetMaturityLevelsForModel(modelId, targetLevel);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Where(l => l.Level <= targetLevel).Should().OnlyContain(l => l.Applicable);
            result.Where(l => l.Level > targetLevel).Should().OnlyContain(l => !l.Applicable);
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityLevelsForModel_WithZeroTargetLevel_ReturnsAllLevelsAsNotApplicable()
        {
            // Arrange
            int modelId = 1;
            int targetLevel = 0;
            var maturityLevels = CreateTestMaturityLevels(modelId, 3);

            SeedMaturityLevelsData(maturityLevels);

            // Act
            var result = _maturityBusiness.GetMaturityLevelsForModel(modelId, targetLevel);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().OnlyContain(l => !l.Applicable);
        }

        #endregion

        #region Target Level Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityTargetLevel_WithValidAssessment_ReturnsTargetLevel()
        {
            // Arrange
            int assessmentId = 1;
            var selectedLevel = CreateTestAssessmentSelectedLevel(assessmentId, "Maturity Level", "3");

            SeedAssessmentSelectedLevelData(selectedLevel);

            // Act
            var result = _maturityBusiness.GetMaturityTargetLevel(assessmentId);

            // Assert
            result.Should().Be(3);
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetMaturityTargetLevel_WithNoSelectedLevel_ReturnsZero()
        {
            // Arrange
            int assessmentId = 1;

            // Act
            var result = _maturityBusiness.GetMaturityTargetLevel(assessmentId);

            // Assert
            result.Should().Be(0);
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetTargetLevel_WithValidAssessment_ReturnsTargetLevel()
        {
            // Arrange
            int assessmentId = 1;
            var selectedLevel = CreateTestAssessmentSelectedLevel(assessmentId, "Maturity Level", "2");

            SeedAssessmentSelectedLevelData(selectedLevel);

            // Act
            var result = _maturityBusiness.GetTargetLevel(assessmentId);

            // Assert
            result.Should().Be(2);
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetTargetLevel_WithInvalidLevelString_ReturnsZero()
        {
            // Arrange
            int assessmentId = 1;
            var selectedLevel = CreateTestAssessmentSelectedLevel(assessmentId, "Maturity Level", "invalid");

            SeedAssessmentSelectedLevelData(selectedLevel);

            // Act
            var result = _maturityBusiness.GetTargetLevel(assessmentId);

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region Model Persistence Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void PersistSelectedMaturityModel_WithValidModelName_SavesModel()
        {
            // Arrange
            int assessmentId = 1;
            string modelName = "EDM";
            var maturityModel = CreateTestMaturityModel(1, modelName, "Enterprise Defense Model");

            SeedMaturityModelsData(new List<MATURITY_MODELS> { maturityModel });

            // Act
            _maturityBusiness.PersistSelectedMaturityModel(assessmentId, modelName);

            // Assert
            var savedModel = _context.AVAILABLE_MATURITY_MODELS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && x.model_id == 1);
            savedModel.Should().NotBeNull();
            savedModel.Selected.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void PersistSelectedMaturityModel_WithInvalidModelName_DoesNothing()
        {
            // Arrange
            int assessmentId = 1;
            string modelName = "InvalidModel";

            // Act
            _maturityBusiness.PersistSelectedMaturityModel(assessmentId, modelName);

            // Assert
            var savedModel = _context.AVAILABLE_MATURITY_MODELS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId);
            savedModel.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void PersistSelectedMaturityModel_WithExistingModel_DoesNotDuplicate()
        {
            // Arrange
            int assessmentId = 1;
            string modelName = "EDM";
            var maturityModel = CreateTestMaturityModel(1, modelName, "Enterprise Defense Model");
            var existingModel = CreateTestAvailableMaturityModel(assessmentId, 1);

            SeedMaturityModelsData(new List<MATURITY_MODELS> { maturityModel });
            SeedAvailableMaturityModelsData(new List<AVAILABLE_MATURITY_MODELS> { existingModel });

            // Act
            _maturityBusiness.PersistSelectedMaturityModel(assessmentId, modelName);

            // Assert
            var savedModels = _context.AVAILABLE_MATURITY_MODELS
                .Where(x => x.Assessment_Id == assessmentId && x.model_id == 1);
            savedModels.Should().HaveCount(1);
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void PersistSelectedMaturityModel_WithCMMCModel_SetsDefaultTargetLevel()
        {
            // Arrange
            int assessmentId = 1;
            string modelName = "CMMC";
            var maturityModel = CreateTestMaturityModel(1, modelName, "CMMC Model");

            SeedMaturityModelsData(new List<MATURITY_MODELS> { maturityModel });

            // Act
            _maturityBusiness.PersistSelectedMaturityModel(assessmentId, modelName);

            // Assert
            var targetLevel = _context.ASSESSMENT_SELECTED_LEVELS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && x.Level_Name == "Maturity Level");
            targetLevel.Should().NotBeNull();
            targetLevel.Standard_Specific_Sal_Level.Should().Be("1");
        }

        #endregion

        #region Answer Storage Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void StoreAnswer_WithValidAnswer_SavesAnswer()
        {
            // Arrange
            int assessmentId = 1;
            var maturityQuestion = CreateTestMaturityQuestion(1, 1);
            var answer = new Answer
            {
                QuestionId = 1,
                QuestionType = "Maturity",
                AnswerText = "Y",
                OptionId = null
            };

            SeedMaturityQuestionsData(new List<MATURITY_QUESTIONS> { maturityQuestion });

            // Act
            var result = _maturityBusiness.StoreAnswer(assessmentId, answer);

            // Assert
            result.Should().NotBeNull();
            result.QuestionId.Should().Be(1);
            result.AnswerText.Should().Be("Y");

            var savedAnswer = _context.ANSWER
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && x.Question_Or_Requirement_Id == 1);
            savedAnswer.Should().NotBeNull();
            savedAnswer.Answer_Text.Should().Be("Y");
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void StoreAnswer_WithNullAnswerText_SetsDefaultUnanswered()
        {
            // Arrange
            int assessmentId = 1;
            var maturityQuestion = CreateTestMaturityQuestion(1, 1);
            var answer = new Answer
            {
                QuestionId = 1,
                QuestionType = "Maturity",
                AnswerText = null,
                OptionId = null
            };

            SeedMaturityQuestionsData(new List<MATURITY_QUESTIONS> { maturityQuestion });

            // Act
            var result = _maturityBusiness.StoreAnswer(assessmentId, answer);

            // Assert
            result.Should().NotBeNull();
            result.AnswerText.Should().Be("U");

            var savedAnswer = _context.ANSWER
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && x.Question_Or_Requirement_Id == 1);
            savedAnswer.Should().NotBeNull();
            savedAnswer.Answer_Text.Should().Be("U");
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void StoreAnswer_WithInvalidQuestionId_ThrowsException()
        {
            // Arrange
            int assessmentId = 1;
            var answer = new Answer
            {
                QuestionId = 999,
                QuestionType = "Maturity",
                AnswerText = "Y",
                OptionId = null
            };

            // Act & Assert
            Action action = () => _maturityBusiness.StoreAnswer(assessmentId, answer);
            action.Should().Throw<Exception>().WithMessage("*Unknown question or requirement ID*");
        }

        #endregion

        #region Score Calculation Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void Get_LevelScoresByGroup_WithValidData_ReturnsScores()
        {
            // Arrange
            int assessmentId = 1;
            int modelId = 1;
            var maturityQuestions = CreateTestMaturityQuestions(modelId, 3);
            var answers = CreateTestMaturityAnswers(assessmentId, new Dictionary<int, string>
            {
                { 1, "Y" },
                { 2, "N" },
                { 3, "Y" }
            });

            SeedMaturityQuestionsData(maturityQuestions);
            SeedAnswerData(answers);

            // Act
            var result = _maturityBusiness.Get_LevelScoresByGroup(assessmentId, modelId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        #endregion

        #region Answer Distribution Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void GetAnswerDistributionByLevel_WithValidData_ReturnsDistribution()
        {
            // Arrange
            int assessmentId = 1;
            var model = CreateTestAvailableMaturityModel(assessmentId, 1);
            var levels = CreateTestMaturityLevels(1, 3);
            var questions = CreateTestMaturityQuestions(1, 6);
            var answers = CreateTestMaturityAnswers(assessmentId, new Dictionary<int, string>
            {
                { 1, "Y" }, { 2, "N" }, { 3, "Y" },
                { 4, "U" }, { 5, "Y" }, { 6, "N" }
            });

            SeedAvailableMaturityModelsData(new List<AVAILABLE_MATURITY_MODELS> { model });
            SeedMaturityLevelsData(levels);
            SeedMaturityQuestionsData(questions);
            SeedAnswerData(answers);

            // Act
            var result = _maturityBusiness.GetAnswerDistributionByLevel(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().HaveCount(3); // One for each level
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void GetAnswerDistributionByDomain_WithValidData_ReturnsDistribution()
        {
            // Arrange
            int assessmentId = 1;
            var model = CreateTestAvailableMaturityModel(assessmentId, 1);
            var groupings = CreateTestMaturityGroupings(1, 2);
            var questions = CreateTestMaturityQuestions(1, 4);
            var answers = CreateTestMaturityAnswers(assessmentId, new Dictionary<int, string>
            {
                { 1, "Y" }, { 2, "N" }, { 3, "Y" }, { 4, "U" }
            });

            SeedAvailableMaturityModelsData(new List<AVAILABLE_MATURITY_MODELS> { model });
            SeedMaturityGroupingsData(groupings);
            SeedMaturityQuestionsData(questions);
            SeedAnswerData(answers);

            // Act
            var result = _maturityBusiness.GetAnswerDistributionByDomain(assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        #endregion

        #region Model Management Tests

        [TestMethod]
        [TestCategory("Fast")]
        public void GetAllModels_ReturnsAllMaturityModels()
        {
            // Arrange
            var models = new List<MATURITY_MODELS>
            {
                CreateTestMaturityModel(1, "EDM", "Enterprise Defense Model"),
                CreateTestMaturityModel(2, "CMMC", "CMMC Model"),
                CreateTestMaturityModel(3, "CRR", "CRR Model")
            };

            SeedMaturityModelsData(models);

            // Act
            var result = _maturityBusiness.GetAllModels();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(m => m.ModelName == "EDM");
            result.Should().Contain(m => m.ModelName == "CMMC");
            result.Should().Contain(m => m.ModelName == "CRR");
        }

        [TestMethod]
        [TestCategory("Fast")]
        public void ClearMaturityModel_WithValidAssessment_RemovesModel()
        {
            // Arrange
            int assessmentId = 1;
            var availableModel = CreateTestAvailableMaturityModel(assessmentId, 1);
            var selectedLevel = CreateTestAssessmentSelectedLevel(assessmentId, "Maturity Level", "3");

            SeedAvailableMaturityModelsData(new List<AVAILABLE_MATURITY_MODELS> { availableModel });
            SeedAssessmentSelectedLevelData(selectedLevel);

            // Act
            _maturityBusiness.ClearMaturityModel(assessmentId);

            // Assert
            var remainingModel = _context.AVAILABLE_MATURITY_MODELS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId);
            remainingModel.Should().BeNull();

            var remainingLevel = _context.ASSESSMENT_SELECTED_LEVELS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && x.Level_Name == "Maturity Level");
            remainingLevel.Should().BeNull();
        }

        #endregion

        #region Helper Methods

        private CSETContext CreateTestContext()
        {
            var options = new DbContextOptionsBuilder<CSETContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options;
            return new CSETContext(options);
        }

        private ASSESSMENTS CreateTestAssessment(string name)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = Faker.Random.Int(1, 10000),
                Assessment_Name = name,
                GalleryItemGuid = Guid.NewGuid(),
                Assessment_Date = Faker.Date.Recent(),
                CreatedDate = DateTime.UtcNow
            };
        }

        private MATURITY_MODELS CreateTestMaturityModel(int id, string name, string title)
        {
            return new MATURITY_MODELS
            {
                Maturity_Model_Id = id,
                Model_Name = name,
                Model_Title = title,
                Questions_Alias = "Questions"
            };
        }

        private AVAILABLE_MATURITY_MODELS CreateTestAvailableMaturityModel(int assessmentId, int modelId)
        {
            return new AVAILABLE_MATURITY_MODELS
            {
                Assessment_Id = assessmentId,
                model_id = modelId,
                Selected = true
            };
        }

        private GALLERY_ITEM CreateTestGalleryItem(Guid guid, string description)
        {
            return new GALLERY_ITEM
            {
                Gallery_Item_Guid = guid,
                Description = description
            };
        }

        private List<MATURITY_LEVELS> CreateTestMaturityLevels(int modelId, int count)
        {
            var levels = new List<MATURITY_LEVELS>();
            for (int i = 1; i <= count; i++)
            {
                levels.Add(new MATURITY_LEVELS
                {
                    Maturity_Level_Id = i,
                    Maturity_Model_Id = modelId,
                    Level = i,
                    Level_Name = $"Level {i}"
                });
            }
            return levels;
        }

        private ASSESSMENT_SELECTED_LEVELS CreateTestAssessmentSelectedLevel(int assessmentId, string levelName, string levelValue)
        {
            return new ASSESSMENT_SELECTED_LEVELS
            {
                Assessment_Id = assessmentId,
                Level_Name = levelName,
                Standard_Specific_Sal_Level = levelValue
            };
        }

        private MATURITY_QUESTIONS CreateTestMaturityQuestion(int questionId, int modelId)
        {
            return new MATURITY_QUESTIONS
            {
                Mat_Question_Id = questionId,
                Maturity_Model_Id = modelId,
                Question_Text = $"Test Question {questionId}",
                Question_Title = $"Question {questionId}",
                Maturity_Level_Id = 1
            };
        }

        private List<MATURITY_QUESTIONS> CreateTestMaturityQuestions(int modelId, int count)
        {
            var questions = new List<MATURITY_QUESTIONS>();
            for (int i = 1; i <= count; i++)
            {
                questions.Add(new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = i,
                    Maturity_Model_Id = modelId,
                    Question_Text = $"Test Question {i}",
                    Question_Title = $"Question {i}",
                    Maturity_Level_Id = ((i - 1) % 3) + 1
                });
            }
            return questions;
        }

        private List<ANSWER> CreateTestMaturityAnswers(int assessmentId, Dictionary<int, string> questionAnswers)
        {
            var answers = new List<ANSWER>();
            foreach (var kvp in questionAnswers)
            {
                answers.Add(new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = kvp.Key,
                    Question_Type = "Maturity",
                    Answer_Text = kvp.Value,
                    Is_Maturity = true
                });
            }
            return answers;
        }

        private List<MATURITY_GROUPINGS> CreateTestMaturityGroupings(int modelId, int count)
        {
            var groupings = new List<MATURITY_GROUPINGS>();
            for (int i = 1; i <= count; i++)
            {
                groupings.Add(new MATURITY_GROUPINGS
                {
                    Grouping_Id = i,
                    Maturity_Model_Id = modelId,
                    Title = $"Group {i}",
                    Grouping_Type = "Domain"
                });
            }
            return groupings;
        }

        private void SeedMaturityModelData(ASSESSMENTS assessment, MATURITY_MODELS model, AVAILABLE_MATURITY_MODELS availableModel, GALLERY_ITEM? galleryItem = null)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.MATURITY_MODELS.Add(model);
            _context.AVAILABLE_MATURITY_MODELS.Add(availableModel);
            if (galleryItem != null)
            {
                _context.GALLERY_ITEM.Add(galleryItem);
            }
            _context.SaveChanges();
        }

        private void SeedMaturityLevelsData(List<MATURITY_LEVELS> levels)
        {
            _context.MATURITY_LEVELS.AddRange(levels);
            _context.SaveChanges();
        }

        private void SeedAssessmentSelectedLevelData(ASSESSMENT_SELECTED_LEVELS selectedLevel)
        {
            _context.ASSESSMENT_SELECTED_LEVELS.Add(selectedLevel);
            _context.SaveChanges();
        }

        private void SeedMaturityModelsData(List<MATURITY_MODELS> models)
        {
            _context.MATURITY_MODELS.AddRange(models);
            _context.SaveChanges();
        }

        private void SeedAvailableMaturityModelsData(List<AVAILABLE_MATURITY_MODELS> availableModels)
        {
            _context.AVAILABLE_MATURITY_MODELS.AddRange(availableModels);
            _context.SaveChanges();
        }

        private void SeedMaturityQuestionsData(List<MATURITY_QUESTIONS> questions)
        {
            _context.MATURITY_QUESTIONS.AddRange(questions);
            _context.SaveChanges();
        }

        private void SeedAnswerData(List<ANSWER> answers)
        {
            _context.ANSWER.AddRange(answers);
            _context.SaveChanges();
        }

        private void SeedMaturityGroupingsData(List<MATURITY_GROUPINGS> groupings)
        {
            _context.MATURITY_GROUPINGS.AddRange(groupings);
            _context.SaveChanges();
        }

        #endregion
    }
}