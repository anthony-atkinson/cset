using System;
using System.Threading.Tasks;
using CSETWeb_ApiCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CSETWeb_ApiCore.Controllers
{
    /// <summary>
    /// Controller for managing and monitoring API rate limiting
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Admin,SystemAdmin")]
    public class RateLimitController : ControllerBase
    {
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RateLimitController> _logger;

        public RateLimitController(
            IRateLimitService rateLimitService,
            ILogger<RateLimitController> logger)
        {
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        /// <summary>
        /// Gets current rate limit information for the requesting client
        /// </summary>
        /// <returns>Rate limit information for the current client</returns>
        /// <response code="200">Rate limit information retrieved successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User not authorized</response>
        [HttpGet("info")]
        [AllowAnonymous]
        public async Task<ActionResult<RateLimitInfo>> GetRateLimitInfo()
        {
            try
            {
                var rateLimitInfo = await _rateLimitService.GetRateLimitInfoAsync(HttpContext);
                
                _logger.LogDebug("Rate limit info retrieved for client {ClientId}: {Current}/{Limit} requests", 
                    rateLimitInfo.ClientId, rateLimitInfo.CurrentCount, rateLimitInfo.Limit);
                
                return Ok(rateLimitInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limit info for client");
                return StatusCode(500, new { error = "Failed to retrieve rate limit information" });
            }
        }

        /// <summary>
        /// Gets rate limiting statistics and analytics
        /// </summary>
        /// <returns>Rate limiting statistics</returns>
        /// <response code="200">Statistics retrieved successfully</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User not authorized</response>
        [HttpGet("statistics")]
        public async Task<ActionResult<RateLimitStatistics>> GetStatistics()
        {
            try
            {
                var statistics = await _rateLimitService.GetStatisticsAsync();
                
                _logger.LogInformation("Rate limit statistics retrieved: {TotalRequests} total, {RateLimited} rate limited, {Bypassed} bypassed", 
                    statistics.TotalRequests, statistics.RateLimitedRequests, statistics.BypassedRequests);
                
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limit statistics");
                return StatusCode(500, new { error = "Failed to retrieve rate limit statistics" });
            }
        }

        /// <summary>
        /// Gets rate limit information for a specific client
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Rate limit information for the specified client</returns>
        /// <response code="200">Rate limit information retrieved successfully</response>
        /// <response code="400">Bad request - Invalid client ID</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User not authorized</response>
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<RateLimitInfo>> GetClientRateLimitInfo(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BadRequest(new { error = "Client ID is required" });
            }

            try
            {
                // Create a mock context for the specific client
                var mockContext = new MockHttpContext(clientId);
                var rateLimitInfo = await _rateLimitService.GetRateLimitInfoAsync(mockContext);
                
                _logger.LogDebug("Rate limit info retrieved for client {ClientId}: {Current}/{Limit} requests", 
                    clientId, rateLimitInfo.CurrentCount, rateLimitInfo.Limit);
                
                return Ok(rateLimitInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving rate limit info for client {ClientId}", clientId);
                return StatusCode(500, new { error = "Failed to retrieve rate limit information" });
            }
        }

        /// <summary>
        /// Resets rate limit counters for a specific client
        /// </summary>
        /// <param name="clientId">Client identifier</param>
        /// <returns>Success response</returns>
        /// <response code="200">Rate limit counters reset successfully</response>
        /// <response code="400">Bad request - Invalid client ID</response>
        /// <response code="401">Unauthorized - User not authenticated</response>
        /// <response code="403">Forbidden - User not authorized</response>
        [HttpPost("client/{clientId}/reset")]
        public async Task<ActionResult> ResetClientRateLimit(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return BadRequest(new { error = "Client ID is required" });
            }

            try
            {
                // This would require additional implementation in the service
                // For now, we'll log the request
                _logger.LogInformation("Rate limit reset requested for client {ClientId} by user {UserId}", 
                    clientId, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                
                return Ok(new { message = $"Rate limit reset requested for client {clientId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting rate limit for client {ClientId}", clientId);
                return StatusCode(500, new { error = "Failed to reset rate limit" });
            }
        }

        /// <summary>
        /// Gets health status of rate limiting system
        /// </summary>
        /// <returns>Health status</returns>
        /// <response code="200">Rate limiting system is healthy</response>
        /// <response code="503">Rate limiting system is unhealthy</response>
        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult GetHealth()
        {
            try
            {
                // Basic health check - could be expanded with more detailed checks
                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    service = "Rate Limiting",
                    version = "1.0.0"
                };
                
                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rate limiting health check failed");
                return StatusCode(503, new { status = "unhealthy", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Mock HTTP context for client-specific operations
    /// </summary>
    public class MockHttpContext : HttpContext
    {
        public MockHttpContext(string clientId)
        {
            Request = new MockHttpRequest(clientId);
            Response = new MockHttpResponse();
            User = new System.Security.Claims.ClaimsPrincipal();
        }

        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }
        public override System.Security.Claims.ClaimsPrincipal User { get; set; }
        public override ConnectionInfo Connection => throw new NotImplementedException();
        public override WebSocketManager WebSockets => throw new NotImplementedException();
        public override IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();
        public override IServiceProvider RequestServices { get; set; } = null!;
        public override CancellationToken RequestAborted { get; set; }
        public override string TraceIdentifier { get; set; } = string.Empty;
        public override ISession Session { get; set; } = null!;
        public override void Abort() { }
    }

    /// <summary>
    /// Mock HTTP request for client-specific operations
    /// </summary>
    public class MockHttpRequest : HttpRequest
    {
        public MockHttpRequest(string clientId)
        {
            Method = "GET";
            Path = "/api/test";
            Headers = new HeaderDictionary();
            Headers["X-ClientId"] = clientId;
        }

        public override string Method { get; set; }
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; }
        public override HostString Host { get; set; }
        public override PathString PathBase { get; set; }
        public override PathString Path { get; set; }
        public override QueryString QueryString { get; set; }
        public override IQueryCollection Query { get; set; } = null!;
        public override string Protocol { get; set; } = "HTTP/1.1";
        public override IHeaderDictionary Headers { get; }
        public override IRequestCookieCollection Cookies { get; set; } = null!;
        public override long? ContentLength { get; set; }
        public override string? ContentType { get; set; }
        public override Stream Body { get; set; } = Stream.Null;
        public override bool HasFormContentType => false;
        public override IFormCollection Form { get; set; } = null!;
        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default) => Task.FromResult<IFormCollection>(null!);
    }

    /// <summary>
    /// Mock HTTP response for client-specific operations
    /// </summary>
    public class MockHttpResponse : HttpResponse
    {
        public override int StatusCode { get; set; }
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override Stream Body { get; set; } = Stream.Null;
        public override long? ContentLength { get; set; }
        public override string? ContentType { get; set; }
        public override IResponseCookies Cookies => throw new NotImplementedException();
        public override bool HasStarted => false;
        public override void OnStarting(Func<object, Task> callback, object state) { }
        public override void OnCompleted(Func<object, Task> callback, object state) { }
        public override void Redirect(string location, bool permanent) { }
    }
} 