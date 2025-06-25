using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for zero trust analytics provider
    /// Handles analytics, metrics, and reporting for zero trust operations
    /// </summary>
    public interface IZeroTrustAnalyticsProvider
    {
        /// <summary>
        /// Gets zero trust analytics for a time range
        /// </summary>
        /// <param name="timeRange">Time range for analytics</param>
        /// <returns>Analytics data</returns>
        Task<ZeroTrustAnalytics> GetAnalyticsAsync(TimeRange timeRange);

        /// <summary>
        /// Gets user access patterns
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="timeRange">Time range for analysis</param>
        /// <returns>User access patterns</returns>
        Task<UserAccessPatterns> GetUserAccessPatternsAsync(string userId, TimeRange timeRange);

        /// <summary>
        /// Gets resource access analytics
        /// </summary>
        /// <param name="resourceId">Resource identifier</param>
        /// <param name="timeRange">Time range for analysis</param>
        /// <returns>Resource access analytics</returns>
        Task<ResourceAccessAnalytics> GetResourceAccessAnalyticsAsync(string resourceId, TimeRange timeRange);

        /// <summary>
        /// Gets risk trend analysis
        /// </summary>
        /// <param name="timeRange">Time range for analysis</param>
        /// <returns>Risk trend data</returns>
        Task<RiskTrendAnalysis> GetRiskTrendsAsync(TimeRange timeRange);

        /// <summary>
        /// Gets security posture trends
        /// </summary>
        /// <param name="timeRange">Time range for analysis</param>
        /// <returns>Posture trend data</returns>
        Task<PostureTrendAnalysis> GetPostureTrendsAsync(TimeRange timeRange);

        /// <summary>
        /// Gets anomaly detection results
        /// </summary>
        /// <param name="timeRange">Time range for analysis</param>
        /// <returns>Anomaly detection data</returns>
        Task<AnomalyDetectionResult> GetAnomaliesAsync(TimeRange timeRange);

        /// <summary>
        /// Exports analytics data
        /// </summary>
        /// <param name="timeRange">Time range for export</param>
        /// <param name="format">Export format</param>
        /// <returns>Export data</returns>
        Task<byte[]> ExportAnalyticsAsync(TimeRange timeRange, ExportFormat format);
    }

    /// <summary>
    /// Time range for analytics queries
    /// </summary>
    public class TimeRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeRangeType Type { get; set; }
    }

    /// <summary>
    /// Time range types
    /// </summary>
    public enum TimeRangeType
    {
        Custom,
        LastHour,
        LastDay,
        LastWeek,
        LastMonth,
        LastQuarter,
        LastYear
    }

    /// <summary>
    /// User access patterns
    /// </summary>
    public class UserAccessPatterns
    {
        public string UserId { get; set; }
        public int TotalAccesses { get; set; }
        public int SuccessfulAccesses { get; set; }
        public int DeniedAccesses { get; set; }
        public Dictionary<string, int> ResourceAccessCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<RiskLevel, int> RiskLevelDistribution { get; set; } = new Dictionary<RiskLevel, int>();
        public List<AccessTimePattern> TimePatterns { get; set; } = new List<AccessTimePattern>();
        public List<string> CommonLocations { get; set; } = new List<string>();
        public List<string> CommonDevices { get; set; } = new List<string>();
    }

    /// <summary>
    /// Access time pattern
    /// </summary>
    public class AccessTimePattern
    {
        public DayOfWeek DayOfWeek { get; set; }
        public int Hour { get; set; }
        public int AccessCount { get; set; }
        public double AverageRiskScore { get; set; }
    }

    /// <summary>
    /// Resource access analytics
    /// </summary>
    public class ResourceAccessAnalytics
    {
        public string ResourceId { get; set; }
        public int TotalAccesses { get; set; }
        public int UniqueUsers { get; set; }
        public Dictionary<string, int> UserAccessCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<RiskLevel, int> RiskLevelDistribution { get; set; } = new Dictionary<RiskLevel, int>();
        public List<AccessTimePattern> TimePatterns { get; set; } = new List<AccessTimePattern>();
        public double AverageRiskScore { get; set; }
        public List<SecurityIncident> RelatedIncidents { get; set; } = new List<SecurityIncident>();
    }

    /// <summary>
    /// Risk trend analysis
    /// </summary>
    public class RiskTrendAnalysis
    {
        public List<RiskTrendPoint> TrendPoints { get; set; } = new List<RiskTrendPoint>();
        public double AverageRiskScore { get; set; }
        public RiskLevel MostCommonRiskLevel { get; set; }
        public List<string> TopRiskFactors { get; set; } = new List<string>();
        public bool IsTrendingUp { get; set; }
        public double TrendSlope { get; set; }
    }

    /// <summary>
    /// Risk trend point
    /// </summary>
    public class RiskTrendPoint
    {
        public DateTime Timestamp { get; set; }
        public double RiskScore { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public int AccessCount { get; set; }
    }

    /// <summary>
    /// Posture trend analysis
    /// </summary>
    public class PostureTrendAnalysis
    {
        public List<PostureTrendPoint> TrendPoints { get; set; } = new List<PostureTrendPoint>();
        public double AveragePostureScore { get; set; }
        public PostureLevel MostCommonPostureLevel { get; set; }
        public List<string> TopSecurityFactors { get; set; } = new List<string>();
        public bool IsTrendingUp { get; set; }
        public double TrendSlope { get; set; }
    }

    /// <summary>
    /// Posture trend point
    /// </summary>
    public class PostureTrendPoint
    {
        public DateTime Timestamp { get; set; }
        public double PostureScore { get; set; }
        public PostureLevel PostureLevel { get; set; }
        public int AssessmentCount { get; set; }
    }

    /// <summary>
    /// Anomaly detection result
    /// </summary>
    public class AnomalyDetectionResult
    {
        public List<SecurityAnomaly> Anomalies { get; set; } = new List<SecurityAnomaly>();
        public int TotalAnomalies { get; set; }
        public Dictionary<AnomalyType, int> AnomalyTypeDistribution { get; set; } = new Dictionary<AnomalyType, int>();
        public List<string> AffectedUsers { get; set; } = new List<string>();
        public List<string> AffectedResources { get; set; } = new List<string>();
    }

    /// <summary>
    /// Security anomaly
    /// </summary>
    public class SecurityAnomaly
    {
        public string AnomalyId { get; set; }
        public AnomalyType Type { get; set; }
        public string Description { get; set; }
        public DateTime DetectedAt { get; set; }
        public RiskLevel Severity { get; set; }
        public string UserId { get; set; }
        public string ResourceId { get; set; }
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
        public bool IsResolved { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    /// <summary>
    /// Anomaly types
    /// </summary>
    public enum AnomalyType
    {
        UnusualAccessTime,
        UnusualLocation,
        UnusualDevice,
        HighRiskAccess,
        FailedAuthentication,
        PrivilegeEscalation,
        DataExfiltration,
        NetworkAnomaly
    }

    /// <summary>
    /// Export formats
    /// </summary>
    public enum ExportFormat
    {
        Csv,
        Json,
        Xml,
        Pdf
    }
} 