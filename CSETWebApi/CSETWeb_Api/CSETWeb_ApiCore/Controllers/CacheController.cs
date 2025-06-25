//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Controllers
{
    /// <summary>
    /// API controller for cache management and monitoring
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly CacheMonitoringService _monitoringService;
        private readonly ILogger<CacheController> _logger;

        public CacheController(
            ICacheService cacheService,
            CacheMonitoringService monitoringService,
            ILogger<CacheController> logger)
        {
            _cacheService = cacheService;
            _monitoringService = monitoringService;
            _logger = logger;
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(CacheStatistics), 200)]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var statistics = await _cacheService.GetStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return StatusCode(500, new { error = "Failed to retrieve cache statistics" });
            }
        }

        /// <summary>
        /// Gets comprehensive cache health report
        /// </summary>
        /// <returns>Cache health report</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(CacheHealthReport), 200)]
        public async Task<IActionResult> GetHealthReport()
        {
            try
            {
                var report = await _monitoringService.GetCacheHealthReportAsync();
                report.Recommendations = _monitoringService.GetRecommendations();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache health report");
                return StatusCode(500, new { error = "Failed to retrieve cache health report" });
            }
        }

        /// <summary>
        /// Gets top performing cache keys
        /// </summary>
        /// <param name="count">Number of keys to return</param>
        /// <returns>Top performing cache keys</returns>
        [HttpGet("top-keys")]
        [ProducesResponseType(typeof(CacheKeyMetrics[]), 200)]
        public IActionResult GetTopPerformingKeys([FromQuery] int count = 10)
        {
            try
            {
                var topKeys = _monitoringService.GetTopPerformingKeys(count);
                return Ok(topKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top performing cache keys");
                return StatusCode(500, new { error = "Failed to retrieve top performing cache keys" });
            }
        }

        /// <summary>
        /// Gets low performing cache keys
        /// </summary>
        /// <param name="threshold">Hit rate threshold</param>
        /// <param name="count">Number of keys to return</param>
        /// <returns>Low performing cache keys</returns>
        [HttpGet("low-performing-keys")]
        [ProducesResponseType(typeof(CacheKeyMetrics[]), 200)]
        public IActionResult GetLowPerformingKeys([FromQuery] double threshold = 0.5, [FromQuery] int count = 10)
        {
            try
            {
                var lowKeys = _monitoringService.GetLowPerformingKeys(threshold, count);
                return Ok(lowKeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low performing cache keys");
                return StatusCode(500, new { error = "Failed to retrieve low performing cache keys" });
            }
        }

        /// <summary>
        /// Gets cache recommendations
        /// </summary>
        /// <returns>Cache recommendations</returns>
        [HttpGet("recommendations")]
        [ProducesResponseType(typeof(CacheRecommendation[]), 200)]
        public IActionResult GetRecommendations()
        {
            try
            {
                var recommendations = _monitoringService.GetRecommendations();
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache recommendations");
                return StatusCode(500, new { error = "Failed to retrieve cache recommendations" });
            }
        }

        /// <summary>
        /// Removes a specific cache entry
        /// </summary>
        /// <param name="key">Cache key to remove</param>
        /// <returns>Success status</returns>
        [HttpDelete("key/{key}")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> RemoveKey(string key)
        {
            try
            {
                var success = await _cacheService.RemoveAsync(key);
                if (success)
                {
                    _logger.LogInformation("Removed cache key: {Key}", key);
                    return Ok(new { success = true, message = $"Cache key '{key}' removed successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = $"Cache key '{key}' not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
                return StatusCode(500, new { error = "Failed to remove cache key" });
            }
        }

        /// <summary>
        /// Clears all cache entries
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete("clear-all")]
        [ProducesResponseType(typeof(bool), 200)]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                var success = await _cacheService.ClearAllAsync();
                if (success)
                {
                    _logger.LogInformation("Cleared all cache entries");
                    return Ok(new { success = true, message = "All cache entries cleared successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to clear cache entries" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing all cache entries");
                return StatusCode(500, new { error = "Failed to clear cache entries" });
            }
        }

        /// <summary>
        /// Invalidates cache entries by pattern
        /// </summary>
        /// <param name="pattern">Pattern to match keys</param>
        /// <returns>Number of keys invalidated</returns>
        [HttpDelete("invalidate-pattern")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<IActionResult> InvalidateByPattern([FromQuery] string pattern)
        {
            try
            {
                if (string.IsNullOrEmpty(pattern))
                {
                    return BadRequest(new { error = "Pattern parameter is required" });
                }

                var invalidatedCount = await _cacheService.InvalidateByPatternAsync(pattern);
                _logger.LogInformation("Invalidated {Count} cache keys matching pattern: {Pattern}", invalidatedCount, pattern);
                
                return Ok(new { 
                    success = true, 
                    invalidatedCount = invalidatedCount, 
                    pattern = pattern,
                    message = $"Invalidated {invalidatedCount} cache keys matching pattern '{pattern}'"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
                return StatusCode(500, new { error = "Failed to invalidate cache by pattern" });
            }
        }

        /// <summary>
        /// Tests cache connectivity
        /// </summary>
        /// <returns>Connectivity test result</returns>
        [HttpGet("test-connectivity")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> TestConnectivity()
        {
            try
            {
                var testKey = "connectivity-test:" + Guid.NewGuid();
                var testValue = "test-value-" + DateTime.UtcNow.Ticks;
                
                // Test set
                var setSuccess = await _cacheService.SetAsync(testKey, testValue, 1);
                if (!setSuccess)
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Failed to set test value in cache",
                        error = "Cache write operation failed"
                    });
                }

                // Test get
                var retrievedValue = await _cacheService.GetAsync<string>(testKey);
                if (retrievedValue != testValue)
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Failed to retrieve test value from cache",
                        error = "Cache read operation failed"
                    });
                }

                // Test remove
                var removeSuccess = await _cacheService.RemoveAsync(testKey);
                if (!removeSuccess)
                {
                    return StatusCode(500, new { 
                        success = false, 
                        message = "Failed to remove test value from cache",
                        error = "Cache remove operation failed"
                    });
                }

                return Ok(new { 
                    success = true, 
                    message = "Cache connectivity test passed",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache connectivity test failed");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Cache connectivity test failed",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Clears monitoring data
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete("clear-monitoring")]
        [ProducesResponseType(typeof(bool), 200)]
        public IActionResult ClearMonitoringData()
        {
            try
            {
                _monitoringService.ClearMonitoringData();
                return Ok(new { success = true, message = "Monitoring data cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing monitoring data");
                return StatusCode(500, new { error = "Failed to clear monitoring data" });
            }
        }
    }
} 