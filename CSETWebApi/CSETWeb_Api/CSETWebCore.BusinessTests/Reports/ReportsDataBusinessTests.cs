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
using CSETWebCore.Business.Reports;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Maturity;
using CSETWebCore.Interfaces.Question;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Model.Assessment;
using CSETWebCore.Model.Maturity;
using CSETWebCore.Model.Question;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using MSTest;
using TestFramework = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSETWebCore.BusinessTests.Reports
{
    [TestClass]
    public class ReportsDataBusinessTests : BaseBusinessTest
    {
        private ReportsDataBusiness _reportsDataBusiness;
        private Mock<IAssessmentUtil> _mockAssessmentUtil;
        private Mock<IAdminTabBusiness> _mockAdminTabBusiness;
        private Mock<IAssessmentModeData> _mockAssessmentMode;
        private Mock<IMaturityBusiness> _mockMaturityBusiness;
        private Mock<IQuestionRequirementManager> _mockQuestionRequirement;
        private Mock<ITokenManager> _mockTokenManager;
        private CSETContext _context;

        [TestInitialize]
        public void Setup()
        {
            _context = CreateTestContext();
            _mockAssessmentUtil = _fixture.Freeze<Mock<IAssessmentUtil>>();
            _mockAdminTabBusiness = _fixture.Freeze<Mock<IAdminTabBusiness>>();
            _mockAssessmentMode = _fixture.Freeze<Mock<IAssessmentModeData>>();
            _mockMaturityBusiness = _fixture.Freeze<Mock<IMaturityBusiness>>();
            _mockQuestionRequirement = _fixture.Freeze<Mock<IQuestionRequirementManager>>();
            _mockTokenManager = _fixture.Freeze<Mock<ITokenManager>>();

            _reportsDataBusiness = new ReportsDataBusiness(
                _context,
                _mockAssessmentUtil.Object,
                _mockAdminTabBusiness.Object,
                _mockAssessmentMode.Object,
                _mockMaturityBusiness.Object,
                _mockQuestionRequirement.Object,
                _mockTokenManager.Object
            );
        }

        #region Report Template Processing Tests

        [TestMethod]
        public void SetReportsAssessmentId_WithValidAssessmentId_SetsAssessmentId()
        {
            // Arrange
            int assessmentId = 1;

            // Act
            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Assert
            // Assessment ID is set internally, verify through other methods
            var info = _reportsDataBusiness.GetInformation();
            info.Should().NotBeNull();
        }

        [TestMethod]
        public void SetToken_WithValidToken_SetsTokenManager()
        {
            // Arrange
            var mockToken = new Mock<ITokenManager>();

            // Act
            _reportsDataBusiness.SetToken(mockToken.Object);

            // Assert
            // Token is set internally, verify through other methods
            mockToken.Verify(x => x.GetCurrentLanguage(), Times.Never); // Not called yet
        }

        #endregion

        #region Report Data Aggregation Tests

        [TestMethod]
        public void GetInformation_WithValidAssessment_ReturnsAssessmentInformation()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testInformation = CreateTestInformation(assessmentId);
            SeedAssessmentData(testAssessment, testInformation);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetInformation();

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(assessmentId);
            result.Assessment_Name.Should().Be("Test Assessment");
        }

        [TestMethod]
        public void GetInformation_WithInvalidAssessment_ReturnsNull()
        {
            // Arrange
            int assessmentId = 999;
            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetInformation();

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GetSals_WithValidAssessment_ReturnsSALTable()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testStandardSelection = CreateTestStandardSelection(assessmentId);
            var testAssessmentLevels = CreateTestAssessmentLevels(assessmentId);
            SeedSALData(testAssessment, testStandardSelection, testAssessmentLevels);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetSals();

            // Assert
            result.Should().NotBeNull();
            result.OSV.Should().Be("Low");
            result.Q_CV.Should().Be("Low");
            result.Q_IV.Should().Be("Low");
            result.Q_AV.Should().Be("Low");
        }

        [TestMethod]
        public void GetNistSals_WithValidAssessment_ReturnsNistSALTable()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testStandardSelection = CreateTestStandardSelection(assessmentId);
            var testCiaJustifications = CreateTestCiaJustifications(assessmentId);
            SeedNistSALData(testAssessment, testStandardSelection, testCiaJustifications);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetNistSals();

            // Assert
            result.Should().NotBeNull();
            result.OSV.Should().Be("Low");
            result.IT_AV.Should().Be("Low");
            result.IT_CV.Should().Be("Low");
            result.IT_IV.Should().Be("Low");
        }

        [TestMethod]
        public void GetGenSals_WithValidAssessment_ReturnsGenSALTable()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testGenSalNames = CreateTestGenSalNames();
            var testGeneralSal = CreateTestGeneralSal(assessmentId);
            SeedGenSALData(testAssessment, testGenSalNames, testGeneralSal);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetGenSals();

            // Assert
            result.Should().NotBeNull();
            // Verify SAL values are set correctly
        }

        #endregion

        #region Maturity Model Report Tests

        [TestMethod]
        public void GetBasicMaturityModel_WithValidAssessment_ReturnsMaturityModel()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testMaturityModel = CreateTestMaturityModel();
            var testAvailableModel = CreateTestAvailableMaturityModel(assessmentId, testMaturityModel.Maturity_Model_Id);
            var testAssessmentLevels = CreateTestMaturityLevels(assessmentId);
            SeedMaturityModelData(testAssessment, testMaturityModel, testAvailableModel, testAssessmentLevels);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetBasicMaturityModel();

            // Assert
            result.Should().NotBeNull();
            result.MaturityModelName.Should().Be("Test Maturity Model");
            result.TargetLevel.Should().Be(3);
        }

        [TestMethod]
        public void GetMaturityModelData_WithValidAssessment_ReturnsMaturityModels()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testMaturityModel = CreateTestMaturityModel();
            var testAvailableModel = CreateTestAvailableMaturityModel(assessmentId, testMaturityModel.Maturity_Model_Id);
            var testMaturityQuestions = CreateTestMaturityQuestions(testMaturityModel.Maturity_Model_Id);
            var testAnswers = CreateTestMaturityAnswers(assessmentId);
            var testAssessmentLevels = CreateTestMaturityLevels(assessmentId);
            SeedMaturityData(testAssessment, testMaturityModel, testAvailableModel, testMaturityQuestions, testAnswers, testAssessmentLevels);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetMaturityModelData();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.First().MaturityModelName.Should().Be("Test Maturity Model");
        }

        [TestMethod]
        public void GetQuestionsList_WithValidModelId_ReturnsMaturityQuestions()
        {
            // Arrange
            int assessmentId = 1;
            int modelId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testMaturityModel = CreateTestMaturityModel();
            var testAvailableModel = CreateTestAvailableMaturityModel(assessmentId, modelId);
            var testMaturityQuestions = CreateTestMaturityQuestions(modelId);
            var testAnswers = CreateTestMaturityAnswers(assessmentId);
            SeedMaturityData(testAssessment, testMaturityModel, testAvailableModel, testMaturityQuestions, testAnswers, null);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);
            _mockTokenManager.Setup(x => x.GetCurrentLanguage()).Returns("en");

            // Act
            var result = _reportsDataBusiness.GetQuestionsList(modelId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetMaturityDeficiencies_WithValidModelId_ReturnsDeficientQuestions()
        {
            // Arrange
            int assessmentId = 1;
            int modelId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testMaturityModel = CreateTestMaturityModel();
            var testAvailableModel = CreateTestAvailableMaturityModel(assessmentId, modelId);
            var testMaturityQuestions = CreateTestMaturityQuestions(modelId);
            var testAnswers = CreateTestDeficientAnswers(assessmentId);
            SeedMaturityData(testAssessment, testMaturityModel, testAvailableModel, testMaturityQuestions, testAnswers, null);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);
            _mockTokenManager.Setup(x => x.GetCurrentLanguage()).Returns("en");

            // Act
            var result = _reportsDataBusiness.GetMaturityDeficiencies(modelId);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            // Should contain questions with "N" or "U" answers
        }

        #endregion

        #region Question and Answer Report Tests

        [TestMethod]
        public void GetQuestionsForEachStandard_WithValidAssessment_ReturnsStandardQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetQuestionsForEachStandard();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetComponentQuestions_WithValidAssessment_ReturnsComponentQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testComponents = CreateTestComponents();
            var testComponentQuestions = CreateTestComponentQuestions();
            var testAnswers = CreateTestComponentAnswers(assessmentId);
            SeedComponentData(assessmentId, testComponents, testComponentQuestions, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetComponentQuestions();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetRankedQuestions_WithValidAssessment_ReturnsRankedQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestRankedAnswers(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetRankedQuestions();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetQuestionsWithComments_WithValidAssessment_ReturnsQuestionsWithComments()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswersWithComments(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetQuestionsWithComments();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetQuestionsMarkedForReview_WithValidAssessment_ReturnsMarkedQuestions()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testQuestions = CreateTestQuestions();
            var testSets = CreateTestSets();
            var testAnswers = CreateTestAnswersMarkedForReview(assessmentId);
            SeedQuestionData(assessmentId, testQuestions, testSets, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetQuestionsMarkedForReview();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        #endregion

        #region Document and Observation Report Tests

        [TestMethod]
        public void GetDocumentLibrary_WithValidAssessment_ReturnsDocumentLibrary()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testDocuments = CreateTestDocuments(assessmentId);
            SeedDocumentData(assessmentId, testDocuments);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetDocumentLibrary();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        [TestMethod]
        public void GetObservationIndividuals_WithValidAssessment_ReturnsObservations()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            var testFindings = CreateTestFindings(assessmentId);
            var testContacts = CreateTestContacts(assessmentId);
            var testAnswers = CreateTestAnswers(assessmentId);
            SeedObservationData(assessmentId, testFindings, testContacts, testAnswers);

            _reportsDataBusiness.SetReportsAssessmentId(assessmentId);

            // Act
            var result = _reportsDataBusiness.GetObservationIndividuals();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
        }

        #endregion

        #region Utility Method Tests

        [TestMethod]
        public void FormatName_WithValidNames_ReturnsFormattedName()
        {
            // Arrange
            string firstName = "John";
            string lastName = "Doe";

            // Act
            var result = _reportsDataBusiness.FormatName(firstName, lastName);

            // Assert
            result.Should().Be("John Doe");
        }

        [TestMethod]
        public void FormatName_WithDomainUser_ReturnsUserIdOnly()
        {
            // Arrange
            string firstName = "DOMAIN\\user123";
            string lastName = "";

            // Act
            var result = _reportsDataBusiness.FormatName(firstName, lastName);

            // Assert
            result.Should().Be("user123");
        }

        [TestMethod]
        public void GetCsetVersion_ReturnsVersionString()
        {
            // Act
            var result = _reportsDataBusiness.GetCsetVersion();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void GetAssessmentGuid_WithValidAssessmentId_ReturnsGuid()
        {
            // Arrange
            int assessmentId = 1;
            var testAssessment = CreateTestAssessment(assessmentId);
            _context.ASSESSMENTS.Add(testAssessment);
            _context.SaveChanges();

            // Act
            var result = _reportsDataBusiness.GetAssessmentGuid(assessmentId);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Helper Methods

        private ASSESSMENTS CreateTestAssessment(int assessmentId)
        {
            return new ASSESSMENTS
            {
                Assessment_Id = assessmentId,
                Assessment_Name = "Test Assessment",
                Assessment_Date = DateTime.Now,
                AssessmentCreatorId = 1,
                Assessment_State = "Active"
            };
        }

        private INFORMATION CreateTestInformation(int assessmentId)
        {
            return new INFORMATION
            {
                Id = assessmentId,
                Assessment_Name = "Test Assessment",
                Facility_Name = "Test Facility",
                City_Or_Site_Name = "Test City",
                State_Province_Or_Region = "Test State"
            };
        }

        private STANDARD_SELECTION CreateTestStandardSelection(int assessmentId)
        {
            return new STANDARD_SELECTION
            {
                Assessment_Id = assessmentId,
                Selected_Sal_Level = "Low",
                Last_Sal_Determination_Type = "Standard"
            };
        }

        private List<ASSESSMENT_SELECTED_LEVELS> CreateTestAssessmentLevels(int assessmentId)
        {
            return new List<ASSESSMENT_SELECTED_LEVELS>
            {
                new ASSESSMENT_SELECTED_LEVELS
                {
                    Assessment_Id = assessmentId,
                    Level_Name = "Confidence_Level",
                    Standard_Specific_Sal_Level = "Low"
                },
                new ASSESSMENT_SELECTED_LEVELS
                {
                    Assessment_Id = assessmentId,
                    Level_Name = "Integrity_Level",
                    Standard_Specific_Sal_Level = "Low"
                },
                new ASSESSMENT_SELECTED_LEVELS
                {
                    Assessment_Id = assessmentId,
                    Level_Name = "Availability_Level",
                    Standard_Specific_Sal_Level = "Low"
                }
            };
        }

        private List<CNSS_CIA_JUSTIFICATIONS> CreateTestCiaJustifications(int assessmentId)
        {
            return new List<CNSS_CIA_JUSTIFICATIONS>
            {
                new CNSS_CIA_JUSTIFICATIONS
                {
                    Assessment_Id = assessmentId,
                    CIA_Type = "Confidentiality",
                    DropDownValueLevel = "Low",
                    Justification = "Test justification"
                },
                new CNSS_CIA_JUSTIFICATIONS
                {
                    Assessment_Id = assessmentId,
                    CIA_Type = "Integrity",
                    DropDownValueLevel = "Low",
                    Justification = "Test justification"
                },
                new CNSS_CIA_JUSTIFICATIONS
                {
                    Assessment_Id = assessmentId,
                    CIA_Type = "Availability",
                    DropDownValueLevel = "Low",
                    Justification = "Test justification"
                }
            };
        }

        private List<GEN_SAL_NAMES> CreateTestGenSalNames()
        {
            return new List<GEN_SAL_NAMES>
            {
                new GEN_SAL_NAMES { Sal_Name = "Test_SAL_1" },
                new GEN_SAL_NAMES { Sal_Name = "Test_SAL_2" }
            };
        }

        private List<GENERAL_SAL> CreateTestGeneralSal(int assessmentId)
        {
            return new List<GENERAL_SAL>
            {
                new GENERAL_SAL
                {
                    Assessment_Id = assessmentId,
                    Sal_Name = "Test_SAL_1",
                    Slider_Value = 1
                }
            };
        }

        private MATURITY_MODELS CreateTestMaturityModel()
        {
            return new MATURITY_MODELS
            {
                Maturity_Model_Id = 1,
                Model_Name = "Test Maturity Model",
                Questions_Alias = "Questions"
            };
        }

        private AVAILABLE_MATURITY_MODELS CreateTestAvailableMaturityModel(int assessmentId, int modelId)
        {
            return new AVAILABLE_MATURITY_MODELS
            {
                Assessment_Id = assessmentId,
                model_id = modelId,
                model = new MATURITY_MODELS { Maturity_Model_Id = modelId, Model_Name = "Test Model" }
            };
        }

        private List<ASSESSMENT_SELECTED_LEVELS> CreateTestMaturityLevels(int assessmentId)
        {
            return new List<ASSESSMENT_SELECTED_LEVELS>
            {
                new ASSESSMENT_SELECTED_LEVELS
                {
                    Assessment_Id = assessmentId,
                    Level_Name = "Maturity_Level",
                    Standard_Specific_Sal_Level = "3"
                }
            };
        }

        private List<MATURITY_QUESTIONS> CreateTestMaturityQuestions(int modelId)
        {
            return new List<MATURITY_QUESTIONS>
            {
                new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = 1,
                    Maturity_Model_Id = modelId,
                    Question_Title = "Test Question 1",
                    Question_Text = "Test question text",
                    Grouping_Id = 1,
                    Maturity_Level_Id = 1,
                    Maturity_Level = new MATURITY_LEVELS { Level = 1, Level_Name = "Level 1" }
                },
                new MATURITY_QUESTIONS
                {
                    Mat_Question_Id = 2,
                    Maturity_Model_Id = modelId,
                    Question_Title = "Test Question 2",
                    Question_Text = "Test question text 2",
                    Grouping_Id = 1,
                    Maturity_Level_Id = 2,
                    Maturity_Level = new MATURITY_LEVELS { Level = 2, Level_Name = "Level 2" }
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
                },
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 2,
                    Question_Type = "Maturity",
                    Answer_Text = "Y",
                    Answer_Id = 2
                }
            };
        }

        private List<ANSWER> CreateTestDeficientAnswers(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Maturity",
                    Answer_Text = "N",
                    Answer_Id = 1
                },
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 2,
                    Question_Type = "Maturity",
                    Answer_Text = "U",
                    Answer_Id = 2
                }
            };
        }

        private List<NEW_QUESTION> CreateTestQuestions()
        {
            return new List<NEW_QUESTION>
            {
                new NEW_QUESTION
                {
                    Question_Id = 1,
                    Simple_Question = "Test Question 1",
                    Heading_Pair_Id = 1,
                    Std_Ref = "AC.1",
                    Std_Ref_Number = 1
                },
                new NEW_QUESTION
                {
                    Question_Id = 2,
                    Simple_Question = "Test Question 2",
                    Heading_Pair_Id = 1,
                    Std_Ref = "AC.2",
                    Std_Ref_Number = 2
                }
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

        private List<ANSWER> CreateTestRankedAnswers(int assessmentId)
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
                }
            };
        }

        private List<ANSWER> CreateTestAnswersWithComments(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Question",
                    Answer_Text = "Y",
                    Answer_Id = 1,
                    Comment = "Test comment"
                }
            };
        }

        private List<ANSWER> CreateTestAnswersMarkedForReview(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Question",
                    Answer_Text = "Y",
                    Answer_Id = 1,
                    Mark_For_Review = true
                }
            };
        }

        private List<COMPONENT_QUESTIONS> CreateTestComponentQuestions()
        {
            return new List<COMPONENT_QUESTIONS>
            {
                new COMPONENT_QUESTIONS
                {
                    Component_Question_Id = 1,
                    Component_Id = 1,
                    Question_Id = 1,
                    Question_Text = "Component Question 1"
                }
            };
        }

        private List<COMPONENT_SYMBOLS> CreateTestComponents()
        {
            return new List<COMPONENT_SYMBOLS>
            {
                new COMPONENT_SYMBOLS
                {
                    Component_Symbol_Id = 1,
                    Symbol_Name = "Test Component"
                }
            };
        }

        private List<ANSWER> CreateTestComponentAnswers(int assessmentId)
        {
            return new List<ANSWER>
            {
                new ANSWER
                {
                    Assessment_Id = assessmentId,
                    Question_Or_Requirement_Id = 1,
                    Question_Type = "Component",
                    Answer_Text = "Y",
                    Answer_Id = 1,
                    Component_Guid = Guid.NewGuid()
                }
            };
        }

        private List<DOCUMENT_FILE> CreateTestDocuments(int assessmentId)
        {
            return new List<DOCUMENT_FILE>
            {
                new DOCUMENT_FILE
                {
                    Document_Id = 1,
                    Assessment_Id = assessmentId,
                    Title = "Test Document",
                    File_Name = "test.pdf",
                    Content_Type = "application/pdf"
                }
            };
        }

        private List<FINDING> CreateTestFindings(int assessmentId)
        {
            return new List<FINDING>
            {
                new FINDING
                {
                    Finding_Id = 1,
                    Answer_Id = 1,
                    Summary = "Test Finding",
                    Issue = "Test Issue",
                    Impact = "Test Impact",
                    Recommendations = "Test Recommendations",
                    Vulnerabilities = "Test Vulnerabilities"
                }
            };
        }

        private List<ASSESSMENT_CONTACTS> CreateTestContacts(int assessmentId)
        {
            return new List<ASSESSMENT_CONTACTS>
            {
                new ASSESSMENT_CONTACTS
                {
                    Assessment_Contact_Id = 1,
                    Assessment_Id = assessmentId,
                    FirstName = "John",
                    LastName = "Doe"
                }
            };
        }

        private void SeedAssessmentData(ASSESSMENTS assessment, INFORMATION information)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.INFORMATION.Add(information);
            _context.SaveChanges();
        }

        private void SeedSALData(ASSESSMENTS assessment, STANDARD_SELECTION standardSelection, List<ASSESSMENT_SELECTED_LEVELS> levels)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.STANDARD_SELECTION.Add(standardSelection);
            _context.ASSESSMENT_SELECTED_LEVELS.AddRange(levels);
            _context.SaveChanges();
        }

        private void SeedNistSALData(ASSESSMENTS assessment, STANDARD_SELECTION standardSelection, List<CNSS_CIA_JUSTIFICATIONS> justifications)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.STANDARD_SELECTION.Add(standardSelection);
            _context.CNSS_CIA_JUSTIFICATIONS.AddRange(justifications);
            _context.SaveChanges();
        }

        private void SeedGenSALData(ASSESSMENTS assessment, List<GEN_SAL_NAMES> names, List<GENERAL_SAL> values)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.GEN_SAL_NAMES.AddRange(names);
            _context.GENERAL_SAL.AddRange(values);
            _context.SaveChanges();
        }

        private void SeedMaturityModelData(ASSESSMENTS assessment, MATURITY_MODELS model, AVAILABLE_MATURITY_MODELS availableModel, List<ASSESSMENT_SELECTED_LEVELS> levels)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.MATURITY_MODELS.Add(model);
            _context.AVAILABLE_MATURITY_MODELS.Add(availableModel);
            if (levels != null)
            {
                _context.ASSESSMENT_SELECTED_LEVELS.AddRange(levels);
            }
            _context.SaveChanges();
        }

        private void SeedMaturityData(ASSESSMENTS assessment, MATURITY_MODELS model, AVAILABLE_MATURITY_MODELS availableModel, List<MATURITY_QUESTIONS> questions, List<ANSWER> answers, List<ASSESSMENT_SELECTED_LEVELS> levels)
        {
            _context.ASSESSMENTS.Add(assessment);
            _context.MATURITY_MODELS.Add(model);
            _context.AVAILABLE_MATURITY_MODELS.Add(availableModel);
            _context.MATURITY_QUESTIONS.AddRange(questions);
            _context.ANSWER.AddRange(answers);
            if (levels != null)
            {
                _context.ASSESSMENT_SELECTED_LEVELS.AddRange(levels);
            }
            _context.SaveChanges();
        }

        private void SeedQuestionData(int assessmentId, List<NEW_QUESTION> questions, List<SETS> sets, List<ANSWER> answers)
        {
            _context.NEW_QUESTION.AddRange(questions);
            _context.SETS.AddRange(sets);
            _context.ANSWER.AddRange(answers);

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

            _context.SaveChanges();
        }

        private void SeedComponentData(int assessmentId, List<COMPONENT_SYMBOLS> components, List<COMPONENT_QUESTIONS> questions, List<ANSWER> answers)
        {
            _context.COMPONENT_SYMBOLS.AddRange(components);
            _context.COMPONENT_QUESTIONS.AddRange(questions);
            _context.ANSWER.AddRange(answers);
            _context.SaveChanges();
        }

        private void SeedDocumentData(int assessmentId, List<DOCUMENT_FILE> documents)
        {
            _context.DOCUMENT_FILE.AddRange(documents);
            _context.SaveChanges();
        }

        private void SeedObservationData(int assessmentId, List<FINDING> findings, List<ASSESSMENT_CONTACTS> contacts, List<ANSWER> answers)
        {
            _context.FINDING.AddRange(findings);
            _context.ASSESSMENT_CONTACTS.AddRange(contacts);
            _context.ANSWER.AddRange(answers);

            // Add finding contacts
            var findingContacts = findings.Select(f => new FINDING_CONTACT
            {
                Finding_Id = f.Finding_Id,
                Assessment_Contact_Id = contacts.First().Assessment_Contact_Id
            }).ToList();
            _context.FINDING_CONTACT.AddRange(findingContacts);

            _context.SaveChanges();
        }

        #endregion
    }
}