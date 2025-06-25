namespace CSETWebBlazor.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads a file to the server
        /// </summary>
        Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string contentType);
        
        /// <summary>
        /// Downloads a file from the server
        /// </summary>
        Task<FileDownloadResult> DownloadFileAsync(string filePath);
        
        /// <summary>
        /// Exports data to CSV format
        /// </summary>
        Task<byte[]> ExportToCsvAsync<T>(IEnumerable<T> data, string fileName);
        
        /// <summary>
        /// Exports data to Excel format
        /// </summary>
        Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string fileName);
        
        /// <summary>
        /// Validates file upload
        /// </summary>
        Task<FileValidationResult> ValidateFileAsync(Stream fileStream, string fileName, string contentType);
        
        /// <summary>
        /// Deletes a file from the server
        /// </summary>
        Task<bool> DeleteFileAsync(string filePath);
        
        /// <summary>
        /// Gets file information
        /// </summary>
        Task<FileInfo> GetFileInfoAsync(string filePath);
        
        /// <summary>
        /// Cleans up old files based on retention policy
        /// </summary>
        Task CleanupOldFilesAsync();
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class FileDownloadResult
    {
        public bool Success { get; set; }
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class FileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }
} 