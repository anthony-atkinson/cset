using System.ComponentModel.DataAnnotations;

namespace CSETWebBlazor.Models
{
    /// <summary>
    /// Represents application configuration
    /// </summary>
    public class AppConfig
    {
        public string InstallationMode { get; set; } = "CSET";
        public string PublicDomainName { get; set; } = string.Empty;
        public bool IsRunningAnonymous { get; set; } = false;
        public ApiConfig Api { get; set; } = new ApiConfig();
        public AppConfig App { get; set; } = new AppConfig();
        public string HelpContactEmail { get; set; } = string.Empty;
        public string HelpContactPhone { get; set; } = string.Empty;
        public string CsetGithubApiUrl { get; set; } = string.Empty;
        public string DhsEmail { get; set; } = string.Empty;
        public List<string> CurrentConfigChain { get; set; } = new List<string>();
        public Dictionary<string, object> Behaviors { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, string> SalLabels { get; set; } = new Dictionary<string, string>();
        public string GalleryLayout { get; set; } = "CSET";
        public string MobileEnvironment { get; set; } = string.Empty;
        public bool UserIsCisaAssessor { get; set; } = false;
    }

    /// <summary>
    /// Represents API configuration
    /// </summary>
    public class ApiConfig
    {
        public string Protocol { get; set; } = "https";
        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5000";
        public string ApiIdentifier { get; set; } = "api";
        public string DocumentsIdentifier { get; set; } = "Documents";
        public string LibraryIdentifier { get; set; } = "library";
    }

    /// <summary>
    /// Represents application configuration
    /// </summary>
    public class AppConfig
    {
        public string Protocol { get; set; } = "https";
        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5001";
    }

    /// <summary>
    /// Represents module behavior configuration
    /// </summary>
    public class ModuleBehavior
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents configuration validation result
    /// </summary>
    public class ConfigValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents configuration update request
    /// </summary>
    public class ConfigUpdateRequest
    {
        public string InstallationMode { get; set; } = string.Empty;
        public string PublicDomainName { get; set; } = string.Empty;
        public bool IsRunningAnonymous { get; set; } = false;
        public ApiConfig? Api { get; set; }
        public AppConfig? App { get; set; }
        public string HelpContactEmail { get; set; } = string.Empty;
        public string HelpContactPhone { get; set; } = string.Empty;
        public string CsetGithubApiUrl { get; set; } = string.Empty;
        public string GalleryLayout { get; set; } = string.Empty;
        public bool UserIsCisaAssessor { get; set; } = false;
    }

    /// <summary>
    /// Represents configuration response
    /// </summary>
    public class ConfigResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AppConfig? Config { get; set; }
        public ConfigValidationResult? Validation { get; set; }
    }

    /// <summary>
    /// Represents system status information
    /// </summary>
    public class SystemStatus
    {
        public bool ApiOnline { get; set; }
        public bool DatabaseOnline { get; set; }
        public bool LocalDocumentsAvailable { get; set; }
        public bool OnlineDocumentsAvailable { get; set; }
        public string ApiUrl { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;
        public string DocUrl { get; set; } = string.Empty;
        public string LibraryUrl { get; set; } = string.Empty;
        public string RefDocUrl { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents version information
    /// </summary>
    public class VersionInfo
    {
        public string Version { get; set; } = string.Empty;
        public string BuildNumber { get; set; } = string.Empty;
        public DateTime BuildDate { get; set; }
        public string CommitHash { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents feature configuration
    /// </summary>
    public class FeatureConfig
    {
        public bool ShowImportButton { get; set; } = true;
        public bool ShowExportAllButton { get; set; } = true;
        public bool ShowQuestionAndRequirementIDs { get; set; } = false;
        public bool ShowAssessmentUpgrade { get; set; } = true;
        public bool ShowBuildTime { get; set; } = false;
        public bool EnableCisaAssessorWorkflow { get; set; } = false;
        public bool EnableMobileFeatures { get; set; } = false;
        public bool EnableOfflineMode { get; set; } = false;
        public bool EnableCollaboration { get; set; } = true;
        public bool EnableAnalytics { get; set; } = true;
        public bool EnableReports { get; set; } = true;
        public bool EnableDiagrams { get; set; } = true;
        public bool EnableDocuments { get; set; } = true;
    }

    /// <summary>
    /// Represents security configuration
    /// </summary>
    public class SecurityConfig
    {
        public bool RequireHttps { get; set; } = true;
        public bool EnableCors { get; set; } = true;
        public List<string> AllowedOrigins { get; set; } = new List<string>();
        public bool EnableRateLimiting { get; set; } = true;
        public int RateLimitRequests { get; set; } = 100;
        public int RateLimitWindowMinutes { get; set; } = 15;
        public bool EnableAntiForgery { get; set; } = true;
        public bool EnableXssProtection { get; set; } = true;
        public bool EnableContentSecurityPolicy { get; set; } = true;
        public string ContentSecurityPolicy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents cache configuration
    /// </summary>
    public class CacheConfig
    {
        public bool EnableMemoryCache { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int AssessmentCacheMinutes { get; set; } = 60;
        public int UserCacheMinutes { get; set; } = 120;
        public int ReportCacheMinutes { get; set; } = 15;
        public string DistributedCacheConnectionString { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents logging configuration
    /// </summary>
    public class LoggingConfig
    {
        public string LogLevel { get; set; } = "Information";
        public bool EnableConsoleLogging { get; set; } = true;
        public bool EnableFileLogging { get; set; } = true;
        public string LogFilePath { get; set; } = "logs/cset-{Date}.log";
        public int MaxFileSizeMB { get; set; } = 10;
        public int MaxFileCount { get; set; } = 30;
        public bool EnableStructuredLogging { get; set; } = true;
        public bool EnableRequestLogging { get; set; } = false;
        public bool EnablePerformanceLogging { get; set; } = false;
    }
} 