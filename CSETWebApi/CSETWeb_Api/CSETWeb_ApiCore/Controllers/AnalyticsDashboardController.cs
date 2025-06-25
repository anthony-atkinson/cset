//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.DataLayer.Manual;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Analytics;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for analytics dashboard functionality in CSET.
    /// This controller handles maturity model analytics and dashboard data generation,
    /// supporting comparative analysis across sectors and industries for cybersecurity
    /// maturity assessments.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class AnalyticsDashboardController : ControllerBase
    {
        private readonly ITokenManager _token;
        private IAnalyticsBusiness _analytics;

        /// <summary>
        /// Initializes a new instance of the AnalyticsDashboardController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="analytics">The analytics business service for dashboard data generation</param>
        public AnalyticsDashboardController(ITokenManager token, IAnalyticsBusiness analytics)
        {
            _token = token;
            _analytics = analytics;
        }

        /// <summary>
        /// Retrieves maturity dashboard data for comparative analysis across sectors and industries.
        /// </summary>
        /// <param name="maturity_model_id">The ID of the maturity model to analyze</param>
        /// <param name="sectorId">Optional sector ID to filter results by sector</param>
        /// <param name="industryId">Optional industry ID to filter results by industry</param>
        /// <returns>
        /// 200 OK with List of AnalyticsMinMaxAvgMedianByGroup containing maturity dashboard data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides comprehensive maturity analytics data including:
        /// - Statistical analysis of maturity scores across organizations
        /// - Comparative metrics by sector and industry
        /// - Min, max, average, and median calculations
        /// - Grouped analysis for trend identification
        /// 
        /// The response includes:
        /// - Statistical summaries by maturity level
        /// - Comparative data across sectors and industries
        /// - Performance metrics and benchmarks
        /// - Trend analysis and insights
        /// 
        /// Analytics features:
        /// - Cross-organizational benchmarking
        /// - Sector-specific performance analysis
        /// - Industry comparison metrics
        /// - Maturity level distribution analysis
        /// 
        /// Common maturity models include:
        /// - CMMC (Cybersecurity Maturity Model Certification)
        /// - C2M2 (Cybersecurity Capability Maturity Model)
        /// - CPG (Cybersecurity Performance Goals)
        /// - EDM (External Dependencies Management)
        /// - CRR (Cyber Resilience Review)
        /// 
        /// The dashboard data supports:
        /// - Executive reporting and presentations
        /// - Competitive analysis and benchmarking
        /// - Performance trend identification
        /// - Strategic planning and goal setting
        /// - Compliance and maturity tracking
        /// 
        /// Filtering options:
        /// - By maturity model for specific framework analysis
        /// - By sector for industry-specific comparisons
        /// - By industry for detailed competitive analysis
        /// - Combined filters for targeted insights
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/analyticsMaturityDashboard")]
        [ProducesResponseType(typeof(List<AnalyticsMinMaxAvgMedianByGroup>), 200)]
        [ProducesResponseType(401)]
        public List<AnalyticsMinMaxAvgMedianByGroup> getMaturityDashboardData([FromQuery] int maturity_model_id, int? sectorId, int? industryId)
        {
            int assessmentId = _token.AssessmentForUser();
            return _analytics.getMaturityDashboardData(maturity_model_id, sectorId, industryId);
        }
    }
}
