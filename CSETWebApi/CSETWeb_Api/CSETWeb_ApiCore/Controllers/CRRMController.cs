using CSETWebCore.DataLayer.Model;
using Microsoft.AspNetCore.Mvc;
using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.AssessmentIO.Export;
using CSETWebCore.Helpers;
using System;
using System.IO;
using CSETWebCore.Business.AssessmentIO.Import;
using CSETWebCore.Interfaces.Helpers;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for CRRM (Cyber Resilience Review Model) operations in CSET.
    /// This controller handles bulk assessment export operations for access key assessments
    /// that have been modified since their last export. Supports automated assessment
    /// synchronization and data exchange workflows.
    /// </summary>
    public class CRRMController : Controller
    {
        private readonly CSETContext _context;

        /// <summary>
        /// Initializes a new instance of the CRRMController.
        /// </summary>
        /// <param name="context">The database context for data access operations</param>
        public CRRMController(CSETContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exports all access key assessments that have been modified since their last export.
        /// </summary>
        /// <returns>
        /// 200 OK with ZIP file containing exported assessments if successful
        /// 204 No Content if no assessments need to be exported
        /// 401 Unauthorized if API key is invalid
        /// 500 Internal Server Error if export operation fails
        /// </returns>
        /// <remarks>
        /// This endpoint performs a bulk export operation for access key assessments:
        /// - Queries for assessments where ModifiedSinceLastExport flag is true
        /// - Determines the appropriate file extension based on assessment type
        /// - Creates a ZIP archive containing all modified assessments
        /// - Returns the archive as a downloadable file
        /// 
        /// The export process:
        /// - Includes all assessment data, questions, answers, and metadata
        /// - Uses the AssessmentExportManager for consistent export formatting
        /// - Supports both CSET and ACET assessment formats
        /// - Handles large numbers of assessments efficiently
        /// 
        /// Common use cases:
        /// - Automated assessment synchronization
        /// - Data backup and archival
        /// - Assessment migration between systems
        /// - Compliance reporting and auditing
        /// 
        /// Requires valid API key in Authorization header.
        /// The API key must have appropriate permissions for bulk export operations.
        /// </remarks>
        [HttpGet]
        [ApiKeyAuthorize]
        [Route("api/crrm/bulkExportAccessKeyAssessments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public IActionResult BulkExportAccessKeyAssessments()
        {
            // This endpoint exports all access key assessments where the "ModifiedSinceLastExport" column is true.
            try
            {
                // determine extension (.csetw, .acet)
                string ext = IOHelper.GetExportFileExtension("CSET");
                AssessmentExportManager exportManager = new AssessmentExportManager(_context);

                Guid[] guidsToExport = _context.ACCESS_KEY_ASSESSMENT
                    .Include(aca => aca.Assessment)
                    .Where(aca => aca.Assessment.ModifiedSinceLastExport == true)
                    .Select(aca => aca.Assessment.Assessment_GUID).ToArray();

                MemoryStream assessmentsExportArchive = exportManager.BulkExportAssessments(guidsToExport, ext);

                if (assessmentsExportArchive == null) 
                {
                    return StatusCode(204);
                }

                return File(assessmentsExportArchive, "application/zip", "BulkAssessmentExport.zip");
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
                return StatusCode(500, "There was an issue bulk exporting assessments");
            }
        }
    }
}
