//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System.Collections.Generic;

namespace CSETWeb_ApiCore.Models
{
    /// <summary>
    /// Configuration settings for Zero Trust Architecture
    /// </summary>
    public class ZeroTrustConfiguration
    {
        /// <summary>
        /// Whether Zero Trust validation is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether to fail open or closed on errors
        /// </summary>
        public bool FailClosed { get; set; } = false;

        /// <summary>
        /// Risk thresholds for different actions
        /// </summary>
        public RiskThresholds RiskThresholds { get; set; } = new RiskThresholds();

        /// <summary>
        /// Network segments configuration
        /// </summary>
        public NetworkSegments NetworkSegments { get; set; } = new NetworkSegments();

        /// <summary>
        /// JIT access configuration
        /// </summary>
        public JitAccessConfiguration JitAccess { get; set; } = new JitAccessConfiguration();

        /// <summary>
        /// Analytics configuration
        /// </summary>
        public AnalyticsConfiguration Analytics { get; set; } = new AnalyticsConfiguration();

        /// <summary>
        /// Logging configuration
        /// </summary>
        public LoggingConfiguration Logging { get; set; } = new LoggingConfiguration();
    }

    /// <summary>
    /// Risk thresholds for different actions
    /// </summary>
    public class RiskThresholds
    {
        /// <summary>
        /// Maximum risk level for read operations
        /// </summary>
        public string MaxRiskForRead { get; set; } = "Medium";

        /// <summary>
        /// Maximum risk level for write operations
        /// </summary>
        public string MaxRiskForWrite { get; set; } = "Low";

        /// <summary>
        /// Maximum risk level for admin operations
        /// </summary>
        public string MaxRiskForAdmin { get; set; } = "Low";

        /// <summary>
        /// Risk score thresholds
        /// </summary>
        public RiskScoreThresholds RiskScores { get; set; } = new RiskScoreThresholds();
    }

    /// <summary>
    /// Risk score thresholds
    /// </summary>
    public class RiskScoreThresholds
    {
        /// <summary>
        /// Low risk threshold
        /// </summary>
        public int Low { get; set; } = 30;

        /// <summary>
        /// Medium risk threshold
        /// </summary>
        public int Medium { get; set; } = 60;

        /// <summary>
        /// High risk threshold
        /// </summary>
        public int High { get; set; } = 80;

        /// <summary>
        /// Critical risk threshold
        /// </summary>
        public int Critical { get; set; } = 90;
    }

    /// <summary>
    /// Network segments configuration
    /// </summary>
    public class NetworkSegments
    {
        /// <summary>
        /// Internal network subnets
        /// </summary>
        public List<string> InternalSubnets { get; set; } = new List<string>
        {
            "192.168.0.0/16",
            "10.0.0.0/8"
        };

        /// <summary>
        /// DMZ network subnets
        /// </summary>
        public List<string> DmzSubnets { get; set; } = new List<string>
        {
            "172.16.0.0/12"
        };

        /// <summary>
        /// Trusted external IPs
        /// </summary>
        public List<string> TrustedExternalIps { get; set; } = new List<string>();
    }

    /// <summary>
    /// JIT access configuration
    /// </summary>
    public class JitAccessConfiguration
    {
        /// <summary>
        /// Whether JIT access is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Default access duration
        /// </summary>
        public string DefaultDuration { get; set; } = "01:00:00";

        /// <summary>
        /// Maximum access duration
        /// </summary>
        public string MaxDuration { get; set; } = "24:00:00";

        /// <summary>
        /// Whether auto-approval is enabled
        /// </summary>
        public bool AutoApprovalEnabled { get; set; } = false;

        /// <summary>
        /// Auto-approval conditions
        /// </summary>
        public AutoApprovalConditions AutoApprovalConditions { get; set; } = new AutoApprovalConditions();
    }

    /// <summary>
    /// Auto-approval conditions for JIT access
    /// </summary>
    public class AutoApprovalConditions
    {
        /// <summary>
        /// Maximum risk level for auto-approval
        /// </summary>
        public string MaxRiskLevel { get; set; } = "Low";

        /// <summary>
        /// Maximum duration for auto-approval
        /// </summary>
        public string MaxDuration { get; set; } = "01:00:00";

        /// <summary>
        /// Whether to allow auto-approval during business hours only
        /// </summary>
        public bool BusinessHoursOnly { get; set; } = true;

        /// <summary>
        /// Business hours start (24-hour format)
        /// </summary>
        public string BusinessHoursStart { get; set; } = "08:00";

        /// <summary>
        /// Business hours end (24-hour format)
        /// </summary>
        public string BusinessHoursEnd { get; set; } = "18:00";
    }

    /// <summary>
    /// Analytics configuration
    /// </summary>
    public class AnalyticsConfiguration
    {
        /// <summary>
        /// Whether analytics are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Data retention period in days
        /// </summary>
        public int DataRetentionDays { get; set; } = 90;

        /// <summary>
        /// Whether to enable real-time analytics
        /// </summary>
        public bool RealTimeEnabled { get; set; } = true;

        /// <summary>
        /// Analytics aggregation interval in minutes
        /// </summary>
        public int AggregationIntervalMinutes { get; set; } = 15;
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingConfiguration
    {
        /// <summary>
        /// Whether to log all events
        /// </summary>
        public bool LogAllEvents { get; set; } = true;

        /// <summary>
        /// Whether to log successful access
        /// </summary>
        public bool LogSuccessfulAccess { get; set; } = false;

        /// <summary>
        /// Whether to log denied access
        /// </summary>
        public bool LogDeniedAccess { get; set; } = true;

        /// <summary>
        /// Whether to log security incidents
        /// </summary>
        public bool LogSecurityIncidents { get; set; } = true;

        /// <summary>
        /// Minimum log level
        /// </summary>
        public string MinLogLevel { get; set; } = "Information";
    }
} 