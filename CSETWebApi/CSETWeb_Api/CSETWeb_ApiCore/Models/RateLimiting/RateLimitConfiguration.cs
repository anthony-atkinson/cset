using System.Collections.Generic;

namespace CSETWeb_ApiCore.Models.RateLimiting
{
    /// <summary>
    /// Configuration for API rate limiting
    /// </summary>
    public class RateLimitConfiguration
    {
        /// <summary>
        /// Whether rate limiting is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// General rate limit settings
        /// </summary>
        public GeneralRateLimit General { get; set; } = new GeneralRateLimit();

        /// <summary>
        /// Client-specific rate limit settings
        /// </summary>
        public ClientRateLimit Client { get; set; } = new ClientRateLimit();

        /// <summary>
        /// Endpoint-specific rate limit settings
        /// </summary>
        public EndpointRateLimit Endpoints { get; set; } = new EndpointRateLimit();

        /// <summary>
        /// Admin bypass settings
        /// </summary>
        public AdminBypass AdminBypass { get; set; } = new AdminBypass();
    }

    /// <summary>
    /// General rate limiting settings
    /// </summary>
    public class GeneralRateLimit
    {
        /// <summary>
        /// Default requests per period
        /// </summary>
        public int DefaultRequestsPerPeriod { get; set; } = 1000;

        /// <summary>
        /// Default period in seconds
        /// </summary>
        public int DefaultPeriodSeconds { get; set; } = 3600; // 1 hour

        /// <summary>
        /// Whether to enable rate limiting for all endpoints
        /// </summary>
        public bool EnableForAllEndpoints { get; set; } = true;

        /// <summary>
        /// Excluded endpoints that should not be rate limited
        /// </summary>
        public List<string> ExcludedEndpoints { get; set; } = new List<string>
        {
            "GET:/api-docs",
            "GET:/api-docs/*",
            "GET:/health",
            "GET:/health/*"
        };
    }

    /// <summary>
    /// Client-specific rate limiting settings
    /// </summary>
    public class ClientRateLimit
    {
        /// <summary>
        /// Whether client-based rate limiting is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Client identifier header (default: X-ClientId)
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-ClientId";

        /// <summary>
        /// Whether to use IP address as client identifier
        /// </summary>
        public bool UseIpAddress { get; set; } = true;

        /// <summary>
        /// Whether to use user ID as client identifier
        /// </summary>
        public bool UseUserId { get; set; } = true;

        /// <summary>
        /// Client-specific limits
        /// </summary>
        public Dictionary<string, ClientLimit> ClientLimits { get; set; } = new Dictionary<string, ClientLimit>
        {
            ["default"] = new ClientLimit { RequestsPerPeriod = 1000, PeriodSeconds = 3600 },
            ["admin"] = new ClientLimit { RequestsPerPeriod = 5000, PeriodSeconds = 3600 },
            ["api"] = new ClientLimit { RequestsPerPeriod = 2000, PeriodSeconds = 3600 }
        };
    }

    /// <summary>
    /// Client-specific limit configuration
    /// </summary>
    public class ClientLimit
    {
        /// <summary>
        /// Number of requests allowed per period
        /// </summary>
        public int RequestsPerPeriod { get; set; } = 1000;

        /// <summary>
        /// Period in seconds
        /// </summary>
        public int PeriodSeconds { get; set; } = 3600;
    }

    /// <summary>
    /// Endpoint-specific rate limiting settings
    /// </summary>
    public class EndpointRateLimit
    {
        /// <summary>
        /// Whether endpoint-specific rate limiting is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Endpoint-specific limits
        /// </summary>
        public Dictionary<string, EndpointLimit> EndpointLimits { get; set; } = new Dictionary<string, EndpointLimit>
        {
            // Assessment endpoints
            ["POST:/api/assessment"] = new EndpointLimit { RequestsPerPeriod = 100, PeriodSeconds = 3600 },
            ["PUT:/api/assessment/*"] = new EndpointLimit { RequestsPerPeriod = 200, PeriodSeconds = 3600 },
            ["GET:/api/assessment/*"] = new EndpointLimit { RequestsPerPeriod = 500, PeriodSeconds = 3600 },
            
            // Question endpoints
            ["POST:/api/question"] = new EndpointLimit { RequestsPerPeriod = 300, PeriodSeconds = 3600 },
            ["PUT:/api/question/*"] = new EndpointLimit { RequestsPerPeriod = 300, PeriodSeconds = 3600 },
            ["GET:/api/question/*"] = new EndpointLimit { RequestsPerPeriod = 1000, PeriodSeconds = 3600 },
            
            // Report endpoints
            ["POST:/api/report"] = new EndpointLimit { RequestsPerPeriod = 50, PeriodSeconds = 3600 },
            ["GET:/api/report/*"] = new EndpointLimit { RequestsPerPeriod = 200, PeriodSeconds = 3600 },
            
            // Document endpoints
            ["POST:/api/document"] = new EndpointLimit { RequestsPerPeriod = 50, PeriodSeconds = 3600 },
            ["GET:/api/document/*"] = new EndpointLimit { RequestsPerPeriod = 300, PeriodSeconds = 3600 },
            
            // Authentication endpoints
            ["POST:/api/auth/login"] = new EndpointLimit { RequestsPerPeriod = 10, PeriodSeconds = 300 }, // 5 minutes
            ["POST:/api/auth/register"] = new EndpointLimit { RequestsPerPeriod = 5, PeriodSeconds = 3600 }, // 1 hour
            
            // Export/Import endpoints
            ["POST:/api/export"] = new EndpointLimit { RequestsPerPeriod = 20, PeriodSeconds = 3600 },
            ["POST:/api/import"] = new EndpointLimit { RequestsPerPeriod = 20, PeriodSeconds = 3600 }
        };
    }

    /// <summary>
    /// Endpoint-specific limit configuration
    /// </summary>
    public class EndpointLimit
    {
        /// <summary>
        /// Number of requests allowed per period
        /// </summary>
        public int RequestsPerPeriod { get; set; } = 100;

        /// <summary>
        /// Period in seconds
        /// </summary>
        public int PeriodSeconds { get; set; } = 3600;
    }

    /// <summary>
    /// Admin bypass settings
    /// </summary>
    public class AdminBypass
    {
        /// <summary>
        /// Whether admin bypass is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Admin role names that can bypass rate limiting
        /// </summary>
        public List<string> AdminRoles { get; set; } = new List<string>
        {
            "Administrator",
            "Admin",
            "SystemAdmin"
        };

        /// <summary>
        /// Admin user IDs that can bypass rate limiting
        /// </summary>
        public List<string> AdminUserIds { get; set; } = new List<string>();

        /// <summary>
        /// Admin IP addresses that can bypass rate limiting
        /// </summary>
        public List<string> AdminIpAddresses { get; set; } = new List<string>
        {
            "127.0.0.1",
            "::1"
        };
    }
} 