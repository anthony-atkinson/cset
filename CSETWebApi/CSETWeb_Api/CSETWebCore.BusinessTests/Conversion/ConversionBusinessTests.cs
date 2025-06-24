//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSETWebCore.Business.Contact;
using CSETWebCore.BusinessTests.Infrastructure;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Helpers;
using CSETWebCore.Model.AssessmentIO;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CSETWebCore.BusinessTests.Conversion
{
    [TestClass]
    public class ConversionBusinessTests : BaseBusinessTest
    {
        private ConversionBusiness _conversionBusiness = null!;
        private CSETContext _context = null!;
        private Mock<IAssessmentUtil> _mockAssessmentUtil = null!;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            
            // Create in-memory database context
            _context = CreateInMemoryDbContext();
            
            // Setup mocks
            _mockAssessmentUtil = new Mock<IAssessmentUtil>();
            
            // Create ConversionBusiness instance
            _conversionBusiness = new ConversionBusiness(_context, _mockAssessmentUtil.Object);
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            _context?.Dispose();
            base.TestCleanup();
        }

        #region IsEntryCF Tests

        [TestMethod]
        public void IsEntryCF_WithCFRraRecord_ReturnsTrue()
        {
            // Arrange
            var assessmentId = 1;
            var cfRraRecord = new DETAILS_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                DataItemName = "MATURITY-SUBMODEL",
                StringValue = "RRA CF"
            };
            _context.DETAILS_DEMOGRAPHICS.Add(cfRraRecord);
            _context.SaveChanges();

            // Act
            var result = _conversionBusiness.IsEntryCF(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsEntryCF_WithFloridaCSFStandard_ReturnsTrue()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V2",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            var result = _conversionBusiness.IsEntryCF(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsEntryCF_WithFloridaCSFV1Standard_ReturnsTrue()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V1",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            var result = _conversionBusiness.IsEntryCF(assessmentId);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsEntryCF_WithNoCFIndicators_ReturnsFalse()
        {
            // Arrange
            var assessmentId = 1;
            // No CF-related records added

            // Act
            var result = _conversionBusiness.IsEntryCF(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsEntryCF_WithNonSelectedFloridaStandard_ReturnsFalse()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V2",
                Selected = false
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            var result = _conversionBusiness.IsEntryCF(assessmentId);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public void IsEntryCF_WithMultipleAssessmentIds_ReturnsCorrectResults()
        {
            // Arrange
            var assessmentIds = new List<int> { 1, 2, 3 };
            
            // Assessment 1: Has CF RRA record
            var cfRraRecord = new DETAILS_DEMOGRAPHICS
            {
                Assessment_Id = 1,
                DataItemName = "MATURITY-SUBMODEL",
                StringValue = "RRA CF"
            };
            _context.DETAILS_DEMOGRAPHICS.Add(cfRraRecord);
            
            // Assessment 2: Has Florida CSF standard
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = 2,
                Set_Name = "Florida_NCSF_V2",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            
            // Assessment 3: No CF indicators
            _context.SaveChanges();

            // Act
            var results = _conversionBusiness.IsEntryCF(assessmentIds);

            // Assert
            results.Should().HaveCount(3);
            results[0].AssessmentId.Should().Be(1);
            results[0].IsEntry.Should().BeTrue();
            results[1].AssessmentId.Should().Be(2);
            results[1].IsEntry.Should().BeTrue();
            results[2].AssessmentId.Should().Be(3);
            results[2].IsEntry.Should().BeFalse();
        }

        #endregion

        #region ConvertCF Tests

        [TestMethod]
        public void ConvertCF_WithCFRraRecord_RemovesRecord()
        {
            // Arrange
            var assessmentId = 1;
            var cfRraRecord = new DETAILS_DEMOGRAPHICS
            {
                Assessment_Id = assessmentId,
                DataItemName = "MATURITY-SUBMODEL",
                StringValue = "RRA CF"
            };
            _context.DETAILS_DEMOGRAPHICS.Add(cfRraRecord);
            _context.SaveChanges();

            // Act
            _conversionBusiness.ConvertCF(assessmentId);

            // Assert
            var remainingRecord = _context.DETAILS_DEMOGRAPHICS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && 
                                   x.DataItemName == "MATURITY-SUBMODEL" && 
                                   x.StringValue == "RRA CF");
            remainingRecord.Should().BeNull();
        }

        [TestMethod]
        public void ConvertCF_WithFloridaCSFStandard_ReplacesWithNCSFV2()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V2",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            _conversionBusiness.ConvertCF(assessmentId);

            // Assert
            var oldStandard = _context.AVAILABLE_STANDARDS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && 
                                   x.Set_Name == "Florida_NCSF_V2");
            oldStandard.Should().BeNull();

            var newStandard = _context.AVAILABLE_STANDARDS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && 
                                   x.Set_Name == "NCSF_V2" && 
                                   x.Selected);
            newStandard.Should().NotBeNull();
        }

        [TestMethod]
        public void ConvertCF_AddsFormerCFEntryFlag()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V2",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            _conversionBusiness.ConvertCF(assessmentId);

            // Assert
            var formerCFFlag = _context.DETAILS_DEMOGRAPHICS
                .FirstOrDefault(x => x.Assessment_Id == assessmentId && 
                                   x.DataItemName == "FORMER-CF-ENTRY" && 
                                   x.StringValue == "true");
            formerCFFlag.Should().NotBeNull();
        }

        [TestMethod]
        public void ConvertCF_CallsTouchAssessment()
        {
            // Arrange
            var assessmentId = 1;
            var availStandard = new AVAILABLE_STANDARDS
            {
                Assessment_Id = assessmentId,
                Set_Name = "Florida_NCSF_V2",
                Selected = true
            };
            _context.AVAILABLE_STANDARDS.Add(availStandard);
            _context.SaveChanges();

            // Act
            _conversionBusiness.ConvertCF(assessmentId);

            // Assert
            _mockAssessmentUtil.Verify(x => x.TouchAssessment(assessmentId), Times.Once);
        }

        [TestMethod]
        public void ConvertCF_WithNoCFIndicators_DoesNotThrowException()
        {
            // Arrange
            var assessmentId = 1;
            // No CF-related records

            // Act & Assert
            Action act = () => _conversionBusiness.ConvertCF(assessmentId);
            act.Should().NotThrow();
        }

        #endregion
    }

    [TestClass]
    public class StandardConverterTests : BaseBusinessTest
    {
        private CSETContext _context = null!;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            _context = CreateInMemoryDbContext();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            _context?.Dispose();
            base.TestCleanup();
        }

        #region ToSet Tests

        [TestMethod]
        public async Task ToSet_ValidExternalStandard_CreatesSet()
        {
            // Arrange
            var externalStandard = new ExternalStandard
            {
                name = "Test Standard",
                shortName = "TEST",
                summary = "Test Summary",
                category = "Test Category",
                requirements = new List<ExternalRequirement>()
            };

            var category = new SETS_CATEGORY
            {
                Set_Category_Id = 1,
                Set_Category_Name = "Test Category"
            };
            _context.SETS_CATEGORY.Add(category);
            _context.SaveChanges();

            // Act
            var result = await externalStandard.ToSet(_context);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Set_Name.Should().Be("Test_Standard");
            result.Result.Full_Name.Should().Be("Test Standard");
            result.Result.Short_Name.Should().Be("TEST");
            result.Result.Standard_ToolTip.Should().Be("Test Summary");
            result.Result.Is_Custom.Should().BeTrue();
        }

        [TestMethod]
        public async Task ToSet_ExistingSetName_LogsError()
        {
            // Arrange
            var existingSet = new SETS
            {
                Set_Name = "Test_Standard",
                Full_Name = "Existing Standard"
            };
            _context.SETS.Add(existingSet);

            var category = new SETS_CATEGORY
            {
                Set_Category_Id = 1,
                Set_Category_Name = "Test Category"
            };
            _context.SETS_CATEGORY.Add(category);
            _context.SaveChanges();

            var externalStandard = new ExternalStandard
            {
                name = "Test Standard",
                shortName = "TEST",
                summary = "Test Summary",
                category = "Test Category",
                requirements = new List<ExternalRequirement>()
            };

            // Act
            var result = await externalStandard.ToSet(_context);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessages.Should().Contain("Module already exists");
        }

        [TestMethod]
        public async Task ToSet_InvalidCategory_LogsError()
        {
            // Arrange
            var externalStandard = new ExternalStandard
            {
                name = "Test Standard",
                shortName = "TEST",
                summary = "Test Summary",
                category = "Invalid Category",
                requirements = new List<ExternalRequirement>()
            };

            // Act
            var result = await externalStandard.ToSet(_context);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessages.Should().Contain("Module Category is invalid");
        }

        #endregion

        #region ToExternalStandard Tests

        [TestMethod]
        public void ToExternalStandard_ValidSet_CreatesExternalStandard()
        {
            // Arrange
            var category = new SETS_CATEGORY
            {
                Set_Category_Id = 1,
                Set_Category_Name = "Test Category"
            };
            _context.SETS_CATEGORY.Add(category);

            var set = new SETS
            {
                Set_Name = "TEST_SET",
                Full_Name = "Test Standard",
                Short_Name = "TEST",
                Standard_ToolTip = "Test Summary",
                Set_Category_Id = 1,
                NEW_REQUIREMENT = new List<NEW_REQUIREMENT>()
            };
            _context.SETS.Add(set);
            _context.SaveChanges();

            // Act
            var result = set.ToExternalStandard(_context);

            // Assert
            result.Should().NotBeNull();
            result.name.Should().Be("Test Standard");
            result.shortName.Should().Be("TEST");
            result.summary.Should().Be("Test Summary");
            result.category.Should().Be("Test Category");
        }

        #endregion
    }

    [TestClass]
    public class ReferenceConverterTests : BaseBusinessTest
    {
        [TestMethod]
        public void ToGenFile_ValidExternalDocument_CreatesGenFile()
        {
            // Arrange
            var externalDoc = new ExternalDocument
            {
                Data = new byte[] { 1, 2, 3, 4 },
                FileSize = 4,
                Name = "Test Document",
                ShortName = "TEST",
                FileName = "test.pdf"
            };

            // Act
            var result = externalDoc.ToGenFile();

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
            result.File_Size.Should().Be(4);
            result.Title.Should().Be("Test Document");
            result.Name.Should().Be("TEST");
            result.Doc_Num.Should().Be("TEST");
            result.Short_Name.Should().Be("TEST");
            result.File_Name.Should().Be("test.pdf");
            result.Is_Uploaded.Should().BeTrue();
        }

        [TestMethod]
        public void ToExternalDocument_ValidGenFile_CreatesExternalDocument()
        {
            // Arrange
            var genFile = new GEN_FILE
            {
                Data = new byte[] { 1, 2, 3, 4 },
                File_Size = 4,
                Title = "Test Document",
                Doc_Num = "TEST",
                File_Name = "test.pdf"
            };

            // Act
            var result = genFile.ToExternalDocument();

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4 });
            result.FileSize.Should().Be(4);
            result.Name.Should().Be("Test Document");
            result.ShortName.Should().Be("TEST");
            result.FileName.Should().Be("test.pdf");
        }
    }

    [TestClass]
    public class RequirementConverterTests : BaseBusinessTest
    {
        private CSETContext _context = null!;

        [TestInitialize]
        public override void TestInitialize()
        {
            base.TestInitialize();
            _context = CreateInMemoryDbContext();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            _context?.Dispose();
            base.TestCleanup();
        }

        [TestMethod]
        public async Task ToRequirement_ValidExternalRequirement_CreatesRequirement()
        {
            // Arrange
            var heading = new QUESTION_GROUP_HEADING
            {
                Question_Group_Heading_Id = 1,
                Question_Group_Heading1 = "Test Heading"
            };
            _context.QUESTION_GROUP_HEADING.Add(heading);

            var subcategory = new UNIVERSAL_SUB_CATEGORY_HEADINGS
            {
                Heading_Pair_Id = 1,
                Question_Group_Heading_Id = 1,
                Universal_Sub_Category_Id = 1
            };
            _context.UNIVERSAL_SUB_CATEGORY_HEADINGS.Add(subcategory);

            var category = new STANDARD_CATEGORY
            {
                Standard_Category_Id = 1,
                Standard_Category1 = "Test Category"
            };
            _context.STANDARD_CATEGORY.Add(category);
            _context.SaveChanges();

            var externalRequirement = new ExternalRequirement
            {
                identifier = "REQ-001",
                text = "Test requirement text",
                heading = "Test Heading",
                subheading = "Test Subheading",
                category = "Test Category",
                weight = 1.0,
                supplemental = "Test supplemental info",
                securityAssuranceLevels = new List<string> { "LOW", "MODERATE" },
                questions = new ExternalRequirement.QuestionList { "Test question?" },
                references = new List<ExternalResource>(),
                source = null
            };

            // Act
            var result = await externalRequirement.ToRequirement("TEST_SET", new ConsoleLogger(), _context);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Result.Should().NotBeNull();
            result.Result.Requirement_Title.Should().Be("REQ-001");
            result.Result.Requirement_Text.Should().Be("Test requirement text");
            result.Result.Supplemental_Info.Should().Be("Test supplemental info");
            result.Result.Weight.Should().Be(1.0);
            result.Result.REQUIREMENT_LEVELS.Should().HaveCount(2);
            result.Result.REQUIREMENT_SETS.Should().HaveCount(1);
            result.Result.REQUIREMENT_SETS.First().Set_Name.Should().Be("TEST_SET");
        }

        [TestMethod]
        public async Task ToRequirement_InvalidHeading_LogsError()
        {
            // Arrange
            var externalRequirement = new ExternalRequirement
            {
                identifier = "REQ-001",
                text = "Test requirement text",
                heading = "Invalid Heading",
                subheading = "Test Subheading",
                category = "Test Category",
                securityAssuranceLevels = new List<string>(),
                questions = new ExternalRequirement.QuestionList(),
                references = new List<ExternalResource>(),
                source = null
            };

            // Act
            var result = await externalRequirement.ToRequirement("TEST_SET", new ConsoleLogger(), _context);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessages.Should().Contain("Heading invalid for requirement");
        }

        [TestMethod]
        public async Task ToRequirement_InvalidSubheading_LogsError()
        {
            // Arrange
            var heading = new QUESTION_GROUP_HEADING
            {
                Question_Group_Heading_Id = 1,
                Question_Group_Heading1 = "Test Heading"
            };
            _context.QUESTION_GROUP_HEADING.Add(heading);
            _context.SaveChanges();

            var externalRequirement = new ExternalRequirement
            {
                identifier = "REQ-001",
                text = "Test requirement text",
                heading = "Test Heading",
                subheading = "Invalid Subheading",
                category = "Test Category",
                securityAssuranceLevels = new List<string>(),
                questions = new ExternalRequirement.QuestionList(),
                references = new List<ExternalResource>(),
                source = null
            };

            // Act
            var result = await externalRequirement.ToRequirement("TEST_SET", new ConsoleLogger(), _context);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessages.Should().Contain("Subheading invalid for requirement");
        }
    }
}