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
    /// Base interface for notification providers
    /// </summary>
    public interface INotificationProvider
    {
        /// <summary>
        /// Gets the notification type this provider handles
        /// </summary>
        NotificationType Type { get; }

        /// <summary>
        /// Gets the display name of this provider
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets whether this provider is enabled
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the priority of this provider (lower numbers = higher priority)
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Sends a notification using this provider
        /// </summary>
        /// <param name="message">Notification message to send</param>
        /// <param name="recipient">Recipient information</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient);

        /// <summary>
        /// Tests connectivity to the notification service
        /// </summary>
        /// <returns>Test result</returns>
        Task<bool> TestConnectivityAsync();

        /// <summary>
        /// Validates the provider configuration
        /// </summary>
        /// <returns>Validation result</returns>
        Task<ValidationResult> ValidateConfigurationAsync();
    }

    /// <summary>
    /// Email notification provider interface
    /// </summary>
    public interface IEmailNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Sends an email notification
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <param name="htmlBody">HTML email body</param>
        /// <param name="from">Sender email address</param>
        /// <param name="replyTo">Reply-to email address</param>
        /// <param name="attachments">Email attachments</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendEmailAsync(
            string to,
            string subject,
            string body,
            string? htmlBody = null,
            string? from = null,
            string? replyTo = null,
            List<EmailAttachment>? attachments = null);

        /// <summary>
        /// Sends an email to multiple recipients
        /// </summary>
        /// <param name="to">List of recipient email addresses</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body</param>
        /// <param name="htmlBody">HTML email body</param>
        /// <param name="from">Sender email address</param>
        /// <param name="replyTo">Reply-to email address</param>
        /// <param name="attachments">Email attachments</param>
        /// <returns>List of delivery results</returns>
        Task<List<DeliveryResult>> SendBulkEmailAsync(
            List<string> to,
            string subject,
            string body,
            string? htmlBody = null,
            string? from = null,
            string? replyTo = null,
            List<EmailAttachment>? attachments = null);
    }

    /// <summary>
    /// SMS notification provider interface
    /// </summary>
    public interface ISmsNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Sends an SMS notification
        /// </summary>
        /// <param name="to">Recipient phone number</param>
        /// <param name="message">SMS message</param>
        /// <param name="from">Sender phone number or identifier</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendSmsAsync(string to, string message, string? from = null);

        /// <summary>
        /// Sends SMS to multiple recipients
        /// </summary>
        /// <param name="to">List of recipient phone numbers</param>
        /// <param name="message">SMS message</param>
        /// <param name="from">Sender phone number or identifier</param>
        /// <returns>List of delivery results</returns>
        Task<List<DeliveryResult>> SendBulkSmsAsync(List<string> to, string message, string? from = null);
    }

    /// <summary>
    /// Push notification provider interface
    /// </summary>
    public interface IPushNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Sends a push notification
        /// </summary>
        /// <param name="token">Device token</param>
        /// <param name="title">Notification title</param>
        /// <param name="body">Notification body</param>
        /// <param name="data">Additional data</param>
        /// <param name="options">Push notification options</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendPushAsync(
            string token,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null);

        /// <summary>
        /// Sends push notifications to multiple devices
        /// </summary>
        /// <param name="tokens">List of device tokens</param>
        /// <param name="title">Notification title</param>
        /// <param name="body">Notification body</param>
        /// <param name="data">Additional data</param>
        /// <param name="options">Push notification options</param>
        /// <returns>List of delivery results</returns>
        Task<List<DeliveryResult>> SendBulkPushAsync(
            List<string> tokens,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null);
    }

    /// <summary>
    /// Webhook notification provider interface
    /// </summary>
    public interface IWebhookNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Sends a webhook notification
        /// </summary>
        /// <param name="url">Webhook URL</param>
        /// <param name="payload">Webhook payload</param>
        /// <param name="headers">HTTP headers</param>
        /// <param name="method">HTTP method</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendWebhookAsync(
            string url,
            object payload,
            Dictionary<string, string>? headers = null,
            string method = "POST");

        /// <summary>
        /// Sends webhook notifications to multiple URLs
        /// </summary>
        /// <param name="urls">List of webhook URLs</param>
        /// <param name="payload">Webhook payload</param>
        /// <param name="headers">HTTP headers</param>
        /// <param name="method">HTTP method</param>
        /// <returns>List of delivery results</returns>
        Task<List<DeliveryResult>> SendBulkWebhookAsync(
            List<string> urls,
            object payload,
            Dictionary<string, string>? headers = null,
            string method = "POST");
    }

    /// <summary>
    /// In-app notification provider interface
    /// </summary>
    public interface IInAppNotificationProvider : INotificationProvider
    {
        /// <summary>
        /// Sends an in-app notification
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        /// <param name="data">Additional data</param>
        /// <returns>Delivery result</returns>
        Task<DeliveryResult> SendInAppAsync(
            string userId,
            string title,
            string message,
            string type = "info",
            Dictionary<string, object>? data = null);

        /// <summary>
        /// Sends in-app notifications to multiple users
        /// </summary>
        /// <param name="userIds">List of user IDs</param>
        /// <param name="title">Notification title</param>
        /// <param name="message">Notification message</param>
        /// <param name="type">Notification type</param>
        /// <param name="data">Additional data</param>
        /// <returns>List of delivery results</returns>
        Task<List<DeliveryResult>> SendBulkInAppAsync(
            List<string> userIds,
            string title,
            string message,
            string type = "info",
            Dictionary<string, object>? data = null);
    }

    #region Supporting Classes

    /// <summary>
    /// Result of a notification delivery attempt
    /// </summary>
    public class DeliveryResult
    {
        /// <summary>
        /// Whether the delivery was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// External delivery ID from the provider
        /// </summary>
        public string? ExternalId { get; set; }

        /// <summary>
        /// Error message if delivery failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Error code if delivery failed
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Delivery timestamp
        /// </summary>
        public DateTime DeliveredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata from the provider
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Result of configuration validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the configuration is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// List of validation warnings
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Additional validation details
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new();
    }

    /// <summary>
    /// Email attachment information
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// Attachment filename
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Attachment content type
        /// </summary>
        public string ContentType { get; set; } = "application/octet-stream";

        /// <summary>
        /// Attachment content as byte array
        /// </summary>
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Whether this is an inline attachment
        /// </summary>
        public bool IsInline { get; set; } = false;

        /// <summary>
        /// Content ID for inline attachments
        /// </summary>
        public string? ContentId { get; set; }
    }

    /// <summary>
    /// Push notification options
    /// </summary>
    public class PushNotificationOptions
    {
        /// <summary>
        /// Whether to play sound
        /// </summary>
        public bool PlaySound { get; set; } = true;

        /// <summary>
        /// Whether to vibrate
        /// </summary>
        public bool Vibrate { get; set; } = true;

        /// <summary>
        /// Badge number
        /// </summary>
        public int? Badge { get; set; }

        /// <summary>
        /// Notification category
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Time to live in seconds
        /// </summary>
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Priority level
        /// </summary>
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// Whether to show when app is in foreground
        /// </summary>
        public bool ShowWhenForeground { get; set; } = true;

        /// <summary>
        /// Action buttons
        /// </summary>
        public List<PushAction> Actions { get; set; } = new();
    }

    /// <summary>
    /// Push notification action
    /// </summary>
    public class PushAction
    {
        /// <summary>
        /// Action identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Action title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Action icon
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Whether this is the default action
        /// </summary>
        public bool IsDefault { get; set; } = false;
    }

    #endregion
} 