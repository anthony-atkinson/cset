using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Maturity;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Reports;
using Microsoft.AspNetCore.Mvc;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for CMMC (Cybersecurity Maturity Model Certification) functionality in CSET.
    /// This controller handles CMMC scoring, level scorecards, and SPRS (Supplier Performance Risk System)
    /// score calculations. Supports CMMC compliance assessment and reporting for Department of Defense
    /// contractors and suppliers. Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class CmmcController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;
        private readonly IReportsDataBusiness _reports;

        /// <summary>
        /// Initializes a new instance of the CmmcController.
        /// </summary>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for CMMC operations</param>
        /// <param name="assessmentUtil">Assessment utility service</param>
        /// <param name="adminTabBusiness">Admin tab business logic service</param>
        /// <param name="reports">Reports data business service</param>
        public CmmcController(ITokenManager tokenManager, CSETContext context, IAssessmentUtil assessmentUtil,
            IAdminTabBusiness adminTabBusiness, IReportsDataBusiness reports)
        {
            _tokenManager = tokenManager;
            _context = context;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
            _reports = reports;
        }

        /// <summary>
        /// Retrieves CMMC scores for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with CMMC scores if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns comprehensive CMMC scoring data for the current assessment.
        /// Includes scores across all CMMC domains and maturity levels.
        /// Used for CMMC compliance reporting and assessment analysis.
        /// Provides detailed scoring information for DoD contractor requirements.
        /// </remarks>
        [HttpGet]
        [Route("api/cmmc/scores")]
        public IActionResult GetCmmcScores()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            return Ok(new CmmcBusiness(_context, _assessmentUtil, _adminTabBusiness).GetCmmcScores(assessmentId));
        }

        /// <summary>
        /// Returns a collection of scorecards for each active maturity level.
        /// </summary>
        /// <returns>
        /// 200 OK with level scorecards if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns detailed scorecards for each CMMC maturity level (1-5).
        /// Provides granular scoring information for each level including
        /// domain scores, practices, and processes. Used for detailed
        /// CMMC compliance analysis and gap assessment.
        /// </remarks>
        [HttpGet]
        [Route("api/cmmc/scorecards")]
        public IActionResult GetLevelScorecards()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz = new CmmcBusiness(_context, _assessmentUtil, _adminTabBusiness);

            return Ok(biz.GetLevelScorecards(assessmentId));
        }

        /// <summary>
        /// Retrieves SPRS (Supplier Performance Risk System) score for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with SPRS score if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns the SPRS score calculated from the current assessment.
        /// SPRS is used by the Department of Defense to assess supplier cybersecurity risk.
        /// This endpoint is marked for deprecation with CMMC2 final release.
        /// Provides risk scoring for DoD supplier evaluation processes.
        /// </remarks>
        [HttpGet]
        [Route("api/SPRSScore")]
        public IActionResult GetSPRSScore()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            return Ok(new CmmcBusiness(_context, _assessmentUtil, _adminTabBusiness).GetSPRSScore(assessmentId));
        }
    }
}
