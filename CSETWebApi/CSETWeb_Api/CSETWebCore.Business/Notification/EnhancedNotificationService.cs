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
using CSETWebCore.DataLayer.Model;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Mail;
using System.Net;
using System.IO;
using HtmlAgilityPack;

namespace CSETWebCore.Business.Notification
{
    /// <summary>
    /// Enhanced notification service implementation for enterprise-grade notification capabilities
    /// </summary>
    public class EnhancedNotificationService : IEnhancedNotificationService
    {
        private readonly ILogger<EnhancedNotificationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly CSETContext _context;
        private readonly INotificationBusiness _legacyNotificationService;
        private readonly IEmailNotificationProvider _emailProvider;
        private readonly ISmsNotificationProvider _smsProvider;
        private readonly IPushNotificationProvider _pushProvider;
        private readonly IWebhookNotificationProvider _webhookProvider;
        private readonly IInAppNotificationProvider _inAppProvider;
        private readonly Dictionary<string, NotificationTemplate> _templateCache;
        private readonly Dictionary<string, NotificationPreferences> _preferencesCache;

        public EnhancedNotificationService(
            ILogger<EnhancedNotificationService> logger,
            IConfiguration configuration,
            CSETContext context,
            INotificationBusiness legacyNotificationService,
            IEmailNotificationProvider emailProvider,
            ISmsNotificationProvider smsProvider,
            IPushNotificationProvider pushProvider,
            IWebhookNotificationProvider webhookProvider,
            IInAppNotificationProvider inAppProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _legacyNotificationService = legacyNotificationService;
            _emailProvider = emailProvider;
            _smsProvider = smsProvider;
            _pushProvider = pushProvider;
            _webhookProvider = webhookProvider;
            _inAppProvider = inAppProvider;
            _templateCache = new Dictionary<string, NotificationTemplate>();
            _preferencesCache = new Dictionary<string, NotificationPreferences>();
        }

        #region Core Notification Methods

