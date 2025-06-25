//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSETWebCore.Business.Notification.Providers
{
    /// <summary>
    /// Email notification provider using SMTP
    /// </summary>
    public class EmailNotificationProvider : IEmailNotificationProvider
    {
        private readonly ILogger<EmailNotificationProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly SmtpClient _smtpClient;
        private readonly EmailConfiguration _emailConfig;

        public EmailNotificationProvider(ILogger<EmailNotificationProvider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _emailConfig = GetEmailConfiguration();
            _smtpClient = CreateSmtpClient();
        }

        public NotificationType Type => NotificationType.Email;
        public string DisplayName => "SMTP Email Provider";
        public bool IsEnabled => _emailConfig.Enabled;
        public int Priority => _emailConfig.Priority;

        public async Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient)
        {
            if (!IsEnabled)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "Email provider is disabled",
                    ErrorCode = "PROVIDER_DISABLED"
                };
            }

            if (string.IsNullOrEmpty(recipient.Email))
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "No email address provided",
                    ErrorCode = "MISSING_EMAIL"
                };
            }

            try
            {
                var result = await SendEmailAsync(
                    recipient.Email,
                    message.Subject,
                    message.Body,
                    message.HtmlBody,
                    message.Source ?? _emailConfig.DefaultFromEmail,
                    null,
                    null);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification to {Email}", recipient.Email);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "EMAIL_SEND_ERROR"
                };
            }
        }

        public async Task<DeliveryResult> SendEmailAsync(
            string to,
            string subject,
            string body,
            string? htmlBody = null,
            string? from = null,
            string? replyTo = null,
            List<EmailAttachment>? attachments = null)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = htmlBody ?? body,
                    IsBodyHtml = !string.IsNullOrEmpty(htmlBody),
                    From = new MailAddress(from ?? _emailConfig.DefaultFromEmail, _emailConfig.DefaultFromName)
                };

                mailMessage.To.Add(new MailAddress(to));

                if (!string.IsNullOrEmpty(replyTo))
                {
                    mailMessage.ReplyToList.Add(new MailAddress(replyTo));
                }

                // Add attachments
                if (attachments?.Any() == true)
                {
                    foreach (var attachment in attachments)
                    {
                        var mailAttachment = new Attachment(
                            new System.IO.MemoryStream(attachment.Content),
                            attachment.Filename,
                            attachment.ContentType);

                        if (attachment.IsInline && !string.IsNullOrEmpty(attachment.ContentId))
                        {
                            mailAttachment.ContentId = attachment.ContentId;
                            mailAttachment.ContentDisposition.Inline = true;
                        }

                        mailMessage.Attachments.Add(mailAttachment);
                    }
                }

                // Apply email template styling if HTML
                if (mailMessage.IsBodyHtml)
                {
                    mailMessage.Body = ApplyEmailStyling(mailMessage.Body);
                }

                await _smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Email}", to);

                return new DeliveryResult
                {
                    Success = true,
                    ExternalId = Guid.NewGuid().ToString(),
                    DeliveredAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        ["recipient"] = to,
                        ["subject"] = subject,
                        ["hasAttachments"] = attachments?.Any() ?? false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", to);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "EMAIL_SEND_ERROR"
                };
            }
        }

        public async Task<List<DeliveryResult>> SendBulkEmailAsync(
            List<string> to,
            string subject,
            string body,
            string? htmlBody = null,
            string? from = null,
            string? replyTo = null,
            List<EmailAttachment>? attachments = null)
        {
            var results = new List<DeliveryResult>();

            // Send emails in parallel with rate limiting
            var semaphore = new System.Threading.SemaphoreSlim(_emailConfig.MaxConcurrentSends);
            var tasks = to.Select(async email =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendEmailAsync(email, subject, body, htmlBody, from, replyTo, attachments);
                    lock (results)
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation("Bulk email operation completed. Sent: {Sent}, Failed: {Failed}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<bool> TestConnectivityAsync()
        {
            try
            {
                // Test SMTP connection
                await _smtpClient.SendMailAsync(new MailMessage
                {
                    From = new MailAddress(_emailConfig.DefaultFromEmail),
                    To = { new MailAddress(_emailConfig.DefaultFromEmail) },
                    Subject = "CSET Email Test",
                    Body = "This is a test email to verify SMTP connectivity.",
                    IsBodyHtml = false
                });

                _logger.LogInformation("Email connectivity test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email connectivity test failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate SMTP settings
            if (string.IsNullOrEmpty(_emailConfig.SmtpHost))
            {
                result.Errors.Add("SMTP host is not configured");
                result.IsValid = false;
            }

            if (_emailConfig.SmtpPort <= 0)
            {
                result.Errors.Add("SMTP port is not configured");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(_emailConfig.DefaultFromEmail))
            {
                result.Errors.Add("Default from email is not configured");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(_emailConfig.DefaultFromName))
            {
                result.Warnings.Add("Default from name is not configured");
            }

            // Test connectivity if configuration is valid
            if (result.IsValid)
            {
                var connectivityTest = await TestConnectivityAsync();
                if (!connectivityTest)
                {
                    result.Errors.Add("SMTP connectivity test failed");
                    result.IsValid = false;
                }
            }

            result.Details["smtpHost"] = _emailConfig.SmtpHost;
            result.Details["smtpPort"] = _emailConfig.SmtpPort;
            result.Details["useSsl"] = _emailConfig.UseSsl;
            result.Details["defaultFromEmail"] = _emailConfig.DefaultFromEmail;
            result.Details["maxConcurrentSends"] = _emailConfig.MaxConcurrentSends;
            result.Details["rateLimitPerMinute"] = _emailConfig.RateLimitPerMinute;

            return result;
        }

        private EmailConfiguration GetEmailConfiguration()
        {
            var config = _configuration.GetSection("EnhancedNotifications:Providers:Email");
            
            return new EmailConfiguration
            {
                Enabled = config.GetValue<bool>("Enabled", true),
                Priority = config.GetValue<int>("Priority", 1),
                MaxConcurrentSends = config.GetValue<int>("MaxConcurrentSends", 10),
                RateLimitPerMinute = config.GetValue<int>("RateLimitPerMinute", 60),
                DefaultFromEmail = config.GetValue<string>("DefaultFromEmail") ?? "no-reply@cset.inl.gov",
                DefaultFromName = config.GetValue<string>("DefaultFromName") ?? "CSET System",
                SmtpHost = _configuration.GetValue<string>("Email:SmtpHost") ?? "localhost",
                SmtpPort = _configuration.GetValue<int>("Email:SmtpPort", 587),
                UseSsl = _configuration.GetValue<bool>("Email:SmtpSsl", false),
                SmtpUsername = _configuration.GetValue<string>("Email:SmtpUsername"),
                SmtpPassword = _configuration.GetValue<string>("Email:SmtpPassword"),
                RequireAuthentication = config.GetValue<bool>("RequireAuthentication", true),
                TimeoutSeconds = config.GetValue<int>("TimeoutSeconds", 30)
            };
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = _emailConfig.SmtpHost,
                Port = _emailConfig.SmtpPort,
                EnableSsl = _emailConfig.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = _emailConfig.TimeoutSeconds * 1000
            };

            if (_emailConfig.RequireAuthentication && 
                !string.IsNullOrEmpty(_emailConfig.SmtpUsername) && 
                !string.IsNullOrEmpty(_emailConfig.SmtpPassword))
            {
                client.Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
            }

            return client;
        }

        private string ApplyEmailStyling(string htmlBody)
        {
            // Apply inline CSS styling for better email compatibility
            var inlineStyles = @"
                <style>
                    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                    .header { background-color: #f8f9fa; padding: 20px; border-radius: 5px; }
                    .content { padding: 20px; }
                    .footer { background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #666; }
                    .button { display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }
                    .alert { padding: 15px; margin: 10px 0; border-radius: 5px; }
                    .alert-success { background-color: #d4edda; border: 1px solid #c3e6cb; color: #155724; }
                    .alert-warning { background-color: #fff3cd; border: 1px solid #ffeaa7; color: #856404; }
                    .alert-error { background-color: #f8d7da; border: 1px solid #f5c6cb; color: #721c24; }
                </style>";

            // Wrap content in container if not already wrapped
            if (!htmlBody.Contains("<div class=\"container\">"))
            {
                htmlBody = $@"
                    <div class=""container"">
                        <div class=""header"">
                            <h2>CSET Notification</h2>
                        </div>
                        <div class=""content"">
                            {htmlBody}
                        </div>
                        <div class=""footer"">
                            <p>This is an automated message from the CSET system.</p>
                            <p>Please do not reply to this email.</p>
                        </div>
                    </div>";
            }

            return htmlBody.Replace("</head>", $"{inlineStyles}</head>");
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }

    /// <summary>
    /// Email configuration settings
    /// </summary>
    public class EmailConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 1;
        public int MaxConcurrentSends { get; set; } = 10;
        public int RateLimitPerMinute { get; set; } = 60;
        public string DefaultFromEmail { get; set; } = "no-reply@cset.inl.gov";
        public string DefaultFromName { get; set; } = "CSET System";
        public string SmtpHost { get; set; } = "localhost";
        public int SmtpPort { get; set; } = 587;
        public bool UseSsl { get; set; } = false;
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool RequireAuthentication { get; set; } = true;
        public int TimeoutSeconds { get; set; } = 30;
    }
} 