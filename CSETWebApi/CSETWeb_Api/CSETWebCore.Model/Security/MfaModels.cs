using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CSETWebCore.Model.Security
{
    /// <summary>
    /// MFA configuration for a user
    /// </summary>
    public class MfaConfiguration
    {
        public string UserId { get; set; }
        public bool IsEnabled { get; set; }
        public MfaType PrimaryMfaType { get; set; }
        public MfaType SecondaryMfaType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool IsVerified { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Types of MFA supported by the system
    /// </summary>
    public enum MfaType
    {
        /// <summary>
        /// Time-based One-Time Password (TOTP) - Google Authenticator, Microsoft Authenticator
        /// </summary>
        Totp = 1,
        
        /// <summary>
        /// SMS-based One-Time Password
        /// </summary>
        Sms = 2,
        
        /// <summary>
        /// Email-based One-Time Password
        /// </summary>
        Email = 3,
        
        /// <summary>
        /// Hardware Security Key (FIDO2/WebAuthn)
        /// </summary>
        HardwareKey = 4,
        
        /// <summary>
        /// Smart Card authentication
        /// </summary>
        SmartCard = 5,
        
        /// <summary>
        /// RSA Token (hardware token)
        /// </summary>
        RsaToken = 6,
        
        /// <summary>
        /// Biometric authentication
        /// </summary>
        Biometric = 7,
        
        /// <summary>
        /// Push notification authentication
        /// </summary>
        PushNotification = 8
    }

    /// <summary>
    /// MFA verification request
    /// </summary>
    public class MfaVerificationRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public MfaType MfaType { get; set; }
        
        [Required]
        public string Code { get; set; }
        
        public string SessionId { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// MFA verification result
    /// </summary>
    public class MfaVerificationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public DateTime VerifiedAt { get; set; }
        public string SessionId { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// MFA setup request
    /// </summary>
    public class MfaSetupRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public MfaType MfaType { get; set; }
        
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string DeviceName { get; set; }
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// MFA setup result
    /// </summary>
    public class MfaSetupResult
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public string SetupCode { get; set; }
        public string QrCodeUrl { get; set; }
        public string BackupCodes { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// TOTP configuration
    /// </summary>
    public class TotpConfiguration
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; } = "CSET";
        public string AccountName { get; set; }
        public int Digits { get; set; } = 6;
        public int Period { get; set; } = 30;
        public string Algorithm { get; set; } = "SHA1";
    }

    /// <summary>
    /// Hardware key registration
    /// </summary>
    public class HardwareKeyRegistration
    {
        public string UserId { get; set; }
        public string KeyId { get; set; }
        public string PublicKey { get; set; }
        public string DeviceName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime LastUsed { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// MFA backup codes
    /// </summary>
    public class MfaBackupCodes
    {
        public string UserId { get; set; }
        public List<string> Codes { get; set; } = new List<string>();
        public DateTime GeneratedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
    }

    /// <summary>
    /// MFA audit log entry
    /// </summary>
    public class MfaAuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public MfaType MfaType { get; set; }
        public string Action { get; set; }
        public bool IsSuccessful { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// MFA policy configuration
    /// </summary>
    public class MfaPolicy
    {
        public bool RequireMfaForAllUsers { get; set; }
        public bool RequireMfaForAdmins { get; set; }
        public bool RequireMfaForRemoteAccess { get; set; }
        public List<MfaType> AllowedMfaTypes { get; set; } = new List<MfaType>();
        public int MaxFailedAttempts { get; set; } = 5;
        public TimeSpan LockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
        public bool RequireBackupCodes { get; set; } = true;
        public int BackupCodeCount { get; set; } = 10;
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(8);
    }

    /// <summary>
    /// MFA status for a user
    /// </summary>
    public class MfaStatus
    {
        public string UserId { get; set; }
        public bool IsEnabled { get; set; }
        public List<MfaType> EnabledTypes { get; set; } = new List<MfaType>();
        public DateTime? LastUsed { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int FailedAttempts { get; set; }
        public bool HasBackupCodes { get; set; }
        public int RemainingBackupCodes { get; set; }
    }
} 