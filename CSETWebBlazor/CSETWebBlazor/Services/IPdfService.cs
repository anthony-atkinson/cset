namespace CSETWebBlazor.Services
{
    public interface IPdfService
    {
        /// <summary>
        /// Generates a basic PDF report
        /// </summary>
        Task<byte[]> GenerateBasicReportAsync(ReportData data);
        
        /// <summary>
        /// Generates an assessment report with charts
        /// </summary>
        Task<byte[]> GenerateAssessmentReportAsync(AssessmentReportData data);
        
        /// <summary>
        /// Generates a comparison report
        /// </summary>
        Task<byte[]> GenerateComparisonReportAsync(ComparisonReportData data);
        
        /// <summary>
        /// Generates a trend analysis report
        /// </summary>
        Task<byte[]> GenerateTrendReportAsync(TrendReportData data);
        
        /// <summary>
        /// Generates a custom report with specified template
        /// </summary>
        Task<byte[]> GenerateCustomReportAsync(CustomReportData data);
    }

    public class ReportData
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public List<ReportSection> Sections { get; set; } = new();
        public List<byte[]> Charts { get; set; } = new();
    }

    public class ReportSection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<ReportTable> Tables { get; set; } = new();
    }

    public class ReportTable
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }

    public class AssessmentReportData : ReportData
    {
        public string AssessmentName { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public double OverallScore { get; set; }
        public List<AssessmentCategory> Categories { get; set; } = new();
    }

    public class AssessmentCategory
    {
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
        public int QuestionCount { get; set; }
        public int AnsweredCount { get; set; }
    }

    public class ComparisonReportData : ReportData
    {
        public List<AssessmentComparison> Comparisons { get; set; } = new();
    }

    public class AssessmentComparison
    {
        public string AssessmentName { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public double Score { get; set; }
        public List<CategoryComparison> Categories { get; set; } = new();
    }

    public class CategoryComparison
    {
        public string CategoryName { get; set; } = string.Empty;
        public double Score { get; set; }
    }

    public class TrendReportData : ReportData
    {
        public List<TrendDataPoint> TrendData { get; set; } = new();
    }

    public class TrendDataPoint
    {
        public DateTime Date { get; set; }
        public double Score { get; set; }
        public string AssessmentName { get; set; } = string.Empty;
    }

    public class CustomReportData : ReportData
    {
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, object> CustomData { get; set; } = new();
    }
} 