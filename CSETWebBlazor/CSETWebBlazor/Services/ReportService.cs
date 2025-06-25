using CSETWebBlazor.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CSETWebBlazor.Services
{
    /// <summary>
    /// Service for report generation and management operations
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IApiClientService _apiClient;
        private readonly IConfigService _configService;
        private readonly IFileService _fileService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReportService> _logger;
        private readonly IErrorHandlingService _errorHandling;

        private List<ConfidentialityLevel> _confidentialityLevels = new List<ConfidentialityLevel>();
        private string _lastError = string.Empty;

        // Report Settings
        public bool ShowGuidance { get; set; } = true;
        public bool ShowReferences { get; set; } = true;
        public bool ShowQuestions { get; set; } = true;
        public string Confidentiality { get; set; } = string.Empty;

        // Events
        public event Action<ReportResponse>? ReportGenerated;
        public event Action<string>? ReportError;

        public ReportService(
            IApiClientService apiClient,
            IConfigService configService,
            IFileService fileService,
            IMemoryCache cache,
            ILogger<ReportService> logger,
            IErrorHandlingService errorHandling)
        {
            _apiClient = apiClient;
            _configService = configService;
            _fileService = fileService;
            _cache = cache;
            _logger = logger;
            _errorHandling = errorHandling;

            InitializeAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Initialize the service by loading confidentiality levels
        /// </summary>
        private async Task InitializeAsync()
        {
            try
            {
                await LoadConfidentialityLevelsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize ReportService");
                await _errorHandling.HandleErrorAsync(ex, "Failed to initialize report service");
            }
        }

        #region Report Generation

        public async Task<ReportResponse> GetReportAsync(ReportRequest request)
        {
            try
            {
                var cacheKey = $"report_{request.ReportId}_{request.AggregationId}_{request.ConfidentialityLevel}";
                if (_cache.TryGetValue(cacheKey, out ReportResponse? cachedReport))
                {
                    return cachedReport ?? new ReportResponse();
                }

                var queryParams = new List<string>();
                if (request.AggregationId.HasValue)
                {
                    queryParams.Add($"aggregationID={request.AggregationId.Value}");
                }
                if (!string.IsNullOrEmpty(request.ConfidentialityLevel))
                {
                    queryParams.Add($"confidentiality={request.ConfidentialityLevel}");
                }
                if (request.IncludeCharts)
                {
                    queryParams.Add("includeCharts=true");
                }
                if (request.IncludeDocuments)
                {
                    queryParams.Add("includeDocuments=true");
                }
                if (!string.IsNullOrEmpty(request.Format))
                {
                    queryParams.Add($"format={request.Format}");
                }

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<ReportResponse>($"reports/{request.ReportId}{queryString}");
                
                if (response != null)
                {
                    _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
                    ReportGenerated?.Invoke(response);
                    return response;
                }

                return new ReportResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report {ReportId}", request.ReportId);
                _lastError = ex.Message;
                ReportError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get report {request.ReportId}");
                return new ReportResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ReportResponse> GenerateReportAsync(GenerateReportRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<ReportResponse>("reports/generate", request);
                
                if (response != null)
                {
                    ReportGenerated?.Invoke(response);
                    return response;
                }

                return new ReportResponse { Success = false, Message = "Failed to generate report" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate report {ReportType}", request.ReportType);
                _lastError = ex.Message;
                ReportError?.Invoke(ex.Message);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to generate report {request.ReportType}");
                return new ReportResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<ReportResponse> GetAggReportAsync(string reportId, int aggregationId)
        {
            try
            {
                var request = new ReportRequest
                {
                    ReportId = reportId,
                    AggregationId = aggregationId
                };
                return await GetReportAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get aggregation report {ReportId} for aggregation {AggregationId}", reportId, aggregationId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get aggregation report {reportId} for aggregation {aggregationId}");
                return new ReportResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<byte[]> GetPdfAsync(string pdfString, string security)
        {
            try
            {
                var queryParams = $"view={pdfString}&security={security}";
                var response = await _apiClient.GetAsync<byte[]>($"getPdf?{queryParams}");
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get PDF");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get PDF");
                return new byte[0];
            }
        }

        #endregion

        #region Report Information

        public async Task<ReportInfo> GetAssessmentInfoForReportAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<ReportInfo>("reports/info");
                return response ?? new ReportInfo();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get assessment info for report");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get assessment info for report");
                return new ReportInfo();
            }
        }

        public async Task<List<ConfidentialityLevel>> GetConfidentialityLevelsAsync()
        {
            try
            {
                if (_confidentialityLevels.Count > 0)
                {
                    return _confidentialityLevels;
                }

                await LoadConfidentialityLevelsAsync();
                return _confidentialityLevels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get confidentiality levels");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get confidentiality levels");
                return new List<ConfidentialityLevel>();
            }
        }

        private async Task LoadConfidentialityLevelsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<ConfidentialityLevel>>("reports/getconfidentialtypes");
                if (response != null)
                {
                    _confidentialityLevels = response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load confidentiality levels");
                await _errorHandling.HandleErrorAsync(ex, "Failed to load confidentiality levels");
            }
        }

        #endregion

        #region Report Content

        public async Task<List<AltAnswer>> GetAltListAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<AltAnswer>>("reports/getAltList");
                return response ?? new List<AltAnswer>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ALT list");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get ALT list");
                return new List<AltAnswer>();
            }
        }

        public async Task<ModuleContent> GetModuleContentAsync(string setName)
        {
            try
            {
                var response = await _apiClient.GetAsync<ModuleContent>($"reports/modulecontent?set={setName}");
                return response ?? new ModuleContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get module content for {SetName}", setName);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get module content for {setName}");
                return new ModuleContent();
            }
        }

        public async Task<ModelContent> GetModelContentAsync(string modelId)
        {
            try
            {
                var response = await _apiClient.GetAsync<ModelContent>($"maturity/structure?modelId={modelId}");
                return response ?? new ModelContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get model content for {ModelId}", modelId);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get model content for {modelId}");
                return new ModelContent();
            }
        }

        public async Task<List<StandardAnsweredQuestion>> GetStandardAnsweredQuestionsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<StandardAnsweredQuestion>>("reports/standardansweredquestions");
                return response ?? new List<StandardAnsweredQuestion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get standard answered questions");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get standard answered questions");
                return new List<StandardAnsweredQuestion>();
            }
        }

        public async Task<List<StandardCommentMfr>> GetStandardCommentsAndMfrAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<StandardCommentMfr>>("reports/standardcommentsandmfr");
                return response ?? new List<StandardCommentMfr>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get standard comments and MFR");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get standard comments and MFR");
                return new List<StandardCommentMfr>();
            }
        }

        public async Task<List<ReviewedQuestion>> GetReviewedQuestionsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<ReviewedQuestion>>("reports/reviewedquestions");
                return response ?? new List<ReviewedQuestion>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get reviewed questions");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get reviewed questions");
                return new List<ReviewedQuestion>();
            }
        }

        #endregion

        #region Specialized Reports

        public async Task<ReportResponse> GetHydroActionItemsReportAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<ReportResponse>("reports/getHydroActionItemsReport");
                return response ?? new ReportResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Hydro action items report");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get Hydro action items report");
                return new ReportResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<List<C2M2DonutData>> GetC2M2DonutsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<C2M2DonutData>>("reports/c2m2donuts");
                return response ?? new List<C2M2DonutData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get C2M2 donuts");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get C2M2 donuts");
                return new List<C2M2DonutData>();
            }
        }

        public async Task<List<C2M2TableData>> GetC2M2TableDataAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<List<C2M2TableData>>("reports/c2m2tabledata");
                return response ?? new List<C2M2TableData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get C2M2 table data");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get C2M2 table data");
                return new List<C2M2TableData>();
            }
        }

        #endregion

        #region Excel Export

        public async Task<byte[]> ExportToExcelAsync(ExcelExportRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync<byte[]>($"reports/{request.ReportType}/excel", request);
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export {ReportType} to Excel", request.ReportType);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to export {request.ReportType} to Excel");
                return new byte[0];
            }
        }

        public async Task<byte[]> ExportPoamToExcelAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<byte[]>("reports/poam/excelexport");
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export POAM to Excel");
                await _errorHandling.HandleErrorAsync(ex, "Failed to export POAM to Excel");
                return new byte[0];
            }
        }

        public async Task<byte[]> ExportObservationsToExcelAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<byte[]>("reports/observations/excel");
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export observations to Excel");
                await _errorHandling.HandleErrorAsync(ex, "Failed to export observations to Excel");
                return new byte[0];
            }
        }

        #endregion

        #region Network Diagram

        public async Task<byte[]> GetNetworkDiagramImageAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<byte[]>("diagram/getimage");
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get network diagram image");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get network diagram image");
                return new byte[0];
            }
        }

        public async Task<byte[]> GetCRRSummaryAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<byte[]>("diagram/getimage");
                return response ?? new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get CRR summary");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get CRR summary");
                return new byte[0];
            }
        }

        #endregion

        #region Report Configuration

        public async Task<bool> IsInstallationAsync(string mode)
        {
            try
            {
                var config = await _configService.GetConfigAsync();
                return config?.InstallationMode == mode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check installation mode");
                await _errorHandling.HandleErrorAsync(ex, "Failed to check installation mode");
                return false;
            }
        }

        public async Task<bool> ValidateCisaAssessorFieldsAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<bool>("reports/validatecisaassessorfields");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate CISA assessor fields");
                await _errorHandling.HandleErrorAsync(ex, "Failed to validate CISA assessor fields");
                return false;
            }
        }

        #endregion

        #region Utility Methods

        public async Task<string> FormatLinebreaksAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("\r\n", "<br />")
                      .Replace("\r", "<br />")
                      .Replace("\n", "<br />");
        }

        public async Task<string> FixWarningNewlinesAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var pieces = text.Split('\n');
            var divs = new List<string>();
            
            foreach (var piece in pieces)
            {
                if (!string.IsNullOrWhiteSpace(piece))
                {
                    divs.Add($"<div>{piece}</div>");
                }
            }
            
            return string.Join("", divs);
        }

        public async Task<string> ScrubGlossaryMarkupAsync(string questionText)
        {
            if (string.IsNullOrEmpty(questionText))
            {
                return string.Empty;
            }

            // Remove glossary markup patterns
            var scrubbed = questionText;
            
            // Remove [[term]] patterns
            scrubbed = System.Text.RegularExpressions.Regex.Replace(scrubbed, @"\[\[([^\]]+)\]\]", "$1");
            
            // Remove {{term}} patterns
            scrubbed = System.Text.RegularExpressions.Regex.Replace(scrubbed, @"\{\{([^\}]+)\}\}", "$1");
            
            return scrubbed;
        }

        public async Task<DateTime> ApplyJwtOffsetAsync(DateTime date, string format)
        {
            try
            {
                // Apply JWT timezone offset if needed
                // This would typically get the offset from the JWT token
                return date;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply JWT offset");
                return date;
            }
        }

        #endregion

        #region Report Links

        public async Task<string> GetReportLinkAsync(string reportType, bool print = false)
        {
            try
            {
                var baseUrl = await _configService.GetConfigAsync().ContinueWith(t => t.Result?.AppUrl ?? "");
                var url = $"{baseUrl}/index.html?returnPath=report/{reportType}";
                
                if (print)
                {
                    url += "&print=true";
                }
                
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get report link for {ReportType}", reportType);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get report link for {reportType}");
                return string.Empty;
            }
        }

        public async Task<string> GetExcelLinkAsync(string reportType)
        {
            try
            {
                var baseUrl = await _configService.GetConfigAsync().ContinueWith(t => t.Result?.ApiUrl ?? "");
                return $"{baseUrl}reports/{reportType}/excel";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Excel link for {ReportType}", reportType);
                await _errorHandling.HandleErrorAsync(ex, $"Failed to get Excel link for {reportType}");
                return string.Empty;
            }
        }

        #endregion

        #region Caching

        public async Task<ReportResponse?> GetCachedReportAsync(string reportId)
        {
            try
            {
                var cacheKey = $"report_{reportId}";
                if (_cache.TryGetValue(cacheKey, out ReportResponse? cachedReport))
                {
                    return cachedReport;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cached report");
                await _errorHandling.HandleErrorAsync(ex, "Failed to get cached report");
                return null;
            }
        }

        public async Task CacheReportAsync(string reportId, ReportResponse report)
        {
            try
            {
                var cacheKey = $"report_{reportId}";
                _cache.Set(cacheKey, report, TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache report");
                await _errorHandling.HandleErrorAsync(ex, "Failed to cache report");
            }
        }

        public async Task ClearReportCacheAsync()
        {
            try
            {
                // Clear all report-related cache entries
                var cacheKeys = new List<string>();
                // This would need to be implemented based on the actual cache keys used
                foreach (var key in cacheKeys)
                {
                    _cache.Remove(key);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear report cache");
                await _errorHandling.HandleErrorAsync(ex, "Failed to clear report cache");
            }
        }

        #endregion

        #region Error Handling

        public async Task<string> GetLastErrorAsync()
        {
            return _lastError;
        }

        public async Task ClearLastErrorAsync()
        {
            _lastError = string.Empty;
        }

        #endregion
    }
} 