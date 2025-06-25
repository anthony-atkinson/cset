using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for security posture provider
    /// Evaluates overall security posture of users, devices, and environments
    /// </summary>
    public interface ISecurityPostureProvider
    {
        /// <summary>
        /// Evaluates security posture for a given context
        /// </summary>
        /// <param name="context">Security context to evaluate</param>
        /// <returns>Security posture result</returns>
        Task<SecurityPostureResult> EvaluatePostureAsync(ZeroTrustContext context);

        /// <summary>
        /// Evaluates user security posture
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User posture assessment</returns>
        Task<UserPostureAssessment> EvaluateUserPostureAsync(string userId);

        /// <summary>
        /// Evaluates device security posture
        /// </summary>
        /// <param name="deviceId">Device identifier</param>
        /// <returns>Device posture assessment</returns>
        Task<DevicePostureAssessment> EvaluateDevicePostureAsync(string deviceId);

        /// <summary>
        /// Evaluates network security posture
        /// </summary>
        /// <param name="networkSegment">Network segment</param>
        /// <returns>Network posture assessment</returns>
        Task<NetworkPostureAssessment> EvaluateNetworkPostureAsync(string networkSegment);
    }

    /// <summary>
    /// User posture assessment
    /// </summary>
    public class UserPostureAssessment
    {
        public string UserId { get; set; }
        public PostureLevel OverallPosture { get; set; }
        public int PostureScore { get; set; }
        public List<SecurityFactor> Factors { get; set; } = new List<SecurityFactor>();
        public List<string> Recommendations { get; set; } = new List<string>();
        public DateTime LastAssessed { get; set; }
    }

    /// <summary>
    /// Device posture assessment
    /// </summary>
    public class DevicePostureAssessment
    {
        public string DeviceId { get; set; }
        public PostureLevel OverallPosture { get; set; }
        public int PostureScore { get; set; }
        public bool IsCompliant { get; set; }
        public List<SecurityFactor> Factors { get; set; } = new List<SecurityFactor>();
        public List<string> ComplianceIssues { get; set; } = new List<string>();
        public DateTime LastAssessed { get; set; }
    }

    /// <summary>
    /// Network posture assessment
    /// </summary>
    public class NetworkPostureAssessment
    {
        public string NetworkSegment { get; set; }
        public PostureLevel OverallPosture { get; set; }
        public int PostureScore { get; set; }
        public List<SecurityFactor> Factors { get; set; } = new List<SecurityFactor>();
        public List<string> SecurityRecommendations { get; set; } = new List<string>();
        public DateTime LastAssessed { get; set; }
    }
} 