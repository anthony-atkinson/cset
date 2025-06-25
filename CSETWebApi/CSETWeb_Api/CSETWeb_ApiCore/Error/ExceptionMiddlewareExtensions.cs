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
                        
                        // Log the error with structured information
                        LogStructuredError(exception, errorDetails, context);
                        
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

            // Determine error type and status code
            switch (exception)
            {
                case ArgumentException argEx:
                    errorDetails.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorDetails.Message = "Invalid request parameters provided.";
                    errorDetails.ErrorType = "ValidationError";
                    errorDetails.RecoverySuggestions = new[]
                    {
                        "Check the request parameters for validity",
                        "Ensure all required fields are provided",
                        "Verify data formats match expected types"
                    };
                    break;

                case UnauthorizedAccessException:
                    errorDetails.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorDetails.Message = "Access denied. Please authenticate to continue.";
                    errorDetails.ErrorType = "AuthenticationError";
                    errorDetails.RecoverySuggestions = new[]
                    {
                        "Log in with valid credentials",
                        "Check if your session has expired",
                        "Verify you have the required permissions"
                    };
                    break;

                default:
                    errorDetails.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorDetails.Message = "An unexpected error occurred while processing your request.";
                    errorDetails.ErrorType = "InternalServerError";
                    errorDetails.RecoverySuggestions = new[]
                    {
                        "Try again in a few moments",
                        "Contact support if the issue persists",
                        "Check the correlation ID for tracking"
                    };
                    break;
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