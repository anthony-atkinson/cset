using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for risk assessment provider
    /// Evaluates various risk factors for access requests
    /// </summary>
    public interface IRiskAssessmentProvider
    {
        /// <summary>
        /// Assesses risk for an access request
        /// </summary>
        /// <param name="request">Access request to assess</param>
        /// <returns>Risk assessment result</returns>
        Task<RiskAssessmentResult> AssessRiskAsync(ZeroTrustAccessRequest request);

        /// <summary>
        /// Evaluates user risk profile
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User risk profile</returns>
        Task<UserRiskProfile> GetUserRiskProfileAsync(string userId);

        /// <summary>
        /// Evaluates resource risk profile
        /// </summary>
        /// <param name="resourceId">Resource identifier</param>
        /// <returns>Resource risk profile</returns>
        Task<ResourceRiskProfile> GetResourceRiskProfileAsync(string resourceId);

        /// <summary>
        /// Evaluates environmental risk factors
        /// </summary>
        /// <param name="context">Environmental context</param>
        /// <returns>Environmental risk assessment</returns>
        Task<EnvironmentalRiskAssessment> AssessEnvironmentalRiskAsync(ZeroTrustContext context);
    }

    /// <summary>
    /// User risk profile
    /// </summary>
    public class UserRiskProfile
    {
        public string UserId { get; set; }
        public RiskLevel OverallRisk { get; set; }
        public int RiskScore { get; set; }
        public List<RiskFactor> Factors { get; set; } = new List<RiskFactor>();
        public DateTime LastAssessed { get; set; }
    }

    /// <summary>
    /// Resource risk profile
    /// </summary>
    public class ResourceRiskProfile
    {
        public string ResourceId { get; set; }
        public RiskLevel SensitivityLevel { get; set; }
        public int RiskScore { get; set; }
        public List<string> RequiredPermissions { get; set; } = new List<string>();
        public List<string> AccessPatterns { get; set; } = new List<string>();
    }

    /// <summary>
    /// Environmental risk assessment
    /// </summary>
    public class EnvironmentalRiskAssessment
    {
        public RiskLevel NetworkRisk { get; set; }
        public RiskLevel LocationRisk { get; set; }
        public RiskLevel TimeRisk { get; set; }
        public RiskLevel DeviceRisk { get; set; }
        public int OverallScore { get; set; }
        public List<string> RiskFactors { get; set; } = new List<string>();
    }
} 