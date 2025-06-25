//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 

using CSETWebCore.Business.Authorization;
using Microsoft.AspNetCore.Mvc;
using CSETWebCore.Business.Maturity;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Reports;


namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for CPG (Cybersecurity Performance Goals) functionality in CSET.
    /// This controller handles CPG-specific maturity model operations including question retrieval,
    /// answer distribution analysis, and SSG (Sector-Specific Goals) model management for
    /// cybersecurity performance assessments.
    /// </summary>
    [CsetAuthorize]
    public class MaturityCpgController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IAssessmentUtil _assessmentUtil;
        private readonly IAdminTabBusiness _adminTabBusiness;
        private readonly IReportsDataBusiness _reports;

        /// <summary>
        /// Initializes a new instance of the MaturityCpgController.
        /// </summary>
        /// <param name="tokenManager">The token manager for user authentication and assessment context</param>
        /// <param name="context">The database context for data access operations</param>
        /// <param name="assessmentUtil">The assessment utility service for assessment operations</param>
        /// <param name="adminTabBusiness">The admin tab business service for administrative operations</param>
        /// <param name="reports">The reports data business service for report generation</param>
        public MaturityCpgController(ITokenManager tokenManager, CSETContext context, IAssessmentUtil assessmentUtil,
    IAdminTabBusiness adminTabBusiness, IReportsDataBusiness reports)
        {
            _tokenManager = tokenManager;
            _context = context;
            _assessmentUtil = assessmentUtil;
            _adminTabBusiness = adminTabBusiness;
            _reports = reports;
        }

        /// <summary>
        /// Retrieves the CPG maturity structure and questions for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with CPG maturity structure and questions
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint returns the complete CPG maturity model structure including:
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
        /// CPG-specific features:
        /// - Cybersecurity Performance Goals framework
        /// - 8 core domains of cybersecurity performance
        /// - Maturity level progression and scoring
        /// - Comprehensive question coverage for cyber resilience
        /// 
        /// The structure supports:
        /// - Interactive assessment interfaces
        /// - Progress tracking and visualization
        /// - Detailed analysis and reporting
        /// - Compliance and performance mapping
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/maturity/structure/cpg")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetQuestions()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var lang = _tokenManager.GetCurrentLanguage();

            var biz = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            var x = biz.GetMaturityStructure(assessmentId, true, lang);

            return Ok(x);
        }

        /// <summary>
        /// Retrieves the CPG maturity structure for a specific bonus model (SSG).
        /// </summary>
        /// <param name="modelId">The ID of the specific SSG model to retrieve</param>
        /// <returns>
        /// 200 OK with CPG maturity structure for the specified model
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint returns CPG maturity structure for Sector-Specific Goals (SSG) models:
        /// - SSG model questions and structure
        /// - Domain-specific content and organization
        /// - Question text and supplemental information
        /// - Answer status and scoring data
        /// 
        /// The response includes:
        /// - SSG-specific question hierarchy
        /// - Domain and subdomain organization
        /// - Question text and detailed descriptions
        /// - Current answer status for each question
        /// - Maturity level mappings and scoring
        /// 
        /// SSG features:
        /// - Sector-specific cybersecurity goals
        /// - Industry-tailored assessment content
        /// - Specialized domain coverage
        /// - Enhanced performance measurement
        /// 
        /// Common SSG models include:
        /// - Chemical sector specific goals
        /// - Energy sector specific goals
        /// - Financial services specific goals
        /// - Healthcare sector specific goals
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/maturity/structure/cpg/bonus")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetQuestionsForModel([FromQuery] int modelId)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var lang = _tokenManager.GetCurrentLanguage();

            var biz = new MaturityBusiness(_context, _assessmentUtil, _adminTabBusiness);
            var x = biz.GetMaturityStructure(assessmentId, true, lang, modelId);

            return Ok(x);
        }

        /// <summary>
        /// Retrieves answer percentage distributions for each of the 8 CPG domains.
        /// </summary>
        /// <returns>
        /// 200 OK with answer distribution data for CPG domains
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides answer distribution analysis for CPG domains:
        /// - Percentage breakdowns by answer type
        /// - Domain-level performance metrics
        /// - Maturity level distribution analysis
        /// - Performance trend identification
        /// 
        /// The response includes:
        /// - Answer distribution percentages per domain
        /// - Maturity level progression data
        /// - Performance metrics and scoring
        /// - Gap analysis and improvement areas
        /// 
        /// CPG domains include:
        /// - Asset Management
        /// - Controls Management
        /// - Configuration and Change Management
        /// - Vulnerability Management
        /// - Incident Management
        /// - Service Continuity Management
        /// - Risk Management
        /// - External Dependencies Management
        /// 
        /// The analysis supports:
        /// - Performance benchmarking
        /// - Gap identification and prioritization
        /// - Progress tracking and trend analysis
        /// - Stakeholder reporting and presentations
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/answerdistrib/cpg/domains")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetAnswerDistribForDomains()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var lang = _tokenManager.GetCurrentLanguage();

            var cpgBiz = new CpgBusiness(_context, lang);
            var resp = cpgBiz.GetAnswerDistribForDomains(assessmentId);

            return Ok(resp);
        }

        /// <summary>
        /// Determines and returns the applicable SSG (Sector-Specific Goals) model ID for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with SSG model ID if applicable, null if no SSG model applies
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint determines which SSG model applies to the current assessment:
        /// - Analyzes assessment demographics and characteristics
        /// - Identifies applicable sector-specific goals
        /// - Returns the appropriate SSG model ID
        /// - Supports enhanced CPG assessments with sector-specific content
        /// 
        /// The determination process:
        /// - Evaluates organization sector and industry
        /// - Considers assessment scope and objectives
        /// - Matches characteristics to available SSG models
        /// - Returns null if no specific SSG model applies
        /// 
        /// Common SSG models include:
        /// - Chemical sector (Chemical SSG)
        /// - Energy sector (Energy SSG)
        /// - Financial services (Financial SSG)
        /// - Healthcare sector (Healthcare SSG)
        /// - Transportation sector (Transportation SSG)
        /// 
        /// SSG benefits:
        /// - Sector-specific cybersecurity guidance
        /// - Industry-tailored assessment questions
        /// - Enhanced relevance and applicability
        /// - Improved performance measurement
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/ssg/modelid")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetSsgModelId()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            var lang = _tokenManager.GetCurrentLanguage();

            var cpgBiz = new CpgBusiness(_context, lang);

            var ssgModelId = cpgBiz.DetermineSsgModel(assessmentId);

            return Ok(ssgModelId);
        }
    }
}
