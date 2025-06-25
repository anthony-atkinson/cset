using Microsoft.AspNetCore.SignalR;
using CSETWebBlazor.Services;
using System.Collections.Concurrent;

namespace CSETWebBlazor.Hubs
{
    public class CSETHub : Hub
    {
        private readonly ILogger<CSETHub> _logger;
        private static readonly ConcurrentDictionary<string, UserPresence> _activeUsers = new();
        private static readonly ConcurrentDictionary<string, List<string>> _assessmentGroups = new();
        private static readonly ConcurrentDictionary<string, EditingIndicator> _editingIndicators = new();
        private static readonly ConcurrentDictionary<string, List<CollaborativeComment>> _assessmentComments = new();

        public CSETHub(ILogger<CSETHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            
            // Remove user from active users
            var userToRemove = _activeUsers.Values.FirstOrDefault(u => u.UserId == Context.UserIdentifier);
            if (userToRemove != null)
            {
                _activeUsers.TryRemove(userToRemove.UserId, out _);
                await NotifyUserDisconnected(userToRemove);
            }

            // Clear editing indicators
            var editingToRemove = _editingIndicators.Values.FirstOrDefault(e => e.UserId == Context.UserIdentifier);
            if (editingToRemove != null)
            {
                _editingIndicators.TryRemove(editingToRemove.UserId, out _);
                await NotifyUserStoppedEditing(editingToRemove.UserId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join an assessment group for real-time updates
        /// </summary>
        public async Task JoinAssessmentGroup(string assessmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");
            
            // Track assessment group membership
            if (!_assessmentGroups.ContainsKey(assessmentId))
            {
                _assessmentGroups[assessmentId] = new List<string>();
            }
            _assessmentGroups[assessmentId].Add(Context.ConnectionId);

            _logger.LogInformation($"Client {Context.ConnectionId} joined assessment group {assessmentId}");
        }

        /// <summary>
        /// Leave an assessment group
        /// </summary>
        public async Task LeaveAssessmentGroup(string assessmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");
            
            // Remove from assessment group tracking
            if (_assessmentGroups.ContainsKey(assessmentId))
            {
                _assessmentGroups[assessmentId].Remove(Context.ConnectionId);
            }

            _logger.LogInformation($"Client {Context.ConnectionId} left assessment group {assessmentId}");
        }

        /// <summary>
        /// Join assessment with user information
        /// </summary>
        public async Task JoinAssessment(int assessmentId, string userDisplayName)
        {
            var userPresence = new UserPresence
            {
                UserId = Context.UserIdentifier ?? Context.ConnectionId,
                DisplayName = userDisplayName,
                AssessmentId = int.Parse(assessmentId),
                ConnectedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsOnline = true
            };

            _activeUsers[userPresence.UserId] = userPresence;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");
            
            // Notify other users in the assessment
            await Clients.Group($"assessment_{assessmentId}").SendAsync("UserJoined", userPresence);

            // Send current active users to the joining user
            var activeUsers = _activeUsers.Values.Where(u => u.AssessmentId == int.Parse(assessmentId)).ToList();
            await Clients.Caller.SendAsync("AssessmentJoined", assessmentId, activeUsers);

            _logger.LogInformation($"User {userDisplayName} joined assessment {assessmentId}");
        }

        /// <summary>
        /// Leave assessment
        /// </summary>
        public async Task LeaveAssessment(string assessmentId)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            if (_activeUsers.TryRemove(userId, out var userPresence))
            {
                await Clients.Group($"assessment_{assessmentId}").SendAsync("UserLeft", userPresence);
                _logger.LogInformation($"User {userPresence.DisplayName} left assessment {assessmentId}");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"assessment_{assessmentId}");
        }

        /// <summary>
        /// Send assessment update to group
        /// </summary>
        public async Task SendAssessmentUpdate(string assessmentId, object update)
        {
            var assessmentUpdate = new AssessmentUpdate
            {
                AssessmentId = int.Parse(assessmentId),
                UpdateType = "general",
                UpdateData = update,
                UserId = Context.UserIdentifier ?? Context.ConnectionId,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group($"assessment_{assessmentId}").SendAsync("AssessmentUpdated", assessmentUpdate);
            _logger.LogInformation($"Assessment {assessmentId} updated by {assessmentUpdate.UserId}");
        }

        /// <summary>
        /// Send collaborative comment
        /// </summary>
        public async Task SendComment(CollaborativeComment comment)
        {
            comment.Id = Guid.NewGuid().ToString();
            comment.Timestamp = DateTime.UtcNow;

            // Store comment for the assessment
            if (!_assessmentComments.ContainsKey(comment.AssessmentId.ToString()))
            {
                _assessmentComments[comment.AssessmentId.ToString()] = new List<CollaborativeComment>();
            }
            _assessmentComments[comment.AssessmentId.ToString()].Add(comment);

            await Clients.Group($"assessment_{comment.AssessmentId}").SendAsync("CommentAdded", comment);
            _logger.LogInformation($"Comment added by {comment.UserName} in assessment {comment.AssessmentId}");
        }

        /// <summary>
        /// Send editing indicator
        /// </summary>
        public async Task SendEditingIndicator(EditingIndicator editingInfo)
        {
            editingInfo.UserId = Context.UserIdentifier ?? Context.ConnectionId;
            editingInfo.StartedAt = DateTime.UtcNow;

            _editingIndicators[editingInfo.UserId] = editingInfo;

            await Clients.Group($"assessment_{editingInfo.AssessmentId}").SendAsync("UserEditing", editingInfo);
            _logger.LogInformation($"User {editingInfo.UserName} started editing in assessment {editingInfo.AssessmentId}");
        }

        /// <summary>
        /// Clear editing indicator
        /// </summary>
        public async Task ClearEditingIndicator(string assessmentId)
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            if (_editingIndicators.TryRemove(userId, out _))
            {
                await Clients.Group($"assessment_{assessmentId}").SendAsync("UserStoppedEditing", userId);
                _logger.LogInformation($"User {userId} stopped editing in assessment {assessmentId}");
            }
        }

        /// <summary>
        /// Resolve conflict
        /// </summary>
        public async Task ResolveConflict(ConflictResolution conflictData)
        {
            conflictData.Id = Guid.NewGuid().ToString();
            conflictData.ResolvedBy = Context.UserIdentifier ?? Context.ConnectionId;
            conflictData.ResolvedAt = DateTime.UtcNow;

            await Clients.Group($"assessment_{conflictData.AssessmentId}").SendAsync("ConflictResolved", conflictData);
            _logger.LogInformation($"Conflict resolved by {conflictData.ResolvedBy} in assessment {conflictData.AssessmentId}");
        }

        /// <summary>
        /// Get active users for assessment
        /// </summary>
        public async Task<List<UserPresence>> GetActiveUsers(string assessmentId)
        {
            var users = _activeUsers.Values.Where(u => u.AssessmentId == int.Parse(assessmentId)).ToList();
            return await Task.FromResult(users);
        }

        /// <summary>
        /// Update user activity
        /// </summary>
        public async Task UpdateActivity()
        {
            var userId = Context.UserIdentifier ?? Context.ConnectionId;
            if (_activeUsers.TryGetValue(userId, out var userPresence))
            {
                userPresence.LastActivity = DateTime.UtcNow;
                _activeUsers[userId] = userPresence;
            }
        }

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        public async Task SendNotification(string userId, string message, string type = "info")
        {
            var notification = new NotificationMessage
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };

            await Clients.User(userId).SendAsync("Notification", notification);
            _logger.LogInformation($"Notification sent to user {userId}: {message}");
        }

        /// <summary>
        /// Send progress update
        /// </summary>
        public async Task SendProgressUpdate(string assessmentId, int progress, string message)
        {
            var progressUpdate = new ProgressUpdate
            {
                AssessmentId = int.Parse(assessmentId),
                Progress = progress,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group($"assessment_{assessmentId}").SendAsync("ProgressUpdated", progressUpdate);
            _logger.LogInformation($"Progress update for assessment {assessmentId}: {progress}% - {message}");
        }

        /// <summary>
        /// Send real-time chart data
        /// </summary>
        public async Task SendChartData(string assessmentId, object chartData)
        {
            var chartUpdate = new ChartDataUpdate
            {
                AssessmentId = int.Parse(assessmentId),
                ChartType = "general",
                ChartData = chartData,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group($"assessment_{assessmentId}").SendAsync("ChartDataUpdated", chartUpdate);
            _logger.LogInformation($"Chart data updated for assessment {assessmentId}");
        }

        /// <summary>
        /// Get assessment comments
        /// </summary>
        public async Task<List<CollaborativeComment>> GetAssessmentComments(string assessmentId)
        {
            if (_assessmentComments.TryGetValue(assessmentId, out var comments))
            {
                return await Task.FromResult(comments);
            }
            return await Task.FromResult(new List<CollaborativeComment>());
        }

        /// <summary>
        /// Get editing indicators for assessment
        /// </summary>
        public async Task<List<EditingIndicator>> GetEditingIndicators(string assessmentId)
        {
            var indicators = _editingIndicators.Values
                .Where(e => e.AssessmentId == int.Parse(assessmentId))
                .ToList();
            return await Task.FromResult(indicators);
        }

        private async Task NotifyUserDisconnected(UserPresence user)
        {
            foreach (var group in _assessmentGroups)
            {
                if (group.Value.Contains(Context.ConnectionId))
                {
                    await Clients.Group($"assessment_{group.Key}").SendAsync("UserDisconnected", user);
                }
            }
        }

        private async Task NotifyUserStoppedEditing(string userId)
        {
            foreach (var group in _assessmentGroups)
            {
                if (group.Value.Contains(Context.ConnectionId))
                {
                    await Clients.Group($"assessment_{group.Key}").SendAsync("UserStoppedEditing", userId);
                }
            }
        }
    }
} 