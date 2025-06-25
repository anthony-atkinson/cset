//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CSETWebCore.Model.Notification
{
    /// <summary>
    /// Represents a notification message with metadata and delivery options
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// Unique identifier for the notification
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Type of notification (Email, SMS, Push, Webhook, etc.)
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Priority level of the notification
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Subject or title of the notification
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Body content of the notification
        /// </summary>
        [Required]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// HTML body content for email notifications
        /// </summary>
        public string? HtmlBody { get; set; }

        /// <summary>
        /// Recipients of the notification
        /// </summary>
        [Required]
        public List<NotificationRecipient> Recipients { get; set; } = new();

        /// <summary>
        /// Template used for this notification
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Template variables for dynamic content
        /// </summary>
        public Dictionary<string, object> TemplateVariables { get; set; } = new();

        /// <summary>
        /// Scheduled delivery time (null for immediate delivery)
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Expiration time for the notification
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Retry configuration for failed deliveries
        /// </summary>
        public RetryConfiguration RetryConfig { get; set; } = new();

        /// <summary>
        /// Metadata and custom properties
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Tags for categorization and filtering
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Source or trigger of the notification
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Related entity information (e.g., AssessmentId, UserId)
        /// </summary>
        public Dictionary<string, object> RelatedEntities { get; set; } = new();
    }

    /// <summary>
    /// Represents a notification recipient
    /// </summary>
    public class NotificationRecipient
    {
        /// <summary>
        /// Unique identifier for the recipient
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name of the recipient
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address for email notifications
        /// </summary>
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Phone number for SMS notifications
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// User ID for push notifications
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Webhook URL for webhook notifications
        /// </summary>
        [Url]
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Preferred notification channels for this recipient
        /// </summary>
        public List<NotificationType> PreferredChannels { get; set; } = new();

        /// <summary>
        /// Language preference for internationalization
        /// </summary>
        public string Language { get; set; } = "en-US";

        /// <summary>
        /// Timezone for time-sensitive notifications
        /// </summary>
        public string Timezone { get; set; } = "UTC";
    }

    /// <summary>
    /// Configuration for retry logic
    /// </summary>
    public class RetryConfiguration
    {
        /// <summary>
        /// Maximum number of retry attempts
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Initial delay between retries in seconds
        /// </summary>
        public int InitialDelaySeconds { get; set; } = 60;

        /// <summary>
        /// Maximum delay between retries in seconds
        /// </summary>
        public int MaxDelaySeconds { get; set; } = 3600;

        /// <summary>
        /// Backoff multiplier for exponential backoff
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;

        /// <summary>
        /// Whether to use exponential backoff
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;
    }

    /// <summary>
    /// Notification template definition
    /// </summary>
    public class NotificationTemplate
    {
        /// <summary>
        /// Unique identifier for the template
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Template name (must be unique)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of notification this template is for
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Template subject with variable placeholders
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string SubjectTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Template body with variable placeholders
        /// </summary>
        [Required]
        public string BodyTemplate { get; set; } = string.Empty;

        /// <summary>
        /// HTML template body for email notifications
        /// </summary>
        public string? HtmlBodyTemplate { get; set; }

        /// <summary>
        /// Available variables for this template
        /// </summary>
        public List<TemplateVariable> Variables { get; set; } = new();

        /// <summary>
        /// Whether this template is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Version of the template
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tags for categorization
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Default priority for notifications using this template
        /// </summary>
        public NotificationPriority DefaultPriority { get; set; } = NotificationPriority.Normal;
    }

    /// <summary>
    /// Template variable definition
    /// </summary>
    public class TemplateVariable
    {
        /// <summary>
        /// Variable name (without braces)
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Variable description
        /// </summary>
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Variable type
        /// </summary>
        public VariableType Type { get; set; } = VariableType.String;

        /// <summary>
        /// Whether this variable is required
        /// </summary>
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Default value for the variable
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Validation pattern for the variable
        /// </summary>
        public string? ValidationPattern { get; set; }
    }

    /// <summary>
    /// Notification delivery status and tracking
    /// </summary>
    public class NotificationDelivery
    {
        /// <summary>
        /// Unique identifier for the delivery
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Associated notification message ID
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// Recipient information
        /// </summary>
        public NotificationRecipient Recipient { get; set; } = new();

        /// <summary>
        /// Delivery status
        /// </summary>
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Next retry attempt time
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// Delivery timestamp
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// External delivery ID (from provider)
        /// </summary>
        public string? ExternalId { get; set; }

        /// <summary>
        /// Delivery metadata
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new();

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// User notification preferences
    /// </summary>
    public class NotificationPreferences
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Email preferences
        /// </summary>
        public EmailPreferences Email { get; set; } = new();

        /// <summary>
        /// SMS preferences
        /// </summary>
        public SmsPreferences Sms { get; set; } = new();

        /// <summary>
        /// Push notification preferences
        /// </summary>
        public PushPreferences Push { get; set; } = new();

        /// <summary>
        /// Webhook preferences
        /// </summary>
        public WebhookPreferences Webhook { get; set; } = new();

        /// <summary>
        /// Global notification settings
        /// </summary>
        public GlobalPreferences Global { get; set; } = new();

        /// <summary>
        /// Category-specific preferences
        /// </summary>
        public Dictionary<string, CategoryPreferences> Categories { get; set; } = new();

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Email-specific preferences
    /// </summary>
    public class EmailPreferences
    {
        /// <summary>
        /// Whether email notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Email address for notifications
        /// </summary>
        [EmailAddress]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// Preferred email format
        /// </summary>
        public EmailFormat Format { get; set; } = EmailFormat.Html;

        /// <summary>
        /// Maximum emails per day
        /// </summary>
        public int MaxPerDay { get; set; } = 50;

        /// <summary>
        /// Quiet hours start (24-hour format)
        /// </summary>
        public TimeSpan? QuietHoursStart { get; set; }

        /// <summary>
        /// Quiet hours end (24-hour format)
        /// </summary>
        public TimeSpan? QuietHoursEnd { get; set; }

        /// <summary>
        /// Timezone for quiet hours
        /// </summary>
        public string Timezone { get; set; } = "UTC";
    }

    /// <summary>
    /// SMS-specific preferences
    /// </summary>
    public class SmsPreferences
    {
        /// <summary>
        /// Whether SMS notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Phone number for SMS notifications
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Maximum SMS per day
        /// </summary>
        public int MaxPerDay { get; set; } = 10;

        /// <summary>
        /// Whether to send SMS during quiet hours
        /// </summary>
        public bool AllowQuietHours { get; set; } = false;
    }

    /// <summary>
    /// Push notification preferences
    /// </summary>
    public class PushPreferences
    {
        /// <summary>
        /// Whether push notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Push notification tokens
        /// </summary>
        public List<string> Tokens { get; set; } = new();

        /// <summary>
        /// Whether to show notifications when app is in foreground
        /// </summary>
        public bool ShowWhenForeground { get; set; } = true;

        /// <summary>
        /// Sound preference for push notifications
        /// </summary>
        public bool PlaySound { get; set; } = true;

        /// <summary>
        /// Vibration preference for push notifications
        /// </summary>
        public bool Vibrate { get; set; } = true;
    }

    /// <summary>
    /// Webhook preferences
    /// </summary>
    public class WebhookPreferences
    {
        /// <summary>
        /// Whether webhook notifications are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Webhook URLs
        /// </summary>
        public List<string> Urls { get; set; } = new();

        /// <summary>
        /// Authentication headers for webhooks
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// Retry configuration for webhook delivery
        /// </summary>
        public RetryConfiguration RetryConfig { get; set; } = new();
    }

    /// <summary>
    /// Global notification preferences
    /// </summary>
    public class GlobalPreferences
    {
        /// <summary>
        /// Whether all notifications are enabled
        /// </summary>
        public bool NotificationsEnabled { get; set; } = true;

        /// <summary>
        /// Default notification channels
        /// </summary>
        public List<NotificationType> DefaultChannels { get; set; } = new() { NotificationType.Email };

        /// <summary>
        /// Language preference
        /// </summary>
        public string Language { get; set; } = "en-US";

        /// <summary>
        /// Timezone for time-sensitive notifications
        /// </summary>
        public string Timezone { get; set; } = "UTC";

        /// <summary>
        /// Whether to group similar notifications
        /// </summary>
        public bool GroupNotifications { get; set; } = true;

        /// <summary>
        /// Maximum notifications to keep in history
        /// </summary>
        public int MaxHistoryCount { get; set; } = 1000;
    }

    /// <summary>
    /// Category-specific notification preferences
    /// </summary>
    public class CategoryPreferences
    {
        /// <summary>
        /// Whether notifications for this category are enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Preferred channels for this category
        /// </summary>
        public List<NotificationType> Channels { get; set; } = new();

        /// <summary>
        /// Minimum priority level for this category
        /// </summary>
        public NotificationPriority MinimumPriority { get; set; } = NotificationPriority.Low;

        /// <summary>
        /// Whether to respect quiet hours for this category
        /// </summary>
        public bool RespectQuietHours { get; set; } = true;
    }

    /// <summary>
    /// Notification statistics and analytics
    /// </summary>
    public class NotificationStatistics
    {
        /// <summary>
        /// Total notifications sent
        /// </summary>
        public long TotalSent { get; set; }

        /// <summary>
        /// Total notifications delivered successfully
        /// </summary>
        public long TotalDelivered { get; set; }

        /// <summary>
        /// Total notifications failed
        /// </summary>
        public long TotalFailed { get; set; }

        /// <summary>
        /// Total notifications pending
        /// </summary>
        public long TotalPending { get; set; }

        /// <summary>
        /// Delivery rate percentage
        /// </summary>
        public double DeliveryRate => TotalSent > 0 ? (double)TotalDelivered / TotalSent * 100 : 0;

        /// <summary>
        /// Failure rate percentage
        /// </summary>
        public double FailureRate => TotalSent > 0 ? (double)TotalFailed / TotalSent * 100 : 0;

        /// <summary>
        /// Statistics by notification type
        /// </summary>
        public Dictionary<NotificationType, TypeStatistics> ByType { get; set; } = new();

        /// <summary>
        /// Statistics by priority
        /// </summary>
        public Dictionary<NotificationPriority, PriorityStatistics> ByPriority { get; set; } = new();

        /// <summary>
        /// Statistics by time period
        /// </summary>
        public List<TimePeriodStatistics> ByTimePeriod { get; set; } = new();

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Statistics for a specific notification type
    /// </summary>
    public class TypeStatistics
    {
        /// <summary>
        /// Total sent for this type
        /// </summary>
        public long Sent { get; set; }

        /// <summary>
        /// Total delivered for this type
        /// </summary>
        public long Delivered { get; set; }

        /// <summary>
        /// Total failed for this type
        /// </summary>
        public long Failed { get; set; }

        /// <summary>
        /// Average delivery time in seconds
        /// </summary>
        public double AverageDeliveryTimeSeconds { get; set; }
    }

    /// <summary>
    /// Statistics for a specific priority level
    /// </summary>
    public class PriorityStatistics
    {
        /// <summary>
        /// Total sent for this priority
        /// </summary>
        public long Sent { get; set; }

        /// <summary>
        /// Total delivered for this priority
        /// </summary>
        public long Delivered { get; set; }

        /// <summary>
        /// Total failed for this priority
        /// </summary>
        public long Failed { get; set; }
    }

    /// <summary>
    /// Statistics for a specific time period
    /// </summary>
    public class TimePeriodStatistics
    {
        /// <summary>
        /// Time period start
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Time period end
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Total sent in this period
        /// </summary>
        public long Sent { get; set; }

        /// <summary>
        /// Total delivered in this period
        /// </summary>
        public long Delivered { get; set; }

        /// <summary>
        /// Total failed in this period
        /// </summary>
        public long Failed { get; set; }
    }

    #region Enums

    /// <summary>
    /// Types of notifications supported by the system
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Email notification
        /// </summary>
        Email = 1,

        /// <summary>
        /// SMS notification
        /// </summary>
        Sms = 2,

        /// <summary>
        /// Push notification
        /// </summary>
        Push = 3,

        /// <summary>
        /// Webhook notification
        /// </summary>
        Webhook = 4,

        /// <summary>
        /// In-app notification
        /// </summary>
        InApp = 5,

        /// <summary>
        /// Slack notification
        /// </summary>
        Slack = 6,

        /// <summary>
        /// Teams notification
        /// </summary>
        Teams = 7
    }

    /// <summary>
    /// Priority levels for notifications
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Low priority - can be delayed
        /// </summary>
        Low = 1,

        /// <summary>
        /// Normal priority - standard delivery
        /// </summary>
        Normal = 2,

        /// <summary>
        /// High priority - expedited delivery
        /// </summary>
        High = 3,

        /// <summary>
        /// Urgent priority - immediate delivery
        /// </summary>
        Urgent = 4,

        /// <summary>
        /// Critical priority - highest priority
        /// </summary>
        Critical = 5
    }

    /// <summary>
    /// Delivery status for notifications
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// Notification is pending delivery
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Notification is being processed
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Notification was delivered successfully
        /// </summary>
        Delivered = 3,

        /// <summary>
        /// Notification delivery failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Notification delivery was cancelled
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// Notification is scheduled for retry
        /// </summary>
        RetryScheduled = 6
    }

    /// <summary>
    /// Variable types for template variables
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// String variable
        /// </summary>
        String = 1,

        /// <summary>
        /// Number variable
        /// </summary>
        Number = 2,

        /// <summary>
        /// Date variable
        /// </summary>
        Date = 3,

        /// <summary>
        /// Boolean variable
        /// </summary>
        Boolean = 4,

        /// <summary>
        /// URL variable
        /// </summary>
        Url = 5,

        /// <summary>
        /// Email variable
        /// </summary>
        Email = 6
    }

    /// <summary>
    /// Email format options
    /// </summary>
    public enum EmailFormat
    {
        /// <summary>
        /// Plain text format
        /// </summary>
        Text = 1,

        /// <summary>
        /// HTML format
        /// </summary>
        Html = 2,

        /// <summary>
        /// Both text and HTML
        /// </summary>
        Both = 3
    }

    #endregion

    #region DTOs

    /// <summary>
    /// DTO for creating a new notification
    /// </summary>
    public class CreateNotificationRequest
    {
        /// <summary>
        /// Type of notification
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Subject or title
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Body content
        /// </summary>
        [Required]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// HTML body content
        /// </summary>
        public string? HtmlBody { get; set; }

        /// <summary>
        /// Recipient email addresses
        /// </summary>
        public List<string>? EmailRecipients { get; set; }

        /// <summary>
        /// Recipient phone numbers
        /// </summary>
        public List<string>? SmsRecipients { get; set; }

        /// <summary>
        /// Recipient user IDs
        /// </summary>
        public List<string>? UserRecipients { get; set; }

        /// <summary>
        /// Webhook URLs
        /// </summary>
        public List<string>? WebhookUrls { get; set; }

        /// <summary>
        /// Template name to use
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Template variables
        /// </summary>
        public Dictionary<string, object>? TemplateVariables { get; set; }

        /// <summary>
        /// Scheduled delivery time
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Expiration time
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Tags for categorization
        /// </summary>
        public List<string>? Tags { get; set; }

        /// <summary>
        /// Source or trigger
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Related entity information
        /// </summary>
        public Dictionary<string, object>? RelatedEntities { get; set; }
    }

    /// <summary>
    /// DTO for notification response
    /// </summary>
    public class NotificationResponse
    {
        /// <summary>
        /// Notification ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Status of the notification
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Number of recipients
        /// </summary>
        public int RecipientCount { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Scheduled delivery time
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        /// <summary>
        /// Error message if any
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// DTO for notification delivery status
    /// </summary>
    public class DeliveryStatusResponse
    {
        /// <summary>
        /// Delivery ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Notification ID
        /// </summary>
        public Guid NotificationId { get; set; }

        /// <summary>
        /// Recipient information
        /// </summary>
        public string Recipient { get; set; } = string.Empty;

        /// <summary>
        /// Delivery status
        /// </summary>
        public DeliveryStatus Status { get; set; }

        /// <summary>
        /// Retry count
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Next retry time
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// Delivery timestamp
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// External delivery ID
        /// </summary>
        public string? ExternalId { get; set; }
    }

    /// <summary>
    /// DTO for notification template
    /// </summary>
    public class TemplateResponse
    {
        /// <summary>
        /// Template ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Template name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Template description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Notification type
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// Whether template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Template version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Available variables
        /// </summary>
        public List<TemplateVariable> Variables { get; set; } = new();

        /// <summary>
        /// Template tags
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last modification timestamp
        /// </summary>
        public DateTime ModifiedAt { get; set; }
    }

    /// <summary>
    /// DTO for notification preferences
    /// </summary>
    public class PreferencesResponse
    {
        /// <summary>
        /// User ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Whether notifications are enabled globally
        /// </summary>
        public bool NotificationsEnabled { get; set; }

        /// <summary>
        /// Default notification channels
        /// </summary>
        public List<NotificationType> DefaultChannels { get; set; } = new();

        /// <summary>
        /// Email preferences
        /// </summary>
        public EmailPreferences Email { get; set; } = new();

        /// <summary>
        /// SMS preferences
        /// </summary>
        public SmsPreferences Sms { get; set; } = new();

        /// <summary>
        /// Push preferences
        /// </summary>
        public PushPreferences Push { get; set; } = new();

        /// <summary>
        /// Category preferences
        /// </summary>
        public Dictionary<string, CategoryPreferences> Categories { get; set; } = new();

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for notification statistics
    /// </summary>
    public class StatisticsResponse
    {
        /// <summary>
        /// Total notifications sent
        /// </summary>
        public long TotalSent { get; set; }

        /// <summary>
        /// Total notifications delivered
        /// </summary>
        public long TotalDelivered { get; set; }

        /// <summary>
        /// Total notifications failed
        /// </summary>
        public long TotalFailed { get; set; }

        /// <summary>
        /// Delivery rate percentage
        /// </summary>
        public double DeliveryRate { get; set; }

        /// <summary>
        /// Failure rate percentage
        /// </summary>
        public double FailureRate { get; set; }

        /// <summary>
        /// Statistics by type
        /// </summary>
        public Dictionary<NotificationType, TypeStatistics> ByType { get; set; } = new();

        /// <summary>
        /// Statistics by priority
        /// </summary>
        public Dictionary<NotificationPriority, PriorityStatistics> ByPriority { get; set; } = new();

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }

    #endregion
} 