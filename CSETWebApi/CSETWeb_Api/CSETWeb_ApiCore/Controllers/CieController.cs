using CSETWebCore.Business.Authorization;
using CSETWebCore.Business.Document;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Document;
using CSETWebCore.Model.Document;
using CSETWebCore.Model.Question;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Model.Map;
using System.Collections.Generic;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for CIE (Critical Infrastructure Evaluation) functionality in CSET.
    /// This controller handles CIE-specific operations including merge conflict resolution
    /// and document management for critical infrastructure assessments. Supports CIE
    /// maturity model operations and assessment data merging workflows.
    /// </summary>
    [CsetAuthorize]
    [ApiController]
    public class CieController : ControllerBase
    {
        public CSETContext _context;
        public IDocumentBusiness _documentBusiness;

        /// <summary>
        /// Initializes a new instance of the CieController.
        /// </summary>
        /// <param name="context">The database context for CIE data access</param>
        /// <param name="documentBusiness">The document business service for document operations</param>
        public CieController(CSETContext context, IDocumentBusiness documentBusiness)
        {
            _context = context;
            _documentBusiness = documentBusiness;
        }

        /// <summary>
        /// Retrieves CIE merge conflict data for multiple assessment IDs.
        /// </summary>
        /// <param name="id1">First assessment ID for merge conflict analysis</param>
        /// <param name="id2">Second assessment ID for merge conflict analysis</param>
        /// <param name="id3">Third assessment ID for merge conflict analysis</param>
        /// <param name="id4">Fourth assessment ID for merge conflict analysis</param>
        /// <param name="id5">Fifth assessment ID for merge conflict analysis</param>
        /// <param name="id6">Sixth assessment ID for merge conflict analysis</param>
        /// <param name="id7">Seventh assessment ID for merge conflict analysis</param>
        /// <param name="id8">Eighth assessment ID for merge conflict analysis</param>
        /// <param name="id9">Ninth assessment ID for merge conflict analysis</param>
        /// <param name="id10">Tenth assessment ID for merge conflict analysis</param>
        /// <returns>
        /// 200 OK with List of Get_Merge_ConflictsResult containing merge conflict data
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves merge conflict data for up to 10 CIE assessments:
        /// - Identifies conflicts between multiple assessments
        /// - Compares answer data across assessments
        /// - Provides conflict resolution information
        /// - Supports CIE assessment merging workflows
        /// 
        /// The merge conflict analysis includes:
        /// - Answer comparison across assessments
        /// - Conflict identification and categorization
        /// - Resolution recommendation data
        /// - Assessment data validation
        /// 
        /// CIE merge features:
        /// - Multi-assessment conflict detection
        /// - Answer comparison and analysis
        /// - Conflict resolution guidance
        /// - Assessment data validation
        /// - Merge workflow support
        /// 
        /// The response includes:
        /// - List of merge conflicts
        /// - Conflict details and descriptions
        /// - Resolution recommendations
        /// - Assessment comparison data
        /// - Conflict categorization
        /// 
        /// Usage scenarios:
        /// - CIE assessment merging
        /// - Conflict resolution workflows
        /// - Multi-assessment analysis
        /// - Data validation and comparison
        /// - Assessment consolidation
        /// 
        /// The merge process supports:
        /// - Critical infrastructure assessments
        /// - CIE maturity model operations
        /// - Assessment data consolidation
        /// - Conflict resolution workflows
        /// - Data integrity validation
        /// 
        /// Assessment IDs should be:
        /// - Valid CIE assessment identifiers
        /// - Assessments accessible to the user
        /// - Compatible for merging operations
        /// - Within the same CIE maturity model
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/getCieMergeData")]
        [ProducesResponseType(typeof(IList<Get_Merge_ConflictsResult>), 200)]
        [ProducesResponseType(401)]
        public IList<Get_Merge_ConflictsResult> GetCieMergeAnswers([FromQuery] int id1, [FromQuery] int id2, [FromQuery] int id3, [FromQuery] int id4, [FromQuery] int id5,
                                                                [FromQuery] int id6, [FromQuery] int id7, [FromQuery] int id8, [FromQuery] int id9, [FromQuery] int id10)
        {
            return _context.Get_Cie_Merge_Conflicts(id1, id2, id3, id4, id5, id6, id7, id8, id9, id10);
        }

        /// <summary>
        /// Saves new documents for CIE assessment merge operations.
        /// </summary>
        /// <param name="documents">The DocumentsForMerge object containing documents to save</param>
        /// <returns>
        /// 200 OK if documents saved successfully for merge
        /// 400 Bad Request if document data is invalid
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// This endpoint saves new documents for CIE assessment merge operations:
        /// - Processes document data for merge operations
        /// - Copies files for merge processing
        /// - Supports CIE assessment document management
        /// - Enables document-based merge workflows
        /// 
        /// The document save process includes:
        /// - Document data validation
        /// - File copying for merge operations
        /// - Document metadata processing
        /// - Merge workflow preparation
        /// 
        /// Document merge features:
        /// - Document data processing
        /// - File copying and management
        /// - Merge workflow support
        /// - Document metadata handling
        /// - Assessment document integration
        /// 
        /// The DocumentsForMerge object contains:
        /// - List of DocumentWithAnswerId objects
        /// - Document metadata and properties
        /// - Answer associations
        /// - Merge configuration data
        /// 
        /// Document processing includes:
        /// - File copying operations
        /// - Metadata extraction
        /// - Answer association mapping
        /// - Merge preparation
        /// - Document validation
        /// 
        /// Usage scenarios:
        /// - CIE assessment document merging
        /// - Document-based assessment consolidation
        /// - File management for merge operations
        /// - Assessment document integration
        /// - Merge workflow preparation
        /// 
        /// The document merge supports:
        /// - Critical infrastructure documentation
        /// - CIE assessment document management
        /// - Multi-assessment document consolidation
        /// - Document-based merge workflows
        /// - Assessment file integration
        /// 
        /// Document types supported:
        /// - Assessment documentation
        /// - Supporting evidence files
        /// - Reference materials
        /// - Assessment artifacts
        /// - Merge-related documents
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpPost]
        [Route("api/saveNewDocumentsForMerge")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public IActionResult SaveNewDocumentsForMerge([FromBody] DocumentsForMerge documents)
        {
            List<DocumentWithAnswerId> documentWithAnswerIds = documents.DocumentWithAnswerId;
            _documentBusiness.CopyFilesForMerge(documentWithAnswerIds);
            return Ok();
        }
    }
}
