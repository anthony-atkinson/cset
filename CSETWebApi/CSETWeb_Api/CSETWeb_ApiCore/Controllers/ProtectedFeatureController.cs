//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CSETWebCore.Business.Authorization;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.Module;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Authorization;

namespace CSETWebCore.Api.Controllers
{
    /** 
     * Note: The functionality of this controller has been updated to work with the current gallery card system.
     * Protected features are now managed through hard-coded Gallery_Item_Guids for cards that should be hidden
     * unless protected features are enabled. Future implementation should add a column to GALLERY_ITEM
     * indicating if a card includes protected sets and should be hidden unless the protected features option
     * is enabled within CSET.
     * 
     * Current Implementation:
     * - Uses hard-coded Gallery_Item_Guids for protected cards
     * - Sets Is_Visible column to true when protected features are enabled
     * - Supports FAA modules and CISA assessor workflow
     * 
     * Future Enhancement:
     * - Add ProtectedFeature column to GALLERY_ITEM table
     * - Implement dynamic protected feature detection
     * - Remove hard-coded GUID dependencies
     * - Improve maintainability and flexibility
     */

    /// <summary>
    /// Provides endpoints for protected feature management in CSET.
    /// This controller handles the enabling and management of protected features including
    /// FAA modules and CISA assessor workflow functionality. Supports feature access control
    /// and workflow mode management for specialized assessment scenarios.
    /// 
    /// Note: This controller functionality is currently outdated as sets are now exposed
    /// through gallery cards. Future implementation should use GALLERY_ITEM columns for
    /// protected feature indication.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class ProtectedFeatureController : ControllerBase
    {
        private CSETContext _context;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Initializes a new instance of the ProtectedFeatureController.
        /// </summary>
        /// <param name="context">The database context for protected feature data access</param>
        /// <param name="tokenManager">The token manager for user authentication and identification</param>
        public ProtectedFeatureController(CSETContext context, ITokenManager tokenManager)
        {
            _context = context;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Retrieves the list of protected modules with their locked/unlocked status.
        /// </summary>
        /// <returns>
        /// 200 OK with List of EnabledModule containing protected module information
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all protected modules and their current status:
        /// - FAA modules and their locked/unlocked state
        /// - Module short names and full names
        /// - Current access status for each module
        /// 
        /// The protected modules include:
        /// - FAA_MAINT (FAA Maintenance)
        /// - FAA_PED_V2 (FAA PED Version 2)
        /// - Other encrypted modules
        /// 
        /// Module features:
        /// - IsEncryptedModule flag indicates lockable modules
        /// - IsEncryptedModuleOpen indicates unlocked status
        /// - Short and full name identification
        /// - Access control management
        /// 
        /// The response includes:
        /// - List of enabled modules
        /// - Module short names
        /// - Module full names
        /// - Unlocked status for each module
        /// 
        /// Protected modules are:
        /// - FAA-specific assessment modules
        /// - Specialized aviation security modules
        /// - Restricted access modules
        /// - Government-specific content
        /// 
        /// Usage scenarios:
        /// - Protected feature management interface
        /// - Module access control
        /// - Feature status verification
        /// - Administrative oversight
        /// - Access control auditing
        /// 
        /// The modules support:
        /// - FAA compliance assessments
        /// - Aviation security evaluations
        /// - Government-specific workflows
        /// - Restricted content access
        /// - Specialized assessment scenarios
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/EnableProtectedFeature/Features")]
        [ProducesResponseType(typeof(List<EnabledModule>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetFeatures()
        {
            var openFaaSets = _context.SETS.Where(s => s.IsEncryptedModule).ToList();

            var enabledModules = new List<EnabledModule>();

            foreach (var s in openFaaSets)
            {
                enabledModules.Add(new EnabledModule()
                {
                    ShortName = s.Short_Name,
                    FullName = s.Full_Name,
                    Unlocked = s.IsEncryptedModuleOpen
                });
            }

            return Ok(enabledModules);
        }

        /// <summary>
        /// Enables all protected modules and makes corresponding gallery cards visible.
        /// </summary>
        /// <param name="set_name">The name of the set to enable (currently unused parameter)</param>
        /// <returns>
        /// 200 OK with success message if modules enabled successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint enables all protected modules and updates gallery visibility:
        /// - Unlocks all encrypted modules in the database
        /// - Makes corresponding gallery cards visible
        /// - Updates FAA module access status
        /// - Enables protected feature access
        /// 
        /// The enable process includes:
        /// - Setting IsEncryptedModuleOpen to true for all protected sets
        /// - Making FAA gallery cards visible
        /// - Updating database records
        /// - Persisting changes to storage
        /// 
        /// Gallery card updates:
        /// - FAA_MAINT card becomes visible
        /// - FAA_PED_V2 card becomes visible
        /// - Other protected cards as configured
        /// - Gallery interface updates
        /// 
        /// The enabled modules include:
        /// - FAA Maintenance modules
        /// - FAA PED Version 2 modules
        /// - Other encrypted modules
        /// - Protected assessment content
        /// 
        /// Usage scenarios:
        /// - Administrative feature enablement
        /// - Protected content access
        /// - FAA module activation
        /// - Gallery interface updates
        /// - Access control management
        /// 
        /// The enablement process:
        /// - Updates all protected sets
        /// - Makes gallery cards visible
        /// - Persists database changes
        /// - Updates user interface
        /// - Enables protected workflows
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/EnableProtectedFeature/enableModules")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult EnableFAA(string set_name)
        {
            AddNewlyEnabledModules();

            var response = new
            {
                Message = ""
            };
            return Ok(response);
        }

        /// <summary>
        /// Sets the CISA assessor workflow mode for the current user.
        /// </summary>
        /// <param name="cisaWorkflowEnabled">Boolean indicating whether to enable CISA assessor workflow</param>
        /// <returns>
        /// 200 OK with success message if workflow mode set successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint toggles the CISA assessor workflow mode for the current user:
        /// - Updates user's CISA assessor workflow setting
        /// - Supports both user accounts and access keys
        /// - Persists workflow mode preference
        /// - Enables specialized assessment workflows
        /// 
        /// The CISA assessor workflow includes:
        /// - Specialized assessment interfaces
        /// - CISA-specific question sets
        /// - Government assessment workflows
        /// - Enhanced reporting capabilities
        /// - Specialized assessment tools
        /// 
        /// Workflow features:
        /// - CISA-specific assessment modes
        /// - Government compliance workflows
        /// - Enhanced assessment capabilities
        /// - Specialized reporting tools
        /// - Professional assessment interfaces
        /// 
        /// User identification:
        /// - Supports user ID-based identification
        /// - Supports access key-based identification
        /// - Handles both authentication methods
        /// - Updates appropriate user record
        /// 
        /// The workflow mode affects:
        /// - Assessment interface display
        /// - Available question sets
        /// - Reporting capabilities
        /// - Assessment workflow
        /// - Tool availability
        /// 
        /// Usage scenarios:
        /// - CISA assessor mode activation
        /// - Government assessment workflows
        /// - Professional assessment tools
        /// - Specialized assessment interfaces
        /// - Workflow preference management
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/EnableProtectedFeature/setCisaAssessorWorkflow")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult SetCisaAssessorWorkflow([FromBody] bool cisaWorkflowEnabled)
        {
            var userId = _tokenManager.GetCurrentUserId();
            var ak = _tokenManager.GetAccessKey();

            if (userId != null)
            {
                _context.USERS.Where(u => u.UserId == userId).FirstOrDefault().CisaAssessorWorkflow = cisaWorkflowEnabled;
            }
            else if (ak != null)
            {
                _context.ACCESS_KEY.Where(a => a.AccessKey == ak).FirstOrDefault().CisaAssessorWorkflow = cisaWorkflowEnabled;
            }

            _context.SaveChanges();

            var response = new
            {
                Message = "'CisaAssessorWorkflow' database property set successfully."
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves the current CISA assessor workflow status for the authenticated user.
        /// </summary>
        /// <returns>
        /// 200 OK with boolean indicating CISA assessor workflow status
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves the current CISA assessor workflow status:
        /// - Checks user's current workflow mode setting
        /// - Supports both user accounts and access keys
        /// - Returns current workflow preference
        /// - Handles unauthenticated scenarios gracefully
        /// 
        /// The workflow status indicates:
        /// - Whether CISA assessor mode is enabled
        /// - Current user workflow preferences
        /// - Assessment interface mode
        /// - Available assessment capabilities
        /// 
        /// Status determination:
        /// - Checks user record for workflow setting
        /// - Checks access key record if applicable
        /// - Returns false for unauthenticated users
        /// - Handles missing user records gracefully
        /// 
        /// The response indicates:
        /// - True if CISA assessor workflow is enabled
        /// - False if workflow is disabled or user not found
        /// - Current workflow preference
        /// - Assessment mode status
        /// 
        /// Usage scenarios:
        /// - Workflow status verification
        /// - Interface mode determination
        /// - Assessment capability checking
        /// - User preference retrieval
        /// - Mode status display
        /// 
        /// The workflow affects:
        /// - Assessment interface display
        /// - Available tools and features
        /// - Question set availability
        /// - Reporting capabilities
        /// - Assessment workflow
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/EnableProtectedFeature/getCisaAssessorWorkflow")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCisaAssessorWorkflow()
        {
            var userId = _tokenManager.GetCurrentUserId();
            var ak = _tokenManager.GetAccessKey();

            // Assume false if we can't find the user or access key (they are probably on the login page).
            bool cisaWorkflowEnabled = false;

            if (userId != null)
            {
                var user = _context.USERS.Where(u => u.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    cisaWorkflowEnabled = user.CisaAssessorWorkflow;
                }
            }
            else if (ak != null)
            {
                var acesskey = _context.ACCESS_KEY.Where(a => a.AccessKey == ak).FirstOrDefault();
                if (acesskey != null)
                {
                    cisaWorkflowEnabled = acesskey.CisaAssessorWorkflow;
                }
            }

            return Ok(cisaWorkflowEnabled);
        }

        /// <summary>
        /// Marks all protected modules as unlocked and makes corresponding gallery cards visible.
        /// </summary>
        /// <remarks>
        /// This private method performs the actual module enablement:
        /// - Sets IsEncryptedModuleOpen to true for all protected sets
        /// - Makes specific FAA gallery cards visible
        /// - Updates database records
        /// - Persists changes to storage
        /// 
        /// Gallery card updates include:
        /// - FAA_MAINT (GUID: 4EE4C330-5A4C-42F0-8584-5188EDDA4E95)
        /// - FAA_PED_V2 (GUID: 4929452F-A737-4AFC-9778-CE8D550DF305)
        /// - Other protected cards as configured
        /// 
        /// This method is called by the EnableFAA endpoint to perform
        /// the actual database updates and gallery card visibility changes.
        /// </remarks>
        private void AddNewlyEnabledModules()
        {
            var sets2 = _context.SETS.Where(s => s.IsEncryptedModule);
            foreach (SETS sts in sets2)
            {
                sts.IsEncryptedModuleOpen = true;
            }

            // FAA_MAINT: GUID = 4EE4C330-5A4C-42F0-8584-5188EDDA4E95; FAA_PED_V2: GUID = 4929452F-A737-4AFC-9778-CE8D550DF305
            var gallItems = _context.GALLERY_ITEM.Where(g => g.Gallery_Item_Guid.Equals(new Guid("4EE4C330-5A4C-42F0-8584-5188EDDA4E95")) || g.Gallery_Item_Guid.Equals(new Guid("4929452F-A737-4AFC-9778-CE8D550DF305")));
            foreach (GALLERY_ITEM gall in gallItems)
            {
                gall.Is_Visible = true;
            }
            _context.SaveChanges();
        }
    }
}
