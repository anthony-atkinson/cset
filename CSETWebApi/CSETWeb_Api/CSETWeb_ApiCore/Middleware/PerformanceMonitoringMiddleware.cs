using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using CSETWebCore.Business.Telemetry;

namespace CSETWeb_ApiCore.Middleware
{
    /// <summary>
    /// Middleware for monitoring API endpoint performance and tracking metrics
    /// </summary>
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly ITelemetryService _telemetryService;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMonitoringMiddleware> logger,
            ITelemetryService telemetryService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                using var memoryStream = new System.IO.MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context);

                stopwatch.Stop();

                // Track API endpoint performance
                var endpoint = context.Request.Path.Value;
                var method = context.Request.Method;
                var statusCode = context.Response.StatusCode;
                var duration = stopwatch.Elapsed;

                _telemetryService.TrackApiEndpoint(endpoint, method, duration, statusCode);

                // Log slow requests
                if (duration.TotalMilliseconds > 1000) // Log requests taking more than 1 second
                {
                    _logger.LogWarning("Slow API request detected: {Method} {Endpoint} took {Duration}ms with status {StatusCode}",
                        method, endpoint, duration.TotalMilliseconds, statusCode);
                }

                // Copy response back to original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Track failed requests
                var endpoint = context.Request.Path.Value;
                var method = context.Request.Method;
                var duration = stopwatch.Elapsed;

                _telemetryService.TrackApiEndpoint(endpoint, method, duration, 500);
                _telemetryService.TrackException(ex, $"API Request: {method} {endpoint}");

                _logger.LogError(ex, "API request failed: {Method} {Endpoint} took {Duration}ms",
                    method, endpoint, duration.TotalMilliseconds);

                // Restore original body stream
                context.Response.Body = originalBodyStream;

                // Re-throw the exception to maintain the original behavior
                throw;
            }
            finally
            {
                // Ensure original body stream is restored
                if (context.Response.Body != originalBodyStream)
                {
                    context.Response.Body = originalBodyStream;
                }
            }
        }
    }

    /// <summary>
    /// Extension methods for registering the performance monitoring middleware
    /// </summary>
    public static class PerformanceMonitoringMiddlewareExtensions
    {
        /// <summary>
        /// Adds performance monitoring middleware to the application pipeline
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>The application builder</returns>
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
        }
    }
} 