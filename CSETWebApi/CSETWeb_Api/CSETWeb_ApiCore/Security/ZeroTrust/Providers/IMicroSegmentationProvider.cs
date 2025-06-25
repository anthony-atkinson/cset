using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for micro-segmentation provider
    /// Handles network access policies and segmentation rules
    /// </summary>
    public interface IMicroSegmentationProvider
    {
        /// <summary>
        /// Applies micro-segmentation policies to network access request
        /// </summary>
        /// <param name="request">Network access request</param>
        /// <returns>Micro-segmentation result</returns>
        Task<MicroSegmentationResult> ApplyPoliciesAsync(NetworkAccessRequest request);

        /// <summary>
        /// Gets network segmentation policies
        /// </summary>
        /// <param name="segment">Network segment</param>
        /// <returns>List of policies</returns>
        Task<List<NetworkPolicy>> GetPoliciesAsync(string segment);

        /// <summary>
        /// Validates network access against policies
        /// </summary>
        /// <param name="source">Source network information</param>
        /// <param name="destination">Destination network information</param>
        /// <returns>Policy validation result</returns>
        Task<PolicyValidationResult> ValidatePolicyAsync(NetworkEndpoint source, NetworkEndpoint destination);
    }

    /// <summary>
    /// Network policy for micro-segmentation
    /// </summary>
    public class NetworkPolicy
    {
        public string PolicyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PolicyAction Action { get; set; }
        public List<NetworkEndpoint> Sources { get; set; } = new List<NetworkEndpoint>();
        public List<NetworkEndpoint> Destinations { get; set; } = new List<NetworkEndpoint>();
        public List<string> Protocols { get; set; } = new List<string>();
        public List<int> Ports { get; set; } = new List<int>();
        public bool IsEnabled { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Network endpoint for policy definition
    /// </summary>
    public class NetworkEndpoint
    {
        public string IpAddress { get; set; }
        public string Subnet { get; set; }
        public string Segment { get; set; }
        public string UserGroup { get; set; }
        public string Application { get; set; }
    }

    /// <summary>
    /// Policy validation result
    /// </summary>
    public class PolicyValidationResult
    {
        public bool IsAllowed { get; set; }
        public string PolicyName { get; set; }
        public string Reason { get; set; }
        public List<string> AppliedRules { get; set; } = new List<string>();
    }

    /// <summary>
    /// Policy actions
    /// </summary>
    public enum PolicyAction
    {
        Allow,
        Deny,
        Log
    }
} 