//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.Demographic;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Assessment;
using CSETWebCore.Interfaces.Demographic;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.Demographic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using CSETWebCore.Business.Demographic.Export;
using CSETWebCore.Business.Demographic.DemographicIO;
using CSETWebCore.Helpers;
using Microsoft.AspNetCore.Http;
using System.IO;
using CSETWebCore.Business.Demographic.Import;
using CSETWebCore.Business.AssessmentIO.Import;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for extended demographic functionality in CSET.
    /// This controller handles extended demographic data collection, management,
    /// and export operations for cybersecurity assessments. Supports comprehensive
    /// demographic information including organization details, sector information,
    /// employee counts, revenue data, and regulatory compliance information.
    /// </summary>
    [ApiController]
    public class DemographicsExtController : ControllerBase
    {
        private readonly ITokenManager _token;
        private readonly IAssessmentBusiness _assessment;
        private readonly IDemographicBusiness _demographic;
        private CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the DemographicsExtController.
        /// </summary>
        /// <param name="token">The token manager for user authentication and assessment context</param>
        /// <param name="assessment">The assessment business service for assessment operations</param>
        /// <param name="demographic">The demographic business service for demographic operations</param>
        /// <param name="context">The database context for data access operations</param>
        public DemographicsExtController(ITokenManager token, IAssessmentBusiness assessment, IDemographicBusiness demographic, CSETContext context)
        {
            _token = token;
            _assessment = assessment;
            _demographic = demographic;
            _context = context;
        }

        /// <summary>
        /// Retrieves extended demographic data for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with DemographicExt containing extended demographic data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides comprehensive extended demographic information:
        /// - Organization type and name
        /// - Sector and subsector information
        /// - Employee counts and revenue data
        /// - CISA region and geographic information
        /// - Regulatory compliance details
        /// - Standards and frameworks used
        /// 
        /// The response includes:
        /// - Complete demographic data structure
        /// - Organization details and characteristics
        /// - Sector and industry classifications
        /// - Employee and revenue information
        /// - Geographic and regulatory data
        /// 
        /// Extended demographic features:
        /// - Comprehensive organization profiling
        /// - Sector-specific data collection
        /// - Regulatory compliance tracking
        /// - Geographic scope identification
        /// - Standards and framework usage
        /// 
        /// The demographic data supports:
        /// - Assessment customization and tailoring
        /// - Sector-specific analysis and reporting
        /// - Regulatory compliance tracking
        /// - Geographic and organizational benchmarking
        /// - Standards and framework analysis
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext2")]
        [ProducesResponseType(typeof(DemographicExt), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetExtended2()
        {
            var assessmentId = _token.AssessmentForUser();

            var mgr = new DemographicExtBusiness(_context);
            var response = mgr.GetDemographics(assessmentId);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves subsectors for a specific sector ID.
        /// </summary>
        /// <param name="id">The sector ID to get subsectors for</param>
        /// <returns>
        /// 200 OK with list of subsectors for the specified sector
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint provides subsector options for sector selection:
        /// - Subsector list based on parent sector
        /// - Hierarchical sector organization
        /// - Dynamic subsector filtering
        /// - Sector-specific subsector options
        /// 
        /// The response includes:
        /// - List of available subsectors
        /// - Subsector IDs and names
        /// - Sector-specific organization
        /// - Hierarchical relationship data
        /// 
        /// Subsector features:
        /// - Dynamic sector-based filtering
        /// - Hierarchical sector organization
        /// - Sector-specific subsector options
        /// - Assessment customization support
        /// 
        /// The subsector data supports:
        /// - Sector-specific assessment tailoring
        /// - Industry classification and analysis
        /// - Assessment customization
        /// - Sector-based reporting and analytics
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/ext2/subsectors/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public IActionResult GetSubsectors(int id)
        {
            var mgr = new DemographicExtBusiness(_context);
            var response = mgr.GetSubsectors(id);
            return Ok(response);
        }

        /// <summary>
        /// Persists extended demographic data for the current assessment.
        /// </summary>
        /// <param name="demographics">The extended demographic data to save</param>
        /// <returns>
        /// 200 OK if demographic data was saved successfully
        /// 400 Bad Request if demographic data is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves comprehensive extended demographic information:
        /// - Organization details and characteristics
        /// - Sector and subsector classifications
        /// - Employee counts and revenue information
        /// - Geographic and regulatory data
        /// - Standards and framework usage
        /// 
        /// The save process includes:
        /// - Data validation and processing
        /// - Database persistence operations
        /// - Assessment naming updates
        /// - Cross-table data synchronization
        /// 
        /// Extended demographic data includes:
        /// - Organization type and name
        /// - Sector and subsector information
        /// - Employee counts (total and unit)
        /// - Annual revenue and critical service percentages
        /// - CISA region and geographic scope
        /// - Regulatory compliance information
        /// - Standards and frameworks used
        /// - Business unit and contact information
        /// 
        /// The save operation supports:
        /// - Comprehensive demographic data collection
        /// - Assessment customization and tailoring
        /// - Sector-specific analysis preparation
        /// - Regulatory compliance tracking
        /// - Geographic and organizational profiling
        /// 
        /// Data is synchronized across multiple tables:
        /// - DEMOGRAPHICS table for core data
        /// - DETAILS_DEMOGRAPHICS for extended fields
        /// - INFORMATION table for organization name
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/demographics/ext2")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult PostExtended2([FromBody] DemographicExt demographics)
        {
            demographics.AssessmentId = _token.AssessmentForUser();
            var userid = _token.GetCurrentUserId();

            var mgr = new DemographicExtBusiness(_context);
            mgr.SaveDemographics(demographics, userid ?? 0);

            return Ok();
        }

        /// <summary>
        /// Exports demographic data for the current assessment.
        /// </summary>
        /// <returns>
        /// 200 OK with file containing exported demographic data
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// This endpoint exports demographic data to a downloadable file:
        /// - Complete demographic data export
        /// - File-based data export functionality
        /// - Assessment-specific demographic export
        /// - Structured data export format
        /// 
        /// The export process includes:
        /// - Demographic data compilation
        /// - File generation and formatting
        /// - Error handling and logging
        /// - File download response
        /// 
        /// Export features:
        /// - Complete demographic data export
        /// - File-based download functionality
        /// - Assessment-specific data export
        /// - Structured export format
        /// 
        /// The export supports:
        /// - Data backup and archival
        /// - Assessment data portability
        /// - External analysis and reporting
        /// - Data sharing and collaboration
        /// - Compliance and audit requirements
        /// 
        /// File format and handling:
        /// - Binary file format for data integrity
        /// - Application/octet-stream content type
        /// - Descriptive filename for identification
        /// - Error logging for troubleshooting
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/demographics/export")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult ExportDemographic()
        {
            try
            {
                int assessmentId = _token.AssessmentForUser();
                DemographicsExportFile result = new DemographicsExportManager(_context).ExportDemographics(assessmentId);

                return File(result.FileContents, "application/octet-stream", result.FileName);
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return null;
        }
    }
}
