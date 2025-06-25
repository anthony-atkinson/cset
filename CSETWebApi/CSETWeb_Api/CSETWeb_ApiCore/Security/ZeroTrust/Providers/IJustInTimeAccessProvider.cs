using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.ApiCore.Security.ZeroTrust.Providers
{
    /// <summary>
    /// Interface for just-in-time access provider
    /// Handles temporary access provisioning and approval workflows
    /// </summary>
    public interface IJustInTimeAccessProvider
    {
        /// <summary>
        /// Provisions just-in-time access
        /// </summary>
        /// <param name="request">JIT access request</param>
        /// <returns>JIT access result</returns>
        Task<JustInTimeAccessResult> ProvisionAccessAsync(JustInTimeAccessRequest request);

        /// <summary>
        /// Approves JIT access request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="approverId">Approver identifier</param>
        /// <param name="comments">Approval comments</param>
        /// <returns>Approval result</returns>
        Task<JustInTimeAccessResult> ApproveRequestAsync(string requestId, string approverId, string comments = null);

        /// <summary>
        /// Denies JIT access request
        /// </summary>
        /// <param name="requestId">Request identifier</param>
        /// <param name="denierId">Denier identifier</param>
        /// <param name="reason">Denial reason</param>
        /// <returns>Denial result</returns>
        Task<bool> DenyRequestAsync(string requestId, string denierId, string reason);

        /// <summary>
        /// Revokes active JIT access
        /// </summary>
        /// <param name="accessToken">Access token to revoke</param>
        /// <param name="revokerId">Revoker identifier</param>
        /// <returns>Revocation result</returns>
        Task<bool> RevokeAccessAsync(string accessToken, string revokerId);

        /// <summary>
        /// Gets pending JIT access requests
        /// </summary>
        /// <param name="approverId">Approver identifier</param>
        /// <returns>List of pending requests</returns>
        Task<List<JustInTimeAccessRequest>> GetPendingRequestsAsync(string approverId);

        /// <summary>
        /// Gets active JIT access sessions
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>List of active sessions</returns>
        Task<List<ActiveJitSession>> GetActiveSessionsAsync(string userId);
    }

    /// <summary>
    /// Active JIT session information
    /// </summary>
    public class ActiveJitSession
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public string ResourceId { get; set; }
        public DateTime GrantedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string ApproverId { get; set; }
        public string Reason { get; set; }
        public bool IsActive => DateTime.UtcNow < ExpiresAt;
    }
} 