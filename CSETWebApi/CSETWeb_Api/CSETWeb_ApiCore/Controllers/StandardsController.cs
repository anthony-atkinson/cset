//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Standards;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Interfaces.Question;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using CSETWebCore.Business.Authorization;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for managing cybersecurity standards and frameworks in CSET assessments.
    /// Supports standard selection, framework detection, and ACET (Assess, Communicate, Evaluate, Test)
    /// framework validation for assessment configuration.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class StandardsController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly IAssessmentUtil _assessmentUtil;
        private IQuestionRequirementManager _questionRequirement;
        private IDemographicBusiness _demographicBusiness;
        private readonly StandardsBusiness _standards;


        /// <summary>
        /// Initializes a new instance of the StandardsController.
        /// </summary>
        /// <param name="tokenManager">Service for JWT token management</param>
        /// <param name="context">Database context</param>
        /// <param name="assessmentUtil">Utility service for assessment operations</param>
        /// <param name="questionRequirement">Service for question requirement management</param>
        /// <param name="demographicBusiness">Service for demographic operations</param>
        public StandardsController(ITokenManager tokenManager, CSETContext context,
             IAssessmentUtil assessmentUtil, IQuestionRequirementManager questionRequirement,
            IDemographicBusiness demographicBusiness)
        {
            _tokenManager = tokenManager;
            _context = context;
            _assessmentUtil = assessmentUtil;
            _questionRequirement = questionRequirement;
            _demographicBusiness = demographicBusiness;
            _standards = new StandardsBusiness(_context, _assessmentUtil, _questionRequirement, _tokenManager,
                _demographicBusiness);
        }

        /// <summary>
        /// Retrieves available standards for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with list of available standards
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all available cybersecurity standards and frameworks
        /// that can be selected for the current assessment. The list includes both
        /// selected and unselected standards with their metadata.
        /// 
        /// The response includes:
        /// - Standard names and codes
        /// - Selection status for current assessment
        /// - Standard categories and descriptions
        /// - Framework indicators
        /// 
        /// Common standards include:
        /// - NIST Cybersecurity Framework
        /// - ISO 27001
        /// - NERC CIP
        /// - CMMC (Cybersecurity Maturity Model Certification)
        /// - ACET (Assess, Communicate, Evaluate, Test)
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/standards")]
        [ProducesResponseType(typeof(List<Standard>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetStandards()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_standards.GetStandards(assessmentId));
        }


        /// <summary>
        /// Persists the selected standards for the current assessment.
        /// </summary>
        /// <param name="selectedStandards">List of standard codes to select</param>
        /// <returns>
        /// 200 OK with updated standards list
        /// 400 Bad Request if standard selection is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves the user's standard selections for the current assessment.
        /// The selected standards determine which questions and requirements will be
        /// included in the assessment.
        /// 
        /// The request should include:
        /// - List of standard codes to select
        /// - Standard codes should match available standards
        /// 
        /// Sample request:
        ///     POST /api/standard
        ///     [
        ///         "NIST_CSF",
        ///         "ISO_27001",
        ///         "CMMC"
        ///     ]
        /// 
        /// The endpoint will:
        /// - Validate the provided standard codes
        /// - Update the assessment's standard selection
        /// - Return the updated list of standards with selection status
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/standard")]
        [ProducesResponseType(typeof(List<Standard>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult PersistSelectedStandards([FromBody] List<string> selectedStandards)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_standards.PersistSelectedStandards(assessmentId, selectedStandards));
        }

        /// <summary>
        /// Sets the default standard for a basic assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with default standard selection
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint sets the default standard selection for a basic assessment.
        /// This is typically used when creating new assessments or when a user
        /// wants to start with a standard baseline configuration.
        /// 
        /// The default standard is usually the NIST Cybersecurity Framework,
        /// which provides a comprehensive baseline for cybersecurity assessments.
        /// 
        /// The endpoint will:
        /// - Set the default standard for the assessment
        /// - Return the updated standards list
        /// - Configure the assessment for basic evaluation
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/basicStandard")]
        [ProducesResponseType(typeof(List<Standard>), 200)]
        [ProducesResponseType(401)]
        public IActionResult PersistDefaultSelectedStandards()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_standards.PersistDefaultSelectedStandard(assessmentId));
        }

        /// <summary>
        /// Checks if the current assessment uses a framework-based approach.
        /// </summary>
        /// <returns>
        /// 200 OK with framework selection status
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint determines whether the current assessment is configured
        /// to use a framework-based approach rather than individual standards.
        /// 
        /// Framework-based assessments typically provide:
        /// - Integrated evaluation across multiple standards
        /// - Unified scoring and reporting
        /// - Cross-standard mapping and analysis
        /// 
        /// Common frameworks include:
        /// - NIST Cybersecurity Framework
        /// - CMMC (Cybersecurity Maturity Model Certification)
        /// - ACET (Assess, Communicate, Evaluate, Test)
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/standard/IsFramework")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetFrameworkSelected()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_standards.GetFramework(assessmentId));
        }

        /// <summary>
        /// Checks if the current assessment uses the ACET framework.
        /// </summary>
        /// <returns>
        /// 200 OK with ACET framework selection status
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint determines whether the current assessment is configured
        /// to use the ACET (Assess, Communicate, Evaluate, Test) framework.
        /// 
        /// ACET framework provides:
        /// - Comprehensive cybersecurity evaluation
        /// - Communication-focused assessment approach
        /// - Evaluation and testing methodologies
        /// - Specialized reporting and analysis
        /// 
        /// ACET is particularly useful for:
        /// - Critical infrastructure assessments
        /// - Government and defense organizations
        /// - Organizations requiring detailed communication protocols
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/standard/IsACET")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetACETSelected()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_standards.GetACET(assessmentId));
        }
    }
}
