//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using CSETWebCore.ApiCore.Security.ZeroTrust;
using CSETWebCore.Model.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides Zero Trust Architecture endpoints for the CSET application.
    /// Implements continuous verification, least privilege access, and micro-segmentation.
    /// </summary>
    [ApiController]
    [Authorize]
    public class ZeroTrustController : ControllerBase
    {
        private readonly IZeroTrustService _zeroTrustService;
        private readonly ILogger<ZeroTrustController> _logger;

        /// <summary>
        /// Initializes a new instance of the ZeroTrustController.
        /// </summary>
        /// <param name="zeroTrustService">Service for zero trust operations</param>
        /// <param name="logger">Logger for controller operations</param>
        public ZeroTrustController(IZeroTrustService zeroTrustService, ILogger<ZeroTrustController> logger)
        {
            _zeroTrustService = zeroTrustService;
            _logger = logger;
        }

        /// <summary>
        /// Validates user access based on zero trust principles
        /// </summary>
        /// <param name="request">Access request details</param>
        /// <returns>
        /// 200 OK with validation result if successful
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if access is denied
        /// 500 Internal Server Error if validation fails
        /// </returns>
        /// <remarks>
        /// This endpoint performs comprehensive zero trust validation including:
        /// - Risk assessment
        /// - Security posture evaluation
        /// - Micro-segmentation checks
        /// - Continuous verification
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/validate
        ///     {
        ///         "UserId": "user123",
        ///         "ResourceId": "assessment-data",
        ///         "Action": "read",
        ///         "ClientIp": "192.168.1.100",
        ///         "DeviceId": "device-001",
        ///         "Location": "Office"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/validate")]
        [ProducesResponseType(typeof(ZeroTrustValidationResult), 200)]
        [ProducesResponseType(typeof(ZeroTrustValidationResult), 400)]
        [ProducesResponseType(typeof(ZeroTrustValidationResult), 401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ValidateAccess([FromBody] ZeroTrustAccessRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ZeroTrustValidationResult
                    {
                        IsAllowed = false,
                        Reason = "Request cannot be null"
                    });
                }

                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.ResourceId))
                {
                    return BadRequest(new ZeroTrustValidationResult
                    {
                        IsAllowed = false,
                        Reason = "UserId and ResourceId are required"
                    });
                }

                var result = await _zeroTrustService.ValidateAccessAsync(request);

                if (result.IsAllowed)
                {
                    return Ok(result);
                }
                else
                {
                    return Unauthorized(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating access for user {UserId}", request?.UserId);
                return StatusCode(500, new ZeroTrustValidationResult
                {
                    IsAllowed = false,
                    Reason = "Internal server error during validation"
                });
            }
        }

        /// <summary>
        /// Performs continuous verification of user session
        /// </summary>
        /// <param name="sessionId">Session identifier</param>
        /// <returns>
        /// 200 OK with verification result if successful
        /// 400 Bad Request if session ID is invalid
        /// 404 Not Found if session doesn't exist
        /// 500 Internal Server Error if verification fails
        /// </returns>
        /// <remarks>
        /// This endpoint performs continuous verification of an active session,
        /// checking for suspicious activity, device compliance, and session validity.
        /// 
        /// Sample request:
        ///     GET /api/zerotrust/verify/session-123
        /// </remarks>
        [HttpGet]
        [Route("api/zerotrust/verify/{sessionId}")]
        [ProducesResponseType(typeof(ZeroTrustVerificationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> VerifySession(string sessionId)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionId))
                {
                    return BadRequest("Session ID is required");
                }

                var result = await _zeroTrustService.VerifySessionAsync(sessionId);

                if (!result.IsValid && result.Warnings.Contains("Session not found"))
                {
                    return NotFound(new { message = "Session not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying session {SessionId}", sessionId);
                return StatusCode(500, new { message = "Internal server error during session verification" });
            }
        }

        /// <summary>
        /// Evaluates security posture for access decision
        /// </summary>
        /// <param name="context">Security context</param>
        /// <returns>
        /// 200 OK with posture evaluation result if successful
        /// 400 Bad Request if context is invalid
        /// 500 Internal Server Error if evaluation fails
        /// </returns>
        /// <remarks>
        /// This endpoint evaluates the security posture of users, devices, and networks
        /// to determine if they meet security requirements for access.
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/posture
        ///     {
        ///         "UserId": "user123",
        ///         "DeviceId": "device-001",
        ///         "Location": "Office",
        ///         "NetworkSegment": "internal"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/posture")]
        [ProducesResponseType(typeof(SecurityPostureResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> EvaluateSecurityPosture([FromBody] ZeroTrustContext context)
        {
            try
            {
                if (context == null)
                {
                    return BadRequest("Security context cannot be null");
                }

                if (string.IsNullOrEmpty(context.UserId))
                {
                    return BadRequest("UserId is required");
                }

                var result = await _zeroTrustService.EvaluateSecurityPostureAsync(context);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating security posture for user {UserId}", context?.UserId);
                return StatusCode(500, new { message = "Internal server error during posture evaluation" });
            }
        }

        /// <summary>
        /// Provisions just-in-time access
        /// </summary>
        /// <param name="request">JIT access request</param>
        /// <returns>
        /// 200 OK with JIT access result if successful
        /// 400 Bad Request if request is invalid
        /// 403 Forbidden if access is denied
        /// 500 Internal Server Error if provisioning fails
        /// </returns>
        /// <remarks>
        /// This endpoint provisions temporary access to resources based on just-in-time principles.
        /// Access may be auto-approved or require manual approval depending on risk level.
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/jit/provision
        ///     {
        ///         "UserId": "user123",
        ///         "ResourceId": "admin-panel",
        ///         "Reason": "Emergency maintenance",
        ///         "Duration": "01:00:00"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/jit/provision")]
        [ProducesResponseType(typeof(JustInTimeAccessResult), 200)]
        [ProducesResponseType(typeof(JustInTimeAccessResult), 400)]
        [ProducesResponseType(typeof(JustInTimeAccessResult), 403)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ProvisionJustInTimeAccess([FromBody] JustInTimeAccessRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new JustInTimeAccessResult
                    {
                        IsApproved = false,
                        Reason = "Request cannot be null"
                    });
                }

                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.ResourceId))
                {
                    return BadRequest(new JustInTimeAccessResult
                    {
                        IsApproved = false,
                        Reason = "UserId and ResourceId are required"
                    });
                }

                var result = await _zeroTrustService.ProvisionJustInTimeAccessAsync(request);

                if (result.IsApproved)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(403, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning JIT access for user {UserId}", request?.UserId);
                return StatusCode(500, new JustInTimeAccessResult
                {
                    IsApproved = false,
                    Reason = "Internal server error during JIT provisioning"
                });
            }
        }

        /// <summary>
        /// Performs risk assessment for access request
        /// </summary>
        /// <param name="request">Access request</param>
        /// <returns>
        /// 200 OK with risk assessment result if successful
        /// 400 Bad Request if request is invalid
        /// 500 Internal Server Error if assessment fails
        /// </returns>
        /// <remarks>
        /// This endpoint performs comprehensive risk assessment including user risk,
        /// resource risk, and environmental risk factors.
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/risk/assess
        ///     {
        ///         "UserId": "user123",
        ///         "ResourceId": "sensitive-data",
        ///         "Action": "write",
        ///         "ClientIp": "192.168.1.100"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/risk/assess")]
        [ProducesResponseType(typeof(RiskAssessmentResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> AssessRisk([FromBody] ZeroTrustAccessRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Access request cannot be null");
                }

                if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.ResourceId))
                {
                    return BadRequest("UserId and ResourceId are required");
                }

                var result = await _zeroTrustService.AssessRiskAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assessing risk for user {UserId}", request?.UserId);
                return StatusCode(500, new { message = "Internal server error during risk assessment" });
            }
        }

        /// <summary>
        /// Applies micro-segmentation policies to network access
        /// </summary>
        /// <param name="request">Network access request</param>
        /// <returns>
        /// 200 OK with micro-segmentation result if successful
        /// 400 Bad Request if request is invalid
        /// 500 Internal Server Error if policy application fails
        /// </returns>
        /// <remarks>
        /// This endpoint applies network segmentation policies to control access
        /// between different network segments and applications.
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/network/segment
        ///     {
        ///         "SourceIp": "192.168.1.100",
        ///         "DestinationIp": "10.0.0.50",
        ///         "Port": 443,
        ///         "Protocol": "HTTPS",
        ///         "UserId": "user123",
        ///         "Application": "CSET"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/network/segment")]
        [ProducesResponseType(typeof(MicroSegmentationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> ApplyMicroSegmentation([FromBody] NetworkAccessRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Network access request cannot be null");
                }

                if (string.IsNullOrEmpty(request.SourceIp) || string.IsNullOrEmpty(request.DestinationIp))
                {
                    return BadRequest("SourceIp and DestinationIp are required");
                }

                var result = await _zeroTrustService.ApplyMicroSegmentationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying micro-segmentation for user {UserId}", request?.UserId);
                return StatusCode(500, new { message = "Internal server error during micro-segmentation" });
            }
        }

        /// <summary>
        /// Logs a zero trust event
        /// </summary>
        /// <param name="event">Zero trust event</param>
        /// <returns>
        /// 200 OK if event logged successfully
        /// 400 Bad Request if event is invalid
        /// 500 Internal Server Error if logging fails
        /// </returns>
        /// <remarks>
        /// This endpoint logs zero trust events for monitoring and analytics purposes.
        /// 
        /// Sample request:
        ///     POST /api/zerotrust/events/log
        ///     {
        ///         "EventType": "AccessValidation",
        ///         "UserId": "user123",
        ///         "ResourceId": "assessment-data",
        ///         "Action": "read",
        ///         "Success": true,
        ///         "Details": "Access granted"
        ///     }
        /// </remarks>
        [HttpPost]
        [Route("api/zerotrust/events/log")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LogEvent([FromBody] ZeroTrustEvent @event)
        {
            try
            {
                if (@event == null)
                {
                    return BadRequest("Event cannot be null");
                }

                if (string.IsNullOrEmpty(@event.EventType))
                {
                    return BadRequest("EventType is required");
                }

                var result = await _zeroTrustService.LogZeroTrustEventAsync(@event);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging zero trust event {EventId}", @event?.EventId);
                return StatusCode(500, new { message = "Internal server error during event logging" });
            }
        }

        /// <summary>
        /// Gets zero trust analytics and metrics
        /// </summary>
        /// <param name="startTime">Start time for analytics (ISO 8601 format)</param>
        /// <param name="endTime">End time for analytics (ISO 8601 format)</param>
        /// <param name="timeRangeType">Type of time range (optional)</param>
        /// <returns>
        /// 200 OK with analytics data if successful
        /// 400 Bad Request if time range is invalid
        /// 500 Internal Server Error if analytics generation fails
        /// </returns>
        /// <remarks>
        /// This endpoint provides comprehensive analytics and metrics for zero trust operations.
        /// 
        /// Sample request:
        ///     GET /api/zerotrust/analytics?startTime=2024-01-01T00:00:00Z&endTime=2024-01-31T23:59:59Z
        /// </remarks>
        [HttpGet]
        [Route("api/zerotrust/analytics")]
        [ProducesResponseType(typeof(ZeroTrustAnalytics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetAnalytics(
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] TimeRangeType timeRangeType = TimeRangeType.Custom)
        {
            try
            {
                if (startTime >= endTime)
                {
                    return BadRequest("Start time must be before end time");
                }

                var timeRange = new TimeRange
                {
                    Start = startTime,
                    End = endTime,
                    Type = timeRangeType
                };

                var result = await _zeroTrustService.GetAnalyticsAsync(timeRange);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for time range {StartTime} to {EndTime}", startTime, endTime);
                return StatusCode(500, new { message = "Internal server error during analytics generation" });
            }
        }

        /// <summary>
        /// Gets zero trust health status
        /// </summary>
        /// <returns>
        /// 200 OK with health status if successful
        /// 500 Internal Server Error if health check fails
        /// </returns>
        /// <remarks>
        /// This endpoint provides health status information for the zero trust system.
        /// </remarks>
        [HttpGet]
        [Route("api/zerotrust/health")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(500)]
        public IActionResult GetHealth()
        {
            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Components = new
                    {
                        RiskAssessment = "Operational",
                        SecurityPosture = "Operational",
                        MicroSegmentation = "Operational",
                        JustInTimeAccess = "Operational",
                        EventLogging = "Operational",
                        Analytics = "Operational"
                    }
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting zero trust health status");
                return StatusCode(500, new { message = "Internal server error during health check" });
            }
        }
    }
} 