//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CSETWebCore.Api.Error;
using CSETWebCore.Interfaces;

namespace CSETWebCore.Api.Services
{
    /// <summary>
    /// Service for tracking and analyzing application errors
    /// </summary>
    public interface IErrorAnalyticsService
    {
        /// <summary>
        /// Track an error occurrence
        /// </summary>
        Task TrackErrorAsync(ErrorDetails errorDetails, Exception exception = null);

        /// <summary>
        /// Get error statistics for a time period
        /// </summary>
        Task<ErrorStatistics> GetErrorStatisticsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get error trends by type
        /// </summary>
        Task<List<ErrorTrend>> GetErrorTrendsAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get most common errors
        /// </summary>
        Task<List<CommonError>> GetMostCommonErrorsAsync(int limit = 10);

        /// <summary>
        /// Get error details by correlation ID
        /// </summary>
        Task<ErrorDetails> GetErrorByCorrelationIdAsync(string correlationId);
    }

    /// <summary>
    /// Implementation of error analytics service
    /// </summary>
    public class ErrorAnalyticsService : IErrorAnalyticsService
    {
        private readonly ILogger<ErrorAnalyticsService> _logger;
        private readonly ITelemetryService _telemetryService;
        private readonly CSETContext _context;

        public ErrorAnalyticsService(
            ILogger<ErrorAnalyticsService> logger,
            ITelemetryService telemetryService,
            CSETContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Track an error occurrence
        /// </summary>
        public async Task TrackErrorAsync(ErrorDetails errorDetails, Exception exception = null)
        {
            try
            {
                // Track with Application Insights
                _telemetryService.TrackException(exception ?? new Exception(errorDetails.Message), 
                    $"Error: {errorDetails.ErrorType}", 
                    !string.IsNullOrEmpty(errorDetails.UserId) ? int.Parse(errorDetails.UserId) : (int?)null);

                // Track custom metrics
                var properties = new Dictionary<string, string>
                {
                    { "ErrorType", errorDetails.ErrorType },
                    { "StatusCode", errorDetails.StatusCode.ToString() },
                    { "RequestPath", errorDetails.RequestPath },
                    { "RequestMethod", errorDetails.RequestMethod },
                    { "CorrelationId", errorDetails.CorrelationId }
                };

                if (!string.IsNullOrEmpty(errorDetails.UserId))
                {
                    properties.Add("UserId", errorDetails.UserId);
                }

                _telemetryService.TrackCustomEvent("ErrorOccurred", properties);

                // Log structured error information
                _logger.LogError("Error tracked: {ErrorType} - {Message} - CorrelationId: {CorrelationId} - UserId: {UserId}",
                    errorDetails.ErrorType, errorDetails.Message, errorDetails.CorrelationId, errorDetails.UserId);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track error analytics for correlation ID: {CorrelationId}", errorDetails.CorrelationId);
            }
        }

        /// <summary>
        /// Get error statistics for a time period
        /// </summary>
        public async Task<ErrorStatistics> GetErrorStatisticsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // This would typically query a database table for error analytics
                // For now, we'll return a mock implementation
                var statistics = new ErrorStatistics
                {
                    TotalErrors = 0,
                    ErrorsByType = new Dictionary<string, int>(),
                    ErrorsByStatusCode = new Dictionary<int, int>(),
                    AverageResponseTime = TimeSpan.Zero,
                    StartDate = startDate,
                    EndDate = endDate
                };

                await Task.CompletedTask;
                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get error statistics for period {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Get error trends by type
        /// </summary>
        public async Task<List<ErrorTrend>> GetErrorTrendsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var trends = new List<ErrorTrend>();
                
                // Mock implementation - would query database for actual trends
                await Task.CompletedTask;
                
                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get error trends for period {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        /// <summary>
        /// Get most common errors
        /// </summary>
        public async Task<List<CommonError>> GetMostCommonErrorsAsync(int limit = 10)
        {
            try
            {
                var commonErrors = new List<CommonError>();
                
                // Mock implementation - would query database for actual common errors
                await Task.CompletedTask;
                
                return commonErrors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get most common errors with limit {Limit}", limit);
                throw;
            }
        }

        /// <summary>
        /// Get error details by correlation ID
        /// </summary>
        public async Task<ErrorDetails> GetErrorByCorrelationIdAsync(string correlationId)
        {
            try
            {
                // Mock implementation - would query database for error details
                await Task.CompletedTask;
                
                return null; // Would return actual error details from database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get error details for correlation ID: {CorrelationId}", correlationId);
                throw;
            }
        }
    }

    /// <summary>
    /// Error statistics data model
    /// </summary>
    public class ErrorStatistics
    {
        public int TotalErrors { get; set; }
        public Dictionary<string, int> ErrorsByType { get; set; }
        public Dictionary<int, int> ErrorsByStatusCode { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Error trend data model
    /// </summary>
    public class ErrorTrend
    {
        public string ErrorType { get; set; }
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Common error data model
    /// </summary>
    public class CommonError
    {
        public string ErrorType { get; set; }
        public string Message { get; set; }
        public int OccurrenceCount { get; set; }
        public double Percentage { get; set; }
        public DateTime LastOccurrence { get; set; }
    }
} 