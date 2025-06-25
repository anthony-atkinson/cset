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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Model.AssessmentIO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO.Compression;

namespace CSETWebCore.Business.AssessmentIO.Export
{
    /// <summary>
    /// Enhanced export manager providing multiple format support, bulk operations,
    /// and advanced export features for CSET assessments.
    /// </summary>
    public class EnhancedExportManager
    {
        private readonly CSETContext _context;
        private readonly AssessmentExportManager _baseExportManager;

        public EnhancedExportManager(CSETContext context)
        {
            _context = context;
            _baseExportManager = new AssessmentExportManager(context);
        }

        /// <summary>
        /// Exports an assessment in the specified format
        /// </summary>
        /// <param name="assessmentId">Assessment ID to export</param>
        /// <param name="format">Export format (JSON, XML, CSV)</param>
        /// <param name="options">Export options</param>
        /// <returns>Export result with file data</returns>
        public async Task<AssessmentExportFile> ExportAssessmentAsync(int assessmentId, ExportFormat format, ExportOptions options = null)
        {
            options ??= new ExportOptions();

            var exportData = await GetAssessmentDataAsync(assessmentId, options);
            
            switch (format)
            {
                case ExportFormat.JSON:
                    return await ExportToJsonAsync(exportData, options);
                case ExportFormat.XML:
                    return await ExportToXmlAsync(exportData, options);
                case ExportFormat.CSV:
                    return await ExportToCsvAsync(exportData, options);
                default:
                    throw new ArgumentException($"Unsupported export format: {format}");
            }
        }

        /// <summary>
        /// Exports multiple assessments in bulk
        /// </summary>
        /// <param name="assessmentIds">List of assessment IDs to export</param>
        /// <param name="format">Export format</param>
        /// <param name="options">Export options</param>
        /// <returns>ZIP file containing all exports</returns>
        public async Task<AssessmentExportFile> BulkExportAssessmentsAsync(IEnumerable<int> assessmentIds, ExportFormat format, ExportOptions options = null)
        {
            options ??= new ExportOptions();

            using var memoryStream = new MemoryStream();
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create);

            foreach (var assessmentId in assessmentIds)
            {
                try
                {
                    var exportFile = await ExportAssessmentAsync(assessmentId, format, options);
                    var entry = archive.CreateEntry(exportFile.FileName);
                    
                    using var entryStream = entry.Open();
                    await exportFile.FileContents.CopyToAsync(entryStream);
                }
                catch (Exception ex)
                {
                    // Log error but continue with other assessments
                    var errorEntry = archive.CreateEntry($"error_{assessmentId}.txt");
                    using var errorStream = errorEntry.Open();
                    var errorBytes = Encoding.UTF8.GetBytes($"Error exporting assessment {assessmentId}: {ex.Message}");
                    await errorStream.WriteAsync(errorBytes, 0, errorBytes.Length);
                }
            }

            memoryStream.Position = 0;
            var fileName = $"bulk_export_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            
            return new AssessmentExportFile(fileName, memoryStream);
        }

        /// <summary>
        /// Exports assessment template (structure without data)
        /// </summary>
        /// <param name="assessmentId">Assessment ID to use as template</param>
        /// <param name="format">Export format</param>
        /// <param name="options">Export options</param>
        /// <returns>Template export file</returns>
        public async Task<AssessmentExportFile> ExportTemplateAsync(int assessmentId, ExportFormat format, ExportOptions options = null)
        {
            options ??= new ExportOptions();
            options.IncludeData = false; // Template should not include actual data
            options.IncludeStructure = true;

            var templateData = await GetAssessmentTemplateDataAsync(assessmentId, options);
            
            switch (format)
            {
                case ExportFormat.JSON:
                    return await ExportToJsonAsync(templateData, options);
                case ExportFormat.XML:
                    return await ExportToXmlAsync(templateData, options);
                case ExportFormat.CSV:
                    return await ExportToCsvAsync(templateData, options);
                default:
                    throw new ArgumentException($"Unsupported export format: {format}");
            }
        }

