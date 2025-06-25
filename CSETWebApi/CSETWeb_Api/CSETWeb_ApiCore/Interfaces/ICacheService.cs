//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Threading.Tasks;

namespace CSETWeb_ApiCore.Interfaces
{
    /// <summary>
    /// Main caching service interface providing both Redis and memory caching capabilities
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets a value from cache
        /// </summary>
        /// <typeparam name="T">Type of the cached value</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached value or default if not found</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// Sets a value in cache with default expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <returns>True if successful</returns>
        Task<bool> SetAsync<T>(string key, T value);

        /// <summary>
        /// Sets a value in cache with custom expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="expirationMinutes">Expiration time in minutes</param>
        /// <returns>True if successful</returns>
        Task<bool> SetAsync<T>(string key, T value, int expirationMinutes);

        /// <summary>
        /// Sets a value in cache with sliding expiration
        /// </summary>
        /// <typeparam name="T">Type of the value to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Value to cache</param>
        /// <param name="slidingExpirationMinutes">Sliding expiration time in minutes</param>
        /// <param name="absoluteExpirationMinutes">Absolute expiration time in minutes</param>
        /// <returns>True if successful</returns>
        Task<bool> SetAsync<T>(string key, T value, int slidingExpirationMinutes, int absoluteExpirationMinutes);

        /// <summary>
        /// Removes a value from cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Gets or sets a value in cache (get if exists, set if not)
        /// </summary>
        /// <typeparam name="T">Type of the value</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Factory function to create value if not cached</param>
        /// <param name="expirationMinutes">Expiration time in minutes</param>
        /// <returns>Cached or newly created value</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, int expirationMinutes = 60);

        /// <summary>
        /// Checks if a key exists in cache
        /// </summary>
        /// <param name="key">Cache key</param>
        /// <returns>True if key exists</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        /// <returns>Cache statistics</returns>
        Task<CacheStatistics> GetStatisticsAsync();

        /// <summary>
        /// Clears all cache entries
        /// </summary>
        /// <returns>True if successful</returns>
        Task<bool> ClearAllAsync();

        /// <summary>
        /// Invalidates cache entries by pattern
        /// </summary>
        /// <param name="pattern">Pattern to match keys</param>
        /// <returns>Number of keys invalidated</returns>
        Task<int> InvalidateByPatternAsync(string pattern);
    }

    /// <summary>
    /// Cache statistics
    /// </summary>
    public class CacheStatistics
    {
        public long TotalKeys { get; set; }
        public long MemoryUsage { get; set; }
        public long HitCount { get; set; }
        public long MissCount { get; set; }
        public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0;
        public long TotalRequests => HitCount + MissCount;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
} 