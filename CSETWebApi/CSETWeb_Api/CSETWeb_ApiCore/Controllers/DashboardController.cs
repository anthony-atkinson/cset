//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Dashboard;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for dashboard functionality in CSET.
    /// This controller handles the generation of chart data and visualizations
    /// for dashboard displays, including maturity model answer distributions
    /// and domain-based analytics. Supports both normalized and domain-specific views.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private CSETContext _context;
        private readonly ITokenManager _tokenManager;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;

        /// <summary>
        /// Initializes a new instance of the DashboardController.
        /// </summary>
        /// <param name="context">Database context for dashboard operations</param>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        /// <param name="assessmentUtil">Assessment utility service</param>
        /// <param name="adminTabBusiness">Admin tab business logic service</param>
        public DashboardController(CSETContext context, ITokenManager tokenManager, IAssessmentUtil assessmentUtil,
            IAdminTabBusiness adminTabBusiness)
        {
            _context = context;
            _tokenManager = tokenManager;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
        }

        /// <summary>
        /// Returns the normalized values for the model's answer options.
        /// </summary>
        /// <param name="modelId">ID of the maturity model to analyze</param>
        /// <returns>
        /// 200 OK with normalized answer distribution data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Generates normalized answer distribution data for maturity model charts.
        /// The data is normalized to provide consistent scaling across different models.
        /// This endpoint is marked as AllowAnonymous to support public dashboard views.
        /// Returns data suitable for chart visualization showing answer patterns.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        [Route("api/chart/maturity/answerdistrib/normalized")]
        public IActionResult GetNormalizedAnswerDistribution([FromQuery] int modelId)
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz = new DashboardChartBusiness(assessmentId, modelId, _context, _assessmentUtil, _adminTabBusiness);
            var resp = biz.GetAnswerDistributionNormalized();

            return Ok(resp);
        }

        /// <summary>
        /// Returns the normalized values for the model's answer options by domain.
        /// </summary>
        /// <param name="modelId">ID of the maturity model to analyze</param>
        /// <returns>
        /// 200 OK with domain-based answer distribution data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Generates answer distribution data organized by domain for maturity model charts.
        /// Provides domain-specific analysis of answer patterns within the maturity model.
        /// This endpoint is marked as AllowAnonymous to support public dashboard views.
        /// Returns data structured by domain for detailed chart visualization.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        [Route("api/chart/maturity/answerdistrib/domain")]
        public IActionResult GetAnswerDistributionByDomain([FromQuery] int modelId)
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz = new DashboardChartBusiness(assessmentId, modelId, _context, _assessmentUtil, _adminTabBusiness);
            var resp = biz.GetAnswerDistributionByDomain();

            return Ok(resp);
        }
    }
}
