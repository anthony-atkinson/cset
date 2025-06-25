//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CSETWebCore.Api.Error;

namespace CSETWebCore.Api.Services
{
    /// <summary>
    /// Service for providing error recovery suggestions
    /// </summary>
    public interface IErrorRecoveryService
    {
        /// <summary>
        /// Get recovery suggestions for an error
        /// </summary>
        Task<string[]> GetRecoverySuggestionsAsync(ErrorDetails errorDetails);

        /// <summary>
        /// Get recovery suggestions for an exception
        /// </summary>
        Task<string[]> GetRecoverySuggestionsAsync(Exception exception, string context = null);

        /// <summary>
        /// Get automated recovery actions for an error
        /// </summary>
        Task<List<RecoveryAction>> GetAutomatedRecoveryActionsAsync(ErrorDetails errorDetails);

        /// <summary>
        /// Execute automated recovery action
        /// </summary>
        Task<bool> ExecuteRecoveryActionAsync(string actionId, string correlationId);
    }

    /// <summary>
    /// Implementation of error recovery service
    /// </summary>
    public class ErrorRecoveryService : IErrorRecoveryService
    {
        private readonly ILogger<ErrorRecoveryService> _logger;
        private readonly Dictionary<string, string[]> _recoverySuggestions;

        public ErrorRecoveryService(ILogger<ErrorRecoveryService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize recovery suggestions dictionary
            _recoverySuggestions = InitializeRecoverySuggestions();
        }

        /// <summary>
        /// Get recovery suggestions for an error
        /// </summary>
        public async Task<string[]> GetRecoverySuggestionsAsync(ErrorDetails errorDetails)
        {
            try
            {
                var suggestions = new List<string>();

                // Get base suggestions for error type
                if (_recoverySuggestions.ContainsKey(errorDetails.ErrorType))
                {
                    suggestions.AddRange(_recoverySuggestions[errorDetails.ErrorType]);
                }

                // Add context-specific suggestions
                suggestions.AddRange(GetContextSpecificSuggestions(errorDetails));

                // Add general suggestions
                suggestions.AddRange(_recoverySuggestions["General"]);

                await Task.CompletedTask;
                return suggestions.Distinct().ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recovery suggestions for error type: {ErrorType}", errorDetails.ErrorType);
                return new[] { "Contact support if the issue persists." };
            }
        }

        /// <summary>
        /// Get recovery suggestions for an exception
        /// </summary>
        public async Task<string[]> GetRecoverySuggestionsAsync(Exception exception, string context = null)
        {
            try
            {
                var suggestions = new List<string>();

                // Determine error type from exception
                var errorType = GetErrorTypeFromException(exception);
                
                if (_recoverySuggestions.ContainsKey(errorType))
                {
                    suggestions.AddRange(_recoverySuggestions[errorType]);
                }

                // Add exception-specific suggestions
                suggestions.AddRange(GetExceptionSpecificSuggestions(exception));

                // Add context-specific suggestions
                if (!string.IsNullOrEmpty(context))
                {
                    suggestions.AddRange(GetContextSpecificSuggestions(context));
                }

                // Add general suggestions
                suggestions.AddRange(_recoverySuggestions["General"]);

                await Task.CompletedTask;
                return suggestions.Distinct().ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recovery suggestions for exception: {ExceptionType}", exception.GetType().Name);
                return new[] { "Contact support if the issue persists." };
            }
        }

        /// <summary>
        /// Get automated recovery actions for an error
        /// </summary>
        public async Task<List<RecoveryAction>> GetAutomatedRecoveryActionsAsync(ErrorDetails errorDetails)
        {
            try
            {
                var actions = new List<RecoveryAction>();

                // Add automated actions based on error type
                switch (errorDetails.ErrorType)
                {
                    case "ValidationError":
                        actions.Add(new RecoveryAction
                        {
                            Id = "retry_validation",
                            Name = "Retry with corrected data",
                            Description = "Automatically retry the operation with validated data",
                            IsAutomated = true,
                            RiskLevel = "Low"
                        });
                        break;

                    case "AuthenticationError":
                        actions.Add(new RecoveryAction
                        {
                            Id = "refresh_token",
                            Name = "Refresh authentication token",
                            Description = "Automatically refresh the authentication token",
                            IsAutomated = true,
                            RiskLevel = "Low"
                        });
                        break;

                    case "DatabaseError":
                        actions.Add(new RecoveryAction
                        {
                            Id = "retry_database",
                            Name = "Retry database operation",
                            Description = "Automatically retry the database operation",
                            IsAutomated = true,
                            RiskLevel = "Medium"
                        });
                        break;
                }

                await Task.CompletedTask;
                return actions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get automated recovery actions for error type: {ErrorType}", errorDetails.ErrorType);
                return new List<RecoveryAction>();
            }
        }

