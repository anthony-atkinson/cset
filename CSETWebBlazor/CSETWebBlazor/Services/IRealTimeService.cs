namespace CSETWebBlazor.Services
{
    public interface IRealTimeService
    {
        /// <summary>
        /// Connection state observable
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Event triggered when connection state changes
        /// </summary>
        event Action<bool> ConnectionStateChanged;
        
        /// <summary>
        /// Event triggered when a user joins
        /// </summary>
        event Action<UserPresence> UserJoined;
        
        /// <summary>
        /// Event triggered when a user leaves
        /// </summary>
        event Action<UserPresence> UserLeft;
        
        /// <summary>
        /// Event triggered when a user disconnects
        /// </summary>
        event Action<UserPresence> UserDisconnected;
        
        /// <summary>
        /// Event triggered when assessment is updated
        /// </summary>
        event Action<AssessmentUpdate> AssessmentUpdated;
        
        /// <summary>
        /// Event triggered when a comment is added
        /// </summary>
        event Action<CollaborativeComment> CommentAdded;
        
        /// <summary>
        /// Event triggered when user editing indicator changes
        /// </summary>
        event Action<EditingIndicator> UserEditing;
        
        /// <summary>
        /// Event triggered when user stops editing
        /// </summary>
        event Action<string> UserStoppedEditing;
        
        /// <summary>
        /// Event triggered when conflict is resolved
        /// </summary>
        event Action<ConflictResolution> ConflictResolved;
        
        /// <summary>
        /// Event triggered when active users list changes
        /// </summary>
        event Action<List<UserPresence>> ActiveUsersChanged;
        
        /// <summary>
        /// Event triggered when notification is received
        /// </summary>
        event Action<NotificationMessage> NotificationReceived;
        
        /// <summary>
        /// Event triggered when progress update is received
        /// </summary>
        event Action<ProgressUpdate> ProgressUpdated;
        
        /// <summary>
        /// Event triggered when chart data is updated
        /// </summary>
        event Action<ChartDataUpdate> ChartDataUpdated;

        /// <summary>
        /// Initialize the real-time connection
        /// </summary>
        Task InitializeAsync();
        
        /// <summary>
        /// Connect to SignalR hub
        /// </summary>
        Task ConnectAsync();
        
        /// <summary>
        /// Disconnect from SignalR hub
        /// </summary>
        Task DisconnectAsync();
        
        /// <summary>
        /// Join an assessment for real-time collaboration
        /// </summary>
        Task JoinAssessmentAsync(int assessmentId, string userDisplayName);
        
        /// <summary>
        /// Leave the current assessment
        /// </summary>
        Task LeaveAssessmentAsync();
        
        /// <summary>
        /// Send assessment update to other users
        /// </summary>
        Task SendAssessmentUpdateAsync(string updateType, object updateData);
        
        /// <summary>
        /// Send collaborative comment
        /// </summary>
        Task SendCommentAsync(CollaborativeComment comment);
        
        /// <summary>
        /// Send editing indicator
        /// </summary>
        Task SendEditingIndicatorAsync(EditingIndicator editingInfo);
        
        /// <summary>
        /// Clear editing indicator
        /// </summary>
        Task ClearEditingIndicatorAsync();
        
        /// <summary>
        /// Resolve conflict
        /// </summary>
        Task ResolveConflictAsync(ConflictResolution conflictData);
        
        /// <summary>
        /// Get active users for current assessment
        /// </summary>
        Task<List<UserPresence>> GetActiveUsersAsync();
        
        /// <summary>
        /// Update activity timestamp
        /// </summary>
        Task UpdateActivityAsync();
        
        /// <summary>
        /// Send notification to specific user
        /// </summary>
        Task SendNotificationAsync(string userId, string message, string type = "info");
        
        /// <summary>
        /// Send progress update
        /// </summary>
        Task SendProgressUpdateAsync(int assessmentId, int progress, string message);
        
        /// <summary>
        /// Send real-time chart data
        /// </summary>
        Task SendChartDataAsync(int assessmentId, object chartData);
    }

    public class UserPresence
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int AssessmentId { get; set; }
        public DateTime ConnectedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsOnline { get; set; }
    }

    public class AssessmentUpdate
    {
        public int AssessmentId { get; set; }
        public string UpdateType { get; set; } = string.Empty;
        public object UpdateData { get; set; } = new();
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class CollaborativeComment
    {
        public string Id { get; set; } = string.Empty;
        public int AssessmentId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Section { get; set; } = string.Empty;
        public string QuestionId { get; set; } = string.Empty;
    }

    public class EditingIndicator
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int AssessmentId { get; set; }
        public string Section { get; set; } = string.Empty;
        public string QuestionId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
    }

    public class ConflictResolution
    {
        public string Id { get; set; } = string.Empty;
        public int AssessmentId { get; set; }
        public string ConflictType { get; set; } = string.Empty;
        public object ConflictData { get; set; } = new();
        public string ResolutionStrategy { get; set; } = string.Empty;
        public string ResolvedBy { get; set; } = string.Empty;
        public DateTime ResolvedAt { get; set; }
    }

    public class NotificationMessage
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; }
    }

    public class ProgressUpdate
    {
        public int AssessmentId { get; set; }
        public int Progress { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ChartDataUpdate
    {
        public int AssessmentId { get; set; }
        public string ChartType { get; set; } = string.Empty;
        public object ChartData { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public enum ConflictResolutionStrategy
    {
        AcceptMine,
        AcceptTheirs,
        Merge,
        Manual
    }
} 