//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSETWebCore.Business.Notification.Providers
{
    /// <summary>
    /// In-app notification provider for managing notifications within the application
    /// </summary>
    public class InAppNotificationProvider : IInAppNotificationProvider
    {
        private readonly ILogger<InAppNotificationProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly InAppConfiguration _inAppConfig;
        private readonly Dictionary<string, List<InAppNotification>> _userNotifications;
        private readonly object _lockObject = new object();

        public InAppNotificationProvider(ILogger<InAppNotificationProvider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _inAppConfig = GetInAppConfiguration();
            _userNotifications = new Dictionary<string, List<InAppNotification>>();
        }

        public NotificationType Type => NotificationType.InApp;
        public string DisplayName => "In-App Notification Provider";
        public bool IsEnabled => _inAppConfig.Enabled;
        public int Priority => _inAppConfig.Priority;

        public async Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient)
        {
            if (!IsEnabled)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "In-app notification provider is disabled",
                    ErrorCode = "PROVIDER_DISABLED"
                };
            }

            if (string.IsNullOrEmpty(recipient.UserId))
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "No user ID provided",
                    ErrorCode = "MISSING_USER_ID"
                };
            }

            try
            {
                var result = await SendInAppAsync(
                    recipient.UserId,
                    message.Subject,
                    message.Body,
                    _inAppConfig.DefaultType,
                    message.RelatedEntities);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending in-app notification to user {UserId}", recipient.UserId);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "INAPP_SEND_ERROR"
                };
            }
        }

        public async Task<DeliveryResult> SendInAppAsync(
            string userId,
            string title,
            string message,
            string type = "info",
            Dictionary<string, object>? data = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No user ID provided",
                        ErrorCode = "MISSING_USER_ID"
                    };
                }

                if (string.IsNullOrEmpty(title))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No title provided",
                        ErrorCode = "MISSING_TITLE"
                    };
                }

                var notification = new InAppNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Data = data ?? new Dictionary<string, object>(),
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,
                    IsDismissed = false
                };

                // Store notification in memory (in production, this would be in a database)
                lock (_lockObject)
                {
                    if (!_userNotifications.ContainsKey(userId))
                    {
                        _userNotifications[userId] = new List<InAppNotification>();
                    }

                    var userNotifications = _userNotifications[userId];

                    // Add new notification
                    userNotifications.Insert(0, notification);

                    // Clean up old notifications if exceeding limit
                    if (userNotifications.Count > _inAppConfig.MaxHistoryPerUser)
                    {
                        userNotifications.RemoveRange(_inAppConfig.MaxHistoryPerUser, 
                            userNotifications.Count - _inAppConfig.MaxHistoryPerUser);
                    }

                    // Auto-cleanup old notifications
                    var cutoffDate = DateTime.UtcNow.AddDays(-_inAppConfig.AutoCleanupDays);
                    userNotifications.RemoveAll(n => n.CreatedAt < cutoffDate);
                }

                _logger.LogInformation("In-app notification sent to user {UserId}: {Title}", userId, title);

                return new DeliveryResult
                {
                    Success = true,
                    ExternalId = notification.Id,
                    DeliveredAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["userId"] = userId,
                        ["title"] = title,
                        ["type"] = type,
                        ["hasData"] = data?.Any() ?? false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending in-app notification to user {UserId}", userId);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "INAPP_SEND_ERROR"
                };
            }
        }

        public async Task<List<DeliveryResult>> SendBulkInAppAsync(
            List<string> userIds,
            string title,
            string message,
            string type = "info",
            Dictionary<string, object>? data = null)
        {
            var results = new List<DeliveryResult>();

            // Send in-app notifications in parallel
            var tasks = userIds.Select(async userId =>
            {
                var result = await SendInAppAsync(userId, title, message, type, data);
                lock (results)
                {
                    results.Add(result);
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Bulk in-app notification operation completed. Sent: {Sent}, Failed: {Failed}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<bool> TestConnectivityAsync()
        {
            try
            {
                // Test by sending a notification to a test user
                var testResult = await SendInAppAsync(
                    "test-user",
                    "CSET In-App Test",
                    "This is a test in-app notification",
                    "info",
                    new Dictionary<string, object> { ["test"] = true });

                return testResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "In-app notification connectivity test failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate configuration
            if (_inAppConfig.MaxHistoryPerUser <= 0)
            {
                result.Errors.Add("Max history per user must be greater than 0");
                result.IsValid = false;
            }

            if (_inAppConfig.AutoCleanupDays <= 0)
            {
                result.Errors.Add("Auto cleanup days must be greater than 0");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(_inAppConfig.DefaultType))
            {
                result.Warnings.Add("Default notification type is not configured");
            }

            // Test connectivity if configuration is valid
            if (result.IsValid)
            {
                var connectivityTest = await TestConnectivityAsync();
                if (!connectivityTest)
                {
                    result.Errors.Add("In-app notification connectivity test failed");
                    result.IsValid = false;
                }
            }

            result.Details["maxHistoryPerUser"] = _inAppConfig.MaxHistoryPerUser;
            result.Details["autoCleanupDays"] = _inAppConfig.AutoCleanupDays;
            result.Details["defaultType"] = _inAppConfig.DefaultType;
            result.Details["maxConcurrentSends"] = _inAppConfig.MaxConcurrentSends;
            result.Details["rateLimitPerMinute"] = _inAppConfig.RateLimitPerMinute;

            return result;
        }

        #region Additional In-App Notification Methods

        /// <summary>
        /// Gets notifications for a user
        /// </summary>
        public async Task<List<InAppNotification>> GetUserNotificationsAsync(string userId, bool includeRead = false, int limit = 50)
        {
            await Task.CompletedTask; // Simulate async operation

            lock (_lockObject)
            {
                if (!_userNotifications.ContainsKey(userId))
                {
                    return new List<InAppNotification>();
                }

                var notifications = _userNotifications[userId];
                
                if (!includeRead)
                {
                    notifications = notifications.Where(n => !n.IsRead).ToList();
                }

                return notifications.Take(limit).ToList();
            }
        }

        /// <summary>
        /// Gets unread notification count for a user
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await GetUserNotificationsAsync(userId, includeRead: false);
            return notifications.Count;
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        public async Task<bool> MarkAsReadAsync(string userId, string notificationId)
        {
            await Task.CompletedTask; // Simulate async operation

            lock (_lockObject)
            {
                if (!_userNotifications.ContainsKey(userId))
                {
                    return false;
                }

                var notification = _userNotifications[userId].FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            await Task.CompletedTask; // Simulate async operation

            lock (_lockObject)
            {
                if (!_userNotifications.ContainsKey(userId))
                {
                    return false;
                }

                var now = DateTime.UtcNow;
                foreach (var notification in _userNotifications[userId].Where(n => !n.IsRead))
                {
                    notification.IsRead = true;
                    notification.ReadAt = now;
                }

                return true;
            }
        }

        /// <summary>
        /// Dismisses a notification
        /// </summary>
        public async Task<bool> DismissNotificationAsync(string userId, string notificationId)
        {
            await Task.CompletedTask; // Simulate async operation

            lock (_lockObject)
            {
                if (!_userNotifications.ContainsKey(userId))
                {
                    return false;
                }

                var notification = _userNotifications[userId].FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsDismissed = true;
                    notification.DismissedAt = DateTime.UtcNow;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        public async Task<bool> DeleteNotificationAsync(string userId, string notificationId)
        {
            await Task.CompletedTask; // Simulate async operation

            lock (_lockObject)
            {
                if (!_userNotifications.ContainsKey(userId))
                {
                    return false;
                }

                var notification = _userNotifications[userId].FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    _userNotifications[userId].Remove(notification);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Cleans up old notifications
        /// </summary>
        public async Task<int> CleanupOldNotificationsAsync()
        {
            await Task.CompletedTask; // Simulate async operation

            var cutoffDate = DateTime.UtcNow.AddDays(-_inAppConfig.AutoCleanupDays);
            var totalRemoved = 0;

            lock (_lockObject)
            {
                foreach (var userNotifications in _userNotifications.Values)
                {
                    var removed = userNotifications.RemoveAll(n => n.CreatedAt < cutoffDate);
                    totalRemoved += removed;
                }
            }

            if (totalRemoved > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old in-app notifications", totalRemoved);
            }

            return totalRemoved;
        }

        #endregion

        private InAppConfiguration GetInAppConfiguration()
        {
            var config = _configuration.GetSection("EnhancedNotifications:Providers:InApp");
            
            return new InAppConfiguration
            {
                Enabled = config.GetValue<bool>("Enabled", true),
                Priority = config.GetValue<int>("Priority", 5),
                MaxConcurrentSends = config.GetValue<int>("MaxConcurrentSends", 50),
                RateLimitPerMinute = config.GetValue<int>("RateLimitPerMinute", 200),
                DefaultType = config.GetValue<string>("DefaultType") ?? "info",
                MaxHistoryPerUser = config.GetValue<int>("MaxHistoryPerUser", 1000),
                AutoCleanupDays = config.GetValue<int>("AutoCleanupDays", 30)
            };
        }
    }

    /// <summary>
    /// In-app notification model
    /// </summary>
    public class InAppNotification
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? DismissedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsDismissed { get; set; }
    }

    /// <summary>
    /// In-app notification configuration settings
    /// </summary>
    public class InAppConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 5;
        public int MaxConcurrentSends { get; set; } = 50;
        public int RateLimitPerMinute { get; set; } = 200;
        public string DefaultType { get; set; } = "info";
        public int MaxHistoryPerUser { get; set; } = 1000;
        public int AutoCleanupDays { get; set; } = 30;
    }
} 