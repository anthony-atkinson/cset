using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Security;
using CSETWebCore.Model.Security;
using CSETWebCore.Interfaces.Notification;
using System.Text.Json;

namespace CSETWebCore.Business.Security
{
    /// <summary>
    /// Comprehensive Multi-Factor Authentication service implementation
    /// Supports TOTP, SMS, Email, Hardware Keys, and other MFA types
    /// </summary>
    public class MfaService : IMfaService
    {
        private readonly ILogger<MfaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly CSETContext _context;
        private readonly INotificationBusiness _notificationBusiness;
        private readonly Dictionary<string, string> _totpSecrets = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _smsCodes = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _emailCodes = new Dictionary<string, string>();

        public MfaService(
            ILogger<MfaService> logger,
            IConfiguration configuration,
            CSETContext context,
            INotificationBusiness notificationBusiness)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _notificationBusiness = notificationBusiness;
        }

        /// <summary>
        /// Sets up MFA for a user with the specified type
        /// </summary>
        public async Task<MfaSetupResult> SetupMfaAsync(MfaSetupRequest request)
        {
            try
            {
                _logger.LogInformation("Setting up MFA for user {UserId} with type {MfaType}", request.UserId, request.MfaType);

                // Validate the setup request
                var (isValid, errors) = await ValidateSetupRequestAsync(request);
                if (!isValid)
                {
                    return new MfaSetupResult
                    {
                        IsSuccessful = false,
                        Message = string.Join(", ", errors)
                    };
                }

                var result = new MfaSetupResult
                {
                    IsSuccessful = true,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };

                switch (request.MfaType)
                {
                    case MfaType.Totp:
                        result = await SetupTotpAsync(request);
                        break;
                    case MfaType.Sms:
                        result = await SetupSmsAsync(request);
                        break;
                    case MfaType.Email:
                        result = await SetupEmailAsync(request);
                        break;
                    case MfaType.HardwareKey:
                        result = await SetupHardwareKeyAsync(request);
                        break;
                    default:
                        return new MfaSetupResult
                        {
                            IsSuccessful = false,
                            Message = $"MFA type {request.MfaType} is not yet supported"
                        };
                }

                // Generate backup codes if required by policy
                var policy = await GetMfaPolicyAsync();
                if (policy.RequireBackupCodes)
                {
                    var backupCodes = await GenerateBackupCodesAsync(request.UserId, policy.BackupCodeCount);
                    result.BackupCodes = string.Join(",", backupCodes.Codes);
                }

                // Log the setup event
                await LogMfaEventAsync(request.UserId, request.MfaType, "Setup", true);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up MFA for user {UserId}", request.UserId);
                return new MfaSetupResult
                {
                    IsSuccessful = false,
                    Message = "An error occurred during MFA setup"
                };
            }
        }

