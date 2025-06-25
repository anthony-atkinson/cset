using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CSETWeb_ApiCore.Interfaces
{
    /// <summary>
    /// Service for managing API rate limiting
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// Checks if the request should be rate limited
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if request should be rate limited</returns>
        Task<bool> ShouldRateLimitAsync(HttpContext context);

        /// <summary>
        /// Gets the current rate limit information for a client
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Rate limit information</returns>
        Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context);

        /// <summary>
        /// Increments the request count for a client
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>Updated rate limit information</returns>
        Task<RateLimitInfo> IncrementRequestCountAsync(HttpContext context);

        /// <summary>
        /// Checks if the request should bypass rate limiting (admin users)
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <returns>True if request should bypass rate limiting</returns>
        Task<bool> ShouldBypassRateLimitAsync(HttpContext context);

        /// <summary>
        /// Gets rate limiting statistics
        /// </summary>
        /// <returns>Rate limiting statistics</returns>
        Task<RateLimitStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Rate limit information for a client
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        /// Client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Current request count
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Maximum allowed requests
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Time period in seconds
        /// </summary>
        public int PeriodSeconds { get; set; }

        /// <summary>
        /// Time remaining until reset in seconds
        /// </summary>
        public int RemainingSeconds { get; set; }

        /// <summary>
        /// Whether the client has exceeded the limit
        /// </summary>
        public bool IsExceeded => CurrentCount >= Limit;

        /// <summary>
        /// Remaining requests allowed
        /// </summary>
        public int RemainingRequests => Math.Max(0, Limit - CurrentCount);
    }

    /// <summary>
    /// Rate limiting statistics
    /// </summary>
    public class RateLimitStatistics
    {
        /// <summary>
        /// Total requests processed
        /// </summary>
        public long TotalRequests { get; set; }

        /// <summary>
        /// Total requests rate limited
        /// </summary>
        public long RateLimitedRequests { get; set; }

        /// <summary>
        /// Total requests bypassed (admin users)
        /// </summary>
        public long BypassedRequests { get; set; }

        /// <summary>
        /// Rate limiting percentage
        /// </summary>
        public double RateLimitPercentage => TotalRequests > 0 ? (double)RateLimitedRequests / TotalRequests * 100 : 0;

        /// <summary>
        /// Bypass percentage
        /// </summary>
        public double BypassPercentage => TotalRequests > 0 ? (double)BypassedRequests / TotalRequests * 100 : 0;

        /// <summary>
        /// Top rate limited clients
        /// </summary>
        public List<ClientRateLimitStats> TopRateLimitedClients { get; set; } = new List<ClientRateLimitStats>();

        /// <summary>
        /// Top rate limited endpoints
        /// </summary>
        public List<EndpointRateLimitStats> TopRateLimitedEndpoints { get; set; } = new List<EndpointRateLimitStats>();
    }

    /// <summary>
    /// Client rate limiting statistics
    /// </summary>
    public class ClientRateLimitStats
    {
        /// <summary>
        /// Client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Number of times rate limited
        /// </summary>
        public int RateLimitedCount { get; set; }

        /// <summary>
        /// Total requests
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Rate limiting percentage for this client
        /// </summary>
        public double RateLimitPercentage => TotalRequests > 0 ? (double)RateLimitedCount / TotalRequests * 100 : 0;
    }

    /// <summary>
    /// Endpoint rate limiting statistics
    /// </summary>
    public class EndpointRateLimitStats
    {
        /// <summary>
        /// Endpoint path
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// HTTP method
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Number of times rate limited
        /// </summary>
        public int RateLimitedCount { get; set; }

        /// <summary>
        /// Total requests
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Rate limiting percentage for this endpoint
        /// </summary>
        public double RateLimitPercentage => TotalRequests > 0 ? (double)RateLimitedCount / TotalRequests * 100 : 0;
    }
} 