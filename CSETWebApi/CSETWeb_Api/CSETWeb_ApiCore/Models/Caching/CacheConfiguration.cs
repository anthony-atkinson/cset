//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System.Collections.Generic;

namespace CSETWeb_ApiCore.Models.Caching
{
    /// <summary>
    /// Configuration for Redis caching
    /// </summary>
    public class RedisCacheConfiguration
    {
        public bool Enabled { get; set; } = true;
        public string InstanceName { get; set; } = "CSET:";
        public int DefaultExpirationMinutes { get; set; } = 60;
        public int SlidingExpirationMinutes { get; set; } = 30;
        public int AbsoluteExpirationMinutes { get; set; } = 1440;
    }

    /// <summary>
    /// Configuration for memory caching
    /// </summary>
    public class MemoryCacheConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int SizeLimit { get; set; } = 1024;
        public int DefaultExpirationMinutes { get; set; } = 15;
        public int SlidingExpirationMinutes { get; set; } = 10;
    }

    /// <summary>
    /// Cache key templates for different data types
    /// </summary>
    public class CacheKeys
    {
        public string Standards { get; set; } = "standards:{0}";
        public string Questions { get; set; } = "questions:{0}";
        public string Assessments { get; set; } = "assessments:{0}";
        public string Users { get; set; } = "users:{0}";
        public string Reports { get; set; } = "reports:{0}";
        public string Frameworks { get; set; } = "frameworks:{0}";
        public string MaturityModels { get; set; } = "maturity:{0}";
        public string QuestionHeadings { get; set; } = "headings:{0}";
        public string UserPreferences { get; set; } = "preferences:{0}";
        public string ReportTemplates { get; set; } = "templates:{0}";
    }

    /// <summary>
    /// Overall caching configuration
    /// </summary>
    public class CachingConfiguration
    {
        public RedisCacheConfiguration Redis { get; set; } = new RedisCacheConfiguration();
        public MemoryCacheConfiguration Memory { get; set; } = new MemoryCacheConfiguration();
        public CacheKeys CacheKeys { get; set; } = new CacheKeys();
    }
} 