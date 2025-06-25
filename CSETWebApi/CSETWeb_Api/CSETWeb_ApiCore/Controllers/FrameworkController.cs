//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 

using CSETWebCore.Business.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSETWebCore.Interfaces.Framework;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Framework;

namespace CSETWebCore.Api.Controllers
{   
    /// <summary>
    /// Provides endpoints for managing cybersecurity framework tiers in CSET assessments.
    /// Supports the NIST Cybersecurity Framework tier selection process, allowing users to
    /// define their organization's cybersecurity maturity level across different framework functions.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class FrameworkController : ControllerBase
    {
        private readonly IFrameworkBusiness _framework;
        private readonly ITokenManager _token;

        /// <summary>
        /// Initializes a new instance of the FrameworkController.
        /// </summary>
        /// <param name="framework">The framework business service for handling framework operations</param>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        public FrameworkController(IFrameworkBusiness framework, ITokenManager token)
        {
            _framework = framework;
            _token = token;
        }

        /// <summary>
        /// Retrieves all available cybersecurity framework tiers for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with FrameworkResponse containing tier types and their associated tiers
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint returns the complete framework structure including:
        /// - All available tier types (e.g., "IDENTIFY", "PROTECT", "DETECT", "RESPOND", "RECOVER")
        /// - Associated tiers for each type (e.g., "Tier 1", "Tier 2", "Tier 3", "Tier 4")
        /// - Current selection state for each tier
        /// - Descriptive questions for each tier
        /// - Unique control IDs for UI radio button grouping
        /// 
        /// The response includes:
        /// - TierTypes: List of framework functions with their associated tiers
        /// - SelectedTier: Currently selected tier for each function
        /// - Tiers: Available options with selection state and descriptions
        /// 
        /// Common tier types include:
        /// - IDENTIFY: Asset management, business environment, governance
        /// - PROTECT: Access control, awareness training, data security
        /// - DETECT: Anomalies and events, security monitoring
        /// - RESPOND: Response planning, communications, analysis
        /// - RECOVER: Recovery planning, improvements, communications
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/frameworks")]
        [ProducesResponseType(typeof(FrameworkResponse), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetFrameworks()
        {
            int assessmentId = _token.AssessmentForUser();
            return Ok(_framework.GetFrameworks(assessmentId));
        }

        /// <summary>
        /// Persists the selected framework tier for a specific tier type to the database.
        /// </summary>
        /// <param name="tier">The tier selection containing tier type and selected tier name</param>
        /// <returns>
        /// 200 OK if tier selection was successfully persisted
        /// 400 Bad Request if tier parameter is null or invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves the user's framework tier selection for assessment configuration.
        /// 
        /// The tier selection process:
        /// - Updates or creates a FRAMEWORK_TIER_TYPE_ANSWER record
        /// - Associates the selection with the current assessment
        /// - Updates the assessment's last modified timestamp
        /// - Handles null tier parameters gracefully
        /// 
        /// Sample request body:
        /// {
        ///   "tierType": "IDENTIFY",
        ///   "tierName": "Tier 2"
        /// }
        /// 
        /// Valid tier types include:
        /// - IDENTIFY, PROTECT, DETECT, RESPOND, RECOVER
        /// 
        /// Valid tier names include:
        /// - Tier 1, Tier 2, Tier 3, Tier 4
        /// 
        /// The selection affects:
        /// - Assessment scoring and maturity calculations
        /// - Report generation and recommendations
        /// - Framework-based analysis and comparisons
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/framework")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult PersistSelectedTierAnswer(TierSelection tier)
        {
            // In case nothing is sent, bail out gracefully
            if (tier == null)
            {
                return Ok();
            }

            int assessmentId = _token.AssessmentForUser();
            _framework.PersistSelectedTierAnswer(assessmentId, tier);
            return Ok();
        }
    }
}
