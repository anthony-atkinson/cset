//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.AssessmentIO.Import;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using CSETWebCore.Business.Authorization;
using ICSharpCode.SharpZipLib.Zip;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for assessment import functionality in CSET.
    /// This controller handles the import of assessments from various sources including
    /// encrypted files, legacy formats, and specialized spreadsheets like AWWA.
    /// Requires authentication and authorization via CsetAuthorize attribute.
    /// </summary>
    [CsetAuthorize]
    public class AssessmentImportController : ControllerBase
    {
        private ITokenManager _tokenManager;
        private CSETContext _context;
        private IImportManager _importManager;

        /// <summary>
        /// Initializes a new instance of the AssessmentImportController.
        /// </summary>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for assessment operations</param>
        /// <param name="importManager">Import manager for processing assessment imports</param>
        public AssessmentImportController(ITokenManager token, CSETContext context, IImportManager importManager)
        {
            _tokenManager = token;
            _context = context;
            _importManager = importManager;
        }

        /// <summary>
        /// Checks if legacy import functionality is installed.
        /// </summary>
        /// <returns>
        /// 200 OK with false (legacy import is not supported in web version)
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns false to indicate that legacy import functionality is not available
        /// in the web version of CSET. Legacy imports must be performed using the desktop version.
        /// </remarks>
        [HttpGet]
        [Route("api/assessment/legacy/import/installed")]
        public IActionResult LegacyImportIsInstalled()
        {
            return Ok(false);
        }

        /// <summary>
        /// Imports a legacy assessment file.
        /// </summary>
        /// <returns>
        /// 200 OK with true if import successful
        /// 400 Bad Request if multiple files uploaded
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if import processing fails
        /// </returns>
        /// <remarks>
        /// Processes legacy assessment files for import into the system.
        /// Only allows a single assessment file to be imported at a time.
        /// The file is processed using the import manager with user authentication.
        /// This endpoint handles older assessment formats that may not be compatible with current standards.
        /// </remarks>
        [HttpPost]
        [Route("api/assessment/legacy/import")]
        public async Task<IActionResult> ImportLegacyAssessment()
        {
            // For now only allowing 1 uploaded file
            if (Request.Form.Files.Count > 1)
            {
                return BadRequest("Only a single assessment may be imported at a time.");
            }

            var assessmentFile = Request.Form.Files[0];

            var target = new MemoryStream();
            assessmentFile.CopyTo(target);

            try
            {
                await _importManager.ProcessCSETAssessmentImport(target.ToArray(), _tokenManager.GetUserId(), _tokenManager.GetAccessKey(), _context);
            }
            catch (Exception)
            {
                return StatusCode(500, "There was an error processing the uploaded assessment.");
            }

            return Ok(true);
        }

        /// <summary>
        /// Imports a modern assessment file with optional password protection.
        /// </summary>
        /// <param name="pwd">Optional password for encrypted assessment files</param>
        /// <returns>
        /// 200 OK with success message if import successful
        /// 401 Unauthorized if user is not authenticated
        /// 406 Not Acceptable if password is invalid
        /// 415 Unsupported Media Type if content type is not multipart
        /// 423 Locked if password is required but not provided
        /// 404 Not Found if custom module is missing
        /// 500 Internal Server Error if import processing fails
        /// </returns>
        /// <remarks>
        /// Imports modern assessment files (.csetw format) with support for password protection.
        /// Validates file format and extracts password hints from encrypted files.
        /// Supports overwrite functionality via x-cset-overwrite header.
        /// Handles various error conditions including password validation and missing dependencies.
        /// Legacy .cset files are rejected and must be imported via desktop version.
        /// </remarks>
        [HttpPost]
        [Route("api/assessment/import")]
        public async Task<IActionResult> ImportAssessment([FromHeader] string pwd)
        {
            // should be multipart
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                // unsupported media type
                return StatusCode(415);
            }
            var assessmentFile = Request.Form.Files[0];

            var currentUserId = _tokenManager.GetCurrentUserId();
            var accessKey = _tokenManager.GetAccessKey();

            ZipEntry hint = null;
            try
            {
                var formFiles = HttpContext.Request.Form.Files;

                foreach (FormFile file in formFiles)
                {
                    if (file.FileName.EndsWith(".cset"))
                    {
                        return Ok("Legacy 8.1 or earlier .cset files must be imported from the desktop version");
                    }

                    var target = new MemoryStream();
                    file.CopyTo(target);
                    var bytes = target.ToArray();

                    // Get the password hint, if there is one.
                    using (Stream fs = new MemoryStream(bytes))
                    {
                        MemoryStream ms = new MemoryStream();

                        var zip = new ZipFile(fs);

                        foreach (ZipEntry entry in zip)
                        {
                            if (entry.Name.Contains(".hint"))
                            {
                                hint = entry;
                            }
                        }
                    }

                    // overwrite the assessment if instructed in the header
                    bool.TryParse(Request.Headers["x-cset-overwrite"], out bool overwrite);

                    await _importManager.ProcessCSETAssessmentImport(bytes, currentUserId, accessKey, _context, pwd, overwrite);
                }
            }
            catch (Exception e)
            {
                var returnMessage = "";

                if (e.Message == "No password available for encrypted stream")
                {
                    returnMessage = (hint == null) ? "Bad Password Exception" : "Bad Password Exception - " + hint.Name;
                    return StatusCode(423, returnMessage);
                }
                else if (e.Message == "The password did not match.")
                {
                    returnMessage = (hint == null) ? "Invalid Password" : "Invalid Password - " + hint.Name;
                    return StatusCode(406, returnMessage);
                }
                else if (e.Message == "Custom module not found")
                {
                    returnMessage = "Custom module not found";
                    return StatusCode(404, returnMessage);
                }
                else
                {
                    return BadRequest(e);
                }
            }

            var response = new
            {
                Message = "Assessment was successfully imported"
            };

            return Ok(response);
        }

        /// <summary>
        /// Imports AWWA (American Water Works Association) spreadsheet data.
        /// </summary>
        /// <returns>
        /// 200 OK with success message if import successful
        /// 401 Unauthorized if user is not authenticated
        /// 415 Unsupported Media Type if content type is not multipart
        /// 500 Internal Server Error if import processing fails
        /// </returns>
        /// <remarks>
        /// Imports Excel spreadsheets (.xlsx or .xls) containing AWWA assessment data.
        /// Validates file format and processes the spreadsheet using specialized AWWA import manager.
        /// Only Microsoft Excel spreadsheets are supported for this import type.
        /// The imported data is associated with the current assessment ID from the user's token.
        /// </remarks>
        [HttpPost]
        [Route("api/import/AWWA")]
        public IActionResult ImportAwwaSpreadsheet()
        {
            var multipartBoundary = HttpRequestMultipartExtensions.GetMultipartBoundary(Request);

            if (multipartBoundary == null)
            {
                // unsupported media type
                return StatusCode(415);
            }

            var assessmentId = int.Parse(_tokenManager.Payload(Constants.Constants.Token_AssessmentId));

            try
            {
                var formFiles = HttpContext.Request.Form.Files;

                foreach (FormFile file in formFiles)
                {
                    if (!file.FileName.EndsWith(".xlsx")
                            && file.FileName.EndsWith(".xls"))
                    {
                        return Ok(new
                        {
                            Message = "Only Microsoft Excel spreadsheets can be uploaded."
                        });
                    }

                    var target = new MemoryStream();
                    file.CopyTo(target);
                    var bytes = target.ToArray();

                    var manager = new ImportManagerAwwa(_context);
                    var importState = manager.ProcessSpreadsheetImport(bytes, assessmentId);
                    if (importState != null)
                    {
                        return StatusCode(500, importState);
                    }
                }
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }

            var response = new
            {
                Message = "Spreadsheet was successfully imported"
            };

            return Ok(response);
        }
    }
}
