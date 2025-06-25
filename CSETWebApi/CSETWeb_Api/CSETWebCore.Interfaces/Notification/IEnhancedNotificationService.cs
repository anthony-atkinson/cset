//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Model.Notification;

namespace CSETWebCore.Interfaces.Notification
{
    /// <summary>
    /// Enhanced notification service interface for enterprise-grade notification capabilities
    /// </summary>
    public interface IEnhancedNotificationService
    {
        #region Core Notification Methods

        /// <summary>
        /// Sends a notification using the specified message
        /// </summary>
        /// <param name="message">The notification message to send</param>
        /// <returns>Notification response with delivery status</returns>
        Task<NotificationResponse> SendNotificationAsync(NotificationMessage message);

        /// <summary>
        /// Sends a notification using a template
        /// </summary>
        /// <param name="templateName">Name of the template to use</param>
        /// <param name="recipients">List of recipients</param>
        /// <param name="variables">Template variables</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>Notification response with delivery status</returns>
        Task<NotificationResponse> SendTemplateNotificationAsync(
            string templateName,
            List<NotificationRecipient> recipients,
            Dictionary<string, object> variables,
            NotificationPriority priority = NotificationPriority.Normal);

        /// <summary>
        /// Sends a notification to multiple recipients
        /// </summary>
        /// <param name="request">Notification creation request</param>
        /// <returns>Notification response with delivery status</returns>
        Task<NotificationResponse> SendNotificationAsync(CreateNotificationRequest request);

        /// <summary>
        /// Schedules a notification for future delivery
        /// </summary>
        /// <param name="message">The notification message to schedule</param>
        /// <param name="scheduledAt">When to deliver the notification</param>
        /// <returns>Notification response with scheduling status</returns>
        Task<NotificationResponse> ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledAt);

        /// <summary>
        /// Cancels a scheduled notification
        /// </summary>
        /// <param name="notificationId">ID of the notification to cancel</param>
        /// <returns>True if cancellation was successful</returns>
        Task<bool> CancelScheduledNotificationAsync(Guid notificationId);

        #endregion

        #region Template Management

        /// <summary>
        /// Creates a new notification template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>Created template</returns>
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);

        /// <summary>
        /// Updates an existing notification template
        /// </summary>
        /// <param name="templateId">ID of the template to update</param>
        /// <param name="template">Updated template data</param>
        /// <returns>Updated template</returns>
        Task<NotificationTemplate> UpdateTemplateAsync(Guid templateId, NotificationTemplate template);

        /// <summary>
        /// Gets a notification template by ID
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Template if found, null otherwise</returns>
        Task<NotificationTemplate?> GetTemplateAsync(Guid templateId);

        /// <summary>
        /// Gets a notification template by name
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <returns>Template if found, null otherwise</returns>
        Task<NotificationTemplate?> GetTemplateByNameAsync(string templateName);

        /// <summary>
        /// Gets all notification templates
        /// </summary>
        /// <param name="type">Optional filter by notification type</param>
        /// <param name="isActive">Optional filter by active status</param>
        /// <returns>List of templates</returns>
        Task<List<NotificationTemplate>> GetTemplatesAsync(
            NotificationType? type = null,
            bool? isActive = null);

        /// <summary>
        /// Deletes a notification template
        /// </summary>
        /// <param name="templateId">ID of the template to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteTemplateAsync(Guid templateId);

        /// <summary>
        /// Activates or deactivates a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="isActive">Whether to activate or deactivate</param>
        /// <returns>True if operation was successful</returns>
        Task<bool> SetTemplateActiveAsync(Guid templateId, bool isActive);

        #endregion

        #region Delivery Tracking

        /// <summary>
        /// Gets delivery status for a notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>List of delivery statuses</returns>
        Task<List<DeliveryStatusResponse>> GetDeliveryStatusAsync(Guid notificationId);

        /// <summary>
        /// Gets delivery status for a specific delivery
        /// </summary>
        /// <param name="deliveryId">Delivery ID</param>
        /// <returns>Delivery status if found, null otherwise</returns>
        Task<DeliveryStatusResponse?> GetDeliveryStatusByIdAsync(Guid deliveryId);

        /// <summary>
        /// Retries failed deliveries for a notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>True if retry was initiated</returns>
        Task<bool> RetryFailedDeliveriesAsync(Guid notificationId);

        /// <summary>
        /// Gets delivery history for a recipient
        /// </summary>
        /// <param name="recipientId">Recipient ID</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="offset">Number of records to skip</param>
        /// <returns>List of delivery statuses</returns>
        Task<List<DeliveryStatusResponse>> GetRecipientDeliveryHistoryAsync(
            string recipientId,
            int limit = 50,
            int offset = 0);

        #endregion

        #region Preferences Management

        /// <summary>
        /// Gets notification preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's notification preferences</returns>
        Task<NotificationPreferences> GetUserPreferencesAsync(string userId);

        /// <summary>
        /// Updates notification preferences for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="preferences">Updated preferences</param>
        /// <returns>Updated preferences</returns>
        Task<NotificationPreferences> UpdateUserPreferencesAsync(string userId, NotificationPreferences preferences);

        /// <summary>
        /// Gets default notification preferences
        /// </summary>
        /// <returns>Default preferences</returns>
        Task<NotificationPreferences> GetDefaultPreferencesAsync();

        /// <summary>
        /// Resets user preferences to defaults
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Reset preferences</returns>
        Task<NotificationPreferences> ResetUserPreferencesAsync(string userId);

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Gets notification statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Notification statistics</returns>
        Task<NotificationStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gets statistics for a specific notification type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Type-specific statistics</returns>
        Task<TypeStatistics> GetTypeStatisticsAsync(
            NotificationType type,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Gets statistics for a specific priority level
        /// </summary>
        /// <param name="priority">Notification priority</param>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Priority-specific statistics</returns>
        Task<PriorityStatistics> GetPriorityStatisticsAsync(
            NotificationPriority priority,
            DateTime? startDate = null,
            DateTime? endDate = null);

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Sends notifications to multiple recipients in bulk
        /// </summary>
        /// <param name="messages">List of notification messages</param>
        /// <returns>List of notification responses</returns>
        Task<List<NotificationResponse>> SendBulkNotificationsAsync(List<NotificationMessage> messages);

        /// <summary>
        /// Sends template notifications to multiple recipients in bulk
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <param name="recipients">List of recipients</param>
        /// <param name="variables">Template variables</param>
        /// <param name="priority">Notification priority</param>
        /// <returns>List of notification responses</returns>
        Task<List<NotificationResponse>> SendBulkTemplateNotificationsAsync(
            string templateName,
            List<NotificationRecipient> recipients,
            Dictionary<string, object> variables,
            NotificationPriority priority = NotificationPriority.Normal);

        #endregion

        #region Health and Diagnostics

        /// <summary>
        /// Tests notification service connectivity
        /// </summary>
        /// <param name="type">Notification type to test</param>
        /// <returns>Test result</returns>
        Task<bool> TestConnectivityAsync(NotificationType type);

        /// <summary>
        /// Gets service health status
        /// </summary>
        /// <returns>Health status information</returns>
        Task<Dictionary<string, object>> GetHealthStatusAsync();

        /// <summary>
        /// Validates notification configuration
        /// </summary>
        /// <returns>Validation results</returns>
        Task<Dictionary<string, object>> ValidateConfigurationAsync();

        #endregion
    }
} 