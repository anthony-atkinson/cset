using System;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust
{
    /// <summary>
    /// Main interface for Zero Trust Architecture implementation
    /// Handles continuous verification, least privilege access, and micro-segmentation
    /// </summary>
    public interface IZeroTrustService
    {
        /// <summary>
        /// Validates user access based on zero trust principles
        /// </summary>
        /// <param name="request">Access request details</param>
        /// <returns>Access validation result</returns>
        Task<ZeroTrustValidationResult> ValidateAccessAsync(ZeroTrustAccessRequest request);

        /// <summary>
        /// Performs continuous verification of user session
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        /// <returns>Verification result</returns>
        Task<ZeroTrustVerificationResult> VerifySessionAsync(string sessionId);

        /// <summary>
        /// Evaluates security posture for access decision
        /// </summary>
        /// <param name="context">Security context</param>
        /// <returns>Posture evaluation result</returns>
        Task<SecurityPostureResult> EvaluateSecurityPostureAsync(ZeroTrustContext context);

        /// <summary>
        /// Implements just-in-time access provisioning
        /// </summary>
        /// <param name="request">JIT access request</param>
        /// <returns>JIT access result</returns>
        Task<JustInTimeAccessResult> ProvisionJustInTimeAccessAsync(JustInTimeAccessRequest request);

        /// <summary>
        /// Performs risk assessment for access request
        /// </summary>
        /// <param name="request">Access request</param>
        /// <returns>Risk assessment result</returns>
        Task<RiskAssessmentResult> AssessRiskAsync(ZeroTrustAccessRequest request);

        /// <summary>
        /// Implements micro-segmentation policies
        /// </summary>
        /// <param name="request">Network access request</param>
        /// <returns>Micro-segmentation result</returns>
        Task<MicroSegmentationResult> ApplyMicroSegmentationAsync(NetworkAccessRequest request);

        /// <summary>
        /// Monitors and logs zero trust events
        /// </summary>
        /// <param name="event">Zero trust event</param>
        /// <returns>Logging result</returns>
        Task<bool> LogZeroTrustEventAsync(ZeroTrustEvent @event);

        /// <summary>
        /// Gets zero trust analytics and metrics
        /// </summary>
        /// <param name="timeRange">Time range for analytics</param>
        /// <returns>Analytics data</returns>
        Task<ZeroTrustAnalytics> GetAnalyticsAsync(TimeRange timeRange);
    }
} 