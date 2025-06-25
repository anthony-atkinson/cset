namespace CSETWebBlazor.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILogger<ErrorHandlingService> _logger;

        public ErrorHandlingService(ILogger<ErrorHandlingService> logger)
        {
            _logger = logger;
        }

        public async Task LogErrorAsync(Exception exception, string context = "")
        {
            var logEntry = new ErrorLogEntry
            {
                Level = "Error",
                Message = exception.Message,
                Context = context,
                StackTrace = exception.StackTrace,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ExceptionType"] = exception.GetType().Name,
                    ["InnerException"] = exception.InnerException?.Message ?? "None"
                }
            };

            _logger.LogError(exception, "Error in context: {Context}. Message: {Message}", context, exception.Message);
            await Task.CompletedTask;
        }

        public async Task LogWarningAsync(string message, string context = "")
        {
            var logEntry = new ErrorLogEntry
            {
                Level = "Warning",
                Message = message,
                Context = context
            };

            _logger.LogWarning("Warning in context: {Context}. Message: {Message}", context, message);
            await Task.CompletedTask;
        }

        public async Task LogInformationAsync(string message, string context = "")
        {
            var logEntry = new ErrorLogEntry
            {
                Level = "Information",
                Message = message,
                Context = context
            };

            _logger.LogInformation("Information in context: {Context}. Message: {Message}", context, message);
            await Task.CompletedTask;
        }

        public string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException => "You don't have permission to perform this action. Please contact your administrator.",
                ArgumentException => "Invalid input provided. Please check your data and try again.",
                InvalidOperationException => "The requested operation cannot be completed at this time. Please try again later.",
                TimeoutException => "The operation timed out. Please try again.",
                HttpRequestException => "Unable to connect to the server. Please check your internet connection and try again.",
                JsonException => "Invalid data format. Please check your input and try again.",
                _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
            };
        }

        public async Task HandleApplicationErrorAsync(Exception exception)
        {
            await LogErrorAsync(exception, "Application Error");
            
            // Additional error handling logic can be added here
            // For example, sending notifications, updating error tracking systems, etc.
            
            await Task.CompletedTask;
        }

        public async Task ReportErrorAsync(Exception exception, string userId = "")
        {
            var logEntry = new ErrorLogEntry
            {
                Level = "Error",
                Message = exception.Message,
                Context = "Error Reporting",
                UserId = userId,
                StackTrace = exception.StackTrace,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ExceptionType"] = exception.GetType().Name,
                    ["UserId"] = userId,
                    ["Timestamp"] = DateTime.UtcNow,
                    ["UserAgent"] = "Blazor Server", // In a real app, you'd get this from the request
                    ["RequestPath"] = "Unknown" // In a real app, you'd get this from the request
                }
            };

            _logger.LogError(exception, "Error reported by user {UserId}: {Message}", userId, exception.Message);
            
            // In a production environment, you might want to send this to an external error tracking service
            // like Application Insights, Sentry, or a custom error tracking system
            
            await Task.CompletedTask;
        }
    }
} 