        /// <summary>
        /// Merges multiple assessments into a single export
        /// </summary>
        /// <param name="assessmentIds">Assessment IDs to merge</param>
        /// <param name="mergeOptions">Merge configuration</param>
        /// <param name="format">Export format</param>
        /// <param name="options">Export options</param>
        /// <returns>Merged assessment export</returns>
        public async Task<AssessmentExportFile> MergeAssessmentsAsync(IEnumerable<int> assessmentIds, MergeOptions mergeOptions, ExportFormat format, ExportOptions options = null)
        {
            options ??= new ExportOptions();

            var mergedData = await MergeAssessmentDataAsync(assessmentIds, mergeOptions);
            
            switch (format)
            {
                case ExportFormat.JSON:
                    return await ExportToJsonAsync(mergedData, options);
                case ExportFormat.XML:
                    return await ExportToXmlAsync(mergedData, options);
                case ExportFormat.CSV:
                    return await ExportToCsvAsync(mergedData, options);
                default:
                    throw new ArgumentException($"Unsupported export format: {format}");
            }
        }

        /// <summary>
        /// Gets assessment data for export
        /// </summary>
        private async Task<object> GetAssessmentDataAsync(int assessmentId, ExportOptions options)
        {
            var assessment = await _context.ASSESSMENTS.FindAsync(assessmentId);
            if (assessment == null)
                throw new ArgumentException($"Assessment {assessmentId} not found");

            var exportData = new
            {
                Metadata = new
                {
                    AssessmentId = assessmentId,
                    AssessmentName = assessment.Assessment_Name,
                    CreatedDate = assessment.AssessmentCreatedDate,
                    LastModifiedDate = assessment.Assessment_Date,
                    Version = "1.0",
                    ExportDate = DateTime.UtcNow,
                    ExportOptions = options
                },
                Assessment = options.IncludeData ? await GetFullAssessmentDataAsync(assessmentId) : null,
                Structure = options.IncludeStructure ? await GetAssessmentStructureAsync(assessmentId) : null
            };

            return exportData;
        }

        /// <summary>
        /// Gets template data (structure without actual data)
        /// </summary>
        private async Task<object> GetAssessmentTemplateDataAsync(int assessmentId, ExportOptions options)
        {
            var assessment = await _context.ASSESSMENTS.FindAsync(assessmentId);
            if (assessment == null)
                throw new ArgumentException($"Assessment {assessmentId} not found");

            var templateData = new
            {
                Metadata = new
                {
                    TemplateId = assessmentId,
                    TemplateName = $"{assessment.Assessment_Name}_Template",
                    CreatedDate = DateTime.UtcNow,
                    Version = "1.0",
                    Type = "Template"
                },
                Structure = await GetAssessmentStructureAsync(assessmentId),
                Standards = await GetAssessmentStandardsAsync(assessmentId),
                Questions = await GetAssessmentQuestionsAsync(assessmentId)
            };

            return templateData;
        }

        /// <summary>
        /// Merges multiple assessment datasets
        /// </summary>
        private async Task<object> MergeAssessmentDataAsync(IEnumerable<int> assessmentIds, MergeOptions mergeOptions)
        {
            var mergedData = new
            {
                Metadata = new
                {
                    MergedAssessmentIds = assessmentIds.ToList(),
                    MergeDate = DateTime.UtcNow,
                    MergeOptions = mergeOptions,
                    Version = "1.0"
                },
                Assessments = new List<object>()
            };

            foreach (var assessmentId in assessmentIds)
            {
                var assessmentData = await GetAssessmentDataAsync(assessmentId, new ExportOptions { IncludeData = true });
                ((dynamic)mergedData).Assessments.Add(assessmentData);
            }

            return mergedData;
        }

        /// <summary>
        /// Exports data to JSON format
        /// </summary>
        private async Task<AssessmentExportFile> ExportToJsonAsync(object data, ExportOptions options)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = options.PrettyPrint ? Formatting.Indented : Formatting.None,
                NullValueHandling = options.IncludeNulls ? NullValueHandling.Include : NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(data, jsonSettings);
            var bytes = Encoding.UTF8.GetBytes(json);

            if (options.Encrypt)
            {
                bytes = await EncryptDataAsync(bytes, options.EncryptionPassword);
            }

            var fileName = $"assessment_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var stream = new MemoryStream(bytes);
            