        public async Task<NotificationResponse> SendNotificationAsync(NotificationMessage message)
        {
            try
            {
                _logger.LogInformation("Sending notification {NotificationId} of type {Type} to {RecipientCount} recipients",
                    message.Id, message.Type, message.Recipients.Count);

                // Validate message
                var validationResult = ValidateNotificationMessage(message);
                if (!validationResult.IsValid)
                {
                    return new NotificationResponse
                    {
                        Id = message.Id,
                        Status = "Failed",
                        ErrorMessage = string.Join("; ", validationResult.Errors),
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Process template if specified
                if (!string.IsNullOrEmpty(message.TemplateName))
                {
                    await ProcessTemplateAsync(message);
                }

                // Create delivery records
                var deliveries = new List<NotificationDelivery>();
                foreach (var recipient in message.Recipients)
                {
                    var delivery = new NotificationDelivery
                    {
                        NotificationId = message.Id,
                        Recipient = recipient,
                        Status = DeliveryStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    deliveries.Add(delivery);
                }

                // Send notifications based on type
                var results = new List<DeliveryResult>();
                switch (message.Type)
                {
                    case NotificationType.Email:
                        results = await SendEmailNotificationsAsync(message, message.Recipients);
                        break;
                    case NotificationType.Sms:
                        results = await SendSmsNotificationsAsync(message, message.Recipients);
                        break;
                    case NotificationType.Push:
                        results = await SendPushNotificationsAsync(message, message.Recipients);
                        break;
                    case NotificationType.Webhook:
                        results = await SendWebhookNotificationsAsync(message, message.Recipients);
                        break;
                    case NotificationType.InApp:
                        results = await SendInAppNotificationsAsync(message, message.Recipients);
                        break;
                    default:
                        throw new NotSupportedException($"Notification type {message.Type} is not supported");
                }

                // Update delivery records
                for (int i = 0; i < deliveries.Count && i < results.Count; i++)
                {
                    var delivery = deliveries[i];
                    var result = results[i];
                    
                    delivery.Status = result.Success ? DeliveryStatus.Delivered : DeliveryStatus.Failed;
                    delivery.ExternalId = result.ExternalId;
                    delivery.ErrorMessage = result.ErrorMessage;
                    delivery.DeliveredAt = result.Success ? result.DeliveredAt : null;
                    delivery.UpdatedAt = DateTime.UtcNow;
                    delivery.Metadata = result.Metadata;
                }

                // Save delivery records to database
                await SaveDeliveryRecordsAsync(deliveries);

                // Update statistics
                await UpdateStatisticsAsync(message, results);

                var successCount = results.Count(r => r.Success);
                var failureCount = results.Count - successCount;

                _logger.LogInformation("Notification {NotificationId} completed: {SuccessCount} successful, {FailureCount} failed",
                    message.Id, successCount, failureCount);

                return new NotificationResponse
                {
                    Id = message.Id,
                    Status = failureCount == 0 ? "Delivered" : failureCount == results.Count ? "Failed" : "PartiallyDelivered",
                    RecipientCount = message.Recipients.Count,
                    CreatedAt = message.CreatedAt,
                    ScheduledAt = message.ScheduledAt,
                    ErrorMessage = failureCount > 0 ? $"{failureCount} deliveries failed" : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification {NotificationId}", message.Id);
                return new NotificationResponse
                {
                    Id = message.Id,
                    Status = "Failed",
                    ErrorMessage = ex.Message,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<NotificationResponse> SendTemplateNotificationAsync(
            string templateName,
            List<NotificationRecipient> recipients,
            Dictionary<string, object> variables,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            var template = await GetTemplateByNameAsync(templateName);
            if (template == null)
            {
                throw new ArgumentException($"Template '{templateName}' not found");
            }

            var message = new NotificationMessage
            {
                Type = template.Type,
                Priority = priority,
                Subject = ProcessTemplate(template.SubjectTemplate, variables),
                Body = ProcessTemplate(template.BodyTemplate, variables),
                HtmlBody = !string.IsNullOrEmpty(template.HtmlBodyTemplate) 
                    ? ProcessTemplate(template.HtmlBodyTemplate, variables) 
                    : null,
                Recipients = recipients,
                TemplateName = templateName,
                TemplateVariables = variables,
                CreatedAt = DateTime.UtcNow
            };

            return await SendNotificationAsync(message);
        }

        public async Task<NotificationResponse> SendNotificationAsync(CreateNotificationRequest request)
        {
            var recipients = new List<NotificationRecipient>();

            // Process email recipients
            if (request.EmailRecipients?.Any() == true)
            {
                foreach (var email in request.EmailRecipients)
                {
                    recipients.Add(new NotificationRecipient
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = email,
                        PreferredChannels = new List<NotificationType> { NotificationType.Email }
                    });
                }
            }

            // Process SMS recipients
            if (request.SmsRecipients?.Any() == true)
            {
                foreach (var phone in request.SmsRecipients)
                {
                    recipients.Add(new NotificationRecipient
                    {
                        Id = Guid.NewGuid().ToString(),
                        PhoneNumber = phone,
                        PreferredChannels = new List<NotificationType> { NotificationType.Sms }
                    });
                }
            }

            // Process user recipients
            if (request.UserRecipients?.Any() == true)
            {
                foreach (var userId in request.UserRecipients)
                {
                    recipients.Add(new NotificationRecipient
                    {
                        Id = userId,
                        UserId = userId,
                        PreferredChannels = new List<NotificationType> { NotificationType.InApp, NotificationType.Email }
                    });
                }
            }

            // Process webhook recipients
            if (request.WebhookUrls?.Any() == true)
            {
                foreach (var url in request.WebhookUrls)
                {
                    recipients.Add(new NotificationRecipient
                    {
                        Id = Guid.NewGuid().ToString(),
                        WebhookUrl = url,
                        PreferredChannels = new List<NotificationType> { NotificationType.Webhook }
                    });
                }
            }

            var message = new NotificationMessage
            {
                Type = request.Type,
                Priority = request.Priority,
                Subject = request.Subject,
                Body = request.Body,
                HtmlBody = request.HtmlBody,
                Recipients = recipients,
                TemplateName = request.TemplateName,
                TemplateVariables = request.TemplateVariables ?? new Dictionary<string, object>(),
                ScheduledAt = request.ScheduledAt,
                ExpiresAt = request.ExpiresAt,
                Tags = request.Tags ?? new List<string>(),
                Source = request.Source,
                RelatedEntities = request.RelatedEntities ?? new Dictionary<string, object>(),
                CreatedAt = DateTime.UtcNow
            };

            return await SendNotificationAsync(message);
        }

        public async Task<NotificationResponse> ScheduleNotificationAsync(NotificationMessage message, DateTime scheduledAt)
        {
            message.ScheduledAt = scheduledAt;
            
            // Store scheduled notification in database
            await StoreScheduledNotificationAsync(message);

            _logger.LogInformation("Scheduled notification {NotificationId} for {ScheduledAt}", 
                message.Id, scheduledAt);

            return new NotificationResponse
            {
                Id = message.Id,
                Status = "Scheduled",
                RecipientCount = message.Recipients.Count,
                CreatedAt = message.CreatedAt,
                ScheduledAt = scheduledAt
            };
        }

        public async Task<bool> CancelScheduledNotificationAsync(Guid notificationId)
        {
            try
            {
                // Remove from scheduled notifications
                await RemoveScheduledNotificationAsync(notificationId);
                
                _logger.LogInformation("Cancelled scheduled notification {NotificationId}", notificationId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling scheduled notification {NotificationId}", notificationId);
                return false;
            }
        }

        #endregion

        #region Template Management

        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
        {
            // Validate template
            var validationResult = ValidateTemplate(template);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join("; ", validationResult.Errors)}");
            }

            // Check for duplicate name
            var existingTemplate = await GetTemplateByNameAsync(template.Name);
            if (existingTemplate != null)
            {
                throw new ArgumentException($"Template with name '{template.Name}' already exists");
            }

            template.Id = Guid.NewGuid();
            template.CreatedAt = DateTime.UtcNow;
            template.ModifiedAt = DateTime.UtcNow;

            // Store template in database
            await StoreTemplateAsync(template);

            // Add to cache
            _templateCache[template.Name] = template;

            _logger.LogInformation("Created notification template {TemplateName}", template.Name);

            return template;
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(Guid templateId, NotificationTemplate template)
        {
            var existingTemplate = await GetTemplateAsync(templateId);
            if (existingTemplate == null)
            {
                throw new ArgumentException($"Template with ID '{templateId}' not found");
            }

            // Validate template
            var validationResult = ValidateTemplate(template);
            if (!validationResult.IsValid)
            {
                throw new ArgumentException($"Template validation failed: {string.Join("; ", validationResult.Errors)}");
            }

            // Check for duplicate name (excluding current template)
            var duplicateTemplate = await GetTemplateByNameAsync(template.Name);
            if (duplicateTemplate != null && duplicateTemplate.Id != templateId)
            {
                throw new ArgumentException($"Template with name '{template.Name}' already exists");
            }

            template.Id = templateId;
            template.Version = existingTemplate.Version + 1;
            template.ModifiedAt = DateTime.UtcNow;

            // Store updated template in database
            await StoreTemplateAsync(template);

            // Update cache
            _templateCache[template.Name] = template;

            _logger.LogInformation("Updated notification template {TemplateName} to version {Version}", 
                template.Name, template.Version);

            return template;
        }

        public async Task<NotificationTemplate?> GetTemplateAsync(Guid templateId)
        {
            // Try to get from database
            return await GetTemplateFromDatabaseAsync(templateId);
        }

        public async Task<NotificationTemplate?> GetTemplateByNameAsync(string templateName)
        {
            // Try cache first
            if (_templateCache.TryGetValue(templateName, out var cachedTemplate))
            {
                return cachedTemplate;
            }

            // Get from database
            var template = await GetTemplateFromDatabaseByNameAsync(templateName);
            if (template != null)
            {
                _templateCache[templateName] = template;
            }

            return template;
        }

        public async Task<List<NotificationTemplate>> GetTemplatesAsync(
            NotificationType? type = null,
            bool? isActive = null)
        {
            // Get from database with filters
            return await GetTemplatesFromDatabaseAsync(type, isActive);
        }

        public async Task<bool> DeleteTemplateAsync(Guid templateId)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                return false;
            }

            // Remove from database
            await RemoveTemplateFromDatabaseAsync(templateId);

            // Remove from cache
            if (_templateCache.ContainsKey(template.Name))
            {
                _templateCache.Remove(template.Name);
            }

            _logger.LogInformation("Deleted notification template {TemplateName}", template.Name);

            return true;
        }

        public async Task<bool> SetTemplateActiveAsync(Guid templateId, bool isActive)
        {
            var template = await GetTemplateAsync(templateId);
            if (template == null)
            {
                return false;
            }

            template.IsActive = isActive;
            template.ModifiedAt = DateTime.UtcNow;

            // Update in database
            await StoreTemplateAsync(template);

            // Update cache
            _templateCache[template.Name] = template;

            _logger.LogInformation("Set notification template {TemplateName} active status to {IsActive}", 
                template.Name, isActive);

            return true;
        }

        #endregion

        #region Delivery Tracking

        public async Task<List<DeliveryStatusResponse>> GetDeliveryStatusAsync(Guid notificationId)
        {
            var deliveries = await GetDeliveryRecordsAsync(notificationId);
            
            return deliveries.Select(d => new DeliveryStatusResponse
            {
                Id = d.Id,
                NotificationId = d.NotificationId,
                Recipient = GetRecipientDisplay(d.Recipient),
                Status = d.Status,
                RetryCount = d.RetryCount,
                NextRetryAt = d.NextRetryAt,
                DeliveredAt = d.DeliveredAt,
                ErrorMessage = d.ErrorMessage,
                ExternalId = d.ExternalId
            }).ToList();
        }

        public async Task<DeliveryStatusResponse?> GetDeliveryStatusByIdAsync(Guid deliveryId)
        {
            var delivery = await GetDeliveryRecordAsync(deliveryId);
            if (delivery == null)
            {
                return null;
            }

            return new DeliveryStatusResponse
            {
                Id = delivery.Id,
                NotificationId = delivery.NotificationId,
                Recipient = GetRecipientDisplay(delivery.Recipient),
                Status = delivery.Status,
                RetryCount = delivery.RetryCount,
                NextRetryAt = delivery.NextRetryAt,
                DeliveredAt = delivery.DeliveredAt,
                ErrorMessage = delivery.ErrorMessage,
                ExternalId = delivery.ExternalId
            };
        }

        public async Task<bool> RetryFailedDeliveriesAsync(Guid notificationId)
        {
            var failedDeliveries = await GetFailedDeliveryRecordsAsync(notificationId);
            if (!failedDeliveries.Any())
            {
                return false;
            }

            foreach (var delivery in failedDeliveries)
            {
                if (delivery.RetryCount < delivery.RetryConfig.MaxRetries)
                {
                    delivery.Status = DeliveryStatus.RetryScheduled;
                    delivery.RetryCount++;
                    delivery.NextRetryAt = CalculateNextRetryTime(delivery);
                    delivery.UpdatedAt = DateTime.UtcNow;

                    await UpdateDeliveryRecordAsync(delivery);
                }
            }

            _logger.LogInformation("Scheduled retry for {Count} failed deliveries of notification {NotificationId}", 
                failedDeliveries.Count, notificationId);

            return true;
        }

        public async Task<List<DeliveryStatusResponse>> GetRecipientDeliveryHistoryAsync(
            string recipientId,
            int limit = 50,
            int offset = 0)
        {
            var deliveries = await GetRecipientDeliveryRecordsAsync(recipientId, limit, offset);
            
            return deliveries.Select(d => new DeliveryStatusResponse
            {
                Id = d.Id,
                NotificationId = d.NotificationId,
                Recipient = GetRecipientDisplay(d.Recipient),
                Status = d.Status,
                RetryCount = d.RetryCount,
                NextRetryAt = d.NextRetryAt,
                DeliveredAt = d.DeliveredAt,
                ErrorMessage = d.ErrorMessage,
                ExternalId = d.ExternalId
            }).ToList();
        }

        #endregion

        #region Preferences Management

        public async Task<NotificationPreferences> GetUserPreferencesAsync(string userId)
        {
            // Try cache first
            if (_preferencesCache.TryGetValue(userId, out var cachedPreferences))
            {
                return cachedPreferences;
            }

            // Get from database
            var preferences = await GetUserPreferencesFromDatabaseAsync(userId);
            if (preferences == null)
            {
                // Create default preferences
                preferences = await GetDefaultPreferencesAsync();
                preferences.UserId = userId;
                await StoreUserPreferencesAsync(preferences);
            }

            // Add to cache
            _preferencesCache[userId] = preferences;

            return preferences;
        }

        public async Task<NotificationPreferences> UpdateUserPreferencesAsync(string userId, NotificationPreferences preferences)
        {
            preferences.UserId = userId;
            preferences.UpdatedAt = DateTime.UtcNow;

            // Store in database
            await StoreUserPreferencesAsync(preferences);

            // Update cache
            _preferencesCache[userId] = preferences;

            _logger.LogInformation("Updated notification preferences for user {UserId}", userId);

            return preferences;
        }

        public async Task<NotificationPreferences> GetDefaultPreferencesAsync()
        {
            return new NotificationPreferences
            {
                Email = new EmailPreferences
                {
                    Enabled = true,
                    Format = EmailFormat.Html,
                    MaxPerDay = 50,
                    Timezone = "UTC"
                },
                Sms = new SmsPreferences
                {
                    Enabled = false,
                    MaxPerDay = 10,
                    AllowQuietHours = false
                },
                Push = new PushPreferences
                {
                    Enabled = true,
                    ShowWhenForeground = true,
                    PlaySound = true,
                    Vibrate = true
                },
                Webhook = new WebhookPreferences
                {
                    Enabled = false
                },
                Global = new GlobalPreferences
                {
                    NotificationsEnabled = true,
                    DefaultChannels = new List<NotificationType> { NotificationType.Email },
                    Language = "en-US",
                    Timezone = "UTC",
                    GroupNotifications = true,
                    MaxHistoryCount = 1000
                },
                Categories = new Dictionary<string, CategoryPreferences>
                {
                    ["assessment"] = new CategoryPreferences
                    {
                        Enabled = true,
                        Channels = new List<NotificationType> { NotificationType.Email, NotificationType.InApp },
                        MinimumPriority = NotificationPriority.Normal,
                        RespectQuietHours = true
                    },
                    ["system"] = new CategoryPreferences
                    {
                        Enabled = true,
                        Channels = new List<NotificationType> { NotificationType.Email },
                        MinimumPriority = NotificationPriority.High,
                        RespectQuietHours = false
                    }
                }
            };
        }

        public async Task<NotificationPreferences> ResetUserPreferencesAsync(string userId)
        {
            var defaultPreferences = await GetDefaultPreferencesAsync();
            return await UpdateUserPreferencesAsync(userId, defaultPreferences);
        }

        #endregion

        #region Statistics and Analytics

        public async Task<NotificationStatistics> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await GetStatisticsFromDatabaseAsync(start, end);

            _logger.LogInformation("Retrieved notification statistics for period {StartDate} to {EndDate}", start, end);

            return statistics;
        }

        public async Task<TypeStatistics> GetTypeStatisticsAsync(
            NotificationType type,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await GetTypeStatisticsFromDatabaseAsync(type, start, end);

            return statistics;
        }

        public async Task<PriorityStatistics> GetPriorityStatisticsAsync(
            NotificationPriority priority,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = await GetPriorityStatisticsFromDatabaseAsync(priority, start, end);

            return statistics;
        }

        #endregion

        #region Bulk Operations

        public async Task<List<NotificationResponse>> SendBulkNotificationsAsync(List<NotificationMessage> messages)
        {
            var responses = new List<NotificationResponse>();

            foreach (var message in messages)
            {
                var response = await SendNotificationAsync(message);
                responses.Add(response);
            }

            _logger.LogInformation("Sent {Count} bulk notifications", messages.Count);

            return responses;
        }

        public async Task<List<NotificationResponse>> SendBulkTemplateNotificationsAsync(
            string templateName,
            List<NotificationRecipient> recipients,
            Dictionary<string, object> variables,
            NotificationPriority priority = NotificationPriority.Normal)
        {
            var responses = new List<NotificationResponse>();

            foreach (var recipient in recipients)
            {
                var response = await SendTemplateNotificationAsync(templateName, new List<NotificationRecipient> { recipient }, variables, priority);
                responses.Add(response);
            }

            _logger.LogInformation("Sent {Count} bulk template notifications using template {TemplateName}", 
                recipients.Count, templateName);

            return responses;
        }

        #endregion

        #region Health and Diagnostics

        public async Task<bool> TestConnectivityAsync(NotificationType type)
        {
            try
            {
                switch (type)
                {
                    case NotificationType.Email:
                        return await _emailProvider.TestConnectivityAsync();
                    case NotificationType.Sms:
                        return await _smsProvider.TestConnectivityAsync();
                    case NotificationType.Push:
                        return await _pushProvider.TestConnectivityAsync();
                    case NotificationType.Webhook:
                        return await _webhookProvider.TestConnectivityAsync();
                    case NotificationType.InApp:
                        return await _inAppProvider.TestConnectivityAsync();
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connectivity for notification type {Type}", type);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetHealthStatusAsync()
        {
            var healthStatus = new Dictionary<string, object>();

            // Test each provider
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                var isHealthy = await TestConnectivityAsync(type);
                healthStatus[$"{type}Provider"] = new
                {
                    Enabled = IsProviderEnabled(type),
                    Healthy = isHealthy,
                    LastTested = DateTime.UtcNow
                };
            }

            // Add system information
            healthStatus["System"] = new
            {
                Version = "1.0.0",
                Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                MemoryUsage = GC.GetTotalMemory(false),
                TemplateCacheSize = _templateCache.Count,
                PreferencesCacheSize = _preferencesCache.Count
            };

            return healthStatus;
        }

        public async Task<Dictionary<string, object>> ValidateConfigurationAsync()
        {
            var validationResults = new Dictionary<string, object>();

            // Validate each provider
            foreach (NotificationType type in Enum.GetValues(typeof(NotificationType)))
            {
                var provider = GetProvider(type);
                if (provider != null)
                {
                    var result = await provider.ValidateConfigurationAsync();
                    validationResults[$"{type}Provider"] = result;
                }
            }

            // Validate database connectivity
            try
            {
                await _context.Database.CanConnectAsync();
                validationResults["Database"] = new { IsValid = true };
            }
            catch (Exception ex)
            {
                validationResults["Database"] = new { IsValid = false, Error = ex.Message };
            }

            return validationResults;
        }

        #endregion

        #region Private Helper Methods

        private ValidationResult ValidateNotificationMessage(NotificationMessage message)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(message.Subject))
            {
                result.Errors.Add("Subject is required");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(message.Body))
            {
                result.Errors.Add("Body is required");
                result.IsValid = false;
            }

            if (!message.Recipients.Any())
            {
                result.Errors.Add("At least one recipient is required");
                result.IsValid = false;
            }

            if (message.ScheduledAt.HasValue && message.ScheduledAt.Value <= DateTime.UtcNow)
            {
                result.Errors.Add("Scheduled time must be in the future");
                result.IsValid = false;
            }

            return result;
        }

        private ValidationResult ValidateTemplate(NotificationTemplate template)
        {
            var result = new ValidationResult { IsValid = true };

            if (string.IsNullOrEmpty(template.Name))
            {
                result.Errors.Add("Template name is required");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(template.SubjectTemplate))
            {
                result.Errors.Add("Subject template is required");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(template.BodyTemplate))
            {
                result.Errors.Add("Body template is required");
                result.IsValid = false;
            }

            return result;
        }

        private async Task ProcessTemplateAsync(NotificationMessage message)
        {
            var template = await GetTemplateByNameAsync(message.TemplateName!);
            if (template == null)
            {
                throw new ArgumentException($"Template '{message.TemplateName}' not found");
            }

            message.Subject = ProcessTemplate(template.SubjectTemplate, message.TemplateVariables);
            message.Body = ProcessTemplate(template.BodyTemplate, message.TemplateVariables);
            
            if (!string.IsNullOrEmpty(template.HtmlBodyTemplate))
            {
                message.HtmlBody = ProcessTemplate(template.HtmlBodyTemplate, message.TemplateVariables);
            }
        }

        private string ProcessTemplate(string template, Dictionary<string, object> variables)
        {
            var result = template;

            foreach (var variable in variables)
            {
                var placeholder = $"{{{{{variable.Key}}}}}";
                var value = variable.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }

            return result;
        }

        private async Task<List<DeliveryResult>> SendEmailNotificationsAsync(NotificationMessage message, List<NotificationRecipient> recipients)
        {
            var results = new List<DeliveryResult>();

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.Email))
                {
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No email address provided",
                        ErrorCode = "MISSING_EMAIL"
                    });
                    continue;
                }

