using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace CSETWebBlazor.Services
{
    public class RealTimeService : IRealTimeService, IAsyncDisposable
    {
        private readonly ILogger<RealTimeService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authService;
        private HubConnection? _hubConnection;
        private int? _currentAssessmentId;
        private string? _currentUserDisplayName;
        private readonly Timer _activityTimer;
        private readonly Timer _reconnectTimer;
        private int _reconnectAttempts = 0;
        private const int MaxReconnectAttempts = 5;
        private const int ReconnectIntervalMs = 5000;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public event Action<bool>? ConnectionStateChanged;
        public event Action<UserPresence>? UserJoined;
        public event Action<UserPresence>? UserLeft;
        public event Action<UserPresence>? UserDisconnected;
        public event Action<AssessmentUpdate>? AssessmentUpdated;
        public event Action<CollaborativeComment>? CommentAdded;
        public event Action<EditingIndicator>? UserEditing;
        public event Action<string>? UserStoppedEditing;
        public event Action<ConflictResolution>? ConflictResolved;
        public event Action<List<UserPresence>>? ActiveUsersChanged;
        public event Action<NotificationMessage>? NotificationReceived;
        public event Action<ProgressUpdate>? ProgressUpdated;
        public event Action<ChartDataUpdate>? ChartDataUpdated;

        public RealTimeService(
            ILogger<RealTimeService> logger,
            IConfiguration configuration,
            IAuthenticationService authService)
        {
            _logger = logger;
            _configuration = configuration;
            _authService = authService;

            // Activity timer to update user activity every 30 seconds
            _activityTimer = new Timer(async _ => await UpdateActivityAsync(), null, Timeout.Infinite, Timeout.Infinite);
            
            // Reconnect timer
            _reconnectTimer = new Timer(async _ => await AttemptReconnectAsync(), null, Timeout.Infinite, Timeout.Infinite);
        }

        public async Task InitializeAsync()
        {
            try
            {
                var hubUrl = _configuration.GetValue<string>("CSET:SignalRHubUrl", "/csetHub");
                var baseUrl = _configuration.GetValue<string>("CSET:ApiBaseUrl", "https://localhost:5001/");
                var fullHubUrl = baseUrl.TrimEnd('/') + hubUrl;

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(fullHubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_authService.GetToken());
                    })
                    .WithAutomaticReconnect(new[] { 0, 2000, 5000, 10000, 30000 })
                    .Build();

                SetupEventHandlers();
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize real-time service");
                throw;
            }
        }

        private void SetupEventHandlers()
        {
            if (_hubConnection == null) return;

            // Connection events
            _hubConnection.Reconnecting += async (exception) =>
            {
                _logger.LogInformation("Attempting to reconnect to SignalR hub...");
                ConnectionStateChanged?.Invoke(false);
                await Task.CompletedTask;
            };

            _hubConnection.Reconnected += async (connectionId) =>
            {
                _logger.LogInformation("Reconnected to SignalR hub with connection ID: {ConnectionId}", connectionId);
                ConnectionStateChanged?.Invoke(true);
                _reconnectAttempts = 0;

                // Rejoin current assessment if any
                if (_currentAssessmentId.HasValue && !string.IsNullOrEmpty(_currentUserDisplayName))
                {
                    await JoinAssessmentAsync(_currentAssessmentId.Value, _currentUserDisplayName);
                }
            };

            _hubConnection.Closed += async (exception) =>
            {
                _logger.LogInformation("SignalR connection closed");
                ConnectionStateChanged?.Invoke(false);
                await AttemptReconnectAsync();
            };

            // Collaboration events
            _hubConnection.On<UserPresence>("UserJoined", (user) =>
            {
                _logger.LogInformation("User joined: {DisplayName}", user.DisplayName);
                UserJoined?.Invoke(user);
            });

            _hubConnection.On<UserPresence>("UserLeft", (user) =>
            {
                _logger.LogInformation("User left: {DisplayName}", user.DisplayName);
                UserLeft?.Invoke(user);
            });

            _hubConnection.On<UserPresence>("UserDisconnected", (user) =>
            {
                _logger.LogInformation("User disconnected: {DisplayName}", user.DisplayName);
                UserDisconnected?.Invoke(user);
            });

            _hubConnection.On<int, List<UserPresence>>("AssessmentJoined", (assessmentId, users) =>
            {
                _logger.LogInformation("Joined assessment: {AssessmentId} with {UserCount} users", assessmentId, users.Count);
                ActiveUsersChanged?.Invoke(users);
            });

            _hubConnection.On<AssessmentUpdate>("AssessmentUpdated", (update) =>
            {
                _logger.LogInformation("Assessment updated: {UpdateType}", update.UpdateType);
                AssessmentUpdated?.Invoke(update);
            });

            _hubConnection.On<CollaborativeComment>("CommentAdded", (comment) =>
            {
                _logger.LogInformation("Comment added by: {UserName}", comment.UserName);
                CommentAdded?.Invoke(comment);
            });

            _hubConnection.On<EditingIndicator>("UserEditing", (indicator) =>
            {
                _logger.LogInformation("User editing: {UserName}", indicator.UserName);
                UserEditing?.Invoke(indicator);
            });

            _hubConnection.On<string>("UserStoppedEditing", (userId) =>
            {
                _logger.LogInformation("User stopped editing: {UserId}", userId);
                UserStoppedEditing?.Invoke(userId);
            });

            _hubConnection.On<ConflictResolution>("ConflictResolved", (resolution) =>
            {
                _logger.LogInformation("Conflict resolved by: {ResolvedBy}", resolution.ResolvedBy);
                ConflictResolved?.Invoke(resolution);
            });

            _hubConnection.On<NotificationMessage>("Notification", (notification) =>
            {
                _logger.LogInformation("Notification received: {Message}", notification.Message);
                NotificationReceived?.Invoke(notification);
            });

            _hubConnection.On<ProgressUpdate>("ProgressUpdated", (progress) =>
            {
                _logger.LogInformation("Progress updated: {Progress}%", progress.Progress);
                ProgressUpdated?.Invoke(progress);
            });

            _hubConnection.On<ChartDataUpdate>("ChartDataUpdated", (chartData) =>
            {
                _logger.LogInformation("Chart data updated: {ChartType}", chartData.ChartType);
                ChartDataUpdated?.Invoke(chartData);
            });
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (_hubConnection != null && _hubConnection.State == HubConnectionState.Disconnected)
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("Connected to SignalR hub");
                    ConnectionStateChanged?.Invoke(true);
                    _reconnectAttempts = 0;

                    // Start activity timer
                    _activityTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SignalR hub");
                ConnectionStateChanged?.Invoke(false);
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    await LeaveAssessmentAsync();
                    await _hubConnection.StopAsync();
                    _logger.LogInformation("Disconnected from SignalR hub");
                    ConnectionStateChanged?.Invoke(false);
                }

                // Stop timers
                _activityTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from SignalR hub");
            }
        }

        public async Task JoinAssessmentAsync(int assessmentId, string userDisplayName)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("JoinAssessmentGroup", assessmentId.ToString());
                    _currentAssessmentId = assessmentId;
                    _currentUserDisplayName = userDisplayName;
                    _logger.LogInformation("Joined assessment: {AssessmentId}", assessmentId);
                }
                else
                {
                    _logger.LogWarning("Cannot join assessment: connection not available");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to join assessment: {AssessmentId}", assessmentId);
                throw;
            }
        }

        public async Task LeaveAssessmentAsync()
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    await _hubConnection.InvokeAsync("LeaveAssessmentGroup", _currentAssessmentId.Value.ToString());
                    _currentAssessmentId = null;
                    _currentUserDisplayName = null;
                    _logger.LogInformation("Left assessment");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to leave assessment");
                throw;
            }
        }

        public async Task SendAssessmentUpdateAsync(string updateType, object updateData)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    await _hubConnection.InvokeAsync("SendAssessmentUpdate", _currentAssessmentId.Value.ToString(), updateData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send assessment update");
                throw;
            }
        }

        public async Task SendCommentAsync(CollaborativeComment comment)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    comment.AssessmentId = _currentAssessmentId.Value;
                    comment.Timestamp = DateTime.UtcNow;
                    await _hubConnection.InvokeAsync("SendComment", comment);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send comment");
                throw;
            }
        }

        public async Task SendEditingIndicatorAsync(EditingIndicator editingInfo)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    editingInfo.AssessmentId = _currentAssessmentId.Value;
                    editingInfo.StartedAt = DateTime.UtcNow;
                    await _hubConnection.InvokeAsync("SendEditingIndicator", editingInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send editing indicator");
                throw;
            }
        }

        public async Task ClearEditingIndicatorAsync()
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    await _hubConnection.InvokeAsync("ClearEditingIndicator", _currentAssessmentId.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear editing indicator");
                throw;
            }
        }

        public async Task ResolveConflictAsync(ConflictResolution conflictData)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    conflictData.AssessmentId = _currentAssessmentId.Value;
                    conflictData.ResolvedAt = DateTime.UtcNow;
                    await _hubConnection.InvokeAsync("ResolveConflict", conflictData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve conflict");
                throw;
            }
        }

        public async Task<List<UserPresence>> GetActiveUsersAsync()
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected && _currentAssessmentId.HasValue)
                {
                    return await _hubConnection.InvokeAsync<List<UserPresence>>("GetActiveUsers", _currentAssessmentId.Value.ToString());
                }
                return new List<UserPresence>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active users");
                return new List<UserPresence>();
            }
        }

        public async Task UpdateActivityAsync()
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("UpdateActivity");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update activity");
            }
        }

        public async Task SendNotificationAsync(string userId, string message, string type = "info")
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendNotification", userId, message, type);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
                throw;
            }
        }

        public async Task SendProgressUpdateAsync(int assessmentId, int progress, string message)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendProgressUpdate", assessmentId.ToString(), progress, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send progress update");
                throw;
            }
        }

        public async Task SendChartDataAsync(int assessmentId, object chartData)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Connected)
                {
                    await _hubConnection.InvokeAsync("SendChartData", assessmentId.ToString(), chartData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send chart data");
                throw;
            }
        }

        private async Task AttemptReconnectAsync()
        {
            if (_reconnectAttempts >= MaxReconnectAttempts)
            {
                _logger.LogWarning("Max reconnection attempts reached");
                return;
            }

            _reconnectAttempts++;
            _logger.LogInformation("Attempting reconnection {Attempt}/{MaxAttempts}", _reconnectAttempts, MaxReconnectAttempts);

            _reconnectTimer.Change(ReconnectIntervalMs, Timeout.Infinite);
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _activityTimer?.Dispose();
            _reconnectTimer?.Dispose();
        }
    }
} 