            return new AssessmentExportFile(fileName, stream);
        }

        /// <summary>
        /// Exports data to XML format
        /// </summary>
        private async Task<AssessmentExportFile> ExportToXmlAsync(object data, ExportOptions options)
        {
            var xmlSettings = new XmlWriterSettings
            {
                Indent = options.PrettyPrint,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using var memoryStream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(memoryStream, xmlSettings);
            
            var serializer = new XmlSerializer(data.GetType());
            serializer.Serialize(xmlWriter, data);
            
            var bytes = memoryStream.ToArray();

            if (options.Encrypt)
            {
                bytes = await EncryptDataAsync(bytes, options.EncryptionPassword);
            }

            var fileName = $"assessment_export_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
            var stream = new MemoryStream(bytes);
            
            return new AssessmentExportFile(fileName, stream);
        }

        /// <summary>
        /// Exports data to CSV format
        /// </summary>
        private async Task<AssessmentExportFile> ExportToCsvAsync(object data, ExportOptions options)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = options.CsvDelimiter ?? ",",
                HasHeaderRecord = true
            });

            // Flatten the data structure for CSV export
            var flattenedData = FlattenDataForCsv(data);
            await csv.WriteRecordsAsync(flattenedData);
            
            var bytes = memoryStream.ToArray();

            if (options.Encrypt)
            {
                bytes = await EncryptDataAsync(bytes, options.EncryptionPassword);
            }

            var fileName = $"assessment_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var stream = new MemoryStream(bytes);
            
            return new AssessmentExportFile(fileName, stream);
        }

        /// <summary>
        /// Encrypts data with AES encryption
        /// </summary>
        private async Task<byte[]> EncryptDataAsync(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(password, aes.IV, 10000);
            aes.Key = key.GetBytes(32);

            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            
            await cryptoStream.WriteAsync(data, 0, data.Length);
            await cryptoStream.FlushFinalBlockAsync();
            
            var encryptedData = memoryStream.ToArray();
            var result = new byte[aes.IV.Length + encryptedData.Length];
            
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedData, 0, result, aes.IV.Length, encryptedData.Length);
            
            return result;
        }

        /// <summary>
        /// Gets full assessment data including all related entities
        /// </summary>
        private async Task<object> GetFullAssessmentDataAsync(int assessmentId)
        {
            // This would include all assessment data, answers, findings, etc.
            // Implementation would depend on specific data requirements
            return new { AssessmentId = assessmentId, Data = "Full assessment data" };
        }

        /// <summary>
        /// Gets assessment structure (questions, standards, etc.)
        /// </summary>
        private async Task<object> GetAssessmentStructureAsync(int assessmentId)
        {
            // This would include the structure without actual data
            return new { AssessmentId = assessmentId, Structure = "Assessment structure" };
        }

        /// <summary>
        /// Gets assessment standards
        /// </summary>
        private async Task<object> GetAssessmentStandardsAsync(int assessmentId)
        {
            // This would include selected standards and frameworks
            return new { AssessmentId = assessmentId, Standards = "Selected standards" };
        }

        /// <summary>
        /// Gets assessment questions
        /// </summary>
        private async Task<object> GetAssessmentQuestionsAsync(int assessmentId)
        {
            // This would include question structure
            return new { AssessmentId = assessmentId, Questions = "Question structure" };
        }

        /// <summary>
        /// Flattens data structure for CSV export
        /// </summary>
        private IEnumerable<object> FlattenDataForCsv(object data)
        {
            // Implementation to flatten complex objects for CSV export
            return new List<object> { data };
        }
    }

    /// <summary>
    /// Supported export formats
    /// </summary>
    public enum ExportFormat
    {
        JSON,
        XML,
        CSV
    }

    /// <summary>
    /// Export configuration options
    /// </summary>
    public class ExportOptions
    {
        public bool IncludeData { get; set; } = true;
        public bool IncludeStructure { get; set; } = true;
        public bool PrettyPrint { get; set; } = true;
        public bool IncludeNulls { get; set; } = false;
        public bool Encrypt { get; set; } = false;
        public string EncryptionPassword { get; set; }
        public string CsvDelimiter { get; set; } = ",";
        public bool IncludeMetadata { get; set; } = true;
    }

    /// <summary>
    /// Merge configuration options
    /// </summary>
    public class MergeOptions
    {
        public bool MergeAnswers { get; set; } = true;
        public bool MergeFindings { get; set; } = true;
        public bool MergeDocuments { get; set; } = false;
        public ConflictResolutionStrategy ConflictResolution { get; set; } = ConflictResolutionStrategy.KeepLatest;
    }

    /// <summary>
    /// Conflict resolution strategies for merging
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        KeepLatest,
        KeepEarliest,
        KeepMostComplete,
        Manual
    }
} 