                try
                {
                    var result = await _emailProvider.SendEmailAsync(
                        recipient.Email,
                        message.Subject,
                        message.Body,
                        message.HtmlBody);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email to {Email}", recipient.Email);
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ErrorCode = "EMAIL_SEND_ERROR"
                    });
                }
            }

            return results;
        }

        private async Task<List<DeliveryResult>> SendSmsNotificationsAsync(NotificationMessage message, List<NotificationRecipient> recipients)
        {
            var results = new List<DeliveryResult>();

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.PhoneNumber))
                {
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No phone number provided",
                        ErrorCode = "MISSING_PHONE"
                    });
                    continue;
                }

                try
                {
                    var result = await _smsProvider.SendSmsAsync(
                        recipient.PhoneNumber,
                        message.Body);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", recipient.PhoneNumber);
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ErrorCode = "SMS_SEND_ERROR"
                    });
                }
            }

            return results;
        }

        private async Task<List<DeliveryResult>> SendPushNotificationsAsync(NotificationMessage message, List<NotificationRecipient> recipients)
        {
            var results = new List<DeliveryResult>();

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.UserId))
                {
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No user ID provided",
                        ErrorCode = "MISSING_USER_ID"
                    });
                    continue;
                }

                try
                {
                    // Get user's push tokens from preferences
                    var preferences = await GetUserPreferencesAsync(recipient.UserId);
                    if (!preferences.Push.Enabled || !preferences.Push.Tokens.Any())
                    {
                        results.Add(new DeliveryResult
                        {
                            Success = false,
                            ErrorMessage = "No push tokens available",
                            ErrorCode = "NO_PUSH_TOKENS"
                        });
                        continue;
                    }

                    var pushOptions = new PushNotificationOptions
                    {
                        PlaySound = preferences.Push.PlaySound,
                        Vibrate = preferences.Push.Vibrate,
                        ShowWhenForeground = preferences.Push.ShowWhenForeground
                    };

                    var result = await _pushProvider.SendPushAsync(
                        preferences.Push.Tokens.First(),
                        message.Subject,
                        message.Body,
                        message.RelatedEntities,
                        pushOptions);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending push notification to user {UserId}", recipient.UserId);
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ErrorCode = "PUSH_SEND_ERROR"
                    });
                }
            }

            return results;
        }

        private async Task<List<DeliveryResult>> SendWebhookNotificationsAsync(NotificationMessage message, List<NotificationRecipient> recipients)
        {
            var results = new List<DeliveryResult>();

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.WebhookUrl))
                {
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No webhook URL provided",
                        ErrorCode = "MISSING_WEBHOOK_URL"
                    });
                    continue;
                }

                try
                {
                    var payload = new
                    {
                        subject = message.Subject,
                        body = message.Body,
                        htmlBody = message.HtmlBody,
                        priority = message.Priority.ToString(),
                        source = message.Source,
                        relatedEntities = message.RelatedEntities,
                        metadata = message.Metadata,
                        tags = message.Tags,
                        createdAt = message.CreatedAt
                    };

                    var result = await _webhookProvider.SendWebhookAsync(
                        recipient.WebhookUrl,
                        payload);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending webhook to {WebhookUrl}", recipient.WebhookUrl);
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ErrorCode = "WEBHOOK_SEND_ERROR"
                    });
                }
            }

            return results;
        }

        private async Task<List<DeliveryResult>> SendInAppNotificationsAsync(NotificationMessage message, List<NotificationRecipient> recipients)
        {
            var results = new List<DeliveryResult>();

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrEmpty(recipient.UserId))
                {
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No user ID provided",
                        ErrorCode = "MISSING_USER_ID"
                    });
                    continue;
                }

                try
                {
                    var result = await _inAppProvider.SendInAppAsync(
                        recipient.UserId,
                        message.Subject,
                        message.Body,
                        "info",
                        message.RelatedEntities);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending in-app notification to user {UserId}", recipient.UserId);
                    results.Add(new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = ex.Message,
                        ErrorCode = "INAPP_SEND_ERROR"
                    });
                }
            }

            return results;
        }

        private string GetRecipientDisplay(NotificationRecipient recipient)
        {
            if (!string.IsNullOrEmpty(recipient.Email))
                return recipient.Email;
            if (!string.IsNullOrEmpty(recipient.PhoneNumber))
                return recipient.PhoneNumber;
            if (!string.IsNullOrEmpty(recipient.UserId))
                return $"User: {recipient.UserId}";
            if (!string.IsNullOrEmpty(recipient.WebhookUrl))
                return $"Webhook: {recipient.WebhookUrl}";
            
            return recipient.Id;
        }

        private DateTime CalculateNextRetryTime(NotificationDelivery delivery)
        {
            var config = delivery.RetryConfig;
            var delaySeconds = config.UseExponentialBackoff
                ? Math.Min(config.InitialDelaySeconds * Math.Pow(config.BackoffMultiplier, delivery.RetryCount - 1), config.MaxDelaySeconds)
                : config.InitialDelaySeconds;

            return DateTime.UtcNow.AddSeconds(delaySeconds);
        }

        private bool IsProviderEnabled(NotificationType type)
        {
            var provider = GetProvider(type);
            return provider?.IsEnabled ?? false;
        }

        private INotificationProvider? GetProvider(NotificationType type)
        {
            return type switch
            {
                NotificationType.Email => _emailProvider,
                NotificationType.Sms => _smsProvider,
                NotificationType.Push => _pushProvider,
                NotificationType.Webhook => _webhookProvider,
                NotificationType.InApp => _inAppProvider,
                _ => null
            };
        }

        // Database operations (to be implemented based on your data access layer)
        private async Task SaveDeliveryRecordsAsync(List<NotificationDelivery> deliveries)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task StoreScheduledNotificationAsync(NotificationMessage message)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task RemoveScheduledNotificationAsync(Guid notificationId)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task StoreTemplateAsync(NotificationTemplate template)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task<NotificationTemplate?> GetTemplateFromDatabaseAsync(Guid templateId)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult<NotificationTemplate?>(null);
        }

        private async Task<NotificationTemplate?> GetTemplateFromDatabaseByNameAsync(string templateName)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult<NotificationTemplate?>(null);
        }

        private async Task<List<NotificationTemplate>> GetTemplatesFromDatabaseAsync(NotificationType? type, bool? isActive)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new List<NotificationTemplate>());
        }

        private async Task RemoveTemplateFromDatabaseAsync(Guid templateId)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task<List<NotificationDelivery>> GetDeliveryRecordsAsync(Guid notificationId)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new List<NotificationDelivery>());
        }

        private async Task<NotificationDelivery?> GetDeliveryRecordAsync(Guid deliveryId)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult<NotificationDelivery?>(null);
        }

        private async Task<List<NotificationDelivery>> GetFailedDeliveryRecordsAsync(Guid notificationId)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new List<NotificationDelivery>());
        }

        private async Task UpdateDeliveryRecordAsync(NotificationDelivery delivery)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task<List<NotificationDelivery>> GetRecipientDeliveryRecordsAsync(string recipientId, int limit, int offset)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new List<NotificationDelivery>());
        }

        private async Task<NotificationPreferences?> GetUserPreferencesFromDatabaseAsync(string userId)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult<NotificationPreferences?>(null);
        }

        private async Task StoreUserPreferencesAsync(NotificationPreferences preferences)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task UpdateStatisticsAsync(NotificationMessage message, List<DeliveryResult> results)
        {
            // Implementation depends on your data access layer
            await Task.CompletedTask;
        }

        private async Task<NotificationStatistics> GetStatisticsFromDatabaseAsync(DateTime startDate, DateTime endDate)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new NotificationStatistics());
        }

        private async Task<TypeStatistics> GetTypeStatisticsFromDatabaseAsync(NotificationType type, DateTime startDate, DateTime endDate)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new TypeStatistics());
        }

        private async Task<PriorityStatistics> GetPriorityStatisticsFromDatabaseAsync(NotificationPriority priority, DateTime startDate, DateTime endDate)
        {
            // Implementation depends on your data access layer
            return await Task.FromResult(new PriorityStatistics());
        }

        #endregion
    }
} 