namespace CSETWebBlazor.Services
{
    public interface IErrorHandlingService
    {
        /// <summary>
        /// Logs an error
        /// </summary>
        Task LogErrorAsync(Exception exception, string context = "");
        
        /// <summary>
        /// Logs a warning
        /// </summary>
        Task LogWarningAsync(string message, string context = "");
        
        /// <summary>
        /// Logs an information message
        /// </summary>
        Task LogInformationAsync(string message, string context = "");
        
        /// <summary>
        /// Gets user-friendly error message
        /// </summary>
        string GetUserFriendlyErrorMessage(Exception exception);
        
        /// <summary>
        /// Handles application errors
        /// </summary>
        Task HandleApplicationErrorAsync(Exception exception);
        
        /// <summary>
        /// Reports error to external service
        /// </summary>
        Task ReportErrorAsync(Exception exception, string userId = "");
    }

    public class ErrorLogEntry
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? StackTrace { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }
} 