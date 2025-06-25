using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSETWebCore.Model.Security
{
    /// <summary>
    /// Zero Trust access request model
    /// </summary>
    public class ZeroTrustAccessRequest
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ResourceId { get; set; }

        [Required]
        public string Action { get; set; }

        public string SessionId { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

        public string DeviceId { get; set; }

        public string Location { get; set; }

        public DateTime RequestTime { get; set; } = DateTime.UtcNow;

        public Dictionary<string, string> Context { get; set; } = new Dictionary<string, string>();

        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Zero Trust validation result
    /// </summary>
    public class ZeroTrustValidationResult
    {
        public bool IsAllowed { get; set; }

        public string Reason { get; set; }

        public RiskLevel RiskLevel { get; set; }

        public List<string> RequiredActions { get; set; } = new List<string>();

        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

        public string ValidationId { get; set; } = Guid.NewGuid().ToString();

        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Zero Trust verification result
    /// </summary>
    public class ZeroTrustVerificationResult
    {
        public bool IsValid { get; set; }

        public string SessionId { get; set; }

        public string UserId { get; set; }

        public DateTime LastVerified { get; set; }

        public DateTime ExpiresAt { get; set; }

        public List<string> Warnings { get; set; } = new List<string>();

        public Dictionary<string, object> SessionContext { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Security posture evaluation result
    /// </summary>
    public class SecurityPostureResult
    {
        public int PostureScore { get; set; }

        public PostureLevel PostureLevel { get; set; }

        public List<SecurityFactor> Factors { get; set; } = new List<SecurityFactor>();

        public List<string> Recommendations { get; set; } = new List<string>();

        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Security factor for posture evaluation
    /// </summary>
    public class SecurityFactor
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public int Score { get; set; }

        public FactorStatus Status { get; set; }

        public string Recommendation { get; set; }
    }

    /// <summary>
    /// Just-in-time access request
    /// </summary>
    public class JustInTimeAccessRequest
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ResourceId { get; set; }

        [Required]
        public string Reason { get; set; }

        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);

        public string ApproverId { get; set; }

        public string Justification { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Just-in-time access result
    /// </summary>
    public class JustInTimeAccessResult
    {
        public bool IsApproved { get; set; }

        public string AccessToken { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string Reason { get; set; }

        public string ApproverId { get; set; }

        public DateTime ApprovedAt { get; set; }
    }

    /// <summary>
    /// Risk assessment result
    /// </summary>
    public class RiskAssessmentResult
    {
        public RiskLevel OverallRisk { get; set; }

        public int RiskScore { get; set; }

        public List<RiskFactor> RiskFactors { get; set; } = new List<RiskFactor>();

        public List<string> MitigationStrategies { get; set; } = new List<string>();

        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Risk factor for assessment
    /// </summary>
    public class RiskFactor
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public RiskLevel Level { get; set; }

        public double Weight { get; set; }

        public string Mitigation { get; set; }
    }

    /// <summary>
    /// Network access request for micro-segmentation
    /// </summary>
    public class NetworkAccessRequest
    {
        public string SourceIp { get; set; }

        public string DestinationIp { get; set; }

        public int Port { get; set; }

        public string Protocol { get; set; }

        public string UserId { get; set; }

        public string Application { get; set; }

        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Micro-segmentation result
    /// </summary>
    public class MicroSegmentationResult
    {
        public bool IsAllowed { get; set; }

        public string PolicyName { get; set; }

        public string Reason { get; set; }

        public List<string> AppliedRules { get; set; } = new List<string>();

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Zero Trust context for evaluation
    /// </summary>
    public class ZeroTrustContext
    {
        public string UserId { get; set; }

        public string DeviceId { get; set; }

        public string Location { get; set; }

        public string NetworkSegment { get; set; }

        public DateTime ContextTime { get; set; } = DateTime.UtcNow;

        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Zero Trust event for logging
    /// </summary>
    public class ZeroTrustEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();

        public string EventType { get; set; }

        public string UserId { get; set; }

        public string ResourceId { get; set; }

        public string Action { get; set; }

        public bool Success { get; set; }

        public string Details { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Zero Trust analytics data
    /// </summary>
    public class ZeroTrustAnalytics
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int TotalRequests { get; set; }

        public int AllowedRequests { get; set; }

        public int DeniedRequests { get; set; }

        public Dictionary<RiskLevel, int> RiskDistribution { get; set; } = new Dictionary<RiskLevel, int>();

        public List<TopUser> TopUsers { get; set; } = new List<TopUser>();

        public List<TopResource> TopResources { get; set; } = new List<TopResource>();

        public List<SecurityIncident> SecurityIncidents { get; set; } = new List<SecurityIncident>();
    }

    /// <summary>
    /// Top user for analytics
    /// </summary>
    public class TopUser
    {
        public string UserId { get; set; }

        public int AccessCount { get; set; }

        public double RiskScore { get; set; }

        public DateTime LastAccess { get; set; }
    }

    /// <summary>
    /// Top resource for analytics
    /// </summary>
    public class TopResource
    {
        public string ResourceId { get; set; }

        public int AccessCount { get; set; }

        public double RiskScore { get; set; }

        public DateTime LastAccess { get; set; }
    }

    /// <summary>
    /// Security incident for analytics
    /// </summary>
    public class SecurityIncident
    {
        public string IncidentId { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public RiskLevel Severity { get; set; }

        public IncidentStatus Status { get; set; }

        public DateTime DetectedAt { get; set; }

        public string AffectedUser { get; set; }

        public string AffectedResource { get; set; }

        public string Details { get; set; }

        public List<string> RelatedEvents { get; set; } = new List<string>();
    }

    /// <summary>
    /// Time range for analytics
    /// </summary>
    public class TimeRange
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public TimeRangeType Type { get; set; }
    }

    /// <summary>
    /// Risk levels
    /// </summary>
    public enum RiskLevel
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Posture levels
    /// </summary>
    public enum PostureLevel
    {
        Poor = 1,
        Fair = 2,
        Good = 3,
        Excellent = 4
    }

    /// <summary>
    /// Factor status
    /// </summary>
    public enum FactorStatus
    {
        Compliant = 1,
        NonCompliant = 2,
        Warning = 3
    }

    /// <summary>
    /// Incident status
    /// </summary>
    public enum IncidentStatus
    {
        Open = 1,
        InProgress = 2,
        Resolved = 3,
        Closed = 4
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
} 