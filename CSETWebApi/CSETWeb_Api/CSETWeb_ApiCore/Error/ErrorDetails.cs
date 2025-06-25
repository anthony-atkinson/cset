//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System.Text.Json;

namespace CSETWebCore.Api.Error
{
    /// <summary>
    /// Enhanced error details for comprehensive error reporting
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// User-friendly error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Unique correlation ID for tracking errors
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Error type/category
        /// </summary>
        public string ErrorType { get; set; }

        /// <summary>
        /// Detailed error information (only in development)
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Suggested actions for error recovery
        /// </summary>
        public string[] RecoverySuggestions { get; set; }

        /// <summary>
        /// Request path that caused the error
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// HTTP method that caused the error
        /// </summary>
        public string RequestMethod { get; set; }

        /// <summary>
        /// User ID if available
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Constructor with default values
        /// </summary>
        public ErrorDetails()
        {
            Timestamp = DateTime.UtcNow;
            CorrelationId = Guid.NewGuid().ToString();
            RecoverySuggestions = new string[0];
        }

        /// <summary>
        /// Serialize to JSON with proper formatting
        /// </summary>
        public override string ToString()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(this, options);
        }
    }
}