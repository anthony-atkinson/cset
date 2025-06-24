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
using CSETWebCore.Model.Dashboard.BarCharts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Dashboard
{
    [TestClass]
    public class DashboardChartBusinessTests : BaseBusinessTest
    {
        private DashboardChartBusiness _dashboardChartBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private Mock<IAdminTabBusiness> _mockAdminTabBusiness;
        private CSETContext _context;
        private int _assessmentId = 1;
        private int _modelId = 1;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateInMemoryDbContext();
            _mockAssessmentUtil = Fixture.Freeze<Mock<IAssessmentUtil>>();
            _mockAdminTabBusiness = Fixture.Freeze<Mock<IAdminTabBusiness>>();

            // Setup test data
            SetupTestData();

            _dashboardChartBusiness = new DashboardChartBusiness(
                _assessmentId, 
                _modelId, 
                _context, 
                _mockAssessmentUtil.Object,
                _mockAdminTabBusiness.Object);
        }

        #region GetAnswerDistributionNormalized Tests

        [TestMethod]
        public void GetAnswerDistributionNormalized_WithValidAnswers_ReturnsNormalizedDistribution()
        {
            // Arrange
            var maturityAnswer1 = CreateTestMaturityAnswer(1, 1, "Y");
            var maturityAnswer2 = CreateTestMaturityAnswer(2, 2, "N");
            var maturityAnswer3 = CreateTestMaturityAnswer(3, 3, "A");
            var maturityAnswer4 = CreateTestMaturityAnswer(4, 4, "U");
            
            _context.Answer_Maturity.AddRange(maturityAnswer1, maturityAnswer2, maturityAnswer3, maturityAnswer4);
            _context.SaveChanges();

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionNormalized();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Y, N, A (U is excluded)
            
            var yesAnswer = result.FirstOrDefault(x => x.Name == "Y");
            yesAnswer.Should().NotBeNull();
            yesAnswer.Value.Should().Be(25.0); // 1 out of 4 = 25%
            
            var noAnswer = result.FirstOrDefault(x => x.Name == "N");
            noAnswer.Should().NotBeNull();
            noAnswer.Value.Should().Be(25.0); // 1 out of 4 = 25%
            
            var altAnswer = result.FirstOrDefault(x => x.Name == "A");
            altAnswer.Should().NotBeNull();
            altAnswer.Value.Should().Be(25.0); // 1 out of 4 = 25%
            
            // U should be excluded
            result.Should().NotContain(x => x.Name == "U");
        }

        [TestMethod]
        public void GetAnswerDistributionNormalized_WithNoAnswers_ReturnsEmptyList()
        {
            // Arrange - No answers in database

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionNormalized();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAnswerDistributionNormalized_WithOnlyUAnswers_ReturnsEmptyList()
        {
            // Arrange
            var maturityAnswer1 = CreateTestMaturityAnswer(1, 1, "U");
            var maturityAnswer2 = CreateTestMaturityAnswer(2, 2, "U");
            
            _context.Answer_Maturity.AddRange(maturityAnswer1, maturityAnswer2);
            _context.SaveChanges();

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionNormalized();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // U answers are excluded
        }

        [TestMethod]
        public void GetAnswerDistributionNormalized_WithUnevenDistribution_ReturnsCorrectPercentages()
        {
            // Arrange
            var maturityAnswer1 = CreateTestMaturityAnswer(1, 1, "Y");
            var maturityAnswer2 = CreateTestMaturityAnswer(2, 2, "Y");
            var maturityAnswer3 = CreateTestMaturityAnswer(3, 3, "N");
            var maturityAnswer4 = CreateTestMaturityAnswer(4, 4, "A");
            var maturityAnswer5 = CreateTestMaturityAnswer(5, 5, "U");
            
            _context.Answer_Maturity.AddRange(maturityAnswer1, maturityAnswer2, maturityAnswer3, maturityAnswer4, maturityAnswer5);
            _context.SaveChanges();

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionNormalized();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            
            var yesAnswer = result.FirstOrDefault(x => x.Name == "Y");
            yesAnswer.Should().NotBeNull();
            yesAnswer.Value.Should().Be(50.0); // 2 out of 4 = 50%
            
            var noAnswer = result.FirstOrDefault(x => x.Name == "N");
            noAnswer.Should().NotBeNull();
            noAnswer.Value.Should().Be(25.0); // 1 out of 4 = 25%
            
            var altAnswer = result.FirstOrDefault(x => x.Name == "A");
            altAnswer.Should().NotBeNull();
            altAnswer.Value.Should().Be(25.0); // 1 out of 4 = 25%
        }

        #endregion

        #region GetAnswerDistributionByDomain Tests

        [TestMethod]
        public void GetAnswerDistributionByDomain_WithValidGroupings_ReturnsDomainDistribution()
        {
            // Arrange
            var grouping = CreateTestGrouping(1, "Test Domain");
            var question1 = CreateTestQuestion(1, 1);
            var question2 = CreateTestQuestion(2, 1);
            var maturityAnswer1 = CreateTestMaturityAnswer(1, 1, "Y");
            var maturityAnswer2 = CreateTestMaturityAnswer(2, 2, "N");
            
            _context.MATURITY_GROUPINGS.Add(grouping);
            _context.MATURITY_QUESTIONS.AddRange(question1, question2);
            _context.Answer_Maturity.AddRange(maturityAnswer1, maturityAnswer2);
            _context.SaveChanges();

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionByDomain();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            
            var domain = result[0];
            domain.Name.Should().Be("Test Domain");
            domain.Series.Should().NotBeNull();
            domain.Series.Should().HaveCount(3); // Y, N, A
            
            var yesSeries = domain.Series.FirstOrDefault(x => x.Name == "Y");
            yesSeries.Should().NotBeNull();
            yesSeries.Value.Should().Be(1.0);
            
            var noSeries = domain.Series.FirstOrDefault(x => x.Name == "N");
            noSeries.Should().NotBeNull();
            noSeries.Value.Should().Be(1.0);
        }

        [TestMethod]
        public void GetAnswerDistributionByDomain_WithNoGroupings_ReturnsEmptyList()
        {
            // Arrange - No groupings in database

            // Act
            var result = _dashboardChartBusiness.GetAnswerDistributionByDomain();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void GetAnswerDistributionByDomain_WithUnselectedGroupings_ExcludesUnselectedGroupings()
        {
            // Arrange
            var modelId = 23; // CRE OPT model with selectable groupings
            var grouping1 = CreateTestGrouping(1, "Selected Domain");
            var grouping2 = CreateTestGrouping(2, "Unselected Domain");
            var groupingSelection = CreateTestGroupingSelection(1, 1); // Only grouping 1 is selected
            
            _context.MATURITY_GROUPINGS.AddRange(grouping1, grouping2);
            _context.GROUPING_SELECTION.Add(groupingSelection);
            _context.SaveChanges();

            var chartBusiness = new DashboardChartBusiness(
                _assessmentId, 
                modelId, 
                _context, 
                _mockAssessmentUtil.Object,
                _mockAdminTabBusiness.Object);

            // Act
            var result = chartBusiness.GetAnswerDistributionByDomain();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Name.Should().Be("Selected Domain");
            result.Should().NotContain(x => x.Name == "Unselected Domain");
        }

        #endregion

        #region GetAnswerCounts Tests

        [TestMethod]
        public void GetAnswerCounts_WithValidQuestionIds_ReturnsAnswerCounts()
        {
            // Arrange
            var questionIds = new List<int> { 1, 2, 3 };
            var maturityAnswer1 = CreateTestMaturityAnswer(1, 1, "Y");
            var maturityAnswer2 = CreateTestMaturityAnswer(2, 2, "Y");
            var maturityAnswer3 = CreateTestMaturityAnswer(3, 3, "N");
            var maturityAnswer4 = CreateTestMaturityAnswer(4, 4, "A"); // Different question ID
            
            _context.Answer_Maturity.AddRange(maturityAnswer1, maturityAnswer2, maturityAnswer3, maturityAnswer4);
            _context.SaveChanges();

            // Act
            var result = _dashboardChartBusiness.GetAnswerCounts(questionIds);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Y, N, A
            
            var yesCount = result.FirstOrDefault(x => x.Name == "Y");
            yesCount.Should().NotBeNull();
            yesCount.Value.Should().Be(2.0); // 2 Y answers for questions 1 and 2
            
            var noCount = result.FirstOrDefault(x => x.Name == "N");
            noCount.Should().NotBeNull();
            noCount.Value.Should().Be(1.0); // 1 N answer for question 3
            
            var altCount = result.FirstOrDefault(x => x.Name == "A");
            altCount.Should().NotBeNull();
            altCount.Value.Should().Be(0.0); // No A answers for the specified questions
        }

        [TestMethod]
        public void GetAnswerCounts_WithEmptyQuestionIds_ReturnsZeroCounts()
        {
            // Arrange
            var questionIds = new List<int>();

            // Act
            var result = _dashboardChartBusiness.GetAnswerCounts(questionIds);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Y, N, A
            
            foreach (var answer in result)
            {
                answer.Value.Should().Be(0.0);
            }
        }

        [TestMethod]
        public void GetAnswerCounts_WithInvalidQuestionIds_ReturnsZeroCounts()
        {
            // Arrange
            var questionIds = new List<int> { 999, 1000 };

            // Act
            var result = _dashboardChartBusiness.GetAnswerCounts(questionIds);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Y, N, A
            
            foreach (var answer in result)
            {
                answer.Value.Should().Be(0.0);
            }
        }

        [TestMethod]
        public void GetAnswerCounts_WithNoAnswers_ReturnsZeroCounts()
        {
            // Arrange
            var questionIds = new List<int> { 1, 2, 3 };
            // No answers in database

            // Act
            var result = _dashboardChartBusiness.GetAnswerCounts(questionIds);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3); // Y, N, A
            
            foreach (var answer in result)
            {
                answer.Value.Should().Be(0.0);
            }
        }

        #endregion

        #region Helper Methods

        private void SetupTestData()
        {
            // Create assessment
            var assessment = CreateTestAssessment(_assessmentId);
            _context.ASSESSMENTS.Add(assessment);

            // Create maturity model
            var maturityModel = CreateTestMaturityModel(_modelId);
            _context.MATURITY_MODELS.Add(maturityModel);

            // Create answer options
            var answerOption1 = CreateTestAnswerOption(_modelId, "Y");
            var answerOption2 = CreateTestAnswerOption(_modelId, "N");
            var answerOption3 = CreateTestAnswerOption(_modelId, "A");
            _context.MATURITY_ANSWER_OPTIONS.AddRange(answerOption1, answerOption2, answerOption3);

            _context.SaveChanges();
        }

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

        private MATURITY_MODELS CreateTestMaturityModel(int modelId)
        {
            return new MATURITY_MODELS
            {
                Maturity_Model_Id = modelId,
                Model_Name = $"Test Model {modelId}",
                Questions_Alias = "Questions"
            };
        }

        private MATURITY_ANSWER_OPTIONS CreateTestAnswerOption(int modelId, string optionText)
        {
            return new MATURITY_ANSWER_OPTIONS
            {
                Maturity_Model_Id = modelId,
                Option_Text = optionText
            };
        }

        private Answer_Maturity CreateTestMaturityAnswer(int answerId, int questionId, string answerText)
        {
            return new Answer_Maturity
            {
                Answer_Id = answerId,
                Question_Or_Requirement_Id = questionId,
                Assessment_Id = _assessmentId,
                Answer_Text = answerText,
                ModelId = _modelId
            };
        }

        private MATURITY_GROUPINGS CreateTestGrouping(int groupingId, string title)
        {
            return new MATURITY_GROUPINGS
            {
                Grouping_Id = groupingId,
                Maturity_Model_Id = _modelId,
                Grouping_Type = "Domain",
                Title = title,
                Sequence = 1
            };
        }

        private MATURITY_QUESTIONS CreateTestQuestion(int questionId, int groupingId)
        {
            return new MATURITY_QUESTIONS
            {
                Mat_Question_Id = questionId,
                Maturity_Model_Id = _modelId,
                Grouping_Id = groupingId,
                Question_Text = $"Test Question {questionId}",
                Sequence = questionId
            };
        }

        private GROUPING_SELECTION CreateTestGroupingSelection(int assessmentId, int groupingId)
        {
            return new GROUPING_SELECTION
            {
                Assessment_Id = assessmentId,
                Grouping_Id = groupingId
            };
        }

        #endregion
    }
} 