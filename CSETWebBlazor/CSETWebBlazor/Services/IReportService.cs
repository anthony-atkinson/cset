using CSETWebBlazor.Models;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Interface for report generation and management services
    /// </summary>
    public interface IReportService
    {
        // Report Generation
        Task<ReportResponse> GetReportAsync(ReportRequest request);
        Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request);
        Task<ReportResponse> GetAggReportAsync(string reportId, int aggregationId);
        Task<byte[]> GetPdfAsync(string pdfString, string security);

        // Report Information
        Task<ReportInfo> GetAssessmentInfoForReportAsync();
        Task<List<ConfidentialityLevel>> GetConfidentialityLevelsAsync();

        // Report Content
        Task<List<AltAnswer>> GetAltListAsync();
        Task<ModuleContent> GetModuleContentAsync(string setName);
        Task<ModelContent> GetModelContentAsync(string modelId);
        Task<List<StandardAnsweredQuestion>> GetStandardAnsweredQuestionsAsync();
        Task<List<StandardCommentMfr>> GetStandardCommentsAndMfrAsync();
        Task<List<ReviewedQuestion>> GetReviewedQuestionsAsync();

        // Specialized Reports
        Task<ReportResponse> GetHydroActionItemsReportAsync();
        Task<List<C2M2DonutData>> GetC2M2DonutsAsync();
        Task<List<C2M2TableData>> GetC2M2TableDataAsync();

        // Excel Export
        Task<byte[]> ExportToExcelAsync(ExcelExportRequest request);
        Task<byte[]> ExportPoamToExcelAsync();
        Task<byte[]> ExportObservationsToExcelAsync();

        // Network Diagram
        Task<byte[]> GetNetworkDiagramImageAsync();
        Task<byte[]> GetCRRSummaryAsync();

        // Report Configuration
        Task<bool> IsInstallationAsync(string mode);
        Task<bool> ValidateCisaAssessorFieldsAsync();

        // Report Settings
        bool ShowGuidance { get; set; }
        bool ShowReferences { get; set; }
        bool ShowQuestions { get; set; }
        string Confidentiality { get; set; }

        // Utility Methods
        Task<string> FormatLinebreaksAsync(string text);
        Task<string> FixWarningNewlinesAsync(string text);
        Task<string> ScrubGlossaryMarkupAsync(string questionText);
        Task<DateTime> ApplyJwtOffsetAsync(DateTime date, string format);

        // Report Links
        Task<string> GetReportLinkAsync(string reportType, bool print = false);
        Task<string> GetExcelLinkAsync(string reportType);

        // Caching
        Task<ReportResponse?> GetCachedReportAsync(string reportId);
        Task CacheReportAsync(string reportId, ReportResponse report);
        Task ClearReportCacheAsync();

        // Error Handling
        Task<string> GetLastErrorAsync();
        Task ClearLastErrorAsync();

        // Events
        event Action<ReportResponse> ReportGenerated;
        event Action<string> ReportError;
    }
} 