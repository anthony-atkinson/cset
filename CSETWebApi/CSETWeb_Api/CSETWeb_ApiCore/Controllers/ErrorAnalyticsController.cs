//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSETWebCore.Api.Services;
using CSETWebCore.Api.Error;

namespace CSETWeb_ApiCore.Controllers
{
    /// <summary>
    /// Controller for error analytics and monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ErrorAnalyticsController : ControllerBase
    {
        private readonly IErrorAnalyticsService _errorAnalyticsService;
        private readonly ILogger<ErrorAnalyticsController> _logger;

        public ErrorAnalyticsController(
            IErrorAnalyticsService errorAnalyticsService,
            ILogger<ErrorAnalyticsController> logger)
        {
            _errorAnalyticsService = errorAnalyticsService ?? throw new ArgumentNullException(nameof(errorAnalyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get error statistics for a specified time period.
        /// </summary>
        /// <param name="startDate">Start date for statistics (ISO 8601 format)</param>
        /// <param name="endDate">End date for statistics (ISO 8601 format)</param>
        /// <returns>
        /// 200 OK with error statistics
        /// 400 Bad Request if date parameters are invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 500 Internal Server Error if statistics retrieval fails
        /// </returns>
        /// <remarks>
        /// Retrieves comprehensive error statistics including total errors, errors by type,
        /// errors by status code, and average response times for the specified period.
        /// Requires authentication and appropriate permissions.
        /// </remarks>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetErrorStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                if (endDate > DateTime.UtcNow)
                {
                    return BadRequest("End date cannot be in the future");
                }

                var statistics = await _errorAnalyticsService.GetErrorStatisticsAsync(startDate, endDate);
                
                _logger.LogInformation("Retrieved error statistics for period {StartDate} to {EndDate}", startDate, endDate);
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve error statistics for period {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(500, "Failed to retrieve error statistics");
            }
        }

        /// <summary>
        /// Get error trends by type for a specified time period.
        /// </summary>
        /// <param name="startDate">Start date for trends (ISO 8601 format)</param>
        /// <param name="endDate">End date for trends (ISO 8601 format)</param>
        /// <returns>
        /// 200 OK with error trends
        /// 400 Bad Request if date parameters are invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 500 Internal Server Error if trends retrieval fails
        /// </returns>
        /// <remarks>
        /// Retrieves error trends showing how different error types have changed over time.
        /// Useful for identifying patterns and trends in application errors.
        /// </remarks>
        [HttpGet("trends")]
        public async Task<IActionResult> GetErrorTrends(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return BadRequest("Start date must be before end date");
                }

                if (endDate > DateTime.UtcNow)
                {
                    return BadRequest("End date cannot be in the future");
                }

                var trends = await _errorAnalyticsService.GetErrorTrendsAsync(startDate, endDate);
                
                _logger.LogInformation("Retrieved error trends for period {StartDate} to {EndDate}", startDate, endDate);
                
                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve error trends for period {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(500, "Failed to retrieve error trends");
            }
        }

        /// <summary>
        /// Get most common errors.
        /// </summary>
        /// <param name="limit">Maximum number of errors to return (default: 10, max: 100)</param>
        /// <returns>
        /// 200 OK with most common errors
        /// 400 Bad Request if limit is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 500 Internal Server Error if retrieval fails
        /// </returns>
        /// <remarks>
        /// Retrieves the most frequently occurring errors, useful for identifying
        /// common issues that need attention.
        /// </remarks>
        [HttpGet("common")]
        public async Task<IActionResult> GetMostCommonErrors([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest("Limit must be between 1 and 100");
                }

                var commonErrors = await _errorAnalyticsService.GetMostCommonErrorsAsync(limit);
                
                _logger.LogInformation("Retrieved {Count} most common errors", commonErrors.Count);
                
                return Ok(commonErrors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve most common errors with limit {Limit}", limit);
                return StatusCode(500, "Failed to retrieve most common errors");
            }
        }

        /// <summary>
        /// Get error details by correlation ID.
        /// </summary>
        /// <param name="correlationId">Correlation ID of the error</param>
        /// <returns>
        /// 200 OK with error details
        /// 400 Bad Request if correlation ID is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 404 Not Found if error not found
        /// 500 Internal Server Error if retrieval fails
        /// </returns>
        /// <remarks>
        /// Retrieves detailed information about a specific error using its correlation ID.
        /// Useful for debugging and investigating specific error occurrences.
        /// </remarks>
        [HttpGet("details/{correlationId}")]
        public async Task<IActionResult> GetErrorDetails(string correlationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correlationId))
                {
                    return BadRequest("Correlation ID is required");
                }

                if (!Guid.TryParse(correlationId, out _))
                {
                    return BadRequest("Invalid correlation ID format");
                }

                var errorDetails = await _errorAnalyticsService.GetErrorByCorrelationIdAsync(correlationId);
                
                if (errorDetails == null)
                {
                    return NotFound($"Error with correlation ID '{correlationId}' not found");
                }

                _logger.LogInformation("Retrieved error details for correlation ID: {CorrelationId}", correlationId);
                
                return Ok(errorDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve error details for correlation ID: {CorrelationId}", correlationId);
                return StatusCode(500, "Failed to retrieve error details");
            }
        }

        /// <summary>
        /// Get current error summary.
        /// </summary>
        /// <returns>
        /// 200 OK with current error summary
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 500 Internal Server Error if retrieval fails
        /// </returns>
        /// <remarks>
        /// Retrieves a summary of current error statistics for the last 24 hours.
        /// Provides a quick overview of recent error activity.
        /// </remarks>
        [HttpGet("summary")]
        public async Task<IActionResult> GetErrorSummary()
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddHours(-24);

                var statistics = await _errorAnalyticsService.GetErrorStatisticsAsync(startDate, endDate);
                var commonErrors = await _errorAnalyticsService.GetMostCommonErrorsAsync(5);

                var summary = new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    TotalErrors = statistics.TotalErrors,
                    ErrorsByType = statistics.ErrorsByType,
                    MostCommonErrors = commonErrors,
                    AverageResponseTime = statistics.AverageResponseTime
                };

                _logger.LogInformation("Retrieved error summary for last 24 hours");
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve error summary");
                return StatusCode(500, "Failed to retrieve error summary");
            }
        }
    }
} 