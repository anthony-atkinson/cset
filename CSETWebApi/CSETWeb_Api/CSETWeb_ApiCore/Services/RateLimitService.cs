using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Models.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CSETWeb_ApiCore.Services
{
    /// <summary>
    /// Service for managing API rate limiting
    /// </summary>
    public class RateLimitService : IRateLimitService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RateLimitService> _logger;
        private readonly RateLimitConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IIpPolicyStore _ipPolicyStore;
        private readonly IClientPolicyStore _clientPolicyStore;

        // Statistics tracking
        private static readonly object _statsLock = new object();
        private static long _totalRequests = 0;
        private static long _rateLimitedRequests = 0;
        private static long _bypassedRequests = 0;
        private static readonly Dictionary<string, ClientRateLimitStats> _clientStats = new Dictionary<string, ClientRateLimitStats>();
        private static readonly Dictionary<string, EndpointRateLimitStats> _endpointStats = new Dictionary<string, EndpointRateLimitStats>();

        public RateLimitService(
            IDistributedCache cache,
            ILogger<RateLimitService> logger,
            IOptions<RateLimitConfiguration> config,
            IHttpContextAccessor httpContextAccessor,
            IIpPolicyStore ipPolicyStore,
            IClientPolicyStore clientPolicyStore)
        {
            _cache = cache;
            _logger = logger;
            _config = config.Value;
            _httpContextAccessor = httpContextAccessor;
            _ipPolicyStore = ipPolicyStore;
            _clientPolicyStore = clientPolicyStore;
        }

        /// <summary>
        /// Checks if the request should be rate limited
        /// </summary>
        public async Task<bool> ShouldRateLimitAsync(HttpContext context)
        {
            if (!_config.Enabled)
                return false;

            // Check if request should bypass rate limiting
            if (await ShouldBypassRateLimitAsync(context))
            {
                IncrementBypassStats(context);
                return false;
            }

            // Check if endpoint is excluded
            if (IsEndpointExcluded(context))
                return false;

            var clientId = await GetClientIdAsync(context);
            var endpoint = GetEndpointKey(context);
            
            // Get rate limit info
            var rateLimitInfo = await GetRateLimitInfoAsync(context);
            
            // Check if limit is exceeded
            if (rateLimitInfo.IsExceeded)
            {
                IncrementRateLimitStats(context, clientId, endpoint);
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Current: {Current}, Limit: {Limit}", 
                    clientId, endpoint, rateLimitInfo.CurrentCount, rateLimitInfo.Limit);
                return true;
            }

            // Increment request count
            await IncrementRequestCountAsync(context);
            IncrementTotalStats(context, clientId, endpoint);
            
            return false;
        }

        /// <summary>
        /// Gets the current rate limit information for a client
        /// </summary>
        public async Task<RateLimitInfo> GetRateLimitInfoAsync(HttpContext context)
        {
            var clientId = await GetClientIdAsync(context);
            var endpoint = GetEndpointKey(context);
            
            // Get limits for this client and endpoint
            var (limit, period) = GetLimits(clientId, endpoint);
            
            // Get current count from cache
            var cacheKey = $"rate_limit:{clientId}:{endpoint}";
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            
            var currentCount = 0;
            var resetTime = DateTime.UtcNow.AddSeconds(period);
            
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var cachedData = JsonConvert.DeserializeObject<CachedRateLimitData>(cachedValue);
                if (cachedData.ResetTime > DateTime.UtcNow)
                {
                    currentCount = cachedData.Count;
                    resetTime = cachedData.ResetTime;
                }
            }

            var remainingSeconds = (int)(resetTime - DateTime.UtcNow).TotalSeconds;

            return new RateLimitInfo
            {
                ClientId = clientId,
                CurrentCount = currentCount,
                Limit = limit,
                PeriodSeconds = period,
                RemainingSeconds = Math.Max(0, remainingSeconds)
            };
        }

        /// <summary>
        /// Increments the request count for a client
        /// </summary>
        public async Task<RateLimitInfo> IncrementRequestCountAsync(HttpContext context)
        {
            var clientId = await GetClientIdAsync(context);
            var endpoint = GetEndpointKey(context);
            
            var (limit, period) = GetLimits(clientId, endpoint);
            var cacheKey = $"rate_limit:{clientId}:{endpoint}";
            
            // Get current cached data
            var cachedValue = await _cache.GetStringAsync(cacheKey);
            var currentCount = 0;
            var resetTime = DateTime.UtcNow.AddSeconds(period);
            
            if (!string.IsNullOrEmpty(cachedValue))
            {
                var cachedData = JsonConvert.DeserializeObject<CachedRateLimitData>(cachedValue);
                if (cachedData.ResetTime > DateTime.UtcNow)
                {
                    currentCount = cachedData.Count;
                    resetTime = cachedData.ResetTime;
                }
            }

            // Increment count
            currentCount++;
            
            // Cache updated data
            var newCachedData = new CachedRateLimitData
            {
                Count = currentCount,
                ResetTime = resetTime
            };
            
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(period)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(newCachedData), options);

            var remainingSeconds = (int)(resetTime - DateTime.UtcNow).TotalSeconds;

            return new RateLimitInfo
            {
                ClientId = clientId,
                CurrentCount = currentCount,
                Limit = limit,
                PeriodSeconds = period,
                RemainingSeconds = Math.Max(0, remainingSeconds)
            };
        }

        /// <summary>
        /// Checks if the request should bypass rate limiting (admin users)
        /// </summary>
        public async Task<bool> ShouldBypassRateLimitAsync(HttpContext context)
        {
            if (!_config.AdminBypass.Enabled)
                return false;

            // Check IP address bypass
            var clientIp = GetClientIpAddress(context);
            if (_config.AdminBypass.AdminIpAddresses.Contains(clientIp))
                return true;

            // Check user authentication and roles
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId) && _config.AdminBypass.AdminUserIds.Contains(userId))
                    return true;

                var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
                if (userRoles.Any(role => _config.AdminBypass.AdminRoles.Contains(role)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets rate limiting statistics
        /// </summary>
        public async Task<RateLimitStatistics> GetStatisticsAsync()
        {
            lock (_statsLock)
            {
                return new RateLimitStatistics
                {
                    TotalRequests = _totalRequests,
                    RateLimitedRequests = _rateLimitedRequests,
                    BypassedRequests = _bypassedRequests,
                    TopRateLimitedClients = _clientStats.Values
                        .OrderByDescending(s => s.RateLimitedCount)
                        .Take(10)
                        .ToList(),
                    TopRateLimitedEndpoints = _endpointStats.Values
                        .OrderByDescending(s => s.RateLimitedCount)
                        .Take(10)
                        .ToList()
                };
            }
        }

        /// <summary>
        /// Gets client identifier for rate limiting
        /// </summary>
        private async Task<string> GetClientIdAsync(HttpContext context)
        {
            var identifiers = new List<string>();

            // Add client ID from header if configured
            if (_config.Client.Enabled && !string.IsNullOrEmpty(_config.Client.ClientIdHeader))
            {
                var clientId = context.Request.Headers[_config.Client.ClientIdHeader].FirstOrDefault();
                if (!string.IsNullOrEmpty(clientId))
                    identifiers.Add($"client:{clientId}");
            }

            // Add IP address if configured
            if (_config.Client.UseIpAddress)
            {
                var ip = GetClientIpAddress(context);
                identifiers.Add($"ip:{ip}");
            }

            // Add user ID if configured and authenticated
            if (_config.Client.UseUserId && context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                    identifiers.Add($"user:{userId}");
            }

            // If no identifiers found, use IP as fallback
            if (!identifiers.Any())
            {
                identifiers.Add($"ip:{GetClientIpAddress(context)}");
            }

            return string.Join("|", identifiers);
        }

        /// <summary>
        /// Gets endpoint key for rate limiting
        /// </summary>
        private string GetEndpointKey(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value?.ToLowerInvariant();
            
            // Remove query string for rate limiting purposes
            if (path?.Contains('?') == true)
                path = path.Substring(0, path.IndexOf('?'));
            
            return $"{method}:{path}";
        }

        /// <summary>
        /// Gets client IP address
        /// </summary>
        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return ips[0].Trim();
            }

            var forwarded = context.Request.Headers["X-Forwarded"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                var ips = forwarded.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return ips[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
                return realIp;

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Gets rate limits for client and endpoint
        /// </summary>
        private (int limit, int period) GetLimits(string clientId, string endpoint)
        {
            // Check for endpoint-specific limits first
            if (_config.Endpoints.Enabled && _config.Endpoints.EndpointLimits.TryGetValue(endpoint, out var endpointLimit))
            {
                return (endpointLimit.RequestsPerPeriod, endpointLimit.PeriodSeconds);
            }

            // Check for client-specific limits
            if (_config.Client.Enabled)
            {
                foreach (var clientLimit in _config.Client.ClientLimits)
                {
                    if (clientId.Contains(clientLimit.Key))
                    {
                        return (clientLimit.Value.RequestsPerPeriod, clientLimit.Value.PeriodSeconds);
                    }
                }
            }

            // Return default limits
            return (_config.General.DefaultRequestsPerPeriod, _config.General.DefaultPeriodSeconds);
        }

        /// <summary>
        /// Checks if endpoint is excluded from rate limiting
        /// </summary>
        private bool IsEndpointExcluded(HttpContext context)
        {
            var endpoint = GetEndpointKey(context);
            return _config.General.ExcludedEndpoints.Any(excluded => 
                endpoint.Equals(excluded, StringComparison.OrdinalIgnoreCase) ||
                excluded.EndsWith("/*") && endpoint.StartsWith(excluded.Substring(0, excluded.Length - 2), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Increments total request statistics
        /// </summary>
        private void IncrementTotalStats(HttpContext context, string clientId, string endpoint)
        {
            lock (_statsLock)
            {
                _totalRequests++;
                
                // Update client stats
                if (!_clientStats.ContainsKey(clientId))
                    _clientStats[clientId] = new ClientRateLimitStats { ClientId = clientId };
                _clientStats[clientId].TotalRequests++;
                
                // Update endpoint stats
                if (!_endpointStats.ContainsKey(endpoint))
                    _endpointStats[endpoint] = new EndpointRateLimitStats { Endpoint = endpoint, Method = context.Request.Method };
                _endpointStats[endpoint].TotalRequests++;
            }
        }

        /// <summary>
        /// Increments rate limit statistics
        /// </summary>
        private void IncrementRateLimitStats(HttpContext context, string clientId, string endpoint)
        {
            lock (_statsLock)
            {
                _rateLimitedRequests++;
                
                // Update client stats
                if (!_clientStats.ContainsKey(clientId))
                    _clientStats[clientId] = new ClientRateLimitStats { ClientId = clientId };
                _clientStats[clientId].RateLimitedCount++;
                
                // Update endpoint stats
                if (!_endpointStats.ContainsKey(endpoint))
                    _endpointStats[endpoint] = new EndpointRateLimitStats { Endpoint = endpoint, Method = context.Request.Method };
                _endpointStats[endpoint].RateLimitedCount++;
            }
        }

        /// <summary>
        /// Increments bypass statistics
        /// </summary>
        private void IncrementBypassStats(HttpContext context)
        {
            lock (_statsLock)
            {
                _bypassedRequests++;
            }
        }

        /// <summary>
        /// Cached rate limit data structure
        /// </summary>
        private class CachedRateLimitData
        {
            public int Count { get; set; }
            public DateTime ResetTime { get; set; }
        }
    }
} 