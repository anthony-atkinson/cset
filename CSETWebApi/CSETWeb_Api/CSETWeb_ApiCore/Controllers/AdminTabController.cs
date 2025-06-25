//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Interfaces.AdminTab;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.AdminTab;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for admin tab functionality in CSET.
    /// This controller handles administrative data management for assessments,
    /// including retrieving admin tab data, saving administrative information,
    /// and managing assessment attributes. Supports both GET and POST operations
    /// for data retrieval and modification.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class AdminTabController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly IAdminTabBusiness _tabBusiness;

        /// <summary>
        /// Initializes a new instance of the AdminTabController.
        /// </summary>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        /// <param name="tabBusiness">Admin tab business logic service</param>
        public AdminTabController(ITokenManager tokenManager, IAdminTabBusiness tabBusiness)
        {
            _tokenManager = tokenManager;
            _tabBusiness = tabBusiness;
        }

        /// <summary>
        /// Retrieves admin tab data for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with admin tab data if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns administrative data associated with the current assessment.
        /// Supports both GET and POST HTTP methods for flexibility.
        /// The data includes administrative information and settings for the assessment.
        /// Used by the admin tab interface to display and manage assessment administrative data.
        /// </remarks>
        [HttpPost, HttpGet]
        [Route("api/admintab/Data")]
        public IActionResult GetList()
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_tabBusiness.GetTabData(assessmentId));
        }

        /// <summary>
        /// Saves administrative data for the current assessment.
        /// </summary>
        /// <param name="save">AdminSaveData object containing administrative information to save</param>
        /// <returns>
        /// 200 OK with save result if successful
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Saves administrative data for the current assessment.
        /// Validates and persists administrative information including settings,
        /// metadata, and configuration data. Returns the result of the save operation.
        /// Used to update administrative information in the admin tab interface.
        /// </remarks>
        [HttpPost]
        [Route("api/admintab/save")]
        public IActionResult SaveData([FromBody] AdminSaveData save)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            return Ok(_tabBusiness.SaveData(assessmentId, save));
        }

        /// <summary>
        /// Saves a specific attribute for the current assessment.
        /// </summary>
        /// <param name="attribute">AttributePair object containing the attribute name and value to save</param>
        /// <returns>
        /// 200 OK if attribute saved successfully
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Saves a single attribute-value pair for the current assessment.
        /// Provides granular control over individual administrative attributes.
        /// Useful for updating specific settings without affecting other administrative data.
        /// Used for real-time attribute updates in the admin interface.
        /// </remarks>
        [HttpPost]
        [Route("api/admintab/saveattribute")]
        public IActionResult SaveDataAttribute([FromBody] AttributePair attribute)
        {
            int assessmentId = _tokenManager.AssessmentForUser();
            _tabBusiness.SaveDataAttribute(assessmentId, attribute);
            return Ok();
        }
    }
}
