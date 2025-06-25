//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.Business.AssessmentIO.Export;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Helpers;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Model.AssessmentIO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides endpoints for assessment export functionality in CSET.
    /// This controller handles the export of assessments in various formats including
    /// encrypted files, JSON format, and enterprise integration exports.
    /// Supports both standalone and enterprise deployment scenarios.
    /// </summary>
    public class AssessmentExportController : ControllerBase
    {
        private ITokenManager _token;
        private CSETContext _context;
        private IHttpContextAccessor _http;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the AssessmentExportController.
        /// </summary>
        /// <param name="token">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for assessment operations</param>
        /// <param name="http">HTTP context accessor for request information</param>
        /// <param name="configuration">Configuration service for application settings</param>
        public AssessmentExportController(ITokenManager token, CSETContext context,
            IHttpContextAccessor http, IConfiguration configuration)
        {
            _token = token;
            _context = context;
            _http = http;
            _configuration = configuration;
        }

        /// <summary>
        /// Exports an assessment as a downloadable file.
        /// </summary>
        /// <param name="password">Optional password for encrypting the exported file</param>
        /// <param name="passwordHint">Optional hint for the password</param>
        /// <returns>
        /// 200 OK with file download if successful
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// Exports the current assessment based on the user's token scope.
        /// The file format is determined by the token scope (.csetw for web, .acet for enterprise).
        /// If a password is provided, the exported file will be encrypted.
        /// The file is returned as a downloadable attachment with appropriate MIME type.
        /// </remarks>
        [HttpGet]
        [Route("api/assessment/export")]
        public IActionResult ExportAssessment([FromQuery] string password = "", [FromQuery] string passwordHint = "")
        {
            try
            {
                int assessmentId = _token.AssessmentForUser();

                // determine extension (.csetw, .acet)
                string ext = IOHelper.GetExportFileExtension(_token.Payload(Constants.Constants.Token_Scope));

                AssessmentExportFile result = new AssessmentExportManager(_context).ExportAssessment(assessmentId, ext, password, passwordHint);

                return File(result.FileContents, "application/octet-stream", result.FileName);
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return Ok();
        }

        /// <summary>
        /// Exports an assessment and sends it to an enterprise system using enterprise token.
        /// </summary>
        /// <returns>
        /// 200 OK if export and send successful
        /// 401 Unauthorized if enterprise token not provided
        /// 400 Bad Request if target URL not configured or send fails
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// Exports the assessment and automatically sends it to a configured enterprise system.
        /// Requires an enterprise token in the RemoteAuthorization header.
        /// The target URL is configured in the application settings (AssessmentUploadUrl).
        /// The file is sent as a multipart form data to the target API endpoint.
        /// This endpoint is used for enterprise integration scenarios.
        /// </remarks>
        [HttpGet]
        [Route("api/assessment/exportandsend")]
        public async Task<IActionResult> ExportAndSendAssessment()
        {
            try
            {
                var token = Request.Headers["RemoteAuthorization"].FirstOrDefault();
                if (token != null)
                {
                    token = token.Replace("Bearer ", "");
                    _token.SetEnterpriseToken(token);
                }
                else
                {
                    return Unauthorized();
                }

                var assessmentId = _token.AssessmentForUser(token);

                string url = _configuration["AssessmentUploadUrl"];
                // Export the assessment
                if (!string.IsNullOrEmpty(url))
                {
                    var exportManager = new AssessmentExportManager(_context);
                    var exportFile = exportManager.ExportAssessment(assessmentId, ".zip", string.Empty, string.Empty);

                    string ext = IOHelper.GetExportFileExtension(_token.Payload(Constants.Constants.Token_Scope));

                    AssessmentExportFile result =
                        new AssessmentExportManager(_context).ExportAssessment(assessmentId, ext, string.Empty,
                            string.Empty);
                    byte[] fileContents;
                    using (var memoryStream = new MemoryStream())
                    {
                        result.FileContents.CopyTo(memoryStream);
                        fileContents = memoryStream.ToArray();
                    }

                    bool isSuccess = await SendFileToApi($"{url}/api/assessment/import", fileContents, result.FileName);
                    if (isSuccess)
                    {
                        return Ok();
                    }
                }

                return BadRequest("There was an error sending the assessment to the target URL");
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
                return StatusCode(500, exc.Message);
            }
        }

        /// <summary>
        /// Exports an assessment as JSON format for CISA assessor sharing.
        /// </summary>
        /// <param name="scrubData">Whether to scrub sensitive data from the export</param>
        /// <returns>
        /// 200 OK with JSON file download if successful
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// A special export format created for sharing assessment data by CISA assessors.
        /// Only the JSON content is returned, with a filename formatted as {assessment-name}.json.
        /// The scrubData parameter allows removal of sensitive information for sharing purposes.
        /// This format is useful for data analysis and integration with other systems.
        /// </remarks>
        [HttpGet]
        [Route("api/assessment/export/json")]
        public IActionResult ExportAssessmentAsJson([FromQuery] bool? scrubData)
        {
            try
            {
                int assessmentId = _token.AssessmentForUser();

                AssessmentExportFileJson result = new AssessmentExportManager(_context).ExportAssessmentJson(assessmentId, scrubData ?? false);
                byte[] contents = Encoding.UTF8.GetBytes(result.JSON);
                return  File(contents, "application/json", result.FileName);
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return null;
        }

        /// <summary>
        /// Sends a file to an external API endpoint.
        /// </summary>
        /// <param name="targetUrl">The URL of the target API endpoint</param>
        /// <param name="fileContents">The file contents as byte array</param>
        /// <param name="fileName">The name of the file to send</param>
        /// <returns>True if the file was sent successfully, false otherwise</returns>
        /// <remarks>
        /// Private helper method that sends assessment files to external APIs.
        /// Uses HTTP client with multipart form data to upload the file.
        /// Includes enterprise token authentication and overwrite headers.
        /// Handles exceptions and logs errors for debugging.
        /// </remarks>
        private async Task<bool> SendFileToApi(string targetUrl, byte[] fileContents, string fileName)
        {
            try
            {
                using (var client = new HttpClient())
                using (var content = new MultipartFormDataContent())
                using (var byteContent = new ByteArrayContent(fileContents))
                {
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _token.GetEnterpriseToken());

                    // Tell the API to overwrite the assessment
                    client.DefaultRequestHeaders.Add("x-cset-overwrite", "true");

                    byteContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                    content.Add(byteContent, "file", "assessment.csetw");
                    var response = await client.PostAsync(targetUrl, content);
                    return response.IsSuccessStatusCode;

                };
            }
            catch (Exception exc)
            {
                NLog.LogManager.GetCurrentClassLogger().Error($"... {exc}");
            }

            return false;
        }
    }
}
