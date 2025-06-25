//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.Analytics;
using CSETWebCore.Model.Analytics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides advanced analytics endpoints for comprehensive cybersecurity assessment analysis.
    /// This controller handles executive summaries, trend analysis, benchmarking, predictive analytics,
    /// and custom reporting for enterprise-level analytics capabilities.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class AdvancedAnalyticsController : ControllerBase
    {
        private readonly IAdvancedAnalyticsBusiness _advancedAnalytics;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Initializes a new instance of the AdvancedAnalyticsController.
        /// </summary>
        /// <param name="advancedAnalytics">Advanced analytics business service</param>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        public AdvancedAnalyticsController(IAdvancedAnalyticsBusiness advancedAnalytics, ITokenManager tokenManager)
        {
            _advancedAnalytics = advancedAnalytics;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Retrieves comprehensive executive summary analytics for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with executive summary analytics data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides a high-level executive summary including:
        /// - Overall compliance score and metrics
        /// - Organization demographics and context
        /// - Risk areas and improvement recommendations
        /// - Key performance indicators
        /// 
        /// The executive summary is designed for leadership presentations and
        /// strategic decision-making with cybersecurity posture insights.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/advanced-analytics/executive-summary")]
        [ProducesResponseType(typeof(ExecutiveSummaryAnalytics), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetExecutiveSummary()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var summary = await _advancedAnalytics.GetExecutiveSummaryAsync(assessmentId);
            return Ok(summary);
        }

        /// <summary>
        /// Retrieves trend analysis data for the current assessment over time.
        /// </summary>
        /// <param name="timeframe">Timeframe for analysis in days (default: 365)</param>
        /// <returns>
        /// 200 OK with trend analysis data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides trend analysis including:
        /// - Historical compliance score progression
        /// - Trend direction and percentage changes
        /// - Performance patterns over time
        /// - Trend strength and confidence metrics
        /// 
        /// The trend analysis helps identify improvement patterns and
        /// validates the effectiveness of cybersecurity initiatives.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/advanced-analytics/trend-analysis")]
        [ProducesResponseType(typeof(TrendAnalysisData), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetTrendAnalysis([FromQuery] int timeframe = 365)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var trendData = await _advancedAnalytics.GetTrendAnalysisAsync(assessmentId, timeframe);
            return Ok(trendData);
        }

        /// <summary>
        /// Retrieves benchmarking data comparing the current assessment to industry standards.
        /// </summary>
        /// <param name="sectorId">Optional sector ID for comparison</param>
        /// <param name="industryId">Optional industry ID for comparison</param>
        /// <returns>
        /// 200 OK with benchmarking data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides benchmarking analysis including:
        /// - Current assessment score vs industry averages
        /// - Percentile ranking and competitive positioning
        /// - Sector and industry-specific comparisons
        /// - Performance gaps and opportunities
        /// 
        /// The benchmarking data helps organizations understand their
        /// relative cybersecurity posture and identify improvement areas.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/advanced-analytics/benchmarking")]
        [ProducesResponseType(typeof(BenchmarkingData), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetBenchmarkingData([FromQuery] int? sectorId = null, [FromQuery] int? industryId = null)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var benchmarkingData = await _advancedAnalytics.GetBenchmarkingDataAsync(assessmentId, sectorId, industryId);
            return Ok(benchmarkingData);
        }

        /// <summary>
        /// Retrieves predictive analytics data for future assessment performance.
        /// </summary>
        /// <returns>
        /// 200 OK with predictive analytics data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides predictive analytics including:
        /// - Forecasted compliance scores (3, 6, 12 months)
        /// - Confidence levels and prediction accuracy
        /// - Risk predictions and mitigation strategies
        /// - Trend strength and reliability metrics
        /// 
        /// The predictive analytics help organizations plan future
        /// cybersecurity investments and track progress toward goals.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/advanced-analytics/predictive")]
        [ProducesResponseType(typeof(PredictiveAnalyticsData), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetPredictiveAnalytics()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var predictiveData = await _advancedAnalytics.GetPredictiveAnalyticsAsync(assessmentId);
            return Ok(predictiveData);
        }

        /// <summary>
        /// Retrieves custom report data based on specified parameters.
        /// </summary>
        /// <param name="reportType">Type of report (compliance, risk, trend, benchmark, comprehensive)</param>
        /// <param name="parameters">Report parameters as key-value pairs</param>
        /// <returns>
        /// 200 OK with custom report data if successful
        /// 400 Bad Request if parameters are invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides flexible custom reporting including:
        /// - Compliance-focused reports with detailed metrics
        /// - Risk assessment reports with mitigation strategies
        /// - Trend analysis reports with historical context
        /// - Benchmarking reports with competitive analysis
        /// - Comprehensive reports combining all analytics
        /// 
        /// Supported report types:
        /// - "compliance": Detailed compliance metrics and gaps
        /// - "risk": Risk areas and improvement recommendations
        /// - "trend": Historical trend analysis and projections
        /// - "benchmark": Industry comparison and positioning
        /// - "comprehensive": Complete analytics package
        /// 
        /// Parameters can include:
        /// - timeframe: Analysis timeframe in days
        /// - sectorId: Sector ID for benchmarking
        /// - industryId: Industry ID for benchmarking
        /// - includeRecommendations: Boolean for improvement suggestions
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/advanced-analytics/custom-report")]
        [ProducesResponseType(typeof(CustomReportData), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCustomReport([FromQuery] string reportType, [FromBody] Dictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(reportType))
            {
                return BadRequest("Report type is required");
            }

            int assessmentId = _tokenManager.AssessmentForUser();
            var reportData = await _advancedAnalytics.GetCustomReportAsync(assessmentId, reportType, parameters ?? new Dictionary<string, object>());
            return Ok(reportData);
        }

        /// <summary>
        /// Retrieves comprehensive analytics dashboard data combining all analytics types.
        /// </summary>
        /// <param name="includePredictive">Whether to include predictive analytics (default: true)</param>
        /// <returns>
        /// 200 OK with comprehensive dashboard data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides a complete analytics dashboard including:
        /// - Executive summary with key metrics
        /// - Trend analysis with historical context
        /// - Benchmarking data with industry comparisons
        /// - Predictive analytics with future projections
        /// - Risk areas and improvement recommendations
        /// 
        /// The comprehensive dashboard is designed for executive-level
        /// cybersecurity posture monitoring and strategic planning.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/advanced-analytics/dashboard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAnalyticsDashboard([FromQuery] bool includePredictive = true)
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var dashboardData = new
            {
                ExecutiveSummary = await _advancedAnalytics.GetExecutiveSummaryAsync(assessmentId),
                TrendAnalysis = await _advancedAnalytics.GetTrendAnalysisAsync(assessmentId),
                Benchmarking = await _advancedAnalytics.GetBenchmarkingDataAsync(assessmentId),
                PredictiveAnalytics = includePredictive ? await _advancedAnalytics.GetPredictiveAnalyticsAsync(assessmentId) : null,
                GeneratedDate = System.DateTime.UtcNow
            };

            return Ok(dashboardData);
        }
    }
} 