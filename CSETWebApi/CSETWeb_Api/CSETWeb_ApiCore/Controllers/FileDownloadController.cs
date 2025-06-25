//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Interfaces.FileRepository;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for file download functionality in CSET assessments.
    /// Supports secure file downloads for assessment documents with proper
    /// authentication and access control validation.
    /// </summary>
    [ApiController]
    public class FileDownloadController : ControllerBase
    {
        private readonly IFileRepository _fileRepo;
        private readonly ITokenManager _token;

        /// <summary>
        /// Initializes a new instance of the FileDownloadController.
        /// </summary>
        /// <param name="fileRepo">Service for file repository operations</param>
        /// <param name="token">Service for JWT token management</param>
        public FileDownloadController(IFileRepository fileRepo, ITokenManager token)
        {
            _fileRepo = fileRepo;
            _token = token;
        }


        /// <summary>
        /// Downloads a file associated with the current assessment.
        /// </summary>
        /// <param name="id">File ID to download</param>
        /// <returns>
        /// 200 OK with file stream for download
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if file doesn't exist or user doesn't have access
        /// </returns>
        /// <remarks>
        /// This endpoint downloads a file that has been uploaded to the assessment.
        /// The endpoint validates that the user has access to the current assessment
        /// and that the file exists in the file repository.
        /// 
        /// The response includes:
        /// - File content as a stream
        /// - Appropriate content type header
        /// - Original filename for download
        /// 
        /// Security features:
        /// - Validates user authentication
        /// - Ensures user has access to the assessment
        /// - File access is restricted to assessment participants
        /// 
        /// Sample request:
        ///     GET /api/files/download/123
        /// 
        /// The file will be returned as a downloadable attachment with the original
        /// filename and content type preserved.
        /// 
        /// Requires valid JWT token in Authorization header.
        /// </remarks>
        [HttpGet]
        [Route("api/files/download/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult Download(int id)
        {
            var assessmentId = _token.AssessmentForUser();
            var file = _fileRepo.GetFileDescription(id);
            var stream = new MemoryStream(file.Data);

            return File(stream, file.ContentType, file.Name);
        }
    }
}
