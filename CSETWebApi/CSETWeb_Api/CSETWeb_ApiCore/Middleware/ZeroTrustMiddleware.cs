//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.ApiCore.Security.ZeroTrust;
using CSETWebCore.Model.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Middleware
{
    /// <summary>
    /// Middleware for Zero Trust Architecture integration
    /// Provides continuous verification and access validation for authenticated requests
    /// </summary>
    public class ZeroTrustMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ZeroTrustMiddleware> _logger;
        private readonly IZeroTrustService _zeroTrustService;

        public ZeroTrustMiddleware(
            RequestDelegate next,
            ILogger<ZeroTrustMiddleware> logger,
            IZeroTrustService zeroTrustService)
        {
            _next = next;
            _logger = logger;
            _zeroTrustService = zeroTrustService;
        }

        /// <summary>
        /// Processes the HTTP request through Zero Trust validation
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip Zero Trust validation for certain endpoints
                if (ShouldSkipZeroTrustValidation(context))
                {
                    await _next(context);
                    return;
                }

                // Extract user information from the request
                var userId = GetUserIdFromContext(context);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No user ID found in request context");
                    await _next(context);
                    return;
                }

                // Create Zero Trust access request
                var accessRequest = new ZeroTrustAccessRequest
                {
                    UserId = userId,
                    ResourceId = GetResourceIdFromRequest(context),
                    Action = GetActionFromRequest(context),
                    ClientIp = GetClientIpAddress(context),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    DeviceId = GetDeviceIdFromContext(context),
                    Location = GetLocationFromContext(context),
                    RequestTime = DateTime.UtcNow,
                    Context = new Dictionary<string, string>
                    {
                        ["Path"] = context.Request.Path,
                        ["Method"] = context.Request.Method,
                        ["Host"] = context.Request.Host.ToString(),
                        ["Referer"] = context.Request.Headers["Referer"].ToString()
                    }
                };

                // Perform Zero Trust validation
                var validationResult = await _zeroTrustService.ValidateAccessAsync(accessRequest);

                if (validationResult.IsAllowed)
                {
                    // Add Zero Trust metadata to the response headers
                    context.Response.Headers["X-ZeroTrust-RiskLevel"] = validationResult.RiskLevel.ToString();
                    context.Response.Headers["X-ZeroTrust-ValidationId"] = validationResult.ValidationId;

                    // Continue with the request pipeline
                    await _next(context);
                }
                else
                {
                    // Access denied by Zero Trust validation
                    _logger.LogWarning("Access denied by Zero Trust validation for user {UserId}. Reason: {Reason}", 
                        userId, validationResult.Reason);

                    context.Response.StatusCode = 403; // Forbidden
                    context.Response.ContentType = "application/json";
                    
                    var errorResponse = new
                    {
                        error = "Access denied by Zero Trust validation",
                        reason = validationResult.Reason,
                        riskLevel = validationResult.RiskLevel.ToString(),
                        requiredActions = validationResult.RequiredActions
                    };

                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(errorResponse));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Zero Trust middleware");
                
                // In case of error, allow the request to continue (fail open for now)
                // In production, you might want to fail closed depending on security requirements
                await _next(context);
            }
        }

        /// <summary>
        /// Determines if Zero Trust validation should be skipped for this request
        /// </summary>
        private bool ShouldSkipZeroTrustValidation(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip validation for health checks, static files, and certain API endpoints
            if (path == null) return true;

            return path.StartsWith("/health") ||
                   path.StartsWith("/api/zerotrust/health") ||
                   path.StartsWith("/api/auth/login") ||
                   path.StartsWith("/api/auth/islocal") ||
                   path.StartsWith("/api/IsRunning") ||
                   path.StartsWith("/swagger") ||
                   path.StartsWith("/favicon.ico") ||
                   path.StartsWith("/static") ||
                   path.StartsWith("/css") ||
                   path.StartsWith("/js") ||
                   path.StartsWith("/images");
        }

        /// <summary>
        /// Extracts user ID from the request context
        /// </summary>
        private string GetUserIdFromContext(HttpContext context)
        {
            // Try to get user ID from JWT token claims
            var userIdClaim = context.User?.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                return userIdClaim;
            }

            // Try to get from email claim
            var emailClaim = context.User?.FindFirst("Email")?.Value;
            if (!string.IsNullOrEmpty(emailClaim))
            {
                return emailClaim;
            }

            // Try to get from name identifier claim
            var nameIdClaim = context.User?.FindFirst("NameIdentifier")?.Value;
            if (!string.IsNullOrEmpty(nameIdClaim))
            {
                return nameIdClaim;
            }

            return null;
        }

        /// <summary>
        /// Extracts resource ID from the request
        /// </summary>
        private string GetResourceIdFromRequest(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            var method = context.Request.Method.ToUpper();

            // Extract resource information from the request path
            if (path?.Contains("/api/assessment/") == true)
            {
                return "assessment-data";
            }
            else if (path?.Contains("/api/analytics/") == true)
            {
                return "analytics-data";
            }
            else if (path?.Contains("/api/admin/") == true)
            {
                return "admin-functions";
            }
            else if (path?.Contains("/api/user/") == true)
            {
                return "user-management";
            }
            else if (path?.Contains("/api/reports/") == true)
            {
                return "report-generation";
            }

            return "general-api";
        }

        /// <summary>
        /// Extracts action from the request
        /// </summary>
        private string GetActionFromRequest(HttpContext context)
        {
            var method = context.Request.Method.ToUpper();
            
            return method switch
            {
                "GET" => "read",
                "POST" => "create",
                "PUT" => "update",
                "PATCH" => "update",
                "DELETE" => "delete",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Gets the client IP address
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first (for proxy scenarios)
            var forwardedHeader = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                return forwardedHeader.Split(',')[0].Trim();
            }

            var realIpHeader = context.Request.Headers["X-Real-IP"].ToString();
            if (!string.IsNullOrEmpty(realIpHeader))
            {
                return realIpHeader;
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Extracts device ID from the request context
        /// </summary>
        private string GetDeviceIdFromContext(HttpContext context)
        {
            // Try to get device ID from headers
            var deviceId = context.Request.Headers["X-Device-ID"].ToString();
            if (!string.IsNullOrEmpty(deviceId))
            {
                return deviceId;
            }

            // Try to get from user agent (simplified device fingerprinting)
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (!string.IsNullOrEmpty(userAgent))
            {
                // Create a simple device fingerprint from user agent
                return $"device-{userAgent.GetHashCode():X}";
            }

            return "unknown-device";
        }

        /// <summary>
        /// Extracts location information from the request context
        /// </summary>
        private string GetLocationFromContext(HttpContext context)
        {
            // Try to get location from headers
            var location = context.Request.Headers["X-Location"].ToString();
            if (!string.IsNullOrEmpty(location))
            {
                return location;
            }

            // Try to get from IP-based location (simplified)
            var clientIp = GetClientIpAddress(context);
            if (!string.IsNullOrEmpty(clientIp) && clientIp != "unknown")
            {
                // Simple location determination based on IP ranges
                if (clientIp.StartsWith("192.168.") || clientIp.StartsWith("10."))
                {
                    return "internal-network";
                }
                else if (clientIp.StartsWith("172."))
                {
                    return "dmz-network";
                }
                else
                {
                    return "external-network";
                }
            }

            return "unknown-location";
        }
    }

    /// <summary>
    /// Extension methods for Zero Trust middleware
    /// </summary>
    public static class ZeroTrustMiddlewareExtensions
    {
        /// <summary>
        /// Adds Zero Trust middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseZeroTrust(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ZeroTrustMiddleware>();
        }
    }
} 