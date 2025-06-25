//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace CSETWebCore.Api.Controllers
{
    /// <summary>
    /// Enhanced notification controller for enterprise-grade notification capabilities
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnhancedNotificationController : ControllerBase
    {
        private readonly IEnhancedNotificationService _notificationService;
        private readonly ILogger<EnhancedNotificationController> _logger;

        public EnhancedNotificationController(
            IEnhancedNotificationService notificationService,
            ILogger<EnhancedNotificationController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        #region Core Notification Endpoints

        /// <summary>
        /// Sends a notification using the specified message
        /// </summary>
        /// <param name="message">The notification message to send</param>
        /// <returns>Notification response with delivery status</returns>
        [HttpPost("send")]
        [ProducesResponseType(typeof(NotificationResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationResponse>> SendNotification([FromBody] NotificationMessage message)
        {
            try
            {
                _logger.LogInformation("Sending notification {NotificationId} of type {Type}", 
                    message.Id, message.Type);

                var response = await _notificationService.SendNotificationAsync(message);

                if (response.Status == "Failed")
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return StatusCode(500, new { error = "Failed to send notification", details = ex.Message });
            }
        }

        /// <summary>
        /// Sends a notification using a template
        /// </summary>
        /// <param name="request">Template notification request</param>
        /// <returns>Notification response with delivery status</returns>
        [HttpPost("send-template")]
        [ProducesResponseType(typeof(NotificationResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationResponse>> SendTemplateNotification([FromBody] SendTemplateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TemplateName))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                if (request.Recipients == null || !request.Recipients.Any())
                {
                    return BadRequest(new { error = "At least one recipient is required" });
                }

                _logger.LogInformation("Sending template notification using template {TemplateName} to {RecipientCount} recipients", 
                    request.TemplateName, request.Recipients.Count);

                var response = await _notificationService.SendTemplateNotificationAsync(
                    request.TemplateName,
                    request.Recipients,
                    request.Variables ?? new Dictionary<string, object>(),
                    request.Priority);

                if (response.Status == "Failed")
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Template notification request validation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending template notification");
                return StatusCode(500, new { error = "Failed to send template notification", details = ex.Message });
            }
        }

        /// <summary>
        /// Sends a notification using the simplified request format
        /// </summary>
        /// <param name="request">Notification creation request</param>
        /// <returns>Notification response with delivery status</returns>
        [HttpPost("send-simple")]
        [ProducesResponseType(typeof(NotificationResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationResponse>> SendSimpleNotification([FromBody] CreateNotificationRequest request)
        {
            try
            {
                _logger.LogInformation("Sending simple notification of type {Type} to {RecipientCount} recipients", 
                    request.Type, GetRecipientCount(request));

                var response = await _notificationService.SendNotificationAsync(request);

                if (response.Status == "Failed")
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending simple notification");
                return StatusCode(500, new { error = "Failed to send notification", details = ex.Message });
            }
        }

        /// <summary>
        /// Schedules a notification for future delivery
        /// </summary>
        /// <param name="request">Scheduled notification request</param>
        /// <returns>Notification response with scheduling status</returns>
        [HttpPost("schedule")]
        [ProducesResponseType(typeof(NotificationResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationResponse>> ScheduleNotification([FromBody] ScheduleNotificationRequest request)
        {
            try
            {
                if (request.ScheduledAt <= DateTime.UtcNow)
                {
                    return BadRequest(new { error = "Scheduled time must be in the future" });
                }

                _logger.LogInformation("Scheduling notification for {ScheduledAt}", request.ScheduledAt);

                var response = await _notificationService.ScheduleNotificationAsync(request.Message, request.ScheduledAt);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling notification");
                return StatusCode(500, new { error = "Failed to schedule notification", details = ex.Message });
            }
        }

        /// <summary>
        /// Cancels a scheduled notification
        /// </summary>
        /// <param name="notificationId">ID of the notification to cancel</param>
        /// <returns>Success status</returns>
        [HttpDelete("schedule/{notificationId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> CancelScheduledNotification(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Cancelling scheduled notification {NotificationId}", notificationId);

                var result = await _notificationService.CancelScheduledNotificationAsync(notificationId);

                if (!result)
                {
                    return NotFound(new { error = "Scheduled notification not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling scheduled notification {NotificationId}", notificationId);
                return StatusCode(500, new { error = "Failed to cancel scheduled notification", details = ex.Message });
            }
        }

        #endregion

        #region Template Management Endpoints

        /// <summary>
        /// Creates a new notification template
        /// </summary>
        /// <param name="template">Template to create</param>
        /// <returns>Created template</returns>
        [HttpPost("templates")]
        [ProducesResponseType(typeof(NotificationTemplate), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationTemplate>> CreateTemplate([FromBody] NotificationTemplate template)
        {
            try
            {
                _logger.LogInformation("Creating notification template {TemplateName}", template.Name);

                var createdTemplate = await _notificationService.CreateTemplateAsync(template);

                return CreatedAtAction(nameof(GetTemplate), new { templateId = createdTemplate.Id }, createdTemplate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Template creation validation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification template");
                return StatusCode(500, new { error = "Failed to create template", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing notification template
        /// </summary>
        /// <param name="templateId">ID of the template to update</param>
        /// <param name="template">Updated template data</param>
        /// <returns>Updated template</returns>
        [HttpPut("templates/{templateId}")]
        [ProducesResponseType(typeof(NotificationTemplate), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationTemplate>> UpdateTemplate(Guid templateId, [FromBody] NotificationTemplate template)
        {
            try
            {
                _logger.LogInformation("Updating notification template {TemplateId}", templateId);

                var updatedTemplate = await _notificationService.UpdateTemplateAsync(templateId, template);

                return Ok(updatedTemplate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Template update validation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "Failed to update template", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a notification template by ID
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Template if found</returns>
        [HttpGet("templates/{templateId}")]
        [ProducesResponseType(typeof(NotificationTemplate), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationTemplate>> GetTemplate(Guid templateId)
        {
            try
            {
                var template = await _notificationService.GetTemplateAsync(templateId);

                if (template == null)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "Failed to retrieve template", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets a notification template by name
        /// </summary>
        /// <param name="templateName">Template name</param>
        /// <returns>Template if found</returns>
        [HttpGet("templates/name/{templateName}")]
        [ProducesResponseType(typeof(NotificationTemplate), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationTemplate>> GetTemplateByName(string templateName)
        {
            try
            {
                var template = await _notificationService.GetTemplateByNameAsync(templateName);

                if (template == null)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(template);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification template by name {TemplateName}", templateName);
                return StatusCode(500, new { error = "Failed to retrieve template", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets all notification templates with optional filtering
        /// </summary>
        /// <param name="type">Optional filter by notification type</param>
        /// <param name="isActive">Optional filter by active status</param>
        /// <returns>List of templates</returns>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(List<NotificationTemplate>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<NotificationTemplate>>> GetTemplates(
            [FromQuery] NotificationType? type = null,
            [FromQuery] bool? isActive = null)
        {
            try
            {
                var templates = await _notificationService.GetTemplatesAsync(type, isActive);

                return Ok(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification templates");
                return StatusCode(500, new { error = "Failed to retrieve templates", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a notification template
        /// </summary>
        /// <param name="templateId">ID of the template to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("templates/{templateId}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> DeleteTemplate(Guid templateId)
        {
            try
            {
                _logger.LogInformation("Deleting notification template {TemplateId}", templateId);

                var result = await _notificationService.DeleteTemplateAsync(templateId);

                if (!result)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification template {TemplateId}", templateId);
                return StatusCode(500, new { error = "Failed to delete template", details = ex.Message });
            }
        }

        /// <summary>
        /// Activates or deactivates a template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <param name="isActive">Whether to activate or deactivate</param>
        /// <returns>Success status</returns>
        [HttpPatch("templates/{templateId}/active")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> SetTemplateActive(Guid templateId, [FromBody] SetActiveRequest request)
        {
            try
            {
                _logger.LogInformation("Setting notification template {TemplateId} active status to {IsActive}", 
                    templateId, request.IsActive);

                var result = await _notificationService.SetTemplateActiveAsync(templateId, request.IsActive);

                if (!result)
                {
                    return NotFound(new { error = "Template not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting template active status {TemplateId}", templateId);
                return StatusCode(500, new { error = "Failed to update template status", details = ex.Message });
            }
        }

        #endregion

        #region Delivery Tracking Endpoints

        /// <summary>
        /// Gets delivery status for a notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>List of delivery statuses</returns>
        [HttpGet("delivery/{notificationId}")]
        [ProducesResponseType(typeof(List<DeliveryStatusResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<DeliveryStatusResponse>>> GetDeliveryStatus(Guid notificationId)
        {
            try
            {
                var deliveryStatuses = await _notificationService.GetDeliveryStatusAsync(notificationId);

                return Ok(deliveryStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery status for notification {NotificationId}", notificationId);
                return StatusCode(500, new { error = "Failed to retrieve delivery status", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets delivery status for a specific delivery
        /// </summary>
        /// <param name="deliveryId">Delivery ID</param>
        /// <returns>Delivery status if found</returns>
        [HttpGet("delivery/status/{deliveryId}")]
        [ProducesResponseType(typeof(DeliveryStatusResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DeliveryStatusResponse>> GetDeliveryStatusById(Guid deliveryId)
        {
            try
            {
                var deliveryStatus = await _notificationService.GetDeliveryStatusByIdAsync(deliveryId);

                if (deliveryStatus == null)
                {
                    return NotFound(new { error = "Delivery not found" });
                }

                return Ok(deliveryStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery status {DeliveryId}", deliveryId);
                return StatusCode(500, new { error = "Failed to retrieve delivery status", details = ex.Message });
            }
        }

        /// <summary>
        /// Retries failed deliveries for a notification
        /// </summary>
        /// <param name="notificationId">Notification ID</param>
        /// <returns>Success status</returns>
        [HttpPost("delivery/{notificationId}/retry")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> RetryFailedDeliveries(Guid notificationId)
        {
            try
            {
                _logger.LogInformation("Retrying failed deliveries for notification {NotificationId}", notificationId);

                var result = await _notificationService.RetryFailedDeliveriesAsync(notificationId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed deliveries for notification {NotificationId}", notificationId);
                return StatusCode(500, new { error = "Failed to retry deliveries", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets delivery history for a recipient
        /// </summary>
        /// <param name="recipientId">Recipient ID</param>
        /// <param name="limit">Maximum number of records to return</param>
        /// <param name="offset">Number of records to skip</param>
        /// <returns>List of delivery statuses</returns>
        [HttpGet("delivery/history/{recipientId}")]
        [ProducesResponseType(typeof(List<DeliveryStatusResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<DeliveryStatusResponse>>> GetRecipientDeliveryHistory(
            string recipientId,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            try
            {
                var deliveryHistory = await _notificationService.GetRecipientDeliveryHistoryAsync(recipientId, limit, offset);

                return Ok(deliveryHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving delivery history for recipient {RecipientId}", recipientId);
                return StatusCode(500, new { error = "Failed to retrieve delivery history", details = ex.Message });
            }
        }

        #endregion

        #region Preferences Management Endpoints

        /// <summary>
        /// Gets notification preferences for the current user
        /// </summary>
        /// <returns>User's notification preferences</returns>
        [HttpGet("preferences")]
        [ProducesResponseType(typeof(NotificationPreferences), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationPreferences>> GetUserPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                var preferences = await _notificationService.GetUserPreferencesAsync(userId);

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user preferences");
                return StatusCode(500, new { error = "Failed to retrieve preferences", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates notification preferences for the current user
        /// </summary>
        /// <param name="preferences">Updated preferences</param>
        /// <returns>Updated preferences</returns>
        [HttpPut("preferences")]
        [ProducesResponseType(typeof(NotificationPreferences), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationPreferences>> UpdateUserPreferences([FromBody] NotificationPreferences preferences)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updatedPreferences = await _notificationService.UpdateUserPreferencesAsync(userId, preferences);

                return Ok(updatedPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences");
                return StatusCode(500, new { error = "Failed to update preferences", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets default notification preferences
        /// </summary>
        /// <returns>Default preferences</returns>
        [HttpGet("preferences/default")]
        [ProducesResponseType(typeof(NotificationPreferences), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationPreferences>> GetDefaultPreferences()
        {
            try
            {
                var defaultPreferences = await _notificationService.GetDefaultPreferencesAsync();

                return Ok(defaultPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default preferences");
                return StatusCode(500, new { error = "Failed to retrieve default preferences", details = ex.Message });
            }
        }

        /// <summary>
        /// Resets user preferences to defaults
        /// </summary>
        /// <returns>Reset preferences</returns>
        [HttpPost("preferences/reset")]
        [ProducesResponseType(typeof(NotificationPreferences), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationPreferences>> ResetUserPreferences()
        {
            try
            {
                var userId = GetCurrentUserId();
                var resetPreferences = await _notificationService.ResetUserPreferencesAsync(userId);

                return Ok(resetPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting user preferences");
                return StatusCode(500, new { error = "Failed to reset preferences", details = ex.Message });
            }
        }

        #endregion

        #region Statistics and Analytics Endpoints

        /// <summary>
        /// Gets notification statistics
        /// </summary>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Notification statistics</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(NotificationStatistics), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<NotificationStatistics>> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _notificationService.GetStatisticsAsync(startDate, endDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification statistics");
                return StatusCode(500, new { error = "Failed to retrieve statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets statistics for a specific notification type
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Type-specific statistics</returns>
        [HttpGet("statistics/type/{type}")]
        [ProducesResponseType(typeof(TypeStatistics), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<TypeStatistics>> GetTypeStatistics(
            NotificationType type,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _notificationService.GetTypeStatisticsAsync(type, startDate, endDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving type statistics for {Type}", type);
                return StatusCode(500, new { error = "Failed to retrieve type statistics", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets statistics for a specific priority level
        /// </summary>
        /// <param name="priority">Notification priority</param>
        /// <param name="startDate">Start date for statistics</param>
        /// <param name="endDate">End date for statistics</param>
        /// <returns>Priority-specific statistics</returns>
        [HttpGet("statistics/priority/{priority}")]
        [ProducesResponseType(typeof(PriorityStatistics), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PriorityStatistics>> GetPriorityStatistics(
            NotificationPriority priority,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _notificationService.GetPriorityStatisticsAsync(priority, startDate, endDate);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving priority statistics for {Priority}", priority);
                return StatusCode(500, new { error = "Failed to retrieve priority statistics", details = ex.Message });
            }
        }

        #endregion

        #region Bulk Operations Endpoints

        /// <summary>
        /// Sends notifications to multiple recipients in bulk
        /// </summary>
        /// <param name="messages">List of notification messages</param>
        /// <returns>List of notification responses</returns>
        [HttpPost("bulk/send")]
        [ProducesResponseType(typeof(List<NotificationResponse>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<NotificationResponse>>> SendBulkNotifications([FromBody] List<NotificationMessage> messages)
        {
            try
            {
                if (messages == null || !messages.Any())
                {
                    return BadRequest(new { error = "At least one notification message is required" });
                }

                _logger.LogInformation("Sending {Count} bulk notifications", messages.Count);

                var responses = await _notificationService.SendBulkNotificationsAsync(messages);

                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk notifications");
                return StatusCode(500, new { error = "Failed to send bulk notifications", details = ex.Message });
            }
        }

        /// <summary>
        /// Sends template notifications to multiple recipients in bulk
        /// </summary>
        /// <param name="request">Bulk template notification request</param>
        /// <returns>List of notification responses</returns>
        [HttpPost("bulk/send-template")]
        [ProducesResponseType(typeof(List<NotificationResponse>), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<NotificationResponse>>> SendBulkTemplateNotifications([FromBody] BulkTemplateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TemplateName))
                {
                    return BadRequest(new { error = "Template name is required" });
                }

                if (request.Recipients == null || !request.Recipients.Any())
                {
                    return BadRequest(new { error = "At least one recipient is required" });
                }

                _logger.LogInformation("Sending {Count} bulk template notifications using template {TemplateName}", 
                    request.Recipients.Count, request.TemplateName);

                var responses = await _notificationService.SendBulkTemplateNotificationsAsync(
                    request.TemplateName,
                    request.Recipients,
                    request.Variables ?? new Dictionary<string, object>(),
                    request.Priority);

                return Ok(responses);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Bulk template notification request validation failed");
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk template notifications");
                return StatusCode(500, new { error = "Failed to send bulk template notifications", details = ex.Message });
            }
        }

        #endregion

        #region Health and Diagnostics Endpoints

        /// <summary>
        /// Tests notification service connectivity
        /// </summary>
        /// <param name="type">Notification type to test</param>
        /// <returns>Test result</returns>
        [HttpGet("health/test/{type}")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> TestConnectivity(NotificationType type)
        {
            try
            {
                var result = await _notificationService.TestConnectivityAsync(type);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connectivity for notification type {Type}", type);
                return StatusCode(500, new { error = "Failed to test connectivity", details = ex.Message });
            }
        }

        /// <summary>
        /// Gets service health status
        /// </summary>
        /// <returns>Health status information</returns>
        [HttpGet("health/status")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> GetHealthStatus()
        {
            try
            {
                var healthStatus = await _notificationService.GetHealthStatusAsync();

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health status");
                return StatusCode(500, new { error = "Failed to retrieve health status", details = ex.Message });
            }
        }

        /// <summary>
        /// Validates notification configuration
        /// </summary>
        /// <returns>Validation results</returns>
        [HttpGet("health/validate")]
        [ProducesResponseType(typeof(Dictionary<string, object>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Dictionary<string, object>>> ValidateConfiguration()
        {
            try
            {
                var validationResults = await _notificationService.ValidateConfigurationAsync();

                return Ok(validationResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration");
                return StatusCode(500, new { error = "Failed to validate configuration", details = ex.Message });
            }
        }

        #endregion

        #region Private Helper Methods

        private string GetCurrentUserId()
        {
            // Implementation depends on your authentication system
            // This is a placeholder - replace with actual user ID extraction
            return User.Identity?.Name ?? "anonymous";
        }

        private int GetRecipientCount(CreateNotificationRequest request)
        {
            var count = 0;
            count += request.EmailRecipients?.Count ?? 0;
            count += request.SmsRecipients?.Count ?? 0;
            count += request.UserRecipients?.Count ?? 0;
            count += request.WebhookUrls?.Count ?? 0;
            return count;
        }

        #endregion

        #region Request Models

        public class SendTemplateRequest
        {
            [Required]
            public string TemplateName { get; set; } = string.Empty;

            [Required]
            public List<NotificationRecipient> Recipients { get; set; } = new();

            public Dictionary<string, object>? Variables { get; set; }

            public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        }

        public class ScheduleNotificationRequest
        {
            [Required]
            public NotificationMessage Message { get; set; } = new();

            [Required]
            public DateTime ScheduledAt { get; set; }
        }

        public class SetActiveRequest
        {
            [Required]
            public bool IsActive { get; set; }
        }

        public class BulkTemplateRequest
        {
            [Required]
            public string TemplateName { get; set; } = string.Empty;

            [Required]
            public List<NotificationRecipient> Recipients { get; set; } = new();

            public Dictionary<string, object>? Variables { get; set; }

            public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
        }

        #endregion
    }
} 