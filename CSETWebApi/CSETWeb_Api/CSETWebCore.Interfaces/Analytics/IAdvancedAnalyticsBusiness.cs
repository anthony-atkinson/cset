//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Model.Analytics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSETWebCore.Interfaces.Analytics
{
    /// <summary>
    /// Interface for advanced analytics business operations.
    /// Provides comprehensive analytics capabilities including trend analysis, 
    /// benchmarking, predictive analytics, and executive reporting.
    /// </summary>
    public interface IAdvancedAnalyticsBusiness
    {
        /// <summary>
        /// Gets comprehensive executive summary analytics for the assessment.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>Executive summary analytics data</returns>
        Task<ExecutiveSummaryAnalytics> GetExecutiveSummaryAsync(int assessmentId);

        /// <summary>
        /// Gets trend analysis data for the assessment over time.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="timeframe">Timeframe for analysis (days)</param>
        /// <returns>Trend analysis data</returns>
        Task<TrendAnalysisData> GetTrendAnalysisAsync(int assessmentId, int timeframe = 365);

        /// <summary>
        /// Gets benchmarking data comparing the assessment to industry standards.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="sectorId">Sector ID for comparison</param>
        /// <param name="industryId">Industry ID for comparison</param>
        /// <returns>Benchmarking data</returns>
        Task<BenchmarkingData> GetBenchmarkingDataAsync(int assessmentId, int? sectorId = null, int? industryId = null);

        /// <summary>
        /// Gets predictive analytics data for future assessment performance.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>Predictive analytics data</returns>
        Task<PredictiveAnalyticsData> GetPredictiveAnalyticsAsync(int assessmentId);

        /// <summary>
        /// Gets custom report data based on specified parameters.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="reportType">Type of report</param>
        /// <param name="parameters">Report parameters</param>
        /// <returns>Custom report data</returns>
        Task<CustomReportData> GetCustomReportAsync(int assessmentId, string reportType, Dictionary<string, object> parameters);
    }
} 