//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using CSETWebCore.Interfaces.Helpers;
using CSETWebCore.DataLayer.Model;
using System.Text.Json;

namespace CSETWeb_ApiCore.Hubs
{
    /// <summary>
    /// SignalR hub for real-time collaboration features in CSET assessments.
    /// Provides user presence, real-time updates, collaborative commenting,
    /// and conflict resolution for multi-user assessment editing.
    /// </summary>
    public class CollaborationHub : Hub
    {
        private readonly ILogger<CollaborationHub> _logger;
        private readonly ITokenManager _tokenManager;
        private readonly CSETContext _context;
        
        // Static collections to track user presence and active sessions
        private static readonly ConcurrentDictionary<string, UserPresence> _userPresence = new();
        private static readonly ConcurrentDictionary<int, AssessmentSession> _activeSessions = new();
        private static readonly ConcurrentDictionary<string, List<string>> _userConnections = new();

        public CollaborationHub(ILogger<CollaborationHub> logger, ITokenManager tokenManager, CSETContext context)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _context = context;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized connection attempt from {ConnectionId}", Context.ConnectionId);
                    Context.Abort();
                    return;
                }

                // Track user connection
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new List<string>();
                }
                _userConnections[userId].Add(Context.ConnectionId);

                _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, Context.ConnectionId);
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    // Remove connection from tracking
                    if (_userConnections.ContainsKey(userId))
                    {
                        _userConnections[userId].Remove(Context.ConnectionId);
                        if (_userConnections[userId].Count == 0)
                        {
                            _userConnections.TryRemove(userId, out _);
                            _userPresence.TryRemove(userId, out _);
                        }
                    }

                    // Notify other users about disconnection
                    await NotifyUserDisconnected(userId);
                }

                _logger.LogInformation("User {UserId} disconnected with connection {ConnectionId}", userId, Context.ConnectionId);
                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
                throw;
            }
        }

        /// <summary>
        /// Join an assessment session for real-time collaboration
        /// </summary>
        /// <param name="assessmentId">Assessment ID to join</param>
        /// <param name="userDisplayName">User's display name</param>
        public async Task JoinAssessment(int assessmentId, string userDisplayName)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                // Validate assessment access
                if (!await HasAssessmentAccess(assessmentId, userId))
                {
                    throw new UnauthorizedAccessException($"User {userId} does not have access to assessment {assessmentId}");
                }

                // Add user to assessment group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");

                // Update or create user presence
                var presence = new UserPresence
                {
                    UserId = userId,
                    DisplayName = userDisplayName,
                    AssessmentId = assessmentId,
                    ConnectedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow
                };
                _userPresence[userId] = presence;

                // Get or create assessment session
                var session = _activeSessions.GetOrAdd(assessmentId, id => new AssessmentSession
                {
                    AssessmentId = id,
                    ActiveUsers = new ConcurrentDictionary<string, UserPresence>(),
                    LastActivity = DateTime.UtcNow
                });

                session.ActiveUsers[userId] = presence;
                session.LastActivity = DateTime.UtcNow;

                // Notify other users in the assessment
                await Clients.Group($"assessment_{assessmentId}").SendAsync("UserJoined", presence);

                // Send current active users to the joining user
                var activeUsers = session.ActiveUsers.Values.ToList();
                await Clients.Caller.SendAsync("AssessmentJoined", assessmentId, activeUsers);

                _logger.LogInformation("User {UserId} joined assessment {AssessmentId}", userId, assessmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining assessment {AssessmentId} for user {UserId}", assessmentId, GetCurrentUserId());
                throw;
            }
        }

        /// <summary>
        /// Leave an assessment session
        /// </summary>
        /// <param name="assessmentId">Assessment ID to leave</param>
        public async Task LeaveAssessment(int assessmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                // Remove user from assessment group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");

                // Remove user from session
                if (_activeSessions.TryGetValue(assessmentId, out var session))
                {
                    session.ActiveUsers.TryRemove(userId, out _);
                    
                    // Remove session if no active users
                    if (session.ActiveUsers.IsEmpty)
                    {
                        _activeSessions.TryRemove(assessmentId, out _);
                    }
                }

                // Remove user presence
                _userPresence.TryRemove(userId, out var presence);

                // Notify other users
                if (presence != null)
                {
                    await Clients.Group($"assessment_{assessmentId}").SendAsync("UserLeft", presence);
                }

                _logger.LogInformation("User {UserId} left assessment {AssessmentId}", userId, assessmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving assessment {AssessmentId} for user {UserId}", assessmentId, GetCurrentUserId());
                throw;
            }
        }

        /// <summary>
        /// Send real-time assessment update to other users
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="updateType">Type of update (question, finding, document, etc.)</param>
        /// <param name="updateData">Update data</param>
        /// <param name="userId">User ID making the update</param>
        public async Task SendAssessmentUpdate(int assessmentId, string updateType, object updateData, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    userId = GetCurrentUserId();
                }

                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                // Validate assessment access
                if (!await HasAssessmentAccess(assessmentId, userId))
                {
                    throw new UnauthorizedAccessException($"User {userId} does not have access to assessment {assessmentId}");
                }

                var update = new AssessmentUpdate
                {
                    AssessmentId = assessmentId,
                    UpdateType = updateType,
                    UpdateData = updateData,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                // Send update to all users in the assessment (except sender)
                await Clients.OthersInGroup($"assessment_{assessmentId}").SendAsync("AssessmentUpdated", update);

                // Update user activity
                if (_userPresence.TryGetValue(userId, out var presence))
                {
                    presence.LastActivity = DateTime.UtcNow;
                }

                _logger.LogDebug("Assessment update sent: {UpdateType} for assessment {AssessmentId} by user {UserId}", 
                    updateType, assessmentId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending assessment update for assessment {AssessmentId} by user {UserId}", 
                    assessmentId, GetCurrentUserId());
                throw;
            }
        }

        /// <summary>
        /// Send collaborative comment to other users
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="comment">Comment data</param>
        public async Task SendComment(int assessmentId, object comment)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                // Validate assessment access
                if (!await HasAssessmentAccess(assessmentId, userId))
                {
                    throw new UnauthorizedAccessException($"User {userId} does not have access to assessment {assessmentId}");
                }

                var commentData = new CollaborativeComment
                {
                    AssessmentId = assessmentId,
                    UserId = userId,
                    Comment = comment,
                    Timestamp = DateTime.UtcNow
                };

                // Send comment to all users in the assessment
                await Clients.Group($"assessment_{assessmentId}").SendAsync("CommentAdded", commentData);

                // Update user activity
                if (_userPresence.TryGetValue(userId, out var presence))
                {
                    presence.LastActivity = DateTime.UtcNow;
                }

                _logger.LogDebug("Comment sent for assessment {AssessmentId} by user {UserId}", assessmentId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending comment for assessment {AssessmentId} by user {UserId}", 
                    assessmentId, GetCurrentUserId());
                throw;
            }
        }

        /// <summary>
        /// Send live editing indicator to other users
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="editingInfo">Information about what is being edited</param>
        public async Task SendEditingIndicator(int assessmentId, object editingInfo)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                var indicator = new EditingIndicator
                {
                    AssessmentId = assessmentId,
                    UserId = userId,
                    EditingInfo = editingInfo,
                    Timestamp = DateTime.UtcNow
                };

                // Send to other users in the assessment
                await Clients.OthersInGroup($"assessment_{assessmentId}").SendAsync("UserEditing", indicator);

                // Update user activity
                if (_userPresence.TryGetValue(userId, out var presence))
                {
                    presence.LastActivity = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending editing indicator for assessment {AssessmentId} by user {UserId}", 
                    assessmentId, GetCurrentUserId());
            }
        }

        /// <summary>
        /// Clear editing indicator when user stops editing
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        public async Task ClearEditingIndicator(int assessmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }

                // Notify other users that editing has stopped
                await Clients.OthersInGroup($"assessment_{assessmentId}").SendAsync("UserStoppedEditing", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing editing indicator for assessment {AssessmentId} by user {UserId}", 
                    assessmentId, GetCurrentUserId());
            }
        }

        /// <summary>
        /// Handle conflict resolution for simultaneous edits
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        /// <param name="conflictData">Conflict information</param>
        public async Task ResolveConflict(int assessmentId, object conflictData)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                var resolution = new ConflictResolution
                {
                    AssessmentId = assessmentId,
                    ResolvedBy = userId,
                    ConflictData = conflictData,
                    Timestamp = DateTime.UtcNow
                };

                // Send resolution to all users in the assessment
                await Clients.Group($"assessment_{assessmentId}").SendAsync("ConflictResolved", resolution);

                _logger.LogInformation("Conflict resolved for assessment {AssessmentId} by user {UserId}", assessmentId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving conflict for assessment {AssessmentId} by user {UserId}", 
                    assessmentId, GetCurrentUserId());
                throw;
            }
        }

        /// <summary>
        /// Get current active users in an assessment
        /// </summary>
        /// <param name="assessmentId">Assessment ID</param>
        public async Task<List<UserPresence>> GetActiveUsers(int assessmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }

                if (_activeSessions.TryGetValue(assessmentId, out var session))
                {
                    return session.ActiveUsers.Values.ToList();
                }

                return new List<UserPresence>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active users for assessment {AssessmentId}", assessmentId);
                throw;
            }
        }

        /// <summary>
        /// Update user activity timestamp
        /// </summary>
        public async Task UpdateActivity()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId) && _userPresence.TryGetValue(userId, out var presence))
                {
                    presence.LastActivity = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity for user {UserId}", GetCurrentUserId());
            }
        }

        /// <summary>
        /// Get current user ID from token
        /// </summary>
        private string GetCurrentUserId()
        {
            try
            {
                // This would need to be implemented based on your authentication system
                // For now, we'll use a placeholder implementation
                return Context.User?.Identity?.Name ?? Context.ConnectionId;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if user has access to assessment
        /// </summary>
        private async Task<bool> HasAssessmentAccess(int assessmentId, string userId)
        {
            try
            {
                // This would need to be implemented based on your authorization system
                // For now, we'll return true as a placeholder
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Notify other users when a user disconnects
        /// </summary>
        private async Task NotifyUserDisconnected(string userId)
        {
            try
            {
                if (_userPresence.TryGetValue(userId, out var presence))
                {
                    await Clients.Group($"assessment_{presence.AssessmentId}").SendAsync("UserDisconnected", presence);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying user disconnection for user {UserId}", userId);
            }
        }
    }

    /// <summary>
    /// User presence information for real-time collaboration
    /// </summary>
    public class UserPresence
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public int AssessmentId { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsOnline => DateTime.UtcNow.Subtract(LastActivity).TotalMinutes < 5;
    }

    /// <summary>
    /// Assessment session information
    /// </summary>
    public class AssessmentSession
    {
        public int AssessmentId { get; set; }
        public ConcurrentDictionary<string, UserPresence> ActiveUsers { get; set; }
        public DateTime LastActivity { get; set; }
    }

    /// <summary>
    /// Assessment update information
    /// </summary>
    public class AssessmentUpdate
    {
        public int AssessmentId { get; set; }
        public string UpdateType { get; set; }
        public object UpdateData { get; set; }
        public string UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Collaborative comment information
    /// </summary>
    public class CollaborativeComment
    {
        public int AssessmentId { get; set; }
        public string UserId { get; set; }
        public object Comment { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Editing indicator information
    /// </summary>
    public class EditingIndicator
    {
        public int AssessmentId { get; set; }
        public string UserId { get; set; }
        public object EditingInfo { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Conflict resolution information
    /// </summary>
    public class ConflictResolution
    {
        public int AssessmentId { get; set; }
        public string ResolvedBy { get; set; }
        public object ConflictData { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 