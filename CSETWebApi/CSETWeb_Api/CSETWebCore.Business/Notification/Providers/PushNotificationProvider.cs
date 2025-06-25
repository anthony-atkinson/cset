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
    /// Push notification provider supporting Firebase Cloud Messaging and other services
    /// </summary>
    public class PushNotificationProvider : IPushNotificationProvider
    {
        private readonly ILogger<PushNotificationProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly PushConfiguration _pushConfig;

        public PushNotificationProvider(ILogger<PushNotificationProvider> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _pushConfig = GetPushConfiguration();
        }

        public NotificationType Type => NotificationType.Push;
        public string DisplayName => $"{_pushConfig.Provider} Push Provider";
        public bool IsEnabled => _pushConfig.Enabled;
        public int Priority => _pushConfig.Priority;

        public async Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient)
        {
            if (!IsEnabled)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "Push notification provider is disabled",
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
                // Get user's push tokens from preferences or database
                var tokens = await GetUserPushTokensAsync(recipient.UserId);
                if (!tokens.Any())
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No push tokens available for user",
                        ErrorCode = "NO_PUSH_TOKENS"
                    };
                }

                // Send to all user's devices
                var results = await SendBulkPushAsync(
                    tokens,
                    message.Subject,
                    message.Body,
                    message.RelatedEntities,
                    GetDefaultPushOptions());

                // Return the first successful result or the first failure
                var successfulResult = results.FirstOrDefault(r => r.Success);
                if (successfulResult != null)
                {
                    return successfulResult;
                }

                return results.FirstOrDefault() ?? new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "All push notification attempts failed",
                    ErrorCode = "ALL_PUSH_FAILED"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to user {UserId}", recipient.UserId);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "PUSH_SEND_ERROR"
                };
            }
        }

        public async Task<DeliveryResult> SendPushAsync(
            string token,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "No push token provided",
                        ErrorCode = "MISSING_TOKEN"
                    };
                }

                DeliveryResult result;
                switch (_pushConfig.Provider.ToLowerInvariant())
                {
                    case "firebase":
                        result = await SendViaFirebaseAsync(token, title, body, data, options);
                        break;
                    case "mock":
                        result = await SendViaMockAsync(token, title, body, data, options);
                        break;
                    default:
                        return new DeliveryResult
                        {
                            Success = false,
                            ErrorMessage = $"Unsupported push provider: {_pushConfig.Provider}",
                            ErrorCode = "UNSUPPORTED_PROVIDER"
                        };
                }

                if (result.Success)
                {
                    _logger.LogInformation("Push notification sent successfully to token {Token}", token);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending push notification to token {Token}", token);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "PUSH_SEND_ERROR"
                };
            }
        }

        public async Task<List<DeliveryResult>> SendBulkPushAsync(
            List<string> tokens,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null)
        {
            var results = new List<DeliveryResult>();

            // Send push notifications in parallel with rate limiting
            var semaphore = new System.Threading.SemaphoreSlim(_pushConfig.MaxConcurrentSends);
            var tasks = tokens.Select(async token =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendPushAsync(token, title, body, data, options);
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

            _logger.LogInformation("Bulk push operation completed. Sent: {Sent}, Failed: {Failed}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<bool> TestConnectivityAsync()
        {
            try
            {
                // Send a test push notification
                var testResult = await SendViaMockAsync(
                    "test-token",
                    "CSET Push Test",
                    "This is a test push notification",
                    new Dictionary<string, object> { ["test"] = true });

                return testResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Push notification connectivity test failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate provider configuration
            if (string.IsNullOrEmpty(_pushConfig.Provider))
            {
                result.Errors.Add("Push notification provider is not configured");
                result.IsValid = false;
            }

            // Validate provider-specific settings
            switch (_pushConfig.Provider.ToLowerInvariant())
            {
                case "firebase":
                    if (string.IsNullOrEmpty(_pushConfig.FirebaseServerKey))
                    {
                        result.Errors.Add("Firebase Server Key is not configured");
                        result.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(_pushConfig.FirebaseProjectId))
                    {
                        result.Warnings.Add("Firebase Project ID is not configured");
                    }
                    break;
            }

            // Test connectivity if configuration is valid
            if (result.IsValid)
            {
                var connectivityTest = await TestConnectivityAsync();
                if (!connectivityTest)
                {
                    result.Errors.Add("Push notification connectivity test failed");
                    result.IsValid = false;
                }
            }

            result.Details["provider"] = _pushConfig.Provider;
            result.Details["maxConcurrentSends"] = _pushConfig.MaxConcurrentSends;
            result.Details["rateLimitPerMinute"] = _pushConfig.RateLimitPerMinute;
            result.Details["defaultSound"] = _pushConfig.DefaultSound;
            result.Details["defaultBadge"] = _pushConfig.DefaultBadge;

            return result;
        }

        private async Task<DeliveryResult> SendViaFirebaseAsync(
            string token,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null)
        {
            try
            {
                var fcmMessage = new
                {
                    to = token,
                    notification = new
                    {
                        title = title,
                        body = body,
                        sound = options?.PlaySound == true ? _pushConfig.DefaultSound : null,
                        badge = options?.Badge ?? _pushConfig.DefaultBadge,
                        click_action = "FLUTTER_NOTIFICATION_CLICK"
                    },
                    data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? ""),
                    priority = options?.Priority ?? "normal",
                    time_to_live = options?.TimeToLive ?? 86400, // 24 hours
                    android = new
                    {
                        priority = options?.Priority ?? "normal",
                        notification = new
                        {
                            sound = options?.PlaySound == true ? _pushConfig.DefaultSound : null,
                            channel_id = "cset_notifications"
                        }
                    },
                    apns = new
                    {
                        payload = new
                        {
                            aps = new
                            {
                                alert = new
                                {
                                    title = title,
                                    body = body
                                },
                                sound = options?.PlaySound == true ? _pushConfig.DefaultSound : null,
                                badge = options?.Badge ?? _pushConfig.DefaultBadge,
                                category = options?.Category
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(fcmMessage);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("key", _pushConfig.FirebaseServerKey);

                var response = await _httpClient.PostAsync("https://fcm.googleapis.com/fcm/send", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var fcmResponse = JsonSerializer.Deserialize<FirebaseResponse>(responseContent);
                    
                    if (fcmResponse?.Success == 1)
                    {
                        return new DeliveryResult
                        {
                            Success = true,
                            ExternalId = fcmResponse.MessageId,
                            DeliveredAt = DateTime.UtcNow,
                            Metadata = new Dictionary<string, object>
                            {
                                ["provider"] = "Firebase",
                                ["token"] = token,
                                ["messageId"] = fcmResponse.MessageId
                            }
                        };
                    }
                    else
                    {
                        return new DeliveryResult
                        {
                            Success = false,
                            ErrorMessage = fcmResponse?.Error ?? "Firebase returned failure",
                            ErrorCode = "FIREBASE_FAILURE"
                        };
                    }
                }
                else
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = $"Firebase API error: {response.StatusCode} - {responseContent}",
                        ErrorCode = "FIREBASE_API_ERROR"
                    };
                }
            }
            catch (Exception ex)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "FIREBASE_SEND_ERROR"
                };
            }
        }

        private async Task<DeliveryResult> SendViaMockAsync(
            string token,
            string title,
            string body,
            Dictionary<string, object>? data = null,
            PushNotificationOptions? options = null)
        {
            // Mock implementation for testing
            await Task.Delay(100); // Simulate network delay

            return new DeliveryResult
            {
                Success = true,
                ExternalId = Guid.NewGuid().ToString(),
                DeliveredAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["provider"] = "Mock",
                    ["token"] = token,
                    ["title"] = title,
                    ["body"] = body,
                    ["isTest"] = true
                }
            };
        }

        private PushConfiguration GetPushConfiguration()
        {
            var config = _configuration.GetSection("EnhancedNotifications:Providers:Push");
            
            return new PushConfiguration
            {
                Enabled = config.GetValue<bool>("Enabled", true),
                Priority = config.GetValue<int>("Priority", 3),
                MaxConcurrentSends = config.GetValue<int>("MaxConcurrentSends", 20),
                RateLimitPerMinute = config.GetValue<int>("RateLimitPerMinute", 100),
                Provider = config.GetValue<string>("Provider") ?? "Mock",
                DefaultSound = config.GetValue<string>("DefaultSound") ?? "default",
                DefaultBadge = config.GetValue<int>("DefaultBadge", 1),
                FirebaseServerKey = _configuration.GetValue<string>("Push:FirebaseServerKey"),
                FirebaseProjectId = _configuration.GetValue<string>("Push:FirebaseProjectId"),
                TimeoutSeconds = config.GetValue<int>("TimeoutSeconds", 30)
            };
        }

        private PushNotificationOptions GetDefaultPushOptions()
        {
            return new PushNotificationOptions
            {
                PlaySound = true,
                Vibrate = true,
                ShowWhenForeground = true,
                Priority = "normal",
                Badge = _pushConfig.DefaultBadge
            };
        }

        private async Task<List<string>> GetUserPushTokensAsync(string userId)
        {
            // This would typically query the database for user's push tokens
            // For now, return a mock token for testing
            await Task.CompletedTask;
            
            return new List<string> { $"mock-token-{userId}" };
        }

        private class FirebaseResponse
        {
            public int Success { get; set; }
            public int Failure { get; set; }
            public string? MessageId { get; set; }
            public string? Error { get; set; }
        }
    }

    /// <summary>
    /// Push notification configuration settings
    /// </summary>
    public class PushConfiguration
    {
        public bool Enabled { get; set; } = true;
        public int Priority { get; set; } = 3;
        public int MaxConcurrentSends { get; set; } = 20;
        public int RateLimitPerMinute { get; set; } = 100;
        public string Provider { get; set; } = "Mock";
        public string DefaultSound { get; set; } = "default";
        public int DefaultBadge { get; set; } = 1;
        public string? FirebaseServerKey { get; set; }
        public string? FirebaseProjectId { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
} 