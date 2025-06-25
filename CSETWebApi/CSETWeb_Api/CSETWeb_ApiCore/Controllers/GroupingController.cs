//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Grouping;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Maturity;
using Microsoft.AspNetCore.Mvc;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for question grouping operations in CSET.
    /// This controller handles maturity model grouping selection and management,
    /// supporting selective assessment of maturity model domains, goals, and
    /// capabilities. Enables users to focus on specific areas of maturity models.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class GroupingController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the GroupingController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="context">The database context for data access operations</param>
        public GroupingController(ITokenManager token, CSETContext context)
        {
            _token = token;
            _context = context;
        }

        /// <summary>
        /// Retrieves the currently selected groupings for the assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with List of int containing selected grouping IDs
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the currently selected groupings for the assessment:
        /// - List of selected grouping IDs
        /// - Assessment-specific grouping selections
        /// - Maturity model grouping state
        /// - Grouping selection persistence
        /// 
        /// The response includes:
        /// - List of selected grouping IDs
        /// - Assessment-specific selections
        /// - Maturity model grouping state
        /// - Grouping hierarchy information
        /// 
        /// Grouping features:
        /// - Maturity model domain selection
        /// - Goal and capability selection
        /// - Assessment-specific grouping state
        /// - Grouping hierarchy management
        /// 
        /// The grouping selections support:
        /// - Focused maturity assessments
        /// - Selective domain evaluation
        /// - Custom assessment scoping
        /// - Maturity model customization
        /// - Assessment efficiency optimization
        /// 
        /// Usage scenarios:
        /// - Maturity model assessment setup
        /// - Domain-specific evaluations
        /// - Custom assessment scoping
        /// - Grouping state management
        /// - Assessment customization
        /// 
        /// Grouping types include:
        /// - Domains (top-level groupings)
        /// - Goals (mid-level groupings)
        /// - Capabilities (low-level groupings)
        /// - Custom groupings
        /// - Model-specific groupings
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/groupselections")]
        [ProducesResponseType(typeof(List<int>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetSelections()
        {
            int assessmentId = _token.AssessmentForUser();

            var biz = new GroupingBusiness(assessmentId, _context);

            return Ok(biz.GetSelections());
        }

        /// <summary>
        /// Updates the grouping selection status for the assessment.
        /// </summary>
        /// <param name="request">The grouping selection request containing grouping IDs and selection status</param>
        /// <returns>
        /// 200 OK if grouping selections were updated successfully
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint updates the grouping selection status for the assessment:
        /// - Add or remove grouping selections
        /// - Bulk grouping selection updates
        /// - Assessment-specific grouping state
        /// - Answer clearing for deselected groupings
        /// 
        /// The update process includes:
        /// - Grouping selection persistence
        /// - Database record management
        /// - Answer clearing for deselected groupings
        /// - Assessment state updates
        /// 
        /// Selection features:
        /// - Add groupings to selection
        /// - Remove groupings from selection
        /// - Bulk selection updates
        /// - Answer clearing for deselected groupings
        /// - Assessment state management
        /// 
        /// The selection updates support:
        /// - Dynamic assessment scoping
        /// - Maturity model customization
        /// - Focused evaluation areas
        /// - Assessment efficiency
        /// - Custom assessment workflows
        /// 
        /// When groupings are deselected:
        /// - Related answers are cleared (set to "U")
        /// - Assessment state is updated
        /// - Grouping selections are removed
        /// - Database consistency is maintained
        /// 
        /// Usage scenarios:
        /// - Initial assessment setup
        /// - Dynamic assessment scoping
        /// - Maturity model customization
        /// - Focused evaluation areas
        /// - Assessment workflow management
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/groupselection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult ChangeSelection([FromBody] GroupSelectionRequest request)
        {
            int assessmentId = _token.AssessmentForUser();

            var biz = new GroupingBusiness(assessmentId, _context);
            biz.PersistSelections(request);

            return Ok();
        }
    }
}
