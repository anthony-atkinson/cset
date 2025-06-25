//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Demographic.Import;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for demographic import functionality in CSET.
    /// This controller handles the import of demographic data from JSON files,
    /// supporting bulk demographic data import for cybersecurity assessments.
    /// Processes CSET-formatted demographic JSON files and validates data structure.
    /// </summary>
    public class DemographicImportController : ControllerBase
    {
        private ITokenManager _tokenManager;
        private CSETContext _context;
        private IDemographicImportManager _demographicImportManager;

        /// <summary>
        /// Initializes a new instance of the DemographicImportController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="context">The database context for data access operations</param>
        /// <param name="demographicImportManager">The demographic import manager for import operations</param>
        public DemographicImportController(ITokenManager token, CSETContext context, IDemographicImportManager demographicImportManager)
        {
            _tokenManager = token;
            _context = context;
            _demographicImportManager = demographicImportManager;
        }

        /// <summary>
        /// Imports demographic data from a JSON file for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with true if import was successful
        /// 200 OK with ResponseMessage(100, "Not JSON") if file is not valid JSON
        /// 200 OK with ResponseMessage(101, "The JSON is not parseable as CSET demographics") if JSON structure is invalid
        /// 400 Bad Request if multiple files are uploaded
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint processes demographic data import from JSON files:
        /// - Single file upload support (one demographic file at a time)
        /// - JSON format validation and parsing
        /// - CSET demographic structure validation
        /// - Comprehensive demographic data import
        /// 
        /// The import process includes:
        /// - File upload and memory stream processing
        /// - JSON parsing and validation
        /// - CSET demographic structure validation
        /// - Database persistence operations
        /// - Error handling and response generation
        /// 
        /// Import features:
        /// - Single file upload limitation
        /// - JSON format validation
        /// - CSET demographic structure validation
        /// - Comprehensive error handling
        /// - Database persistence support
        /// 
        /// The import supports:
        /// - Bulk demographic data import
        /// - Assessment data migration
        /// - Demographic data backup restoration
        /// - Cross-assessment demographic transfer
        /// - Data standardization and validation
        /// 
        /// File requirements:
        /// - JSON format with CSET demographic structure
        /// - Single file upload (multiple files not supported)
        /// - Valid CSET demographic data model
        /// - Proper JSON syntax and structure
        /// 
        /// Error handling:
        /// - JSON parsing errors return code 100
        /// - Invalid CSET structure returns code 101
        /// - Multiple file uploads return 400 Bad Request
        /// - Successful imports return 200 OK with true
        /// 
        /// Imported data includes:
        /// - Core demographic information
        /// - Extended demographic details
        /// - CIS CSI service demographics
        /// - Service composition data
        /// - Organization details and information
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/demographics/import")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ImportDemographic()
        {
            // For now only allowing 1 uploaded file
            if (Request.Form.Files.Count > 1)
            {
                return BadRequest("Only a single demographic may be imported at a time.");
            }

            var assessmentFile = Request.Form.Files[0];

            var target = new MemoryStream();
            assessmentFile.CopyTo(target);

            var assessmentId = _tokenManager.AssessmentForUser();
            var currentUserId = _tokenManager.GetUserId();

            try
            {
                await _demographicImportManager.ProcessCSETDemographicImport(target.ToArray(), currentUserId, assessmentId, _tokenManager.GetAccessKey(), _context);
            }
            catch (JsonReaderException)
            {
                // The file content could not be parsed as JSON.  Return a successful response, but indicate an error condition.
                return StatusCode(200, new Models.ResponseMessage(100, "Not JSON"));
            }
            catch (Exception)
            {
                return StatusCode(200, new Models.ResponseMessage(101, "The JSON is not parseable as CSET demographics"));
            }

            return Ok(true);
        }
    }
}
