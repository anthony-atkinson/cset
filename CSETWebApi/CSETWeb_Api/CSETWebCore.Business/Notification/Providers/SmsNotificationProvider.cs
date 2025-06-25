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
    /// SMS notification provider supporting multiple SMS services
    /// </summary>
    public class SmsNotificationProvider : ISmsNotificationProvider
    {
        private readonly ILogger<SmsNotificationProvider> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly SmsConfiguration _smsConfig;

        public SmsNotificationProvider(ILogger<SmsNotificationProvider> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _smsConfig = GetSmsConfiguration();
        }

        public NotificationType Type => NotificationType.Sms;
        public string DisplayName => $"{_smsConfig.Provider} SMS Provider";
        public bool IsEnabled => _smsConfig.Enabled;
        public int Priority => _smsConfig.Priority;

        public async Task<DeliveryResult> SendAsync(NotificationMessage message, NotificationRecipient recipient)
        {
            if (!IsEnabled)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "SMS provider is disabled",
                    ErrorCode = "PROVIDER_DISABLED"
                };
            }

            if (string.IsNullOrEmpty(recipient.PhoneNumber))
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = "No phone number provided",
                    ErrorCode = "MISSING_PHONE"
                };
            }

            try
            {
                var result = await SendSmsAsync(
                    recipient.PhoneNumber,
                    message.Body,
                    _smsConfig.DefaultFromNumber);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS notification to {PhoneNumber}", recipient.PhoneNumber);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "SMS_SEND_ERROR"
                };
            }
        }

        public async Task<DeliveryResult> SendSmsAsync(string to, string message, string? from = null)
        {
            try
            {
                // Validate phone number format
                var normalizedPhone = NormalizePhoneNumber(to);
                if (string.IsNullOrEmpty(normalizedPhone))
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid phone number format",
                        ErrorCode = "INVALID_PHONE_FORMAT"
                    };
                }

                // Truncate message if too long
                var truncatedMessage = TruncateMessage(message, _smsConfig.MaxMessageLength);

                DeliveryResult result;
                switch (_smsConfig.Provider.ToLowerInvariant())
                {
                    case "twilio":
                        result = await SendViaTwilioAsync(normalizedPhone, truncatedMessage, from);
                        break;
                    case "aws":
                        result = await SendViaAwsSnsAsync(normalizedPhone, truncatedMessage, from);
                        break;
                    case "mock":
                        result = await SendViaMockAsync(normalizedPhone, truncatedMessage, from);
                        break;
                    default:
                        return new DeliveryResult
                        {
                            Success = false,
                            ErrorMessage = $"Unsupported SMS provider: {_smsConfig.Provider}",
                            ErrorCode = "UNSUPPORTED_PROVIDER"
                        };
                }

                if (result.Success)
                {
                    _logger.LogInformation("SMS sent successfully to {PhoneNumber}", to);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", to);
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "SMS_SEND_ERROR"
                };
            }
        }

        public async Task<List<DeliveryResult>> SendBulkSmsAsync(List<string> to, string message, string? from = null)
        {
            var results = new List<DeliveryResult>();

            // Send SMS in parallel with rate limiting
            var semaphore = new System.Threading.SemaphoreSlim(_smsConfig.MaxConcurrentSends);
            var tasks = to.Select(async phoneNumber =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await SendSmsAsync(phoneNumber, message, from);
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

            _logger.LogInformation("Bulk SMS operation completed. Sent: {Sent}, Failed: {Failed}",
                results.Count(r => r.Success),
                results.Count(r => !r.Success));

            return results;
        }

        public async Task<bool> TestConnectivityAsync()
        {
            try
            {
                // Send a test SMS to a test number or use provider's test endpoint
                var testResult = await SendSmsAsync(_smsConfig.TestPhoneNumber, "CSET SMS Test", _smsConfig.DefaultFromNumber);
                return testResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS connectivity test failed");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateConfigurationAsync()
        {
            var result = new ValidationResult { IsValid = true };

            // Validate provider configuration
            if (string.IsNullOrEmpty(_smsConfig.Provider))
            {
                result.Errors.Add("SMS provider is not configured");
                result.IsValid = false;
            }

            if (string.IsNullOrEmpty(_smsConfig.DefaultFromNumber))
            {
                result.Warnings.Add("Default from number is not configured");
            }

            // Validate provider-specific settings
            switch (_smsConfig.Provider.ToLowerInvariant())
            {
                case "twilio":
                    if (string.IsNullOrEmpty(_smsConfig.TwilioAccountSid))
                    {
                        result.Errors.Add("Twilio Account SID is not configured");
                        result.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(_smsConfig.TwilioAuthToken))
                    {
                        result.Errors.Add("Twilio Auth Token is not configured");
                        result.IsValid = false;
                    }
                    break;
                case "aws":
                    if (string.IsNullOrEmpty(_smsConfig.AwsAccessKeyId))
                    {
                        result.Errors.Add("AWS Access Key ID is not configured");
                        result.IsValid = false;
                    }
                    if (string.IsNullOrEmpty(_smsConfig.AwsSecretAccessKey))
                    {
                        result.Errors.Add("AWS Secret Access Key is not configured");
                        result.IsValid = false;
                    }
                    break;
            }

            // Test connectivity if configuration is valid
            if (result.IsValid)
            {
                var connectivityTest = await TestConnectivityAsync();
                if (!connectivityTest)
                {
                    result.Errors.Add("SMS connectivity test failed");
                    result.IsValid = false;
                }
            }

            result.Details["provider"] = _smsConfig.Provider;
            result.Details["defaultFromNumber"] = _smsConfig.DefaultFromNumber;
            result.Details["maxConcurrentSends"] = _smsConfig.MaxConcurrentSends;
            result.Details["rateLimitPerMinute"] = _smsConfig.RateLimitPerMinute;
            result.Details["maxMessageLength"] = _smsConfig.MaxMessageLength;

            return result;
        }

        private async Task<DeliveryResult> SendViaTwilioAsync(string to, string message, string? from)
        {
            try
            {
                var url = $"https://api.twilio.com/2010-04-01/Accounts/{_smsConfig.TwilioAccountSid}/Messages.json";
                
                var formData = new Dictionary<string, string>
                {
                    ["To"] = to,
                    ["From"] = from ?? _smsConfig.DefaultFromNumber,
                    ["Body"] = message
                };

                var content = new FormUrlEncodedContent(formData);
                var authHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_smsConfig.TwilioAccountSid}:{_smsConfig.TwilioAuthToken}"));

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var twilioResponse = JsonSerializer.Deserialize<TwilioResponse>(responseContent);
                    return new DeliveryResult
                    {
                        Success = true,
                        ExternalId = twilioResponse?.Sid,
                        DeliveredAt = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            ["provider"] = "Twilio",
                            ["recipient"] = to,
                            ["messageLength"] = message.Length
                        }
                    };
                }
                else
                {
                    return new DeliveryResult
                    {
                        Success = false,
                        ErrorMessage = $"Twilio API error: {response.StatusCode} - {responseContent}",
                        ErrorCode = "TWILIO_API_ERROR"
                    };
                }
            }
            catch (Exception ex)
            {
                return new DeliveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = "TWILIO_SEND_ERROR"
                };
            }
        }

        private async Task<DeliveryResult> SendViaAwsSnsAsync(string to, string message, string? from)
        {
            // AWS SNS implementation would go here
            // This is a placeholder for future implementation
            return new DeliveryResult
            {
                Success = false,
                ErrorMessage = "AWS SNS SMS provider not yet implemented",
                ErrorCode = "AWS_SNS_NOT_IMPLEMENTED"
            };
        }

        private async Task<DeliveryResult> SendViaMockAsync(string to, string message, string? from)
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
                    ["recipient"] = to,
                    ["messageLength"] = message.Length,
                    ["isTest"] = true
                }
            };
        }

        private SmsConfiguration GetSmsConfiguration()
        {
            var config = _configuration.GetSection("EnhancedNotifications:Providers:Sms");
            
            return new SmsConfiguration
            {
                Enabled = config.GetValue<bool>("Enabled", false),
                Priority = config.GetValue<int>("Priority", 2),
                MaxConcurrentSends = config.GetValue<int>("MaxConcurrentSends", 5),
                RateLimitPerMinute = config.GetValue<int>("RateLimitPerMinute", 10),
                Provider = config.GetValue<string>("Provider") ?? "Mock",
                DefaultFromNumber = config.GetValue<string>("DefaultFromNumber") ?? "",
                MaxMessageLength = config.GetValue<int>("MaxMessageLength", 160),
                TestPhoneNumber = config.GetValue<string>("TestPhoneNumber") ?? "+1234567890",
                TwilioAccountSid = _configuration.GetValue<string>("Sms:TwilioAccountSid"),
                TwilioAuthToken = _configuration.GetValue<string>("Sms:TwilioAuthToken"),
                AwsAccessKeyId = _configuration.GetValue<string>("Sms:AwsAccessKeyId"),
                AwsSecretAccessKey = _configuration.GetValue<string>("Sms:AwsSecretAccessKey"),
                AwsRegion = _configuration.GetValue<string>("Sms:AwsRegion") ?? "us-east-1",
                TimeoutSeconds = config.GetValue<int>("TimeoutSeconds", 30)
            };
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Add country code if missing (assume US for now)
            if (digits.Length == 10)
            {
                return "+1" + digits;
            }
            else if (digits.Length == 11 && digits.StartsWith("1"))
            {
                return "+" + digits;
            }
            else if (digits.Length > 11)
            {
                return "+" + digits;
            }
            
            return string.Empty; // Invalid format
        }

        private string TruncateMessage(string message, int maxLength)
        {
            if (message.Length <= maxLength)
                return message;

            // Try to truncate at word boundary
            var truncated = message.Substring(0, maxLength - 3);
            var lastSpace = truncated.LastIndexOf(' ');
            
            if (lastSpace > maxLength * 0.8) // If we can find a space in the last 20%
            {
                return truncated.Substring(0, lastSpace) + "...";
            }
            
            return truncated + "...";
        }

        private class TwilioResponse
        {
            public string? Sid { get; set; }
            public string? Status { get; set; }
            public string? ErrorCode { get; set; }
            public string? ErrorMessage { get; set; }
        }
    }

    /// <summary>
    /// SMS configuration settings
    /// </summary>
    public class SmsConfiguration
    {
        public bool Enabled { get; set; } = false;
        public int Priority { get; set; } = 2;
        public int MaxConcurrentSends { get; set; } = 5;
        public int RateLimitPerMinute { get; set; } = 10;
        public string Provider { get; set; } = "Mock";
        public string DefaultFromNumber { get; set; } = "";
        public int MaxMessageLength { get; set; } = 160;
        public string TestPhoneNumber { get; set; } = "+1234567890";
        public string? TwilioAccountSid { get; set; }
        public string? TwilioAuthToken { get; set; }
        public string? AwsAccessKeyId { get; set; }
        public string? AwsSecretAccessKey { get; set; }
        public string AwsRegion { get; set; } = "us-east-1";
        public int TimeoutSeconds { get; set; } = 30;
    }
} 