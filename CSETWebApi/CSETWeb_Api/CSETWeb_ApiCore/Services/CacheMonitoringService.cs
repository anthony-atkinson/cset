//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Models.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Services
{
    /// <summary>
    /// Cache monitoring service for statistics and health monitoring
    /// </summary>
    public class CacheMonitoringService
    {
        private readonly ICacheService _cacheService;
        private readonly CachingConfiguration _config;
        private readonly ILogger<CacheMonitoringService> _logger;
        private readonly Dictionary<string, CacheKeyMetrics> _keyMetrics;

        public CacheMonitoringService(
            ICacheService cacheService,
            IOptions<CachingConfiguration> config,
            ILogger<CacheMonitoringService> logger)
        {
            _cacheService = cacheService;
            _config = config.Value;
            _logger = logger;
            _keyMetrics = new Dictionary<string, CacheKeyMetrics>();
        }

        /// <summary>
        /// Gets comprehensive cache statistics
        /// </summary>
        public async Task<CacheHealthReport> GetCacheHealthReportAsync()
        {
            try
            {
                var statistics = await _cacheService.GetStatisticsAsync();
                var report = new CacheHealthReport
                {
                    Statistics = statistics,
                    Configuration = _config,
                    KeyMetrics = _keyMetrics,
                    HealthStatus = await DetermineHealthStatusAsync(),
                    LastUpdated = DateTime.UtcNow
                };

                _logger.LogDebug("Generated cache health report with {HitRate:P2} hit rate", statistics.HitRate);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating cache health report");
                return new CacheHealthReport
                {
                    HealthStatus = CacheHealthStatus.Error,
                    LastUpdated = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Records cache access for a specific key
        /// </summary>
        public void RecordCacheAccess(string key, bool isHit, TimeSpan responseTime)
        {
            if (!_keyMetrics.ContainsKey(key))
            {
                _keyMetrics[key] = new CacheKeyMetrics { Key = key };
            }

            var metrics = _keyMetrics[key];
            metrics.TotalRequests++;
            metrics.ResponseTimeTotal += responseTime.TotalMilliseconds;
            metrics.AverageResponseTime = metrics.ResponseTimeTotal / metrics.TotalRequests;

            if (isHit)
            {
                metrics.HitCount++;
            }
            else
            {
                metrics.MissCount++;
            }

            metrics.HitRate = metrics.TotalRequests > 0 ? (double)metrics.HitCount / metrics.TotalRequests : 0;
            metrics.LastAccessed = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets top performing cache keys
        /// </summary>
        public List<CacheKeyMetrics> GetTopPerformingKeys(int count = 10)
        {
            var sortedKeys = new List<CacheKeyMetrics>(_keyMetrics.Values);
            sortedKeys.Sort((a, b) => b.HitRate.CompareTo(a.HitRate));
            return sortedKeys.GetRange(0, Math.Min(count, sortedKeys.Count));
        }

        /// <summary>
        /// Gets cache keys with low hit rates
        /// </summary>
        public List<CacheKeyMetrics> GetLowPerformingKeys(double threshold = 0.5, int count = 10)
        {
            var lowPerformingKeys = new List<CacheKeyMetrics>();
            foreach (var kvp in _keyMetrics)
            {
                if (kvp.Value.HitRate < threshold && kvp.Value.TotalRequests > 10)
                {
                    lowPerformingKeys.Add(kvp.Value);
                }
            }

            lowPerformingKeys.Sort((a, b) => a.HitRate.CompareTo(b.HitRate));
            return lowPerformingKeys.GetRange(0, Math.Min(count, lowPerformingKeys.Count));
        }

        /// <summary>
        /// Gets cache performance recommendations
        /// </summary>
        public List<CacheRecommendation> GetRecommendations()
        {
            var recommendations = new List<CacheRecommendation>();
            var statistics = _cacheService.GetStatisticsAsync().Result;

            // Check hit rate
            if (statistics.HitRate < 0.7)
            {
                recommendations.Add(new CacheRecommendation
                {
                    Type = RecommendationType.LowHitRate,
                    Severity = RecommendationSeverity.Warning,
                    Message = $"Cache hit rate is {statistics.HitRate:P2}, consider increasing cache size or adjusting expiration times",
                    SuggestedAction = "Review cache configuration and consider increasing memory cache size or Redis memory limits"
                });
            }

            // Check for low performing keys
            var lowPerformingKeys = GetLowPerformingKeys(0.3, 5);
            if (lowPerformingKeys.Count > 0)
            {
                recommendations.Add(new CacheRecommendation
                {
                    Type = RecommendationType.LowPerformingKeys,
                    Severity = RecommendationSeverity.Info,
                    Message = $"Found {lowPerformingKeys.Count} cache keys with hit rates below 30%",
                    SuggestedAction = "Consider removing or adjusting expiration for frequently missed keys",
                    AffectedKeys = lowPerformingKeys.ConvertAll(k => k.Key)
                });
            }

            // Check Redis connectivity if enabled
            if (_config.Redis.Enabled)
            {
                try
                {
                    var testKey = "health-check:" + Guid.NewGuid();
                    _cacheService.SetAsync(testKey, "test", 1).Wait();
                    var result = _cacheService.GetAsync<string>(testKey).Result;
                    _cacheService.RemoveAsync(testKey).Wait();

                    if (result != "test")
                    {
                        recommendations.Add(new CacheRecommendation
                        {
                            Type = RecommendationType.RedisConnectivity,
                            Severity = RecommendationSeverity.Error,
                            Message = "Redis connectivity issues detected",
                            SuggestedAction = "Check Redis server status and connection configuration"
                        });
                    }
                }
                catch (Exception ex)
                {
                    recommendations.Add(new CacheRecommendation
                    {
                        Type = RecommendationType.RedisConnectivity,
                        Severity = RecommendationSeverity.Error,
                        Message = $"Redis connectivity error: {ex.Message}",
                        SuggestedAction = "Check Redis server status and connection configuration"
                    });
                }
            }

            return recommendations;
        }

        /// <summary>
        /// Determines overall cache health status
        /// </summary>
        private async Task<CacheHealthStatus> DetermineHealthStatusAsync()
        {
            try
            {
                var statistics = await _cacheService.GetStatisticsAsync();
                
                if (statistics.HitRate >= 0.8)
                    return CacheHealthStatus.Excellent;
                else if (statistics.HitRate >= 0.6)
                    return CacheHealthStatus.Good;
                else if (statistics.HitRate >= 0.4)
                    return CacheHealthStatus.Fair;
                else
                    return CacheHealthStatus.Poor;
            }
            catch
            {
                return CacheHealthStatus.Error;
            }
        }

        /// <summary>
        /// Clears monitoring data
        /// </summary>
        public void ClearMonitoringData()
        {
            _keyMetrics.Clear();
            _logger.LogInformation("Cleared cache monitoring data");
        }
    }

    /// <summary>
    /// Cache health status
    /// </summary>
    public enum CacheHealthStatus
    {
        Excellent,
        Good,
        Fair,
        Poor,
        Error
    }

    /// <summary>
    /// Cache health report
    /// </summary>
    public class CacheHealthReport
    {
        public CacheStatistics Statistics { get; set; }
        public CachingConfiguration Configuration { get; set; }
        public Dictionary<string, CacheKeyMetrics> KeyMetrics { get; set; }
        public CacheHealthStatus HealthStatus { get; set; }
        public DateTime LastUpdated { get; set; }
        public string ErrorMessage { get; set; }
        public List<CacheRecommendation> Recommendations { get; set; } = new List<CacheRecommendation>();
    }

    /// <summary>
    /// Cache key metrics
    /// </summary>
    public class CacheKeyMetrics
    {
        public string Key { get; set; }
        public long TotalRequests { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRate { get; set; }
        public double ResponseTimeTotal { get; set; }
        public double AverageResponseTime { get; set; }
        public DateTime LastAccessed { get; set; }
    }

    /// <summary>
    /// Cache recommendation
    /// </summary>
    public class CacheRecommendation
    {
        public RecommendationType Type { get; set; }
        public RecommendationSeverity Severity { get; set; }
        public string Message { get; set; }
        public string SuggestedAction { get; set; }
        public List<string> AffectedKeys { get; set; } = new List<string>();
    }

    /// <summary>
    /// Recommendation types
    /// </summary>
    public enum RecommendationType
    {
        LowHitRate,
        LowPerformingKeys,
        RedisConnectivity,
        MemoryUsage,
        ExpirationOptimization
    }

    /// <summary>
    /// Recommendation severity
    /// </summary>
    public enum RecommendationSeverity
    {
        Info,
        Warning,
        Error
    }
} 