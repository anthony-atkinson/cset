//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSETWebCore.Business.Collaboration;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Business.Authorization;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Provides API endpoints for collaboration features including history tracking,
    /// statistics, and conflict resolution for multi-user assessment editing.
    /// </summary>
    [ApiController]
    [CsetAuthorize]
    public class CollaborationController : ControllerBase
    {
        private readonly ILogger<CollaborationController> _logger;
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        private readonly CollaborationManager _collaborationManager;

        /// <summary>
        /// Initializes a new instance of the CollaborationController.
        /// </summary>
        /// <param name="logger">Logger for collaboration operations</param>
        /// <param name="tokenManager">Token manager for authentication</param>
        /// <param name="context">Database context</param>
        /// <param name="collaborationManager">Collaboration business logic manager</param>
        public CollaborationController(
            ILogger<CollaborationController> logger,
            ITokenManager tokenManager,
            CSETContext context,
            CollaborationManager collaborationManager)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _context = context;
            _collaborationManager = collaborationManager;
        }

        /// <summary>
        /// Gets collaboration history for an assessment.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>
        /// 200 OK with collaboration history
        /// 400 Bad Request if parameters are invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Retrieves a chronological list of collaboration activities for the specified assessment.
        /// Activities include user joins, edits, comments, and conflict resolutions.
        /// </remarks>
        [HttpGet]
        [Route("api/collaboration/history/{assessmentId}")]
        public async Task<IActionResult> GetCollaborationHistory(
            int assessmentId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Validate assessment access
                var currentAssessmentId = _tokenManager.AssessmentForUser();
                if (currentAssessmentId != assessmentId)
                {
                    return Forbid("Access denied to this assessment");
                }

                // Validate permissions
                var userId = _tokenManager.GetCurrentUserId();
                if (!await _collaborationManager.ValidatePermissionAsync(assessmentId, userId, CollaborationPermission.ViewHistory))
                {
                    return Forbid("Insufficient permissions to view collaboration history");
                }

                var history = await _collaborationManager.GetCollaborationHistoryAsync(assessmentId, startDate, endDate);

                return Ok(new
                {
                    AssessmentId = assessmentId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Activities = history,
                    TotalCount = history.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collaboration history for assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Failed to retrieve collaboration history");
            }
        }

        /// <summary>
        /// Gets collaboration statistics for an assessment.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>
        /// 200 OK with collaboration statistics
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Returns comprehensive statistics about collaboration activities including
        /// total activities, unique users, most active user, and conflict count.
        /// </remarks>
        [HttpGet]
        [Route("api/collaboration/statistics/{assessmentId}")]
        public async Task<IActionResult> GetCollaborationStatistics(int assessmentId)
        {
            try
            {
                // Validate assessment access
                var currentAssessmentId = _tokenManager.AssessmentForUser();
                if (currentAssessmentId != assessmentId)
                {
                    return Forbid("Access denied to this assessment");
                }

                // Validate permissions
                var userId = _tokenManager.GetCurrentUserId();
                if (!await _collaborationManager.ValidatePermissionAsync(assessmentId, userId, CollaborationPermission.View))
                {
                    return Forbid("Insufficient permissions to view collaboration statistics");
                }

                var statistics = await _collaborationManager.GetCollaborationStatisticsAsync(assessmentId);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting collaboration statistics for assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Failed to retrieve collaboration statistics");
            }
        }

        /// <summary>
        /// Resolves a collaboration conflict using the specified strategy.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="request">Conflict resolution request</param>
        /// <returns>
        /// 200 OK with resolution result
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Resolves conflicts between simultaneous edits using the specified resolution strategy.
        /// Available strategies include KeepLatest, KeepEarliest, KeepMostComplete, and Manual.
        /// </remarks>
        [HttpPost]
        [Route("api/collaboration/resolve-conflict/{assessmentId}")]
        public async Task<IActionResult> ResolveConflict(int assessmentId, [FromBody] ConflictResolutionRequest request)
        {
            try
            {
                // Validate assessment access
                var currentAssessmentId = _tokenManager.AssessmentForUser();
                if (currentAssessmentId != assessmentId)
                {
                    return Forbid("Access denied to this assessment");
                }

                // Validate permissions
                var userId = _tokenManager.GetCurrentUserId();
                if (!await _collaborationManager.ValidatePermissionAsync(assessmentId, userId, CollaborationPermission.ResolveConflicts))
                {
                    return Forbid("Insufficient permissions to resolve conflicts");
                }

                // Validate request
                if (request == null || request.ConflictData == null)
                {
                    return BadRequest("Invalid conflict resolution request");
                }

                var resolution = await _collaborationManager.ResolveConflictAsync(
                    assessmentId,
                    request.ConflictData,
                    request.ResolutionStrategy,
                    userId);

                if (!resolution.Success)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        ErrorMessage = resolution.ErrorMessage
                    });
                }

                return Ok(resolution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict for assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Failed to resolve conflict");
            }
        }

        /// <summary>
        /// Records a collaboration activity for audit trail.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="request">Activity recording request</param>
        /// <returns>
        /// 200 OK with recorded activity
        /// 400 Bad Request if request is invalid
        /// 401 Unauthorized if user is not authenticated
        /// 403 Forbidden if user lacks permission
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Records a collaboration activity for audit trail purposes.
        /// This endpoint is typically called by the SignalR hub when activities occur.
        /// </remarks>
        [HttpPost]
        [Route("api/collaboration/record-activity/{assessmentId}")]
        public async Task<IActionResult> RecordActivity(int assessmentId, [FromBody] ActivityRecordingRequest request)
        {
            try
            {
                // Validate assessment access
                var currentAssessmentId = _tokenManager.AssessmentForUser();
                if (currentAssessmentId != assessmentId)
                {
                    return Forbid("Access denied to this assessment");
                }

                // Validate request
                if (request == null || string.IsNullOrEmpty(request.ActivityType))
                {
                    return BadRequest("Invalid activity recording request");
                }

                var userId = _tokenManager.GetCurrentUserId();
                var activity = await _collaborationManager.RecordActivityAsync(
                    assessmentId,
                    userId,
                    request.ActivityType,
                    request.ActivityData);

                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording activity for assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Failed to record activity");
            }
        }

        /// <summary>
        /// Gets user permissions for collaboration features.
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <returns>
        /// 200 OK with user permissions
        /// 401 Unauthorized if user is not authenticated
        /// 404 Not Found if assessment doesn't exist
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Returns the current user's permissions for collaboration features on the specified assessment.
        /// </remarks>
        [HttpGet]
        [Route("api/collaboration/permissions/{assessmentId}")]
        public async Task<IActionResult> GetUserPermissions(int assessmentId)
        {
            try
            {
                // Validate assessment access
                var currentAssessmentId = _tokenManager.AssessmentForUser();
                if (currentAssessmentId != assessmentId)
                {
                    return Forbid("Access denied to this assessment");
                }

                var userId = _tokenManager.GetCurrentUserId();
                var permissions = new Dictionary<CollaborationPermission, bool>();

                // Check each permission
                foreach (CollaborationPermission permission in Enum.GetValues(typeof(CollaborationPermission)))
                {
                    permissions[permission] = await _collaborationManager.ValidatePermissionAsync(assessmentId, userId, permission);
                }

                return Ok(new
                {
                    AssessmentId = assessmentId,
                    UserId = userId,
                    Permissions = permissions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions for assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Failed to retrieve user permissions");
            }
        }

        /// <summary>
        /// Gets active collaboration sessions.
        /// </summary>
        /// <returns>
        /// 200 OK with active sessions
        /// 401 Unauthorized if user is not authenticated
        /// 500 Internal Server Error if operation fails
        /// </returns>
        /// <remarks>
        /// Returns information about currently active collaboration sessions.
        /// This is primarily for administrative purposes.
        /// </remarks>
        [HttpGet]
        [Route("api/collaboration/active-sessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            try
            {
                // This would return active collaboration sessions
                // For now, we'll return a placeholder
                var sessions = new List<object>();

                return Ok(new
                {
                    ActiveSessions = sessions,
                    TotalCount = sessions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active collaboration sessions");
                return StatusCode(500, "Failed to retrieve active sessions");
            }
        }
    }

    /// <summary>
    /// Request model for conflict resolution
    /// </summary>
    public class ConflictResolutionRequest
    {
        public object ConflictData { get; set; }
        public ConflictResolutionStrategy ResolutionStrategy { get; set; }
    }

    /// <summary>
    /// Request model for activity recording
    /// </summary>
    public class ActivityRecordingRequest
    {
        public string ActivityType { get; set; }
        public object ActivityData { get; set; }
    }
} 