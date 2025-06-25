//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using NLog;

namespace CSETWebCore.Api.Error
{
    /// <summary>
    /// Enhanced exception handling middleware with correlation IDs and structured logging
    /// </summary>
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// Configure enhanced exception handler with correlation IDs and structured logging
        /// </summary>
        /// <param name="app">Application builder</param>
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var correlationId = Guid.NewGuid().ToString();
                    context.Response.Headers.Add("X-Correlation-ID", correlationId);

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var exception = contextFeature.Error;
                        var errorDetails = CreateErrorDetails(context, exception, correlationId);
                        
                        // Get services for enhanced error handling
                        var errorAnalyticsService = context.RequestServices.GetService<Services.IErrorAnalyticsService>();
                        var errorRecoveryService = context.RequestServices.GetService<Services.IErrorRecoveryService>();
                        
                        // Log the error with structured information
                        LogStructuredError(exception, errorDetails, context);
                        
                        // Track error analytics
                        if (errorAnalyticsService != null)
                        {
                            await errorAnalyticsService.TrackErrorAsync(errorDetails, exception);
                        }
                        
                        // Get recovery suggestions
                        if (errorRecoveryService != null)
                        {
                            var suggestions = await errorRecoveryService.GetRecoverySuggestionsAsync(errorDetails);
                            errorDetails.RecoverySuggestions = suggestions;
                        }
                        
                        // Set response
                        context.Response.StatusCode = errorDetails.StatusCode;
                        context.Response.ContentType = "application/json";
                        
                        // Remove sensitive details in production
                        if (!context.RequestServices.GetService<IHostEnvironment>().IsDevelopment())
                        {
                            errorDetails.Details = null;
                        }

                        await context.Response.WriteAsync(errorDetails.ToString());
                    }
                });
            });
        }

        /// <summary>
        /// Create comprehensive error details from exception
        /// </summary>
        private static ErrorDetails CreateErrorDetails(HttpContext context, Exception exception, string correlationId)
        {
            var errorDetails = new ErrorDetails
            {
                CorrelationId = correlationId,
                RequestPath = context.Request.Path,
                RequestMethod = context.Request.Method,
                UserId = GetUserId(context)
            };

            // Handle custom CSET exceptions
            if (exception is CSETException csetException)
            {
                errorDetails.StatusCode = csetException.StatusCode;
                errorDetails.Message = csetException.Message;
                errorDetails.ErrorType = csetException.ErrorCode;
                
                // Add specific handling for different exception types
                switch (csetException)
                {
                    case ValidationException validationEx:
                        errorDetails.ErrorType = "ValidationError";
                        break;
                    case AuthenticationException authEx:
                        errorDetails.ErrorType = "AuthenticationError";
                        break;
                    case BusinessLogicException businessEx:
                        errorDetails.ErrorType = "BusinessLogicError";
                        break;
                    case FileOperationException fileEx:
                        errorDetails.ErrorType = "FileOperationError";
                        break;
                    case ImportExportException importEx:
                        errorDetails.ErrorType = "ImportExportError";
                        break;
                    case AssessmentException assessmentEx:
                        errorDetails.ErrorType = "AssessmentError";
                        break;
                }
            }
            else
            {
                // Determine error type and status code for standard exceptions
                switch (exception)
                {
                    case ArgumentException argEx:
                        errorDetails.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.Message = "Invalid request parameters provided.";
                        errorDetails.ErrorType = "ValidationError";
                        break;

                    case UnauthorizedAccessException:
                        errorDetails.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorDetails.Message = "Access denied. Please authenticate to continue.";
                        errorDetails.ErrorType = "AuthenticationError";
                        break;

                    case InvalidOperationException:
                        errorDetails.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.Message = "The requested operation cannot be performed.";
                        errorDetails.ErrorType = "BusinessLogicError";
                        break;

                    case System.Data.SqlClient.SqlException:
                        errorDetails.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorDetails.Message = "A database error occurred while processing your request.";
                        errorDetails.ErrorType = "DatabaseError";
                        break;

                    case System.IO.IOException:
                        errorDetails.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorDetails.Message = "A file system error occurred while processing your request.";
                        errorDetails.ErrorType = "FileOperationError";
                        break;

                    default:
                        errorDetails.StatusCode = (int)HttpStatusCode.InternalServerError;
                        errorDetails.Message = "An unexpected error occurred while processing your request.";
                        errorDetails.ErrorType = "InternalServerError";
                        break;
                }
            }

            // Add detailed information for development
            if (context.RequestServices.GetService<IHostEnvironment>().IsDevelopment())
            {
                errorDetails.Details = exception.ToString();
            }

            return errorDetails;
        }

        /// <summary>
        /// Log error with structured information
        /// </summary>
        private static void LogStructuredError(Exception exception, ErrorDetails errorDetails, HttpContext context)
        {
            var logger = LogManager.GetCurrentClassLogger();
            
            var logEvent = new LogEventInfo(NLog.LogLevel.Error, logger.Name, 
                $"Error occurred: {errorDetails.Message}")
            {
                Exception = exception
            };

            // Add structured properties
            logEvent.Properties["CorrelationId"] = errorDetails.CorrelationId;
            logEvent.Properties["StatusCode"] = errorDetails.StatusCode;
            logEvent.Properties["ErrorType"] = errorDetails.ErrorType;
            logEvent.Properties["RequestPath"] = errorDetails.RequestPath;
            logEvent.Properties["RequestMethod"] = errorDetails.RequestMethod;
            logEvent.Properties["UserId"] = errorDetails.UserId;
            logEvent.Properties["UserAgent"] = context.Request.Headers["User-Agent"].ToString();
            logEvent.Properties["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString();
            logEvent.Properties["Timestamp"] = errorDetails.Timestamp;

            logger.Log(logEvent);
        }

        /// <summary>
        /// Extract user ID from context if available
        /// </summary>
        private static string GetUserId(HttpContext context)
        {
            try
            {
                var user = context.User;
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = user.FindFirst("UserId") ?? user.FindFirst("sub");
                    return userIdClaim?.Value;
                }
            }
            catch
            {
                // Ignore errors in user ID extraction
            }
            return null;
        }
    }
}