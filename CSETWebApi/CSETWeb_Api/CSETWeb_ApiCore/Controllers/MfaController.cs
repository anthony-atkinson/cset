using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSETWebCore.Interfaces.Security;
using CSETWebCore.Model.Security;
using CSETWebCore.Interfaces.Helpers;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides Multi-Factor Authentication endpoints for the CSET application.
    /// Supports TOTP, SMS, Email, Hardware Keys, and other MFA types with comprehensive
    /// setup, verification, and management capabilities.
    /// </summary>
    [ApiController]
    [Authorize]
    public class MfaController : ControllerBase
    {
        private readonly IMfaService _mfaService;
        private readonly ITokenManager _tokenManager;
        private readonly ILogger<MfaController> _logger;

        /// <summary>
        /// Initializes a new instance of the MfaController.
        /// </summary>
        /// <param name="mfaService">Service for MFA operations</param>
        /// <param name="tokenManager">Service for token management</param>
        /// <param name="logger">Logger for controller operations</param>
        public MfaController(IMfaService mfaService, ITokenManager tokenManager, ILogger<MfaController> logger)
        {
            _mfaService = mfaService;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        /// <summary>
        /// Sets up MFA for the current user
        /// </summary>
        /// <param name="request">MFA setup request</param>
        /// <returns>
        /// 200 OK with setup result if successful
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if setup fails
        /// </returns>
        /// <remarks>
        /// Sets up Multi-Factor Authentication for the authenticated user.
        /// Supports TOTP, SMS, Email, and Hardware Key MFA types.
        /// 
        /// Sample request for TOTP:
        ///     POST /api/mfa/setup
        ///     {
        ///         "MfaType": "Totp",
        ///         "DeviceName": "My Phone"
        ///     }
        /// 
        /// Sample request for SMS:
        ///     POST /api/mfa/setup
        ///     {
        ///         "MfaType": "Sms",
        ///         "PhoneNumber": "+1234567890"
        ///     }
        /// 
        /// Sample request for Email:
        ///     POST /api/mfa/setup
        ///     {
        ///         "MfaType": "Email",
        ///         "Email": "user@example.com"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/setup")]
        [ProducesResponseType(typeof(MfaSetupResult), 200)]
        [ProducesResponseType(typeof(MfaSetupResult), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SetupMfa([FromBody] MfaSetupRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new MfaSetupResult
                    {
                        IsSuccessful = false,
                        Message = "Request cannot be null"
                    });
                }

                // Get current user ID from token
                var userId = _tokenManager.GetCurrentUserId().ToString();
                request.UserId = userId;

                var result = await _mfaService.SetupMfaAsync(request);

                if (result.IsSuccessful)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up MFA for user");
                return StatusCode(500, new MfaSetupResult
                {
                    IsSuccessful = false,
                    Message = "An error occurred during MFA setup"
                });
            }
        }

        /// <summary>
        /// Verifies an MFA code for the current user
        /// </summary>
        /// <param name="request">MFA verification request</param>
        /// <returns>
        /// 200 OK with verification result if successful
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if verification fails
        /// 500 Internal Server Error if verification fails
        /// </returns>
        /// <remarks>
        /// Verifies a Multi-Factor Authentication code for the authenticated user.
        /// Supports all configured MFA types including backup codes.
        /// 
        /// Sample request:
        ///     POST /api/mfa/verify
        ///     {
        ///         "MfaType": "Totp",
        ///         "Code": "123456"
        ///     }
        /// 
        /// Sample backup code request:
        ///     POST /api/mfa/verify
        ///     {
        ///         "MfaType": "BackupCode",
        ///         "Code": "ABCD1234"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/verify")]
        [ProducesResponseType(typeof(MfaVerificationResult), 200)]
        [ProducesResponseType(typeof(MfaVerificationResult), 400)]
        [ProducesResponseType(typeof(MfaVerificationResult), 401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> VerifyMfa([FromBody] MfaVerificationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new MfaVerificationResult
                    {
                        IsValid = false,
                        Message = "Request cannot be null"
                    });
                }

                // Get current user ID from token
                var userId = _tokenManager.GetCurrentUserId().ToString();
                request.UserId = userId;

                // Add request context
                request.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                request.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _mfaService.VerifyMfaAsync(request);

                if (result.IsValid)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(401, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying MFA for user");
                return StatusCode(500, new MfaVerificationResult
                {
                    IsValid = false,
                    Message = "An error occurred during MFA verification"
                });
            }
        }

        /// <summary>
        /// Gets MFA status for the current user
        /// </summary>
        /// <returns>
        /// 200 OK with MFA status if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if status retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns the current MFA status for the authenticated user including
        /// enabled types, lockout status, and backup code information.
        /// 
        /// Sample response:
        ///     {
        ///         "userId": "123",
        ///         "isEnabled": true,
        ///         "enabledTypes": ["Totp", "Sms"],
        ///         "isLocked": false,
        ///         "failedAttempts": 0,
        ///         "hasBackupCodes": true,
        ///         "remainingBackupCodes": 8
        ///     }
        /// </remarks>
        [HttpGet]
        [Route("api/mfa/status")]
        [ProducesResponseType(typeof(MfaStatus), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMfaStatus()
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var status = await _mfaService.GetMfaStatusAsync(userId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA status for user");
                return StatusCode(500, new { message = "An error occurred while retrieving MFA status" });
            }
        }

        /// <summary>
        /// Gets MFA configuration for the current user
        /// </summary>
        /// <returns>
        /// 200 OK with MFA configuration if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if configuration retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns the MFA configuration for the authenticated user including
        /// setup details and verification status.
        /// </remarks>
        [HttpGet]
        [Route("api/mfa/configuration")]
        [ProducesResponseType(typeof(MfaConfiguration), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMfaConfiguration()
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var config = await _mfaService.GetMfaConfigurationAsync(userId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA configuration for user");
                return StatusCode(500, new { message = "An error occurred while retrieving MFA configuration" });
            }
        }

        /// <summary>
        /// Disables MFA for the current user
        /// </summary>
        /// <param name="mfaType">Type of MFA to disable</param>
        /// <returns>
        /// 200 OK if MFA disabled successfully
        /// 400 Bad Request if MFA type is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if disable operation fails
        /// </returns>
        /// <remarks>
        /// Disables the specified MFA type for the authenticated user.
        /// 
        /// Sample request:
        ///     DELETE /api/mfa/disable/Totp
        /// </remarks>
        [HttpDelete]
        [Route("api/mfa/disable/{mfaType}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DisableMfa(MfaType mfaType)
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var success = await _mfaService.DisableMfaAsync(userId, mfaType);

                if (success)
                {
                    return Ok(new { message = $"MFA type {mfaType} disabled successfully" });
                }
                else
                {
                    return BadRequest(new { message = $"Failed to disable MFA type {mfaType}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disabling MFA for user");
                return StatusCode(500, new { message = "An error occurred while disabling MFA" });
            }
        }

        /// <summary>
        /// Generates backup codes for the current user
        /// </summary>
        /// <param name="count">Number of backup codes to generate (default: 10)</param>
        /// <returns>
        /// 200 OK with backup codes if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if generation fails
        /// </returns>
        /// <remarks>
        /// Generates new backup codes for the authenticated user.
        /// Backup codes can be used to access the account if primary MFA is unavailable.
        /// 
        /// Sample request:
        ///     POST /api/mfa/backup-codes?count=10
        /// 
        /// Sample response:
        ///     {
        ///         "userId": "123",
        ///         "codes": ["ABCD1234", "EFGH5678", ...],
        ///         "generatedAt": "2024-01-01T00:00:00Z",
        ///         "expiresAt": "2025-01-01T00:00:00Z"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/backup-codes")]
        [ProducesResponseType(typeof(MfaBackupCodes), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GenerateBackupCodes([FromQuery] int count = 10)
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var backupCodes = await _mfaService.GenerateBackupCodesAsync(userId, count);
                return Ok(backupCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating backup codes for user");
                return StatusCode(500, new { message = "An error occurred while generating backup codes" });
            }
        }

        /// <summary>
        /// Registers a hardware security key for the current user
        /// </summary>
        /// <param name="publicKey">Public key from the hardware key</param>
        /// <param name="deviceName">Name for the device</param>
        /// <returns>
        /// 200 OK with registration result if successful
        /// 400 Bad Request if parameters are invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if registration fails
        /// </returns>
        /// <remarks>
        /// Registers a hardware security key (FIDO2/WebAuthn) for the authenticated user.
        /// 
        /// Sample request:
        ///     POST /api/mfa/hardware-key
        ///     {
        ///         "publicKey": "base64-encoded-public-key",
        ///         "deviceName": "YubiKey 5C"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/hardware-key")]
        [ProducesResponseType(typeof(HardwareKeyRegistration), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterHardwareKey([FromBody] HardwareKeyRegistrationRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.PublicKey) || string.IsNullOrEmpty(request.DeviceName))
                {
                    return BadRequest(new { message = "Public key and device name are required" });
                }

                var userId = _tokenManager.GetCurrentUserId().ToString();
                var registration = await _mfaService.RegisterHardwareKeyAsync(userId, request.PublicKey, request.DeviceName);
                return Ok(registration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering hardware key for user");
                return StatusCode(500, new { message = "An error occurred while registering hardware key" });
            }
        }

        /// <summary>
        /// Sends an SMS verification code
        /// </summary>
        /// <param name="phoneNumber">Phone number to send code to</param>
        /// <returns>
        /// 200 OK if SMS sent successfully
        /// 400 Bad Request if phone number is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if SMS sending fails
        /// </returns>
        /// <remarks>
        /// Sends an SMS verification code to the specified phone number.
        /// 
        /// Sample request:
        ///     POST /api/mfa/send-sms?phoneNumber=+1234567890
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/send-sms")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendSmsCode([FromQuery] string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber))
                {
                    return BadRequest(new { message = "Phone number is required" });
                }

                var userId = _tokenManager.GetCurrentUserId().ToString();
                var success = await _mfaService.SendSmsCodeAsync(userId, phoneNumber);

                if (success)
                {
                    return Ok(new { message = "SMS verification code sent successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to send SMS verification code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS code to user");
                return StatusCode(500, new { message = "An error occurred while sending SMS code" });
            }
        }

        /// <summary>
        /// Sends an email verification code
        /// </summary>
        /// <param name="email">Email address to send code to</param>
        /// <returns>
        /// 200 OK if email sent successfully
        /// 400 Bad Request if email is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if email sending fails
        /// </returns>
        /// <remarks>
        /// Sends an email verification code to the specified email address.
        /// 
        /// Sample request:
        ///     POST /api/mfa/send-email?email=user@example.com
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/send-email")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SendEmailCode([FromQuery] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message = "Email address is required" });
                }

                var userId = _tokenManager.GetCurrentUserId().ToString();
                var success = await _mfaService.SendEmailCodeAsync(userId, email);

                if (success)
                {
                    return Ok(new { message = "Email verification code sent successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to send email verification code" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email code to user");
                return StatusCode(500, new { message = "An error occurred while sending email code" });
            }
        }

        /// <summary>
        /// Gets MFA audit logs for the current user
        /// </summary>
        /// <param name="startDate">Start date for logs (optional)</param>
        /// <param name="endDate">End date for logs (optional)</param>
        /// <returns>
        /// 200 OK with audit logs if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if log retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns MFA audit logs for the authenticated user within the specified date range.
        /// 
        /// Sample request:
        ///     GET /api/mfa/audit-logs?startDate=2024-01-01&endDate=2024-01-31
        /// </remarks>
        [HttpGet]
        [Route("api/mfa/audit-logs")]
        [ProducesResponseType(typeof(List<MfaAuditLog>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var logs = await _mfaService.GetAuditLogsAsync(userId, startDate, endDate);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting audit logs for user");
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs" });
            }
        }

        /// <summary>
        /// Gets MFA policy configuration
        /// </summary>
        /// <returns>
        /// 200 OK with policy configuration if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if policy retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns the current MFA policy configuration including requirements and allowed types.
        /// </remarks>
        [HttpGet]
        [Route("api/mfa/policy")]
        [ProducesResponseType(typeof(MfaPolicy), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetMfaPolicy()
        {
            try
            {
                var policy = await _mfaService.GetMfaPolicyAsync();
                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting MFA policy");
                return StatusCode(500, new { message = "An error occurred while retrieving MFA policy" });
            }
        }

        /// <summary>
        /// Updates MFA policy configuration (Admin only)
        /// </summary>
        /// <param name="policy">Updated policy configuration</param>
        /// <returns>
        /// 200 OK if policy updated successfully
        /// 400 Bad Request if policy is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user is not admin
        /// 500 Internal Server Error if policy update fails
        /// </returns>
        /// <remarks>
        /// Updates the MFA policy configuration. Requires admin privileges.
        /// 
        /// Sample request:
        ///     PUT /api/mfa/policy
        ///     {
        ///         "requireMfaForAllUsers": true,
        ///         "requireMfaForAdmins": true,
        ///         "allowedMfaTypes": ["Totp", "Sms", "Email"],
        ///         "maxFailedAttempts": 5,
        ///         "lockoutDuration": "00:15:00"
        ///     }
        /// </remarks>
        [HttpPut]
        [Route("api/mfa/policy")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateMfaPolicy([FromBody] MfaPolicy policy)
        {
            try
            {
                if (policy == null)
                {
                    return BadRequest(new { message = "Policy configuration cannot be null" });
                }

                var success = await _mfaService.UpdateMfaPolicyAsync(policy);

                if (success)
                {
                    return Ok(new { message = "MFA policy updated successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update MFA policy" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating MFA policy");
                return StatusCode(500, new { message = "An error occurred while updating MFA policy" });
            }
        }

        /// <summary>
        /// Gets available MFA types for the current user
        /// </summary>
        /// <returns>
        /// 200 OK with available MFA types if successful
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if retrieval fails
        /// </returns>
        /// <remarks>
        /// Returns the list of MFA types available to the authenticated user based on policy.
        /// </remarks>
        [HttpGet]
        [Route("api/mfa/available-types")]
        [ProducesResponseType(typeof(List<MfaType>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAvailableMfaTypes()
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var types = await _mfaService.GetAvailableMfaTypesAsync(userId);
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available MFA types for user");
                return StatusCode(500, new { message = "An error occurred while retrieving available MFA types" });
            }
        }

        /// <summary>
        /// Unlocks MFA account for the current user
        /// </summary>
        /// <returns>
        /// 200 OK if account unlocked successfully
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if unlock operation fails
        /// </returns>
        /// <remarks>
        /// Unlocks the MFA account for the authenticated user if it was previously locked.
        /// </remarks>
        [HttpPost]
        [Route("api/mfa/unlock")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UnlockMfaAccount()
        {
            try
            {
                var userId = _tokenManager.GetCurrentUserId().ToString();
                var success = await _mfaService.UnlockMfaAccountAsync(userId);

                if (success)
                {
                    return Ok(new { message = "MFA account unlocked successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Failed to unlock MFA account" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking MFA account for user");
                return StatusCode(500, new { message = "An error occurred while unlocking MFA account" });
            }
        }
    }

    /// <summary>
    /// Request model for hardware key registration
    /// </summary>
    public class HardwareKeyRegistrationRequest
    {
        public string PublicKey { get; set; }
        public string DeviceName { get; set; }
    }
} 