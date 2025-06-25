//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWeb_ApiCore.Interfaces;
using CSETWeb_ApiCore.Models.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Services
{
    /// <summary>
    /// Main caching service implementation providing both Redis and memory caching
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMemoryCache _memoryCache;
        private readonly CachingConfiguration _config;
        private readonly ILogger<CacheService> _logger;
        private readonly CacheStatistics _statistics;

        public CacheService(
            IDistributedCache distributedCache,
            IMemoryCache memoryCache,
            IOptions<CachingConfiguration> config,
            ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _memoryCache = memoryCache;
            _config = config.Value;
            _logger = logger;
            _statistics = new CacheStatistics();
        }

        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                _statistics.TotalRequests++;
                
                // Try Redis first if enabled
                if (_config.Redis.Enabled)
                {
                    var redisKey = $"{_config.Redis.InstanceName}{key}";
                    var cachedValue = await _distributedCache.GetStringAsync(redisKey);
                    
                    if (!string.IsNullOrEmpty(cachedValue))
                    {
                        _statistics.HitCount++;
                        _logger.LogDebug("Cache hit for key: {Key} in Redis", key);
                        return JsonConvert.DeserializeObject<T>(cachedValue);
                    }
                }

                // Fallback to memory cache
                if (_config.Memory.Enabled && _memoryCache.TryGetValue(key, out T memoryValue))
                {
                    _statistics.HitCount++;
                    _logger.LogDebug("Cache hit for key: {Key} in memory", key);
                    return memoryValue;
                }

                _statistics.MissCount++;
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return default(T);
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            return await SetAsync(key, value, _config.Redis.Enabled ? _config.Redis.DefaultExpirationMinutes : _config.Memory.DefaultExpirationMinutes);
        }

        public async Task<bool> SetAsync<T>(string key, T value, int expirationMinutes)
        {
            try
            {
                var success = true;

                // Set in Redis if enabled
                if (_config.Redis.Enabled)
                {
                    var redisKey = $"{_config.Redis.InstanceName}{key}";
                    var serializedValue = JsonConvert.SerializeObject(value);
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(_config.Redis.SlidingExpirationMinutes)
                    };

                    await _distributedCache.SetStringAsync(redisKey, serializedValue, options);
                    _logger.LogDebug("Cached value for key: {Key} in Redis with expiration: {ExpirationMinutes} minutes", key, expirationMinutes);
                }

                // Set in memory cache as backup
                if (_config.Memory.Enabled)
                {
                    var memoryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(_config.Memory.SlidingExpirationMinutes),
                        Size = 1 // Simple size tracking
                    };

                    _memoryCache.Set(key, value, memoryOptions);
                    _logger.LogDebug("Cached value for key: {Key} in memory with expiration: {ExpirationMinutes} minutes", key, expirationMinutes);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetAsync<T>(string key, T value, int slidingExpirationMinutes, int absoluteExpirationMinutes)
        {
            try
            {
                var success = true;

                // Set in Redis if enabled
                if (_config.Redis.Enabled)
                {
                    var redisKey = $"{_config.Redis.InstanceName}{key}";
                    var serializedValue = JsonConvert.SerializeObject(value);
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteExpirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes)
                    };

                    await _distributedCache.SetStringAsync(redisKey, serializedValue, options);
                }

                // Set in memory cache as backup
                if (_config.Memory.Enabled)
                {
                    var memoryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(absoluteExpirationMinutes),
                        SlidingExpiration = TimeSpan.FromMinutes(slidingExpirationMinutes),
                        Size = 1
                    };

                    _memoryCache.Set(key, value, memoryOptions);
                }

                _logger.LogDebug("Cached value for key: {Key} with sliding: {SlidingMinutes}, absolute: {AbsoluteMinutes} minutes", 
                    key, slidingExpirationMinutes, absoluteExpirationMinutes);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string key)
        {
            try
            {
                var success = true;

                // Remove from Redis if enabled
                if (_config.Redis.Enabled)
                {
                    var redisKey = $"{_config.Redis.InstanceName}{key}";
                    await _distributedCache.RemoveAsync(redisKey);
                }

                // Remove from memory cache
                if (_config.Memory.Enabled)
                {
                    _memoryCache.Remove(key);
                }

                _logger.LogDebug("Removed cache entry for key: {Key}", key);
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 60)
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null && !cachedValue.Equals(default(T)))
            {
                return cachedValue;
            }

            var value = await factory();
            await SetAsync(key, value, expirationMinutes);
            return value;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                // Check Redis first if enabled
                if (_config.Redis.Enabled)
                {
                    var redisKey = $"{_config.Redis.InstanceName}{key}";
                    var cachedValue = await _distributedCache.GetStringAsync(redisKey);
                    if (!string.IsNullOrEmpty(cachedValue))
                    {
                        return true;
                    }
                }

                // Check memory cache
                if (_config.Memory.Enabled)
                {
                    return _memoryCache.TryGetValue(key, out _);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<CacheStatistics> GetStatisticsAsync()
        {
            try
            {
                // Update statistics from Redis if available
                if (_config.Redis.Enabled)
                {
                    // Note: Redis statistics would require additional Redis commands
                    // This is a simplified implementation
                }

                _statistics.LastUpdated = DateTime.UtcNow;
                return _statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache statistics");
                return _statistics;
            }
        }

        public async Task<bool> ClearAllAsync()
        {
            try
            {
                var success = true;

                // Clear Redis cache if enabled
                if (_config.Redis.Enabled)
                {
                    // Note: This would require additional Redis commands to clear all keys
                    // For now, we'll just log that this feature needs Redis-specific implementation
                    _logger.LogWarning("Redis cache clear all not implemented - requires Redis-specific commands");
                }

                // Clear memory cache
                if (_config.Memory.Enabled)
                {
                    if (_memoryCache is MemoryCache memoryCache)
                    {
                        memoryCache.Compact(1.0);
                    }
                }

                _logger.LogInformation("Cache cleared");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
                return false;
            }
        }

        public async Task<int> InvalidateByPatternAsync(string pattern)
        {
            try
            {
                var invalidatedCount = 0;

                // Note: Pattern invalidation would require additional Redis commands
                // This is a simplified implementation that logs the request
                _logger.LogInformation("Pattern invalidation requested for pattern: {Pattern}", pattern);

                return invalidatedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
                return 0;
            }
        }
    }
} 