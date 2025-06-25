using System;
using System.Threading.Tasks;
using CSETWeb_ApiCore.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CSETWeb_ApiCore.Middleware
{
    /// <summary>
    /// Middleware for handling API rate limiting
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitMiddleware> _logger;
        private readonly IRateLimitService _rateLimitService;

        public RateLimitMiddleware(
            RequestDelegate next,
            ILogger<RateLimitMiddleware> logger,
            IRateLimitService rateLimitService)
        {
            _next = next;
            _logger = logger;
            _rateLimitService = rateLimitService;
        }

        /// <summary>
        /// Processes the HTTP request and applies rate limiting
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check if request should be rate limited
                if (await _rateLimitService.ShouldRateLimitAsync(context))
                {
                    await HandleRateLimitExceeded(context);
                    return;
                }

                // Add rate limit headers to response
                await AddRateLimitHeaders(context);

                // Continue with the request pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware for request {Path}", context.Request.Path);
                
                // Continue with the request pipeline even if rate limiting fails
                await _next(context);
            }
        }

        /// <summary>
        /// Handles rate limit exceeded responses
        /// </summary>
        private async Task HandleRateLimitExceeded(HttpContext context)
        {
            var rateLimitInfo = await _rateLimitService.GetRateLimitInfoAsync(context);
            
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = "Rate limit exceeded",
                message = $"Too many requests. Limit: {rateLimitInfo.Limit} requests per {rateLimitInfo.PeriodSeconds} seconds.",
                retryAfter = rateLimitInfo.RemainingSeconds,
                limit = rateLimitInfo.Limit,
                remaining = rateLimitInfo.RemainingRequests,
                reset = DateTime.UtcNow.AddSeconds(rateLimitInfo.RemainingSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit"] = rateLimitInfo.Limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = rateLimitInfo.RemainingRequests.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTime.UtcNow.AddSeconds(rateLimitInfo.RemainingSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");
            context.Response.Headers["Retry-After"] = rateLimitInfo.RemainingSeconds.ToString();

            var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            await context.Response.WriteAsync(jsonResponse);

            _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Status: 429", 
                rateLimitInfo.ClientId, context.Request.Path);
        }

        /// <summary>
        /// Adds rate limit headers to the response
        /// </summary>
        private async Task AddRateLimitHeaders(HttpContext context)
        {
            try
            {
                var rateLimitInfo = await _rateLimitService.GetRateLimitInfoAsync(context);
                
                context.Response.Headers["X-RateLimit-Limit"] = rateLimitInfo.Limit.ToString();
                context.Response.Headers["X-RateLimit-Remaining"] = rateLimitInfo.RemainingRequests.ToString();
                context.Response.Headers["X-RateLimit-Reset"] = DateTime.UtcNow.AddSeconds(rateLimitInfo.RemainingSeconds).ToString("yyyy-MM-ddTHH:mm:ssZ");
                context.Response.Headers["X-RateLimit-Client"] = rateLimitInfo.ClientId;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to add rate limit headers for request {Path}", context.Request.Path);
            }
        }
    }

    /// <summary>
    /// Extension methods for rate limiting middleware
    /// </summary>
    public static class RateLimitMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitMiddleware>();
        }
    }
} 