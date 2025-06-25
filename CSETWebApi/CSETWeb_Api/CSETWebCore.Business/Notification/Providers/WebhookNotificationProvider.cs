//////////////////////////////// 
// 
//   Copyright 2025 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CSETWebCore.Interfaces.Notification;
using CSETWebCore.Model.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CSETWebCore.Business.Notification.Providers
{
    /// <summary>
    /// Webhook notification provider for sending notifications to external endpoints
    /// </summary>
    public class WebhookNotificationProvider : IWebhookNotificationProvider
    {
        private readonly ILogger<WebhookNotificationProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly WebhookConfiguration _webhookConfig;

        public WebhookNotificationProvider(ILogger<WebhookNotificationProvider> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _webhookConfig = GetWebhookConfiguration();
        }

        public NotificationType Type => NotificationType.Webhook;
        public string DisplayName => "Webhook Provider";
        public bool IsEnabled => _webhookConfig.Enabled;
        public int Priority => _webhookConfig.Priority;

        public async Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient)
        {
            if (!IsEnabled)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "Webhook provider is disabled",
                    ErrorCode = "PROVIDER_DISABLED"
                };
            }

            if (string.IsNullOrEmpty(recipient.WebhookUrl))
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "No webhook URL provided",
                    ErrorCode = "MISSING_WEBHOOK_URL"
                };
            }

            try
            {
                var payload = CreateWebhookPayload(message, recipient);
                var result = await SendWebhookAsync(recipient.WebhookUrl, payload);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending webhook notification to {WebhookUrl}", recipient.WebhookUrl);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "WEBHOOK_SEND_ERROR"
                };
            }
        }

        public async Task<DeliveryResult> SendWebhookAsync(string url, object payload)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No webhook URL provided",
                        ErrorCode = "MISSING_URL"
                    };
                }

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid webhook URL format",
                        ErrorCode = "INVALID_URL"
                    };
                }

                // Validate URL scheme
                if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "Webhook URL must use HTTP or HTTPS",
                        ErrorCode = "INVALID_URL_SCHEME"
                    };
                }

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Add custom headers if configured
                if (_webhookConfig.DefaultHeaders?.Any() == true)
                {
                    foreach (var header in _webhookConfig.DefaultHeaders)
                    {
                        content.Headers.Add(header.Key, header.Value);
                    }
                }

                // Add authentication if configured
                if (!string.IsNullOrEmpty(_webhookConfig.DefaultAuthToken))
                {
                    content.Headers.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _webhookConfig.DefaultAuthToken);
                }

                var timeout = TimeSpan.FromSeconds(_webhookConfig.DefaultTimeoutSeconds);
                using var cts = new System.Threading.CancellationTokenSource(timeout);

                var response = await _httpClient.PostAsync(url, content, cts.Token);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook sent successfully to {Url}", url);

                    return new DeliveryResult
                    {
                        Success = true,
                        ExternalId = response.Headers.Contains("X-Request-ID") 
                            ? response.Headers.GetValues("X-Request-ID").FirstOrDefault() 
                            : Guid.NewGuid().ToString(),
                        DeliveredAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            ["url"] = url,
                            ["statusCode"] = (int)response.StatusCode,
                            ["responseSize"] = responseContent.Length,
                            ["contentType"] = response.Content.Headers.ContentType?.ToString() ?? "unknown"
                        }
                    };
                }
                else
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = $"Webhook endpoint returned error: {response.StatusCode} - {responseContent}",
                        ErrorCode = $"HTTP_{response.StatusCode}",
                        Metadata = new Dictionary<string, object>
                        {
                            ["url"] = url,
                            ["statusCode"] = (int)response.StatusCode,
                            ["responseContent"] = responseContent
                        }
                    };
                }
            }
            catch (TaskCanceledException)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "Webhook request timed out",
                    ErrorCode = "TIMEOUT"
                };
            }
            catch (HttpRequestException ex)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = $"HTTP request failed: {ex.Message}",
                    ErrorCode = "HTTP_REQUEST_ERROR"
                };
            }
            catch (Exception ex)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "WEBHOOK_SEND_ERROR"
                };
            }
        }

        public async Task<List<DeliveryResult>> SendBulkWebhookAsync(List<string> urls, object payload)
        {
            var results = new List<DeliveryResult>();

            // Send webhooks in parallel with rate limiting
            var semaphore = new System.Threading.SemaphoreSlim(_webhookConfig.MaxConcurrentSends);
            var tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendWebhookAsync(url, payload);
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

            _logger.LogInformation("Bulk webhook operation completed. Sent: {Sent}, Failed: {Failed}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<bool> TestConnectivityAsync()
        {
            try
            {
                // Test with a simple payload
                var testPayload = new
                {
                    test = true,
                    timestamp = DateTime.UtcNow,
                    message = "CSET Webhook Test"
                };

                // Use a test webhook service or mock endpoint
                var testUrl = "https://httpbin.org/post";
                var result = await SendWebhookAsync(testUrl, testPayload);

                return result.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook connectivity test failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate basic configuration
            if (_webhookConfig.MaxConcurrentSends <= 0)
            {
                result.Errors.Add("Max concurrent sends must be greater than 0");
                result.IsValid = false;
            }

            if (_webhookConfig.DefaultTimeoutSeconds <= 0)
            {
                result.Errors.Add("Default timeout must be greater than 0");
                result.IsValid = false;
            }

            if (_webhookConfig.MaxRetries < 0)
            {
                result.Errors.Add("Max retries cannot be negative");
                result.IsValid = false;
            }

            // Test connectivity if configuration is valid
            if (result.IsValid)
            {
                var connectivityTest = await TestConnectivityAsync();
                if (!connectivityTest)
                {
                    result.Warnings.Add("Webhook connectivity test failed - this may be expected in some environments");
                }
            }

            result.Details["maxConcurrentSends"] = _webhookConfig.MaxConcurrentSends;
            result.Details["rateLimitPerMinute"] = _webhookConfig.RateLimitPerMinute;
            result.Details["defaultTimeoutSeconds"] = _webhookConfig.DefaultTimeoutSeconds;
            result.Details["maxRetries"] = _webhookConfig.MaxRetries;
            result.Details["retryDelaySeconds"] = _webhookConfig.RetryDelaySeconds;
            result.Details["hasDefaultHeaders"] = _webhookConfig.DefaultHeaders?.Any() ?? false;
            result.Details["hasDefaultAuth"] = !string.IsNullOrEmpty(_webhookConfig.DefaultAuthToken);

            return result;
        }

        private object CreateWebhookPayload(NotificationMessage message, NotificationRecipient recipient)
        {
            return new
            {
                // Standard webhook fields
                id = message.Id.ToString(),
                type = "notification",
                timestamp = message.CreatedAt,
                source = message.Source ?? "CSET",
                priority = message.Priority.ToString().ToLowerInvariant(),

                // Notification content
                subject = message.Subject,
                body = message.Body,
                htmlBody = message.HtmlBody,

                // Recipient information
                recipient = new
                {
                    id = recipient.Id,
                    name = recipient.Name,
                    email = recipient.Email,
                    phoneNumber = recipient.PhoneNumber,
                    userId = recipient.UserId,
                    webhookUrl = recipient.WebhookUrl,
                    language = recipient.Language,
                    timezone = recipient.Timezone
                },

                // Template information
                templateName = message.TemplateName,
                templateVariables = message.TemplateVariables,

                // Additional metadata
                tags = message.Tags,
                relatedEntities = message.RelatedEntities,
                metadata = message.Metadata,

                // Delivery information
                scheduledAt = message.ScheduledAt,
                expiresAt = message.ExpiresAt,

                // CSET-specific fields
                csetVersion = "1.0.0",
                notificationType = message.Type.ToString().ToLowerInvariant()
            };
        }

        private WebhookConfiguration GetWebhookConfiguration()
        {
            var config = _configuration.GetSection("EnhancedNotifications:Providers:Webhook");
            
            var defaultHeaders = new Dictionary<string, string>();
            var headersConfig = config.GetSection("DefaultHeaders");
            foreach (var header in headersConfig.GetChildren())
            {
                defaultHeaders[header.Key] = header.Value ?? "";
            }

            return new WebhookConfiguration
            {
                Enabled = config.GetValue<bool>("Enabled", false),
                Priority = config.GetValue<int>("Priority", 4),
                MaxConcurrentSends = config.GetValue<int>("MaxConcurrentSends", 5),
                RateLimitPerMinute = config.GetValue<int>("RateLimitPerMinute", 30),
                DefaultTimeoutSeconds = config.GetValue<int>("DefaultTimeoutSeconds", 30),
                MaxRetries = config.GetValue<int>("MaxRetries", 3),
                RetryDelaySeconds = config.GetValue<int>("RetryDelaySeconds", 60),
                DefaultHeaders = defaultHeaders,
                DefaultAuthToken = _configuration.GetValue<string>("Webhook:DefaultAuthToken"),
                AllowedDomains = config.GetSection("AllowedDomains").Get<string[]>() ?? Array.Empty<string>(),
                BlockedDomains = config.GetSection("BlockedDomains").Get<string[]>() ?? Array.Empty<string>()
            };
        }
    }

    /// <summary>
    /// Webhook configuration settings
    /// </summary>
    public class WebhookConfiguration
    {
        public bool Enabled { get; set; } = false;
        public int Priority { get; set; } = 4;
        public int MaxConcurrentSends { get; set; } = 5;
        public int RateLimitPerMinute { get; set; } = 30;
        public int DefaultTimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 60;
        public Dictionary<string, string> DefaultHeaders { get; set; } = new();
        public string? DefaultAuthToken { get; set; }
        public string[] AllowedDomains { get; set; } = Array.Empty<string>();
        public string[] BlockedDomains { get; set; } = Array.Empty<string>();
    }
} 