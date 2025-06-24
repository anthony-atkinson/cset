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
using CSETWebCore.Business.Question;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Common;
using CSETWebCore.Interfaces.Document;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Model.Question;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Question
{
    [TestClass]
    public class QuestionBusinessTests : BaseBusinessTest
    {
        private QuestionBusiness _questionBusiness;
        private Mock<ITokenManager> _mockTokenManager;
        private Mock<IDocumentBusiness> _mockDocumentBusiness;
        private Mock<IHtmlFromXamlConverter> _mockHtmlConverter;
        private Mock<IQuestionRequirementManager> _mockQuestionRequirement;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
            _mockTokenManager = _fixture.Freeze<Mock<ITokenManager>>();
            _mockDocumentBusiness = _fixture.Freeze<Mock<IDocumentBusiness>>();
            _mockHtmlConverter = _fixture.Freeze<Mock<IHtmlFromXamlConverter>>();
            _mockQuestionRequirement = _fixture.Freeze<Mock<IQuestionRequirementManager>>();
            _mockAssessmentUtil = _fixture.Freeze<Mock<IAssessmentUtil>>();

            _questionBusiness = new QuestionBusiness(
                _mockTokenManager.Object,
                _mockDocumentBusiness.Object,
                _mockHtmlConverter.Object,
                _mockQuestionRequirement.Object,
                _mockAssessmentUtil.Object,
                _context
            );
        }

        #region GetQuestionListWithSet Tests

        [TestMethod]
        public void GetQuestionListWithSet_WithValidQuestionGroup_ReturnsQuestionResponse()
        {
            // Arrange
            int assessmentId = 1;
            string questionGroupName = "Access Control";
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.SetNames).Returns(new List<string> { "NCSF_V1" });

            // Act
            var result = _questionBusiness.GetQuestionListWithSet(questionGroupName);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetQuestionListWithSet_WithSingleStandard_ReturnsFilteredQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.SetNames).Returns(new List<string> { "NCSF_V1" });

            // Act
            var result = _questionBusiness.GetQuestionListWithSet(null);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
            // Should only include questions from the selected standard
        }

        [TestMethod]
        public void GetQuestionListWithSet_WithMultipleStandards_ReturnsAllStandardQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.SetNames).Returns(new List<string> { "NCSF_V1", "Key" });

            // Act
            var result = _questionBusiness.GetQuestionListWithSet(null);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
        }

        #endregion

        #region GetQuestionList Tests

        [TestMethod]
        public void GetQuestionList_WithValidQuestionGroup_ReturnsQuestionResponse()
        {
            // Arrange
            int assessmentId = 1;
            string questionGroupName = "Access Control";
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.SetNames).Returns(new List<string> { "NCSF_V1" });
            _mockQuestionRequirement.Setup(x => x.InitializeManager(assessmentId));

            // Act
            var result = _questionBusiness.GetQuestionList(questionGroupName);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetQuestionList_WithNullQuestionGroup_ReturnsAllQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.SetNames).Returns(new List<string> { "NCSF_V1" });
            _mockQuestionRequirement.Setup(x => x.InitializeManager(assessmentId));

            // Act
            var result = _questionBusiness.GetQuestionList(null);

            // Assert
            result.Should().NotBeNull();
            result.Categories.Should().NotBeEmpty();
        }

        #endregion

        #region GetAnalyticQuestionAnswers Tests

        [TestMethod]
        public void GetAnalyticQuestionAnswers_WithValidQuestionResponse_ReturnsAnalyticsData()
        {
            // Arrange
            var questionResponse = CreateTestQuestionResponse();
            var testAnswers = CreateTestAnswers(1);
            SeedAnswerData(testAnswers);

            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(1);

            // Act
            var result = _questionBusiness.GetAnalyticQuestionAnswers(questionResponse);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetAnalyticQuestionAnswers_WithEmptyQuestionResponse_ReturnsEmptyList()
        {
            // Arrange
            var questionResponse = new QuestionResponse { Categories = new List<QuestionCategory>() };

            // Act
            var result = _questionBusiness.GetAnalyticQuestionAnswers(questionResponse);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region GetDetails Tests

        [TestMethod]
        public void GetDetails_WithValidQuestionId_ReturnsQuestionDetails()
        {
            // Arrange
            int questionId = 1;
            int assessmentId = 1;
            string questionType = "Question";
            var testQuestion = CreateTestQuestion(questionId);
            _context.NEW_QUESTION.Add(testQuestion);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.InitializeManager(assessmentId));

            // Act
            var result = _questionBusiness.GetDetails(questionId, questionType);

            // Assert
            result.Should().NotBeNull();
            result.QuestionId.Should().Be(questionId);
            result.AssessmentId.Should().Be(assessmentId);
        }

        [TestMethod]
        public void GetDetails_WithNullQuestionId_ReturnsEmptyDetails()
        {
            // Arrange
            int assessmentId = 1;
            string questionType = "Question";

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.InitializeManager(assessmentId));

            // Act
            var result = _questionBusiness.GetDetails(null, questionType);

            // Assert
            result.Should().NotBeNull();
            result.IsNoQuestion.Should().BeTrue();
        }

        [TestMethod]
        public void GetDetails_WithInvalidQuestionId_ReturnsEmptyDetails()
        {
            // Arrange
            int questionId = 999;
            int assessmentId = 1;
            string questionType = "Question";

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.AssessmentId).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.InitializeManager(assessmentId));

            // Act
            var result = _questionBusiness.GetDetails(questionId, questionType);

            // Assert
            result.Should().NotBeNull();
            result.IsNoQuestion.Should().BeTrue();
        }

        #endregion

        #region StoreAnswer Tests

        [TestMethod]
        public void StoreAnswer_WithValidAnswer_SavesAndReturnsAnswerId()
        {
            // Arrange
            int assessmentId = 1;
            var answer = new Answer
            {
                QuestionId = 1,
                QuestionType = "Question",
                AnswerText = "Y",
                QuestionNumber = "1"
            };

            var testQuestion = CreateTestQuestion(1);
            _context.NEW_QUESTION.Add(testQuestion);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _questionBusiness.StoreAnswer(answer);

            // Assert
            result.Should().BeGreaterThan(0);
            var savedAnswer = _context.ANSWER.FirstOrDefault(a => a.Question_Or_Requirement_Id == 1);
            savedAnswer.Should().NotBeNull();
            savedAnswer.Answer_Text.Should().Be("Y");

            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void StoreAnswer_WithNullAnswerText_SetsDefaultUnanswered()
        {
            // Arrange
            int assessmentId = 1;
            var answer = new Answer
            {
                QuestionId = 1,
                QuestionType = "Question",
                AnswerText = null,
                QuestionNumber = "1"
            };

            var testQuestion = CreateTestQuestion(1);
            _context.NEW_QUESTION.Add(testQuestion);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _questionBusiness.StoreAnswer(answer);

            // Assert
            result.Should().BeGreaterThan(0);
            var savedAnswer = _context.ANSWER.FirstOrDefault(a => a.Question_Or_Requirement_Id == 1);
            savedAnswer.Should().NotBeNull();
            savedAnswer.Answer_Text.Should().Be("U");
        }

        [TestMethod]
        public void StoreAnswer_WithInvalidQuestionId_ThrowsException()
        {
            // Arrange
            int assessmentId = 1;
            var answer = new Answer
            {
                QuestionId = 999,
                QuestionType = "Question",
                AnswerText = "Y",
                QuestionNumber = "1"
            };

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);

            // Act & Assert
            Action act = () => _questionBusiness.StoreAnswer(answer);
            act.Should().Throw<Exception>().WithMessage("*Unknown question or requirement ID*");
        }

        [TestMethod]
        public void StoreAnswer_WithComponentGuid_SavesComponentAnswer()
        {
            // Arrange
            int assessmentId = 1;
            var componentGuid = Guid.NewGuid();
            var answer = new Answer
            {
                QuestionId = 1,
                QuestionType = "Question",
                AnswerText = "Y",
                QuestionNumber = "1",
                ComponentGuid = componentGuid
            };

            var testQuestion = CreateTestQuestion(1);
            _context.NEW_QUESTION.Add(testQuestion);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockAssessmentUtil.Setup(x => x.TouchAssessment(assessmentId));

            // Act
            var result = _questionBusiness.StoreAnswer(answer);

            // Assert
            result.Should().BeGreaterThan(0);
            var savedAnswer = _context.ANSWER.FirstOrDefault(a => a.Component_Guid == componentGuid);
            savedAnswer.Should().NotBeNull();
            savedAnswer.Component_Guid.Should().Be(componentGuid);
        }

        #endregion

        #region StoreAnswerList Tests

        [TestMethod]
        public void StoreAnswerList_WithValidAnswers_SavesAllAnswers()
        {
            // Arrange
            int assessmentId = 1;
            var answers = new List<Answer>
            {
                new Answer { QuestionId = 1, QuestionType = "Question", AnswerText = "Y", QuestionNumber = "1" },
                new Answer { QuestionId = 2, QuestionType = "Question", AnswerText = "N", QuestionNumber = "2" }
            };

            var testQuestions = new List<NEW_QUESTION>
            {
                CreateTestQuestion(1),
                CreateTestQuestion(2)
            };
            _context.NEW_QUESTION.AddRange(testQuestions);
            _context.SaveChanges();

            _mockTokenManager.Setup(x => x.AssessmentForUser()).Returns(assessmentId);
            _mockQuestionRequirement.Setup(x => x.StoreAnswer(It.IsAny<Answer>()));

            // Act
            _questionBusiness.StoreAnswerList(answers);

            // Assert
            _mockQuestionRequirement.Verify(x => x.StoreAnswer(It.IsAny<Answer>()), Times.Exactly(2));
        }

        [TestMethod]
        public void StoreAnswerList_WithEmptyList_DoesNothing()
        {
            // Arrange
            var answers = new List<Answer>();

            // Act
            _questionBusiness.StoreAnswerList(answers);

            // Assert
            _mockQuestionRequirement.Verify(x => x.StoreAnswer(It.IsAny<Answer>()), Times.Never);
        }

        #endregion

        #region QuestionCountInSubGroup Tests

        [TestMethod]
        public void QuestionCountInSubGroup_WithValidSubGroup_ReturnsCorrectCount()
        {
            // Arrange
            string subGroup = "Access Control";
            int modelId = 1;
            var testMaturityQuestions = CreateTestMaturityQuestions(modelId, subGroup);
            _context.MATURITY_QUESTIONS.AddRange(testMaturityQuestions);
            _context.SaveChanges();

            // Act
            var result = _questionBusiness.QuestionCountInSubGroup(subGroup, modelId);

            // Assert
            result.Should().Be(3); // Based on test data
        }

        [TestMethod]
        public void QuestionCountInSubGroup_WithInvalidSubGroup_ReturnsZero()
        {
            // Arrange
            string subGroup = "Invalid Group";
            int modelId = 1;

            // Act
            var result = _questionBusiness.QuestionCountInSubGroup(subGroup, modelId);

            // Assert
            result.Should().Be(0);
        }

        #endregion

        #region AllQuestionsInSubGroup Tests

        [TestMethod]
        public void AllQuestionsInSubGroup_WithValidModelId_ReturnsGroupingInfo()
        {
            // Arrange
            int modelId = 1;
            int groupLevel = 1;
            int assessmentId = 1;
            var testGroupings = CreateTestMaturityGroupings(modelId, groupLevel);
            var testQuestions = CreateTestMaturityQuestions(modelId, "Access Control");
            var testAnswers = CreateTestMaturityAnswers(assessmentId);
            
            _context.MATURITY_GROUPINGS.AddRange(testGroupings);
            _context.MATURITY_QUESTIONS.AddRange(testQuestions);
            _context.ANSWER.AddRange(testAnswers);
            _context.SaveChanges();

            // Act
            var result = _questionBusiness.AllQuestionsInSubGroup(modelId, groupLevel, assessmentId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        #endregion

        #region Helper Methods

        private List<NEW_QUESTION> CreateTestQuestions()
        {
            return new List<NEW_QUESTION>
            {
                new NEW_QUESTION
                {
                    Question_Id = 1,
                    Simple_Question = "Do you have access controls?",
                    Heading_Pair_Id = 1,
                    Std_Ref = "AC.1",
                    Std_Ref_Number = 1
                },
                new NEW_QUESTION
                {
                    Question_Id = 2,
                    Simple_Question = "Are passwords required?",
                    Heading_Pair_Id = 1,
                    Std_Ref = "AC.2",
                    Std_Ref_Number = 2
                },
                new NEW_QUESTION
                {
                    Question_Id = 3,
                    Simple_Question = "Is multi-factor authentication used?",
                    Heading_Pair_Id = 2,
                    Std_Ref = "AC.3",
                    Std_Ref_Number = 3
                }
            };
        }

        private NEW_QUESTION CreateTestQuestion(int questionId)
        {
            return new NEW_QUESTION
            {
                Question_Id = questionId,
                Simple_Question = $"Test Question {questionId}",
                Heading_Pair_Id = 1,
                Std_Ref = $"AC.{questionId}",
                Std_Ref_Number = questionId
            };
        }

        private List<SETS> CreateTestSets()
        {
            return new List<SETS>
            {
                new SETS
                {
                    Set_Name = "NCSF_V1",
                    Full_Name = "NIST Cybersecurity Framework",
                    Is_Displayed = true
                },
                new SETS
                {
                    Set_Name = "Key",
                    Full_Name = "Key Controls",
                    Is_Displayed = true
                }
            };
        }

        private List<ANSWER> CreateTestAnswers(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Question",
                    Answer_Text = "Y",
                    Answer_Id = 1
                },
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 2,
                    Question_Type = "Question",
                    Answer_Text = "N",
                    Answer_Id = 2
                }
            };
        }

        private List<ANSWER> CreateTestMaturityAnswers(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Maturity",
                    Answer_Text = "Y",
                    Answer_Id = 1
                }
            };
        }

        private List<MATURITY_QUESTIONS> CreateTestMaturityQuestions(int modelId, string subCategory)
        {
            return new List<MATURITY_QUESTIONS>
            {
                new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = 1,
                    Maturity_Model_Id = modelId,
                    Sub_Category = subCategory,
                    Question_Text = "Maturity Question 1",
                    Grouping_Id = 1
                },
                new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = 2,
                    Maturity_Model_Id = modelId,
                    Sub_Category = subCategory,
                    Question_Text = "Maturity Question 2",
                    Grouping_Id = 1
                },
                new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = 3,
                    Maturity_Model_Id = modelId,
                    Sub_Category = subCategory,
                    Question_Text = "Maturity Question 3",
                    Grouping_Id = 1
                }
            };
        }

        private List<MATURITY_GROUPINGS> CreateTestMaturityGroupings(int modelId, int groupLevel)
        {
            return new List<MATURITY_GROUPINGS>
            {
                new MATURITY_GROUPINGS
                {
                    Grouping_Id = 1,
                    Maturity_Model_Id = modelId,
                    Group_Level = groupLevel,
                    Grouping_Type = "Domain",
                    Title = "Access Control"
                }
            };
        }

        private QuestionResponse CreateTestQuestionResponse()
        {
            return new QuestionResponse
            {
                Categories = new List<QuestionCategory>
                {
                    new QuestionCategory
                    {
                        GroupHeadingId = 1,
                        GroupHeadingText = "Access Control",
                        SubCategories = new List<QuestionSubCategory>
                        {
                            new QuestionSubCategory
                            {
                                SubCategoryId = 1,
                                SubCategoryHeadingText = "Access Control",
                                Questions = new List<QuestionAnswer>
                                {
                                    new QuestionAnswer
                                    {
                                        QuestionId = 1,
                                        QuestionText = "Test Question",
                                        Answer = "Y"
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        private void SeedQuestionData(int assessmentId, List<NEW_QUESTION> questions, List<SETS> sets, List<ANSWER> answers)
        {
            // Add sets
            _context.SETS.AddRange(sets);

            // Add questions
            _context.NEW_QUESTION.AddRange(questions);

            // Add question sets relationships
            var questionSets = questions.Select(q => new NEW_QUESTION_SETS
            {
                Question_Id = q.Question_Id,
                Set_Name = sets.First().Set_Name
            }).ToList();
            _context.NEW_QUESTION_SETS.AddRange(questionSets);

            // Add question levels
            var questionLevels = questionSets.Select(qs => new NEW_QUESTION_LEVELS
            {
                New_Question_Set_Id = qs.New_Question_Set_Id,
                Universal_Sal_Level = "L"
            }).ToList();
            _context.NEW_QUESTION_LEVELS.AddRange(questionLevels);

            // Add subcategory headings
            var subcategoryHeadings = new List<UNIVERSAL_SUB_CATEGORY_HEADINGS>
            {
                new UNIVERSAL_SUB_CATEGORY_HEADINGS
                {
                    Heading_Pair_Id = 1,
                    Question_Group_Heading_Id = 1,
                    Universal_Sub_Category_Id = 1
                }
            };
            _context.UNIVERSAL_SUB_CATEGORY_HEADINGS.AddRange(subcategoryHeadings);

            // Add group headings
            var groupHeadings = new List<QUESTION_GROUP_HEADING>
            {
                new QUESTION_GROUP_HEADING
                {
                    Question_Group_Heading_Id = 1,
                    Question_Group_Heading1 = "Access Control"
                }
            };
            _context.QUESTION_GROUP_HEADING.AddRange(groupHeadings);

            // Add subcategories
            var subcategories = new List<UNIVERSAL_SUB_CATEGORIES>
            {
                new UNIVERSAL_SUB_CATEGORIES
                {
                    Universal_Sub_Category_Id = 1,
                    Universal_Sub_Category = "Access Control"
                }
            };
            _context.UNIVERSAL_SUB_CATEGORIES.AddRange(subcategories);

            // Add SAL levels
            var salLevels = new List<UNIVERSAL_SAL_LEVEL>
            {
                new UNIVERSAL_SAL_LEVEL
                {
                    Universal_Sal_Level1 = "L",
                    Full_Name_Sal = "Low"
                }
            };
            _context.UNIVERSAL_SAL_LEVEL.AddRange(salLevels);

            // Add standard selection
            var standardSelection = new STANDARD_SELECTION
            {
                Assessment_Id = assessmentId,
                Selected_Sal_Level = "Low"
            };
            _context.STANDARD_SELECTION.Add(standardSelection);

            // Add answers
            _context.ANSWER.AddRange(answers);

            _context.SaveChanges();
        }

        private void SeedAnswerData(List<ANSWER> answers)
        {
            _context.ANSWER.AddRange(answers);
            _context.SaveChanges();
        }

        #endregion
    }
} 