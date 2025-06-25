//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Mvc;
using CSETWebCore.Business.Aggregation;
using CSETWebCore.Business.Authorization;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for maturity model aggregation analysis in CSET.
    /// This controller handles maturity-based assessment aggregation analysis, including
    /// compliance scoring by model and domain, cross-assessment maturity comparisons,
    /// and maturity model-specific analytics. Supports various maturity models with
    /// model-specific rollup calculations and compliance metrics.
    /// </summary>
    [CsetAuthorize]
    public class AggregationMaturityController : Controller
    {
        private CSETContext _context;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Initializes a new instance of the AggregationMaturityController.
        /// </summary>
        /// <param name="tokenManager">The token manager for user authentication and assessment context</param>
        /// <param name="context">The database context for data access operations</param>
        public AggregationMaturityController(ITokenManager tokenManager, CSETContext context)
        {
            _context = context;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Returns domain scores for the assessments in the aggregation by maturity model and domain.
        /// </summary>
        /// <returns>
        /// 200 OK with List of BarChartX containing compliance data by model and domain
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides maturity model compliance analysis across assessments:
        /// - Domain-level compliance scores for each maturity model
        /// - Cross-assessment comparison within the same maturity model
        /// - Model-specific rollup calculations and compliance metrics
        /// - No cross-model comparisons (each model analyzed separately)
        /// 
        /// The response includes:
        /// - Bar chart data for each maturity model
        /// - Domain-level compliance percentages
        /// - Assessment aliases and comparison data
        /// - Model-specific rollup calculations
        /// 
        /// Maturity model support includes:
        /// - CMMC (Cybersecurity Maturity Model Certification)
        /// - C2M2 (Cybersecurity Capability Maturity Model)
        /// - EDM (External Dependencies Management)
        /// - RRA (Ransomware Readiness Assessment)
        /// - Custom maturity models with configurable rollup levels
        /// 
        /// Analysis features:
        /// - Model-specific domain rollup calculations
        /// - Cross-assessment benchmarking within models
        /// - Compliance percentage calculations
        /// - Domain-level performance analysis
        /// 
        /// The compliance analysis supports:
        /// - Maturity model performance tracking
        /// - Cross-assessment benchmarking
        /// - Domain-level improvement analysis
        /// - Executive reporting and presentations
        /// - Strategic planning and goal setting
        /// 
        /// Rollup level variations:
        /// - Most models: Domain-level rollup
        /// - RRA and CMMC2: Goal-level rollup (single domain)
        /// - EDM: Mixed rollup (MIL-1 domains + MIL-2-5 goals)
        /// 
        /// Requires valid JWT token in Authorization header with aggregation context.
        /// </remarks>
        [HttpGet]
        [Route("api/aggregation/analysis/maturity/compliance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetComplianceByModelAndDomain()
        {   
            var aggregationID = _tokenManager.PayloadInt("aggreg");
            if (aggregationID == null)
            {
                return Ok();
            }
            var amb = new AggregationMaturityBusiness(_context);
            var resp = amb.GetMaturityModelComplianceChart(aggregationID.Value);

            return Ok(resp);
        }
    }
}
