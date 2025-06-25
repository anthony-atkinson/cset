using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Models
{
    /// <summary>
    /// Represents analytics aggregation data
    /// </summary>
    public class AnalyticsAggregation
    {
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double AverageCompletionRate { get; set; }
        public Dictionary<string, int> AssessmentsByWorkflow { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssessmentsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AssessmentsBySector { get; set; } = new Dictionary<string, int>();
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new List<MonthlyTrend>();
        public List<MaturityModelUsage> MaturityModelUsage { get; set; } = new List<MaturityModelUsage>();
        public List<StandardUsage> StandardUsage { get; set; } = new List<StandardUsage>();
    }

    /// <summary>
    /// Represents monthly trend data
    /// </summary>
    public class MonthlyTrend
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int NewAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double AverageScore { get; set; }
    }

    /// <summary>
    /// Represents maturity model usage statistics
    /// </summary>
    public class MaturityModelUsage
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ModelTitle { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Represents standard usage statistics
    /// </summary>
    public class StandardUsage
    {
        public string StandardName { get; set; } = string.Empty;
        public string StandardTitle { get; set; } = string.Empty;
        public int UsageCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Represents maturity model analytics results
    /// </summary>
    public class MaturityAnalyticsResult
    {
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string ModelTitle { get; set; } = string.Empty;
        public List<MaturityLevelData> Levels { get; set; } = new List<MaturityLevelData>();
        public List<SectorData> Sectors { get; set; } = new List<SectorData>();
        public List<QuestionCategoryData> Categories { get; set; } = new List<QuestionCategoryData>();
    }

    /// <summary>
    /// Represents maturity level data
    /// </summary>
    public class MaturityLevelData
    {
        public int LevelId { get; set; }
        public string LevelName { get; set; } = string.Empty;
        public string LevelTitle { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Represents sector data
    /// </summary>
    public class SectorData
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; } = string.Empty;
        public int AssessmentCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Represents question category data
    /// </summary>
    public class QuestionCategoryData
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }

    /// <summary>
    /// Request model for analytics token
    /// </summary>
    public class AnalyticsTokenRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string TzOffset { get; set; } = string.Empty;
        
        public string Scope { get; set; } = "CSET";
    }

    /// <summary>
    /// Response model for analytics token
    /// </summary>
    public class AnalyticsTokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request model for posting analytics data
    /// </summary>
    public class PostAnalyticsRequest
    {
        [Required]
        public string RemoteToken { get; set; } = string.Empty;
        
        public int? AssessmentId { get; set; }
        
        public bool IncludeCharts { get; set; } = true;
        
        public bool IncludeDocuments { get; set; } = false;
        
        public string ExportFormat { get; set; } = "JSON";
    }

    /// <summary>
    /// Response model for analytics posting
    /// </summary>
    public class PostAnalyticsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ExportId { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; }
        public int RecordsExported { get; set; }
    }

    /// <summary>
    /// Request model for maturity analytics
    /// </summary>
    public class MaturityAnalyticsRequest
    {
        public int? MaturityModelId { get; set; }
        public int? SectorId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// Request model for sector analytics
    /// </summary>
    public class SectorAnalyticsRequest
    {
        public int? SectorId { get; set; }
        public string? Workflow { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// Represents sector analytics results
    /// </summary>
    public class SectorAnalyticsResult
    {
        public int SectorId { get; set; }
        public string SectorName { get; set; } = string.Empty;
        public int TotalAssessments { get; set; }
        public int ActiveAssessments { get; set; }
        public int CompletedAssessments { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
        public List<WorkflowData> Workflows { get; set; } = new List<WorkflowData>();
        public List<MonthlyTrend> Trends { get; set; } = new List<MonthlyTrend>();
    }

    /// <summary>
    /// Represents workflow data
    /// </summary>
    public class WorkflowData
    {
        public string WorkflowName { get; set; } = string.Empty;
        public string WorkflowTitle { get; set; } = string.Empty;
        public int AssessmentCount { get; set; }
        public double AverageScore { get; set; }
        public double CompletionRate { get; set; }
    }
} 