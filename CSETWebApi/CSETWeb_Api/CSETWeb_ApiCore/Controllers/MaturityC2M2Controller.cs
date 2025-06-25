//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Aggregation;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Maturity;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Reports;
using CSETWebCore.Model.Maturity;
using Microsoft.AspNetCore.Mvc;


namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for C2M2 (Cybersecurity Capability Maturity Model) functionality in CSET.
    /// This controller handles C2M2-specific maturity model operations including question retrieval,
    /// donut charts, heatmaps, and question table generation for cybersecurity capability assessments.
    /// </summary>
    [CsetAuthorize]
    public class MaturityC2M2Controller : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;
        private readonly IReportsDataBusiness _reports;

        private const int _c2m2ModelId = 12;

        /// <summary>
        /// Initializes a new instance of the MaturityC2M2Controller.
        /// </summary>
        /// <param name="context">The database context for data access operations</param>
        /// <param name="tokenManager">The token manager for user authentication and assessment context</param>
        /// <param name="assessmentUtil">The assessment utility service for assessment operations</param>
        /// <param name="adminTabBusiness">The admin tab business service for administrative operations</param>
        /// <param name="reports">The reports data business service for report generation</param>
        public MaturityC2M2Controller(
            CSETContext context,
            ITokenManager tokenManager,
            IAssessmentUtil assessmentUtil,
            IAdminTabBusiness adminTabBusiness,
            IReportsDataBusiness reports)
        {
            _tokenManager = tokenManager;
            _context = context;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
            _reports = reports;
        }

        /// <summary>
        /// Retrieves the C2M2 maturity structure and questions for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with JSON containing C2M2 maturity structure and questions
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint returns the complete C2M2 maturity model structure including:
        /// - Domain hierarchy and organization
        /// - Question text and supplemental information
        /// - Answer status and scoring data
        /// - Maturity level information
        /// 
        /// The response includes:
        /// - Complete question hierarchy with domains and subdomains
        /// - Question text and detailed descriptions
        /// - Supplemental information and guidance
        /// - Current answer status for each question
        /// - Maturity level mappings and scoring
        /// 
        /// C2M2-specific features:
        /// - Cybersecurity capability assessment framework
        /// - Maturity level progression (Incomplete to Innovative)
        /// - Domain-based organization of capabilities
        /// - Comprehensive question coverage for cyber resilience
        /// 
        /// The structure is optimized for:
        /// - Interactive assessment interfaces
        /// - Progress tracking and visualization
        /// - Detailed analysis and reporting
        /// - Compliance and capability mapping
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/maturitystructure/c2m2")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetQuestions()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            var options = new StructureOptions() { IncludeQuestionText = true, IncludeSupplemental = true };
            var x = biz.GetMaturityStructureAsXml(assessmentId, options);

            var json = Helpers.CustomJsonWriter.Serialize(x.Root);
            return Ok(json);
        }

        /// <summary>
        /// Retrieves C2M2 donut charts and heatmaps for visualization.
        /// </summary>
        /// <returns>
        /// 200 OK with object containing donut charts and heatmap data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint generates C2M2-specific visualizations including:
        /// - Donut charts showing maturity level distribution
        /// - Heatmaps displaying question answer patterns
        /// - Domain-level performance visualizations
        /// - Capability maturity scoring displays
        /// 
        /// The response includes:
        /// - Donut charts for each domain showing maturity distribution
        /// - Heatmaps for detailed question analysis
        /// - Color-coded performance indicators
        /// - Interactive visualization data
        /// 
        /// Visualization features:
        /// - Maturity level progression indicators
        /// - Domain-specific performance metrics
        /// - Answer distribution patterns
        /// - Capability gap identification
        /// 
        /// The visualizations support:
        /// - Executive dashboards and summaries
        /// - Detailed capability analysis
        /// - Progress tracking and trend analysis
        /// - Stakeholder reporting and presentations
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/c2m2/donutheatmap")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetDonuts()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz1 = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            var model = biz1.GetMaturityStructureForModel(_c2m2ModelId, assessmentId);

            var biz2 = new C2M2Business();
            var response = biz2.DonutsAndHeatmap(model);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves C2M2 question table data for grid display.
        /// </summary>
        /// <returns>
        /// 200 OK with object containing question table data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides structured question data for tabular display:
        /// - Question details and metadata
        /// - Answer status and scoring information
        /// - Domain and subdomain organization
        /// - Maturity level mappings
        /// 
        /// The response includes:
        /// - Question text and descriptions
        /// - Current answer status
        /// - Domain and subdomain classifications
        /// - Maturity level information
        /// - Scoring and assessment data
        /// 
        /// Table features:
        /// - Structured data for grid components
        /// - Sortable and filterable question lists
        /// - Bulk answer management capabilities
        /// - Progress tracking and completion status
        /// 
        /// The table data supports:
        /// - Interactive question interfaces
        /// - Bulk assessment operations
        /// - Progress monitoring and reporting
        /// - Data export and analysis
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/c2m2/questionTable")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetQuestionGrid()
        {
            int assessmentId = _tokenManager.AssessmentForUser();

            var biz1 = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            var model = biz1.GetMaturityStructureForModel(_c2m2ModelId, assessmentId);

            var biz2 = new C2M2Business();
            var response = biz2.QuestionTables(model);

            return Ok(response);
        }
    }
}
