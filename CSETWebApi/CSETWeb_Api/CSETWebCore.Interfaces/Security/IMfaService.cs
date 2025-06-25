using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Security;

namespace CSETWebCore.Interfaces.Security
{
    /// <summary>
    /// Service interface for Multi-Factor Authentication operations
    /// Provides comprehensive MFA functionality including setup, verification, and management
    /// </summary>
    public interface IMfaService
    {
        /// <summary>
        /// Sets up MFA for a user with the specified type
        /// </summary>
        /// <param name="request">MFA setup request</param>
        /// <returns>Setup result with configuration details</returns>
        Task<MfaSetupResult> SetupMfaAsync(MfaSetupRequest request);

        /// <summary>
        /// Verifies an MFA code for a user
        /// </summary>
        /// <param name="request">MFA verification request</param>
        /// <returns>Verification result</returns>
        Task<MfaVerificationResult> VerifyMfaAsync(MfaVerificationRequest request);

        /// <summary>
        /// Disables MFA for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="mfaType">Type of MFA to disable</param>
        /// <returns>Success status</returns>
        Task<bool> DisableMfaAsync(string userId, MfaType mfaType);

        /// <summary>
        /// Gets MFA status for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>MFA status information</returns>
        Task<MfaStatus> GetMfaStatusAsync(string userId);

        /// <summary>
        /// Gets MFA configuration for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>MFA configuration</returns>
        Task<MfaConfiguration> GetMfaConfigurationAsync(string userId);

        /// <summary>
        /// Generates backup codes for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="count">Number of backup codes to generate</param>
        /// <returns>Backup codes</returns>
        Task<MfaBackupCodes> GenerateBackupCodesAsync(string userId, int count = 10);

        /// <summary>
        /// Verifies a backup code
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="code">Backup code to verify</param>
        /// <returns>Verification result</returns>
        Task<bool> VerifyBackupCodeAsync(string userId, string code);

        /// <summary>
        /// Registers a hardware security key
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="publicKey">Public key from the hardware key</param>
        /// <param name="deviceName">Name for the device</param>
        /// <returns>Registration result</returns>
        Task<HardwareKeyRegistration> RegisterHardwareKeyAsync(string userId, string publicKey, string deviceName);

        /// <summary>
        /// Verifies a hardware security key challenge
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="challenge">Challenge to verify</param>
        /// <param name="signature">Signature from the hardware key</param>
        /// <returns>Verification result</returns>
        Task<bool> VerifyHardwareKeyAsync(string userId, string challenge, string signature);

        /// <summary>
        /// Sends an SMS verification code
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="phoneNumber">Phone number to send code to</param>
        /// <returns>Success status</returns>
        Task<bool> SendSmsCodeAsync(string userId, string phoneNumber);

        /// <summary>
        /// Sends an email verification code
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="email">Email address to send code to</param>
        /// <returns>Success status</returns>
        Task<bool> SendEmailCodeAsync(string userId, string email);

        /// <summary>
        /// Sends a push notification for authentication
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="deviceToken">Device token for push notification</param>
        /// <returns>Success status</returns>
        Task<bool> SendPushNotificationAsync(string userId, string deviceToken);

        /// <summary>
        /// Gets MFA audit logs for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="startDate">Start date for logs</param>
        /// <param name="endDate">End date for logs</param>
        /// <returns>List of audit log entries</returns>
        Task<List<MfaAuditLog>> GetAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets MFA policy configuration
        /// </summary>
        /// <returns>MFA policy settings</returns>
        Task<MfaPolicy> GetMfaPolicyAsync();

        /// <summary>
        /// Updates MFA policy configuration
        /// </summary>
        /// <param name="policy">Updated policy settings</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateMfaPolicyAsync(MfaPolicy policy);

        /// <summary>
        /// Checks if MFA is required for a user based on policy
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="context">Authentication context</param>
        /// <returns>Whether MFA is required</returns>
        Task<bool> IsMfaRequiredAsync(string userId, Dictionary<string, object> context = null);

        /// <summary>
        /// Resets failed attempt counter for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Success status</returns>
        Task<bool> ResetFailedAttemptsAsync(string userId);

        /// <summary>
        /// Unlocks a user's MFA account
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>Success status</returns>
        Task<bool> UnlockMfaAccountAsync(string userId);

        /// <summary>
        /// Gets available MFA types for a user
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>List of available MFA types</returns>
        Task<List<MfaType>> GetAvailableMfaTypesAsync(string userId);

        /// <summary>
        /// Validates MFA setup request
        /// </summary>
        /// <param name="request">Setup request to validate</param>
        /// <returns>Validation result</returns>
        Task<(bool IsValid, List<string> Errors)> ValidateSetupRequestAsync(MfaSetupRequest request);
    }
} 