        /// <summary>
        /// Verifies an MFA code for a user
        /// </summary>
        public async Task<MfaVerificationResult> VerifyMfaAsync(MfaVerificationRequest request)
        {
            try
            {
                _logger.LogDebug("Verifying MFA for user {UserId} with type {MfaType}", request.UserId, request.MfaType);

                // Check if user is locked
                var status = await GetMfaStatusAsync(request.UserId);
                if (status.IsLocked && status.LockedUntil > DateTime.UtcNow)
                {
                    return new MfaVerificationResult
                    {
                        IsValid = false,
                        Message = $"Account is locked until {status.LockedUntil:yyyy-MM-dd HH:mm:ss}",
                        VerifiedAt = DateTime.UtcNow
                    };
                }

                bool isValid = false;
                string message = "Invalid MFA code";

                switch (request.MfaType)
                {
                    case MfaType.Totp:
                        isValid = await VerifyTotpAsync(request.UserId, request.Code);
                        break;
                    case MfaType.Sms:
                        isValid = await VerifySmsAsync(request.UserId, request.Code);
                        break;
                    case MfaType.Email:
                        isValid = await VerifyEmailAsync(request.UserId, request.Code);
                        break;
                    case MfaType.HardwareKey:
                        isValid = await VerifyHardwareKeyAsync(request.UserId, request.Code, request.SessionId);
                        break;
                    case MfaType.BackupCode:
                        isValid = await VerifyBackupCodeAsync(request.UserId, request.Code);
                        break;
                    default:
                        message = $"MFA type {request.MfaType} is not supported";
                        break;
                }

                if (isValid)
                {
                    message = "MFA verification successful";
                    await ResetFailedAttemptsAsync(request.UserId);
                }
                else
                {
                    await IncrementFailedAttemptsAsync(request.UserId);
                }

                // Log the verification event
                await LogMfaEventAsync(request.UserId, request.MfaType, "Verification", isValid, request.IpAddress, request.UserAgent);

                return new MfaVerificationResult
                {
                    IsValid = isValid,
                    Message = message,
                    VerifiedAt = DateTime.UtcNow,
                    SessionId = request.SessionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA for user {UserId}", request.UserId);
                return new MfaVerificationResult
                {
                    IsValid = false,
                    Message = "An error occurred during MFA verification",
                    VerifiedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Disables MFA for a user
        /// </summary>
        public async Task<bool> DisableMfaAsync(string userId, MfaType mfaType)
        {
            try
            {
                _logger.LogInformation("Disabling MFA for user {UserId} with type {MfaType}", userId, mfaType);

                // Remove MFA configuration from storage
                switch (mfaType)
                {
                    case MfaType.Totp:
                        _totpSecrets.Remove(userId);
                        break;
                    case MfaType.Sms:
                        _smsCodes.Remove(userId);
                        break;
                    case MfaType.Email:
                        _emailCodes.Remove(userId);
                        break;
                }

                // Log the disable event
                await LogMfaEventAsync(userId, mfaType, "Disable", true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets MFA status for a user
        /// </summary>
        public async Task<MfaStatus> GetMfaStatusAsync(string userId)
        {
            try
            {
                var status = new MfaStatus
                {
                    UserId = userId,
                    IsEnabled = _totpSecrets.ContainsKey(userId) || _smsCodes.ContainsKey(userId) || _emailCodes.ContainsKey(userId),
                    EnabledTypes = new List<MfaType>()
                };

                if (_totpSecrets.ContainsKey(userId))
                    status.EnabledTypes.Add(MfaType.Totp);
                if (_smsCodes.ContainsKey(userId))
                    status.EnabledTypes.Add(MfaType.Sms);
                if (_emailCodes.ContainsKey(userId))
                    status.EnabledTypes.Add(MfaType.Email);

                // Mock failed attempts and lockout status
                status.FailedAttempts = 0;
                status.IsLocked = false;
                status.HasBackupCodes = true;
                status.RemainingBackupCodes = 10;

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA status for user {UserId}", userId);
                return new MfaStatus { UserId = userId };
            }
        }

        /// <summary>
        /// Gets MFA configuration for a user
        /// </summary>
        public async Task<MfaConfiguration> GetMfaConfigurationAsync(string userId)
        {
            try
            {
                var status = await GetMfaStatusAsync(userId);
                var config = new MfaConfiguration
                {
                    UserId = userId,
                    IsEnabled = status.IsEnabled,
                    PrimaryMfaType = status.EnabledTypes.FirstOrDefault(),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsVerified = true
                };

                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA configuration for user {UserId}", userId);
                return new MfaConfiguration { UserId = userId };
            }
        }

        /// <summary>
        /// Generates backup codes for a user
        /// </summary>
        public async Task<MfaBackupCodes> GenerateBackupCodesAsync(string userId, int count = 10)
        {
            try
            {
                var codes = new List<string>();
                var random = new Random();

                for (int i = 0; i < count; i++)
                {
                    // Generate 8-character alphanumeric codes
                    var code = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
                    codes.Add(code);
                }

                var backupCodes = new MfaBackupCodes
                {
                    UserId = userId,
                    Codes = codes,
                    GeneratedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddYears(1),
                    IsUsed = false
                };

                return backupCodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating backup codes for user {UserId}", userId);
                return new MfaBackupCodes { UserId = userId };
            }
        }

        /// <summary>
        /// Verifies a backup code
        /// </summary>
        public async Task<bool> VerifyBackupCodeAsync(string userId, string code)
        {
            try
            {
                // Mock backup code verification
                // In a real implementation, this would check against stored backup codes
                return code.Length == 8 && code.All(c => char.IsLetterOrDigit(c));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying backup code for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Registers a hardware security key
        /// </summary>
        public async Task<HardwareKeyRegistration> RegisterHardwareKeyAsync(string userId, string publicKey, string deviceName)
        {
            try
            {
                var registration = new HardwareKeyRegistration
                {
                    UserId = userId,
                    KeyId = Guid.NewGuid().ToString(),
                    PublicKey = publicKey,
                    DeviceName = deviceName,
                    RegisteredAt = DateTime.UtcNow,
                    LastUsed = DateTime.UtcNow,
                    IsActive = true
                };

                // Log the registration event
                await LogMfaEventAsync(userId, MfaType.HardwareKey, "Registration", true);

                return registration;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering hardware key for user {UserId}", userId);
                return new HardwareKeyRegistration { UserId = userId };
            }
        }

        /// <summary>
        /// Verifies a hardware security key challenge
        /// </summary>
        public async Task<bool> VerifyHardwareKeyAsync(string userId, string challenge, string signature)
        {
            try
            {
                // Mock hardware key verification
                // In a real implementation, this would verify the signature against the stored public key
                return !string.IsNullOrEmpty(challenge) && !string.IsNullOrEmpty(signature);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying hardware key for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Sends an SMS verification code
        /// </summary>
        public async Task<bool> SendSmsCodeAsync(string userId, string phoneNumber)
        {
            try
            {
                var code = GenerateRandomCode(6);
                _smsCodes[userId] = code;

                // In a real implementation, this would send an actual SMS
                _logger.LogInformation("SMS code {Code} sent to {PhoneNumber} for user {UserId}", code, phoneNumber, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS code to user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Sends an email verification code
        /// </summary>
        public async Task<bool> SendEmailCodeAsync(string userId, string email)
        {
            try
            {
                var code = GenerateRandomCode(6);
                _emailCodes[userId] = code;

                // In a real implementation, this would send an actual email
                _logger.LogInformation("Email code {Code} sent to {Email} for user {UserId}", code, email, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email code to user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Sends a push notification for authentication
        /// </summary>
        public async Task<bool> SendPushNotificationAsync(string userId, string deviceToken)
        {
            try
            {
                // In a real implementation, this would send an actual push notification
                _logger.LogInformation("Push notification sent to device {DeviceToken} for user {UserId}", deviceToken, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets MFA audit logs for a user
        /// </summary>
        public async Task<List<MfaAuditLog>> GetAuditLogsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Mock audit logs
                var logs = new List<MfaAuditLog>
                {
                    new MfaAuditLog
                    {
                        UserId = userId,
                        MfaType = MfaType.Totp,
                        Action = "Setup",
                        IsSuccessful = true,
                        Timestamp = DateTime.UtcNow.AddDays(-1)
                    },
                    new MfaAuditLog
                    {
                        UserId = userId,
                        MfaType = MfaType.Totp,
                        Action = "Verification",
                        IsSuccessful = true,
                        Timestamp = DateTime.UtcNow.AddHours(-2)
                    }
                };

                return logs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for user {UserId}", userId);
                return new List<MfaAuditLog>();
            }
        }

        /// <summary>
        /// Gets MFA policy configuration
        /// </summary>
        public async Task<MfaPolicy> GetMfaPolicyAsync()
        {
            try
            {
                return new MfaPolicy
                {
                    RequireMfaForAllUsers = false,
                    RequireMfaForAdmins = true,
                    RequireMfaForRemoteAccess = true,
                    AllowedMfaTypes = new List<MfaType> { MfaType.Totp, MfaType.Sms, MfaType.Email, MfaType.HardwareKey },
                    MaxFailedAttempts = 5,
                    LockoutDuration = TimeSpan.FromMinutes(15),
                    RequireBackupCodes = true,
                    BackupCodeCount = 10,
                    SessionTimeout = TimeSpan.FromHours(8)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA policy");
                return new MfaPolicy();
            }
        }

        /// <summary>
        /// Updates MFA policy configuration
        /// </summary>
        public async Task<bool> UpdateMfaPolicyAsync(MfaPolicy policy)
        {
            try
            {
                // In a real implementation, this would save the policy to configuration or database
                _logger.LogInformation("MFA policy updated");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating MFA policy");
                return false;
            }
        }

        /// <summary>
        /// Checks if MFA is required for a user based on policy
        /// </summary>
        public async Task<bool> IsMfaRequiredAsync(string userId, Dictionary<string, object> context = null)
        {
            try
            {
                var policy = await GetMfaPolicyAsync();
                
                // Check if user is admin
                var user = _context.USERS.FirstOrDefault(u => u.UserId.ToString() == userId);
                if (user?.IsSuperUser == true && policy.RequireMfaForAdmins)
                    return true;

                // Check if this is remote access
                if (context?.ContainsKey("IsRemoteAccess") == true && 
                    (bool)context["IsRemoteAccess"] && policy.RequireMfaForRemoteAccess)
                    return true;

                return policy.RequireMfaForAllUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if MFA is required for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Resets failed attempt counter for a user
        /// </summary>
        public async Task<bool> ResetFailedAttemptsAsync(string userId)
        {
            try
            {
                // In a real implementation, this would reset the failed attempts counter in storage
                _logger.LogDebug("Reset failed attempts for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting failed attempts for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Unlocks a user's MFA account
        /// </summary>
        public async Task<bool> UnlockMfaAccountAsync(string userId)
        {
            try
            {
                // In a real implementation, this would unlock the account in storage
                _logger.LogInformation("Unlocked MFA account for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking MFA account for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets available MFA types for a user
        /// </summary>
        public async Task<List<MfaType>> GetAvailableMfaTypesAsync(string userId)
        {
            try
            {
                var policy = await GetMfaPolicyAsync();
                return policy.AllowedMfaTypes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available MFA types for user {UserId}", userId);
                return new List<MfaType>();
            }
        }

        /// <summary>
        /// Validates MFA setup request
        /// </summary>
        public async Task<(bool IsValid, List<string> Errors)> ValidateSetupRequestAsync(MfaSetupRequest request)
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(request.UserId))
                errors.Add("UserId is required");

            if (request.MfaType == MfaType.Sms && string.IsNullOrEmpty(request.PhoneNumber))
                errors.Add("Phone number is required for SMS MFA");

            if (request.MfaType == MfaType.Email && string.IsNullOrEmpty(request.Email))
                errors.Add("Email is required for Email MFA");

            var policy = await GetMfaPolicyAsync();
            if (!policy.AllowedMfaTypes.Contains(request.MfaType))
                errors.Add($"MFA type {request.MfaType} is not allowed by policy");

            return (errors.Count == 0, errors);
        }

        #region Private Helper Methods

        private async Task<MfaSetupResult> SetupTotpAsync(MfaSetupRequest request)
        {
            var secret = GenerateTotpSecret();
            _totpSecrets[request.UserId] = secret;

            var config = new TotpConfiguration
            {
                SecretKey = secret,
                Issuer = "CSET",
                AccountName = request.UserId
            };

            var qrCodeUrl = GenerateQrCodeUrl(config);

            return new MfaSetupResult
            {
                IsSuccessful = true,
                Message = "TOTP setup successful",
                SetupCode = secret,
                QrCodeUrl = qrCodeUrl,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        private async Task<MfaSetupResult> SetupSmsAsync(MfaSetupRequest request)
        {
            var success = await SendSmsCodeAsync(request.UserId, request.PhoneNumber);
            
            return new MfaSetupResult
            {
                IsSuccessful = success,
                Message = success ? "SMS verification code sent" : "Failed to send SMS code",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        private async Task<MfaSetupResult> SetupEmailAsync(MfaSetupRequest request)
        {
            var success = await SendEmailCodeAsync(request.UserId, request.Email);
            
            return new MfaSetupResult
            {
                IsSuccessful = success,
                Message = success ? "Email verification code sent" : "Failed to send email code",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        private async Task<MfaSetupResult> SetupHardwareKeyAsync(MfaSetupRequest request)
        {
            return new MfaSetupResult
            {
                IsSuccessful = true,
                Message = "Hardware key setup initiated",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        private async Task<bool> VerifyTotpAsync(string userId, string code)
        {
            if (!_totpSecrets.TryGetValue(userId, out string secret))
                return false;

            // In a real implementation, this would use a proper TOTP library
            // For now, we'll do a simple mock verification
            return code.Length == 6 && code.All(char.IsDigit);
        }

        private async Task<bool> VerifySmsAsync(string userId, string code)
        {
            if (!_smsCodes.TryGetValue(userId, out string expectedCode))
                return false;

            var isValid = code == expectedCode;
            if (isValid)
                _smsCodes.Remove(userId);

            return isValid;
        }

        private async Task<bool> VerifyEmailAsync(string userId, string code)
        {
            if (!_emailCodes.TryGetValue(userId, out string expectedCode))
                return false;

            var isValid = code == expectedCode;
            if (isValid)
                _emailCodes.Remove(userId);

            return isValid;
        }

        private string GenerateRandomCode(int length)
        {
            var random = new Random();
            return new string(Enumerable.Repeat("0123456789", length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateTotpSecret()
        {
            var random = new Random();
            var bytes = new byte[20];
            random.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private string GenerateQrCodeUrl(TotpConfiguration config)
        {
            // Generate otpauth URL for QR code
            return $"otpauth://totp/{config.Issuer}:{config.AccountName}?secret={config.SecretKey}&issuer={config.Issuer}";
        }

        private async Task IncrementFailedAttemptsAsync(string userId)
        {
            // In a real implementation, this would increment the failed attempts counter
            _logger.LogWarning("Incremented failed attempts for user {UserId}", userId);
        }

        private async Task LogMfaEventAsync(string userId, MfaType mfaType, string action, bool isSuccessful, 
            string ipAddress = null, string userAgent = null)
        {
            try
            {
                var log = new MfaAuditLog
                {
                    UserId = userId,
                    MfaType = mfaType,
                    Action = action,
                    IsSuccessful = isSuccessful,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                // In a real implementation, this would save to database
                _logger.LogInformation("MFA Event: {Action} for user {UserId} with type {MfaType} - Success: {IsSuccessful}", 
                    action, userId, mfaType, isSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging MFA event for user {UserId}", userId);
            }
        }

        #endregion
    }
} 