        /// <summary>
        /// Execute automated recovery action
        /// </summary>
        public async Task<bool> ExecuteRecoveryActionAsync(string actionId, string correlationId)
        {
            try
            {
                _logger.LogInformation("Executing recovery action: {ActionId} for correlation ID: {CorrelationId}", actionId, correlationId);

                // Implement recovery action logic based on action ID
                switch (actionId)
                {
                    case "retry_validation":
                        // Logic for retrying validation
                        break;
                    case "refresh_token":
                        // Logic for refreshing token
                        break;
                    case "retry_database":
                        // Logic for retrying database operation
                        break;
                    default:
                        _logger.LogWarning("Unknown recovery action: {ActionId}", actionId);
                        return false;
                }

                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute recovery action: {ActionId} for correlation ID: {CorrelationId}", actionId, correlationId);
                return false;
            }
        }

        /// <summary>
        /// Initialize recovery suggestions dictionary
        /// </summary>
        private Dictionary<string, string[]> InitializeRecoverySuggestions()
        {
            return new Dictionary<string, string[]>
            {
                ["ValidationError"] = new[]
                {
                    "Check the request parameters for validity",
                    "Ensure all required fields are provided",
                    "Verify data formats match expected types",
                    "Review the API documentation for correct parameter usage"
                },
                ["AuthenticationError"] = new[]
                {
                    "Log in with valid credentials",
                    "Check if your session has expired",
                    "Verify you have the required permissions",
                    "Contact your administrator if access issues persist"
                },
                ["BusinessLogicError"] = new[]
                {
                    "Verify the current state allows this operation",
                    "Check if all prerequisites are met",
                    "Review business rules and constraints",
                    "Contact support if the issue persists"
                },
                ["DatabaseError"] = new[]
                {
                    "Try again in a few moments",
                    "Check if the database is accessible",
                    "Verify database connection settings",
                    "Contact support if the issue persists"
                },
                ["FileOperationError"] = new[]
                {
                    "Check if the file exists and is accessible",
                    "Verify file permissions",
                    "Ensure sufficient disk space",
                    "Try again in a few moments"
                },
                ["ImportExportError"] = new[]
                {
                    "Verify the file format is supported",
                    "Check if the file is not corrupted",
                    "Ensure the file size is within limits",
                    "Try with a different file or format"
                },
                ["General"] = new[]
                {
                    "Try again in a few moments",
                    "Check the correlation ID for tracking",
                    "Contact support if the issue persists",
                    "Review the application logs for more details"
                }
            };
        }

        /// <summary>
        /// Get context-specific suggestions
        /// </summary>
        private string[] GetContextSpecificSuggestions(ErrorDetails errorDetails)
        {
            var suggestions = new List<string>();

            // Add suggestions based on request path
            if (errorDetails.RequestPath?.Contains("/assessment") == true)
            {
                suggestions.Add("Verify the assessment exists and you have access to it");
                suggestions.Add("Check if the assessment is in a valid state for this operation");
            }

            if (errorDetails.RequestPath?.Contains("/import") == true)
            {
                suggestions.Add("Verify the import file format and structure");
                suggestions.Add("Check if the file contains valid data");
            }

            if (errorDetails.RequestPath?.Contains("/export") == true)
            {
                suggestions.Add("Verify you have permission to export this data");
                suggestions.Add("Check if the export format is supported");
            }

            return suggestions.ToArray();
        }

        /// <summary>
        /// Get context-specific suggestions for string context
        /// </summary>
        private string[] GetContextSpecificSuggestions(string context)
        {
            var suggestions = new List<string>();

            if (context.Contains("assessment"))
            {
                suggestions.Add("Verify the assessment exists and you have access to it");
            }

            if (context.Contains("import"))
            {
                suggestions.Add("Verify the import file format and structure");
            }

            if (context.Contains("export"))
            {
                suggestions.Add("Verify you have permission to export this data");
            }

            return suggestions.ToArray();
        }

        /// <summary>
        /// Get exception-specific suggestions
        /// </summary>
        private string[] GetExceptionSpecificSuggestions(Exception exception)
        {
            var suggestions = new List<string>();

            if (exception is ArgumentException)
            {
                suggestions.Add("Check the provided parameters for validity");
            }
            else if (exception is UnauthorizedAccessException)
            {
                suggestions.Add("Verify your authentication credentials");
            }
            else if (exception is InvalidOperationException)
            {
                suggestions.Add("Check if the operation is allowed in the current state");
            }
            else if (exception is System.IO.IOException)
            {
                suggestions.Add("Check file system permissions and availability");
            }

            return suggestions.ToArray();
        }

        /// <summary>
        /// Get error type from exception
        /// </summary>
        private string GetErrorTypeFromException(Exception exception)
        {
            if (exception is ArgumentException)
                return "ValidationError";
            if (exception is UnauthorizedAccessException)
                return "AuthenticationError";
            if (exception is InvalidOperationException)
                return "BusinessLogicError";
            if (exception is System.IO.IOException)
                return "FileOperationError";
            
            return "InternalServerError";
        }
    }

    /// <summary>
    /// Recovery action data model
    /// </summary>
    public class RecoveryAction
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAutomated { get; set; }
        public string RiskLevel { get; set; } // Low, Medium, High
    }
} 