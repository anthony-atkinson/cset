using CsvHelper;
using System.Globalization;
using System.Text;
using System.IO.Compression;
using System.Security.Cryptography;

namespace CSETWebBlazor.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;
        private readonly string _logPath;

        public FileService(ILogger<FileService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "file-access.log");
            
            // Ensure directories exist
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
            
            if (!Directory.Exists(Path.GetDirectoryName(_logPath)!))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            }
        }

        public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var validation = await ValidateFileAsync(fileStream, fileName, contentType);
                if (!validation.IsValid)
                {
                    await LogFileAccess("UPLOAD_REJECTED", fileName, string.Join(", ", validation.Errors));
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = string.Join(", ", validation.Errors)
                    };
                }

                // Security check - simulate virus scanning
                if (!await PerformSecurityScan(fileStream, fileName))
                {
                    await LogFileAccess("SECURITY_REJECTED", fileName, "File failed security scan");
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = "File failed security scan"
                    };
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{SanitizeFileName(fileName)}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                // Compress large files
                if (fileStream.Length > 1024 * 1024) // 1MB
                {
                    filePath += ".gz";
                    using var compressedStream = new GZipStream(new FileStream(filePath, FileMode.Create), CompressionMode.Compress);
                    await fileStream.CopyToAsync(compressedStream);
                }
                else
                {
                    using var fileStream2 = new FileStream(filePath, FileMode.Create);
                    await fileStream.CopyToAsync(fileStream2);
                }

                await LogFileAccess("UPLOAD_SUCCESS", fileName, $"File uploaded to {filePath}");

                return new FileUploadResult
                {
                    Success = true,
                    FilePath = filePath,
                    FileName = fileName,
                    FileSize = fileStream.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file {FileName}", fileName);
                await LogFileAccess("UPLOAD_ERROR", fileName, ex.Message);
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<FileDownloadResult> DownloadFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    await LogFileAccess("DOWNLOAD_NOT_FOUND", Path.GetFileName(filePath), "File not found");
                    return new FileDownloadResult
                    {
                        Success = false,
                        ErrorMessage = "File not found"
                    };
                }

                byte[] fileContent;
                var fileName = Path.GetFileName(filePath);

                // Handle compressed files
                if (filePath.EndsWith(".gz"))
                {
                    using var compressedStream = new GZipStream(new FileStream(filePath, FileMode.Open), CompressionMode.Decompress);
                    using var memoryStream = new MemoryStream();
                    await compressedStream.CopyToAsync(memoryStream);
                    fileContent = memoryStream.ToArray();
                    fileName = fileName.Replace(".gz", "");
                }
                else
                {
                    fileContent = await File.ReadAllBytesAsync(filePath);
                }

                var contentType = GetContentType(fileName);

                await LogFileAccess("DOWNLOAD_SUCCESS", fileName, $"File downloaded from {filePath}");

                return new FileDownloadResult
                {
                    Success = true,
                    FileContent = fileContent,
                    FileName = fileName,
                    ContentType = contentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file {FilePath}", filePath);
                await LogFileAccess("DOWNLOAD_ERROR", Path.GetFileName(filePath), ex.Message);
                return new FileDownloadResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string fileName)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                await csv.WriteRecordsAsync(data);
                await writer.FlushAsync();

                await LogFileAccess("EXPORT_CSV", fileName, $"Exported {data.Count()} records");
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to CSV");
                throw;
            }
        }

        public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string fileName)
        {
            try
            {
                // For Excel export, we'll use a simple CSV format that Excel can open
                // In a production environment, you might want to use a library like EPPlus or ClosedXML
                var csvData = await ExportToCsvAsync(data, fileName);
                await LogFileAccess("EXPORT_EXCEL", fileName, $"Exported {data.Count()} records");
                return csvData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to Excel");
                throw;
            }
        }

        public async Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var result = new FileValidationResult
            {
                FileSize = fileStream.Length,
                ContentType = contentType
            };

            // Check file size
            var maxSize = _configuration.GetValue<long>("CSET:MaxFileUploadSize", 10 * 1024 * 1024); // 10MB default
            if (fileStream.Length > maxSize)
            {
                result.IsValid = false;
                result.Errors.Add($"File size exceeds maximum allowed size of {maxSize / (1024 * 1024)}MB");
            }

            // Check file extension
            var allowedExtensions = _configuration.GetSection("CSET:AllowedFileTypes").Get<string[]>() ?? 
                new[] { ".json", ".xml", ".csv", ".xlsx", ".pdf" };
            
            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                result.IsValid = false;
                result.Errors.Add($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
            }

            // Check content type
            var allowedContentTypes = new[]
            {
                "application/json",
                "text/xml",
                "application/xml",
                "text/csv",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/pdf",
                "application/octet-stream"
            };

            if (!allowedContentTypes.Contains(contentType.ToLowerInvariant()))
            {
                result.IsValid = false;
                result.Warnings.Add($"Content type {contentType} may not be supported");
            }

            // Check for potentially malicious file names
            if (IsPotentiallyMalicious(fileName))
            {
                result.IsValid = false;
                result.Errors.Add("File name contains potentially malicious characters");
            }

            if (result.Errors.Count == 0)
            {
                result.IsValid = true;
            }

            return await Task.FromResult(result);
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    await LogFileAccess("DELETE_SUCCESS", Path.GetFileName(filePath), $"File deleted from {filePath}");
                    return await Task.FromResult(true);
                }
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
                await LogFileAccess("DELETE_ERROR", Path.GetFileName(filePath), ex.Message);
                return await Task.FromResult(false);
            }
        }

        public async Task<FileInfo> GetFileInfoAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return await Task.FromResult(new FileInfo());
                }

                var fileInfo = new System.IO.FileInfo(filePath);
                return await Task.FromResult(new FileInfo
                {
                    Name = fileInfo.Name,
                    Path = fileInfo.FullName,
                    Size = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    ContentType = GetContentType(fileInfo.Name)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info for {FilePath}", filePath);
                return await Task.FromResult(new FileInfo());
            }
        }

        // Clean up old files based on retention policy
        public async Task CleanupOldFilesAsync()
        {
            try
            {
                var retentionDays = _configuration.GetValue<int>("CSET:FileRetentionDays", 30);
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var deletedCount = 0;

                foreach (var file in Directory.GetFiles(_uploadPath))
                {
                    var fileInfo = new System.IO.FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        deletedCount++;
                        await LogFileAccess("CLEANUP_DELETED", fileInfo.Name, $"File cleaned up after {retentionDays} days");
                    }
                }

                _logger.LogInformation("Cleaned up {DeletedCount} old files", deletedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file cleanup");
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private string SanitizeFileName(string fileName)
        {
            // Remove potentially dangerous characters
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            foreach (var c in invalidChars)
            {
                sanitized = sanitized.Replace(c, '_');
            }
            return sanitized;
        }

        private bool IsPotentiallyMalicious(string fileName)
        {
            var suspiciousPatterns = new[]
            {
                "..", "\\", "/", ":", "*", "?", "\"", "<", ">", "|",
                "cmd", "bat", "exe", "com", "scr", "pif", "vbs", "js"
            };

            var lowerFileName = fileName.ToLowerInvariant();
            return suspiciousPatterns.Any(pattern => lowerFileName.Contains(pattern));
        }

        private async Task<bool> PerformSecurityScan(Stream fileStream, string fileName)
        {
            try
            {
                // Simulate virus scanning - in production, integrate with actual antivirus service
                var fileHash = await CalculateFileHash(fileStream);
                
                // Check against known malicious hashes (simplified)
                var maliciousHashes = _configuration.GetSection("CSET:MaliciousFileHashes").Get<string[]>() ?? Array.Empty<string>();
                if (maliciousHashes.Contains(fileHash))
                {
                    _logger.LogWarning("File {FileName} matches known malicious hash", fileName);
                    return false;
                }

                // Additional security checks could be added here
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during security scan for {FileName}", fileName);
                return false; // Fail safe - reject if scan fails
            }
        }

        private async Task<string> CalculateFileHash(Stream fileStream)
        {
            using var sha256 = SHA256.Create();
            var hash = await sha256.ComputeHashAsync(fileStream);
            return Convert.ToBase64String(hash);
        }

        private async Task LogFileAccess(string action, string fileName, string details)
        {
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action} | {fileName} | {details}";
                await File.AppendAllTextAsync(_logPath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging file access");
            }
        }
    }
} 