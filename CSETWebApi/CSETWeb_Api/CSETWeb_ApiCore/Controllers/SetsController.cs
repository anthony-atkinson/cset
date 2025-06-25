//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.ModuleIO;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Model.AssessmentIO;
using CSETWebCore.Business.GalleryParser;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for managing question sets and standards in CSET.
    /// Supports set retrieval, import of external standards, and export of
    /// existing standards for sharing and distribution.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class SetsController : ControllerBase
    {
        private CSETContext _context;
        private readonly IGalleryEditor _galleryEditor;

        /// <summary>
        /// Initializes a new instance of the SetsController.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="galleryEditor">Service for gallery editing operations</param>
        public SetsController(CSETContext context, IGalleryEditor galleryEditor)
        {
            _context = context;
            _galleryEditor = galleryEditor;
        }

        /// <summary>
        /// Retrieves all available question sets and standards.
        /// </summary>
        /// <returns>
        /// 200 OK with list of available sets
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves all question sets and standards that are available
        /// for use in CSET assessments. Only sets marked as displayed are returned.
        /// 
        /// The response includes:
        /// - Set full names for display
        /// - Set names for internal reference
        /// - Ordered alphabetically by name
        /// 
        /// Question sets include:
        /// - Standard cybersecurity frameworks
        /// - Industry-specific standards
        /// - Custom question sets
        /// - Maturity model questions
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/sets")]
        [ProducesResponseType(typeof(object[]), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetAllSets()
        {
            var sets = _context.SETS.Where(s => s.Is_Displayed)
                .Select(s => new { Name = s.Full_Name, SetName = s.Set_Name })
                .OrderBy(s => s.Name)
                .ToArray();
            return Ok(sets);
        }


        /// <summary>
        /// Imports a new external standard into CSET.
        /// </summary>
        /// <param name="externalStandard">External standard data to import</param>
        /// <returns>
        /// 200 OK if import successful
        /// 400 Bad Request if standard data is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if import fails
        /// </returns>
        /// <remarks>
        /// This endpoint imports a new external standard or question set into CSET.
        /// The imported standard becomes available for use in assessments after
        /// successful import.
        /// 
        /// The request should include:
        /// - Standard name and metadata
        /// - Question definitions and requirements
        /// - Reference materials and documentation
        /// - Category and classification information
        /// 
        /// The import process:
        /// - Validates the standard structure
        /// - Processes questions and requirements
        /// - Creates necessary database records
        /// - Updates the gallery configuration
        /// 
        /// Sample request:
        ///     POST /api/sets/import
        ///     {
        ///         "Name": "Custom Security Standard",
        ///         "ShortName": "CSS",
        ///         "Category": "Custom",
        ///         "Questions": [...],
        ///         "Requirements": [...]
        ///     }
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/sets/import")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult Import([FromBody] ExternalStandard externalStandard)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var mp = new ModuleImporter(_context, _galleryEditor);
                        mp.ProcessStandard(externalStandard);
                    }
                    catch (Exception exc)
                    {
                        NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");

                        throw;
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        /// <summary>
        /// Exports a question set or standard for external use.
        /// </summary>
        /// <param name="setName">Name of the set to export</param>
        /// <returns>
        /// 200 OK with exported standard data
        /// 400 Bad Request if set name is not found
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint exports a complete question set or standard from CSET
        /// in a format suitable for sharing, backup, or import into other systems.
        /// 
        /// The exported data includes:
        /// - Complete set metadata and configuration
        /// - All questions and requirements
        /// - Reference materials and documentation
        /// - Category and classification information
        /// - Requirement levels and relationships
        /// 
        /// Sample request:
        ///     GET /api/sets/export/NIST_CSF
        /// 
        /// The exported format is compatible with the import endpoint,
        /// allowing standards to be shared between CSET installations.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/sets/export/{setName}")]
        [ProducesResponseType(typeof(ExternalStandard), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult Export(string setName)
        {
            var set = _context.SETS
                .Include(s => s.Set_Category)
                .Include(s => s.REQUIREMENT_SETS)
                    .ThenInclude(r => r.Requirement)
                        .ThenInclude(rf => rf.REQUIREMENT_REFERENCES)
                            .ThenInclude(gf => gf.Gen_File)
                .Include(s => s.REQUIREMENT_SETS)
                    .ThenInclude(r => r.Requirement)
                        .ThenInclude(r => r.REQUIREMENT_LEVELS)
                .Where(s => (s.Is_Displayed) && s.Set_Name == setName).FirstOrDefault();

            if (set == null)
            {
                BadRequest($"A Set named '{setName}' was not found.");
            }

            return Ok(set.ToExternalStandard(_context));
        }
    }
}
