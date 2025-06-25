using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Models
{
    /// <summary>
    /// Represents confidentiality level information
    /// </summary>
    public class ConfidentialityLevel
    {
        public int ConfidentialityLevelId { get; set; }
        public string ConfidentialityLevelName { get; set; } = string.Empty;
        public string ConfidentialityLevelDescription { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Represents basic report information
    /// </summary>
    public class ReportInfo
    {
        public int AssessmentId { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string AssessmentType { get; set; } = string.Empty;
        public string Workflow { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorOrganization { get; set; } = string.Empty;
        public string CreatorEmail { get; set; } = string.Empty;
        public string CreatorPhone { get; set; } = string.Empty;
        public string CreatorTitle { get; set; } = string.Empty;
        public List<string> MaturityModels { get; set; } = new List<string>();
        public List<string> Standards { get; set; } = new List<string>();
        public bool HasDiagram { get; set; }
        public string ConfidentialityLevel { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a report request
    /// </summary>
    public class ReportRequest
    {
        [Required]
        public string ReportId { get; set; } = string.Empty;
        
        public int? AggregationId { get; set; }
        
        public string ConfidentialityLevel { get; set; } = string.Empty;
        
        public bool IncludeCharts { get; set; } = true;
        
        public bool IncludeDocuments { get; set; } = false;
        
        public string Format { get; set; } = "PDF";
        
        public Dictionary<string, object> CustomParameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a report response
    /// </summary>
    public class ReportResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public byte[]? ReportData { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public int FileSize { get; set; }
    }

    /// <summary>
    /// Represents alternative answer information
    /// </summary>
    public class AltAnswer
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public DateTime? AnswerDate { get; set; }
        public string AnsweredBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents module content information
    /// </summary>
    public class ModuleContent
    {
        public string SetName { get; set; } = string.Empty;
        public string SetTitle { get; set; } = string.Empty;
        public string SetDescription { get; set; } = string.Empty;
        public List<ModuleCategory> Categories { get; set; } = new List<ModuleCategory>();
        public List<ModuleQuestion> Questions { get; set; } = new List<ModuleQuestion>();
    }

    /// <summary>
    /// Represents a module category
    /// </summary>
    public class ModuleCategory
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<ModuleQuestion> Questions { get; set; } = new List<ModuleQuestion>();
    }

    /// <summary>
    /// Represents a module question
    /// </summary>
    public class ModuleQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionTitle { get; set; } = string.Empty;
        public string QuestionDescription { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<string> AnswerOptions { get; set; } = new List<string>();
        public string Guidance { get; set; } = string.Empty;
        public List<string> References { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents model content information
    /// </summary>
    public class ModelContent
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ModelTitle { get; set; } = string.Empty;
        public string ModelDescription { get; set; } = string.Empty;
        public List<ModelLevel> Levels { get; set; } = new List<ModelLevel>();
    }

    /// <summary>
    /// Represents a model level
    /// </summary>
    public class ModelLevel
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelTitle { get; set; } = string.Empty;
        public string LevelDescription { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<ModelQuestion> Questions { get; set; } = new List<ModelQuestion>();
    }

    /// <summary>
    /// Represents a model question
    /// </summary>
    public class ModelQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionTitle { get; set; } = string.Empty;
        public string QuestionDescription { get; set; } = string.Empty;
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<string> AnswerOptions { get; set; } = new List<string>();
        public string Guidance { get; set; } = string.Empty;
        public List<string> References { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents C2M2 donut chart data
    /// </summary>
    public class C2M2DonutData
    {
        public string Category { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents C2M2 table data
    /// </summary>
    public class C2M2TableData
    {
        public string Domain { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string Practice { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents standard answered questions
    /// </summary>
    public class StandardAnsweredQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string RequirementId { get; set; } = string.Empty;
        public string RequirementText { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents standard comments and MFR
    /// </summary>
    public class StandardCommentMfr
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string MarkForReview { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string RequirementId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents reviewed questions
    /// </summary>
    public class ReviewedQuestion
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string ReviewedBy { get; set; } = string.Empty;
        public DateTime? ReviewedDate { get; set; }
        public string ReviewComment { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for generating a report
    /// </summary>
    public class GenerateReportRequest
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;
        
        public int? AssessmentId { get; set; }
        
        public int? AggregationId { get; set; }
        
        public string ConfidentialityLevel { get; set; } = string.Empty;
        
        public bool IncludeCharts { get; set; } = true;
        
        public bool IncludeDocuments { get; set; } = false;
        
        public string Format { get; set; } = "PDF";
        
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Request model for exporting to Excel
    /// </summary>
    public class ExcelExportRequest
    {
        [Required]
        public string ReportType { get; set; } = string.Empty;
        
        public int? AssessmentId { get; set; }
        
        public int? AggregationId { get; set; }
        
        public string ConfidentialityLevel { get; set; } = string.Empty;
        
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
} 