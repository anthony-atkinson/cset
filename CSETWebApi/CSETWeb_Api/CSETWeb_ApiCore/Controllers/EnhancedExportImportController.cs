//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CSETWebCore.Business.AssessmentIO.Export;
using CSETWebCore.Business.AssessmentIO.Import;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.Business.Authorization;
using System.Text.Json;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides enhanced endpoints for assessment export and import functionality in CSET.
    /// This controller handles multiple export formats (JSON, XML, CSV), bulk operations,
    /// template management, assessment merging, and advanced import validation.
    /// Supports both standalone and enterprise deployment scenarios.
    /// </summary>
    [ApiController]
    [CsetAuthorize]
    public class EnhancedExportImportController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly EnhancedExportManager _exportManager;
        private readonly EnhancedImportManager _importManager;

        /// <summary>
        /// Initializes a new instance of the EnhancedExportImportController.
        /// </summary>
        /// <param name="tokenManager">Token manager for authentication and authorization</param>
        /// <param name="context">Database context for assessment operations</param>
        public EnhancedExportImportController(ITokenManager tokenManager, CSETContext context)
        {
            _tokenManager = tokenManager;
            _context = context;
            _exportManager = new EnhancedExportManager(context);
            _importManager = new EnhancedImportManager(context);
        }

        /// <summary>
        /// Exports an assessment in the specified format with enhanced options.
        /// </summary>
        /// <param name="format">Export format (json, xml, csv)</param>
        /// <param name="options">Export configuration options</param>
        /// <returns>
        /// 200 OK with file download if successful
        /// 400 Bad Request if format is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// Exports the current assessment based on the user's token scope.
        /// Supports multiple formats with configurable options including encryption,
        /// data inclusion/exclusion, and formatting preferences.
        /// </remarks>
        [HttpGet]
        [Route("api/enhanced/export")]
        public async Task<IActionResult> ExportAssessment(
            [FromQuery] string format = "json",
            [FromQuery] bool includeData = true,
            [FromQuery] bool includeStructure = true,
            [FromQuery] bool prettyPrint = true,
            [FromQuery] bool encrypt = false,
            [FromQuery] string password = null)
        {
            try
            {
                var assessmentId = _tokenManager.AssessmentForUser();
                var exportFormat = ParseExportFormat(format);
                
                var options = new ExportOptions
                {
                    IncludeData = includeData,
                    IncludeStructure = includeStructure,
                    PrettyPrint = prettyPrint,
                    Encrypt = encrypt,
                    EncryptionPassword = password
                };

                var result = await _exportManager.ExportAssessmentAsync(assessmentId, exportFormat, options);
                
                var contentType = GetContentType(exportFormat);
                return File(result.FileContents, contentType, result.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports multiple assessments in bulk with the specified format.
        /// </summary>
        /// <param name="request">Bulk export request containing assessment IDs and options</param>
        /// <returns>
        /// 200 OK with ZIP file download if successful
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// Exports multiple assessments in a single ZIP file for efficient bulk operations.
        /// Each assessment is exported in the specified format with the given options.
        /// </remarks>
        [HttpPost]
        [Route("api/enhanced/export/bulk")]
        public async Task<IActionResult> BulkExportAssessments([FromBody] BulkExportRequest request)
        {
            try
            {
                if (request.AssessmentIds == null || !request.AssessmentIds.Any())
                {
                    return BadRequest("At least one assessment ID must be specified");
                }

                var exportFormat = ParseExportFormat(request.Format);
                var options = new ExportOptions
                {
                    IncludeData = request.IncludeData,
                    IncludeStructure = request.IncludeStructure,
                    PrettyPrint = request.PrettyPrint,
                    Encrypt = request.Encrypt,
                    EncryptionPassword = request.Password
                };

                var result = await _exportManager.BulkExportAssessmentsAsync(request.AssessmentIds, exportFormat, options);
                
                return File(result.FileContents, "application/zip", result.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Bulk export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports an assessment template (structure without data).
        /// </summary>
        /// <param name="format">Export format (json, xml, csv)</param>
        /// <param name="options">Export configuration options</param>
        /// <returns>
        /// 200 OK with template file download if successful
        /// 400 Bad Request if format is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if export fails
        /// </returns>
        /// <remarks>
        /// Exports the assessment structure as a template that can be used to create new assessments.
        /// Templates include questions, standards, and structure but exclude actual assessment data.
        /// </remarks>
        [HttpGet]
        [Route("api/enhanced/export/template")]
        public async Task<IActionResult> ExportTemplate(
            [FromQuery] string format = "json",
            [FromQuery] bool prettyPrint = true,
            [FromQuery] bool encrypt = false,
            [FromQuery] string password = null)
        {
            try
            {
                var assessmentId = _tokenManager.AssessmentForUser();
                var exportFormat = ParseExportFormat(format);
                
                var options = new ExportOptions
                {
                    IncludeData = false, // Templates should not include data
                    IncludeStructure = true,
                    PrettyPrint = prettyPrint,
                    Encrypt = encrypt,
                    EncryptionPassword = password
                };

                var result = await _exportManager.ExportTemplateAsync(assessmentId, exportFormat, options);
                
                var contentType = GetContentType(exportFormat);
                return File(result.FileContents, contentType, result.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Template export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Merges multiple assessments into a single export.
        /// </summary>
        /// <param name="request">Merge request containing assessment IDs and merge options</param>
        /// <returns>
        /// 200 OK with merged file download if successful
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if merge fails
        /// </returns>
        /// <remarks>
        /// Merges multiple assessments into a single export file with configurable merge options.
        /// Supports different conflict resolution strategies and selective data merging.
        /// </remarks>
        [HttpPost]
        [Route("api/enhanced/export/merge")]
        public async Task<IActionResult> MergeAssessments([FromBody] MergeExportRequest request)
        {
            try
            {
                if (request.AssessmentIds == null || request.AssessmentIds.Count < 2)
                {
                    return BadRequest("At least two assessment IDs must be specified for merging");
                }

                var exportFormat = ParseExportFormat(request.Format);
                var mergeOptions = new MergeOptions
                {
                    MergeAnswers = request.MergeAnswers,
                    MergeFindings = request.MergeFindings,
                    MergeDocuments = request.MergeDocuments,
                    ConflictResolution = request.ConflictResolution
                };

                var options = new ExportOptions
                {
                    IncludeData = true,
                    IncludeStructure = true,
                    PrettyPrint = request.PrettyPrint,
                    Encrypt = request.Encrypt,
                    EncryptionPassword = request.Password
                };

                var result = await _exportManager.MergeAssessmentsAsync(request.AssessmentIds, mergeOptions, exportFormat, options);
                
                var contentType = GetContentType(exportFormat);
                return File(result.FileContents, contentType, result.FileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Merge failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Imports an assessment from the specified format with enhanced validation.
        /// </summary>
        /// <param name="format">Import format (json, xml, csv)</param>
        /// <param name="options">Import configuration options</param>
        /// <returns>
        /// 200 OK with import result if successful
        /// 400 Bad Request if format is invalid or validation fails
        /// 401 Unauthorized if user is not authenticated
        /// 415 Unsupported Media Type if content type is not multipart
        /// 500 Internal Server Error if import fails
        /// </returns>
        /// <remarks>
        /// Imports an assessment from the specified format with comprehensive validation.
        /// Supports encrypted files, template imports, and merge operations.
        /// </remarks>
        [HttpPost]
        [Route("api/enhanced/import")]
        public async Task<IActionResult> ImportAssessment(
            [FromQuery] string format = "json",
            [FromQuery] bool isTemplate = false,
            [FromQuery] bool isMerge = false,
            [FromQuery] int? targetAssessmentId = null,
            [FromQuery] bool isEncrypted = false,
            [FromQuery] string password = null,
            [FromQuery] string csvDelimiter = ",",
            [FromQuery] bool validateOnly = false)
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var file = Request.Form.Files[0];
                var importFormat = ParseImportFormat(format);
                
                var options = new ImportOptions
                {
                    IsTemplate = isTemplate,
                    IsMerge = isMerge,
                    TargetAssessmentId = targetAssessmentId,
                    IsEncrypted = isEncrypted,
                    DecryptionPassword = password,
                    CsvDelimiter = csvDelimiter,
                    ValidateOnly = validateOnly
                };

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                var result = await _importManager.ImportAssessmentAsync(fileData, importFormat, options);
                
                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Errors = result.Errors,
                        Warnings = result.Warnings
                    });
                }

                return Ok(new
                {
                    Success = true,
                    ImportedAssessmentId = result.ImportedAssessmentId,
                    Messages = result.Messages,
                    Warnings = result.Warnings
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates an import file without actually importing it.
        /// </summary>
        /// <param name="format">Import format (json, xml, csv)</param>
        /// <param name="options">Validation options</param>
        /// <returns>
        /// 200 OK with validation result
        /// 400 Bad Request if format is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 415 Unsupported Media Type if content type is not multipart
        /// </returns>
        /// <remarks>
        /// Performs comprehensive validation on an import file without importing it.
        /// Useful for checking file format, data structure, and business rules before import.
        /// </remarks>
        [HttpPost]
        [Route("api/enhanced/import/validate")]
        public async Task<IActionResult> ValidateImport(
            [FromQuery] string format = "json",
            [FromQuery] bool isEncrypted = false,
            [FromQuery] string password = null,
            [FromQuery] string csvDelimiter = ",")
        {
            try
            {
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No file uploaded");
                }

                var file = Request.Form.Files[0];
                var importFormat = ParseImportFormat(format);
                
                var options = new ImportOptions
                {
                    IsEncrypted = isEncrypted,
                    DecryptionPassword = password,
                    CsvDelimiter = csvDelimiter,
                    ValidateOnly = true
                };

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileData = memoryStream.ToArray();

                var result = await _importManager.ImportAssessmentAsync(fileData, importFormat, options);
                
                return Ok(new
                {
                    IsValid = result.Success,
                    Errors = result.Errors,
                    Warnings = result.Warnings,
                    Messages = result.Messages
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets available export formats and their capabilities.
        /// </summary>
        /// <returns>
        /// 200 OK with format information
        /// 401 Unauthorized if user is not authenticated
        /// </returns>
        /// <remarks>
        /// Returns information about supported export formats and their features.
        /// Useful for client applications to determine available options.
        /// </remarks>
        [HttpGet]
        [Route("api/enhanced/formats")]
        public IActionResult GetSupportedFormats()
        {
            var formats = new[]
            {
                new
                {
                    Format = "json",
                    Name = "JSON",
                    Description = "JavaScript Object Notation format",
                    SupportsEncryption = true,
                    SupportsPrettyPrint = true,
                    SupportsTemplates = true,
                    SupportsMerging = true
                },
                new
                {
                    Format = "xml",
                    Name = "XML",
                    Description = "Extensible Markup Language format",
                    SupportsEncryption = true,
                    SupportsPrettyPrint = true,
                    SupportsTemplates = true,
                    SupportsMerging = true
                },
                new
                {
                    Format = "csv",
                    Name = "CSV",
                    Description = "Comma-Separated Values format",
                    SupportsEncryption = true,
                    SupportsPrettyPrint = false,
                    SupportsTemplates = false,
                    SupportsMerging = false
                }
            };

            return Ok(formats);
        }

        /// <summary>
        /// Parses export format string to enum
        /// </summary>
        private ExportFormat ParseExportFormat(string format)
        {
            return format?.ToLower() switch
            {
                "json" => ExportFormat.JSON,
                "xml" => ExportFormat.XML,
                "csv" => ExportFormat.CSV,
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };
        }

        /// <summary>
        /// Parses import format string to enum
        /// </summary>
        private ImportFormat ParseImportFormat(string format)
        {
            return format?.ToLower() switch
            {
                "json" => ImportFormat.JSON,
                "xml" => ImportFormat.XML,
                "csv" => ImportFormat.CSV,
                _ => throw new ArgumentException($"Unsupported import format: {format}")
            };
        }

        /// <summary>
        /// Gets content type for export format
        /// </summary>
        private string GetContentType(ExportFormat format)
        {
            return format switch
            {
                ExportFormat.JSON => "application/json",
                ExportFormat.XML => "application/xml",
                ExportFormat.CSV => "text/csv",
                _ => "application/octet-stream"
            };
        }
    }

    /// <summary>
    /// Request model for bulk export operations
    /// </summary>
    public class BulkExportRequest
    {
        public List<int> AssessmentIds { get; set; }
        public string Format { get; set; } = "json";
        public bool IncludeData { get; set; } = true;
        public bool IncludeStructure { get; set; } = true;
        public bool PrettyPrint { get; set; } = true;
        public bool Encrypt { get; set; } = false;
        public string Password { get; set; }
    }

    /// <summary>
    /// Request model for merge export operations
    /// </summary>
    public class MergeExportRequest
    {
        public List<int> AssessmentIds { get; set; }
        public string Format { get; set; } = "json";
        public bool MergeAnswers { get; set; } = true;
        public bool MergeFindings { get; set; } = true;
        public bool MergeDocuments { get; set; } = false;
        public ConflictResolutionStrategy ConflictResolution { get; set; } = ConflictResolutionStrategy.KeepLatest;
        public bool PrettyPrint { get; set; } = true;
        public bool Encrypt { get; set; } = false;
        public string Password { get; set; }
    }
} 