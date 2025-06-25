# CSET Enhanced Notification System

## Overview

The CSET Enhanced Notification System provides enterprise-grade notification capabilities for CSET deployments. This system extends the existing notification infrastructure with advanced features including multi-channel delivery, template management, scheduling, analytics, and comprehensive tracking.

## Features

### Core Capabilities

- **Multi-Channel Delivery**: Support for Email, SMS, Push, Webhook, and In-App notifications
- **Template Management**: Dynamic templates with variable substitution and version control
- **Scheduling**: Future delivery and recurring notifications with timezone support
- **Delivery Tracking**: Comprehensive tracking of notification delivery status
- **Retry Logic**: Configurable retry mechanisms with exponential backoff
- **Bulk Operations**: Efficient sending to multiple recipients
- **User Preferences**: Granular control over notification preferences
- **Analytics**: Detailed statistics and performance metrics
- **Health Monitoring**: Service health checks and diagnostics

### Enterprise Features

- **Rate Limiting**: Configurable rate limits per channel and user
- **Security**: Content validation, HTML sanitization, and domain filtering
- **Caching**: Template and preference caching for performance
- **Monitoring**: Real-time metrics and alerting
- **Scalability**: Designed for high-volume enterprise deployments

## Architecture

### Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Enhanced Notification System             │
├─────────────────────────────────────────────────────────────┤
│  API Controllers  │  Service Layer  │  Provider Layer      │
│                   │                  │                      │
│ • Core Endpoints  │ • Business Logic │ • Email Provider     │
│ • Templates       │ • Validation     │ • SMS Provider       │
│ • Preferences     │ • Caching        │ • Push Provider      │
│ • Analytics       │ • Tracking       │ • Webhook Provider   │
│ • Health          │ • Scheduling     │ • In-App Provider    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Layer                               │
│                                                             │
│ • Templates Storage    │ • Delivery Tracking               │
│ • User Preferences     │ • Statistics & Analytics          │
│ • Scheduled Notifications │ • Health Metrics               │
└─────────────────────────────────────────────────────────────┘
```

### Service Interfaces

- `IEnhancedNotificationService`: Main service interface
- `INotificationProvider`: Base provider interface
- `IEmailNotificationProvider`: Email-specific provider
- `ISmsNotificationProvider`: SMS-specific provider
- `IPushNotificationProvider`: Push notification provider
- `IWebhookNotificationProvider`: Webhook provider
- `IInAppNotificationProvider`: In-app notification provider

## API Reference

### Core Notification Endpoints

#### Send Notification
```http
POST /api/enhancednotification/send
Content-Type: application/json

{
  "type": "Email",
  "priority": "Normal",
  "subject": "Assessment Invitation",
  "body": "You have been invited to participate in a CSET assessment.",
  "htmlBody": "<h1>Assessment Invitation</h1><p>You have been invited...</p>",
  "recipients": [
    {
      "id": "user123",
      "name": "John Doe",
      "email": "john.doe@example.com"
    }
  ],
  "templateName": "assessment-invitation",
  "templateVariables": {
    "userName": "John Doe",
    "assessmentName": "Critical Infrastructure Assessment"
  },
  "scheduledAt": "2024-01-15T10:00:00Z",
  "tags": ["assessment", "invitation"]
}
```

#### Send Template Notification
```http
POST /api/enhancednotification/send-template
Content-Type: application/json

{
  "templateName": "assessment-invitation",
  "recipients": [
    {
      "id": "user123",
      "name": "John Doe",
      "email": "john.doe@example.com"
    }
  ],
  "variables": {
    "userName": "John Doe",
    "assessmentName": "Critical Infrastructure Assessment",
    "dueDate": "2024-02-15"
  },
  "priority": "High"
}
```

#### Send Simple Notification
```http
POST /api/enhancednotification/send-simple
Content-Type: application/json

{
  "type": "Email",
  "priority": "Normal",
  "subject": "Password Reset",
  "body": "Your password has been reset successfully.",
  "emailRecipients": ["user@example.com"],
  "tags": ["security", "password-reset"]
}
```

#### Schedule Notification
```http
POST /api/enhancednotification/schedule
Content-Type: application/json

{
  "message": {
    "type": "Email",
    "subject": "Reminder: Assessment Due",
    "body": "Your assessment is due in 3 days.",
    "recipients": [...]
  },
  "scheduledAt": "2024-01-20T09:00:00Z"
}
```

### Template Management

#### Create Template
```http
POST /api/enhancednotification/templates
Content-Type: application/json

{
  "name": "assessment-invitation",
  "description": "Template for assessment invitations",
  "type": "Email",
  "subjectTemplate": "Invitation to {{assessmentName}}",
  "bodyTemplate": "Dear {{userName}},\n\nYou have been invited to participate in {{assessmentName}}.",
  "htmlBodyTemplate": "<h1>Invitation to {{assessmentName}}</h1><p>Dear {{userName}},</p><p>You have been invited...</p>",
  "variables": [
    {
      "name": "userName",
      "description": "Recipient's full name",
      "type": "String",
      "isRequired": true
    },
    {
      "name": "assessmentName",
      "description": "Name of the assessment",
      "type": "String",
      "isRequired": true
    }
  ],
  "tags": ["assessment", "invitation"],
  "defaultPriority": "Normal"
}
```

#### Get Templates
```http
GET /api/enhancednotification/templates?type=Email&isActive=true
```

#### Update Template
```http
PUT /api/enhancednotification/templates/{templateId}
Content-Type: application/json

{
  "name": "assessment-invitation-v2",
  "description": "Updated template for assessment invitations",
  "subjectTemplate": "You're invited: {{assessmentName}}",
  "bodyTemplate": "Hello {{userName}},\n\nYou're invited to {{assessmentName}}.",
  "isActive": true
}
```

### Delivery Tracking

#### Get Delivery Status
```http
GET /api/enhancednotification/delivery/{notificationId}
```

#### Retry Failed Deliveries
```http
POST /api/enhancednotification/delivery/{notificationId}/retry
```

#### Get Recipient History
```http
GET /api/enhancednotification/delivery/history/{recipientId}?limit=50&offset=0
```

### User Preferences

#### Get User Preferences
```http
GET /api/enhancednotification/preferences
```

#### Update User Preferences
```http
PUT /api/enhancednotification/preferences
Content-Type: application/json

{
  "email": {
    "enabled": true,
    "emailAddress": "user@example.com",
    "format": "Html",
    "maxPerDay": 50,
    "quietHoursStart": "22:00:00",
    "quietHoursEnd": "08:00:00",
    "timezone": "America/New_York"
  },
  "sms": {
    "enabled": false,
    "phoneNumber": "+1234567890",
    "maxPerDay": 10
  },
  "push": {
    "enabled": true,
    "showWhenForeground": true,
    "playSound": true,
    "vibrate": true
  },
  "global": {
    "notificationsEnabled": true,
    "defaultChannels": ["Email", "InApp"],
    "language": "en-US",
    "timezone": "America/New_York",
    "groupNotifications": true,
    "maxHistoryCount": 1000
  },
  "categories": {
    "assessment": {
      "enabled": true,
      "channels": ["Email", "InApp"],
      "minimumPriority": "Normal",
      "respectQuietHours": true
    },
    "system": {
      "enabled": true,
      "channels": ["Email"],
      "minimumPriority": "High",
      "respectQuietHours": false
    }
  }
}
```

### Analytics

#### Get Statistics
```http
GET /api/enhancednotification/statistics?startDate=2024-01-01&endDate=2024-01-31
```

#### Get Type Statistics
```http
GET /api/enhancednotification/statistics/type/Email?startDate=2024-01-01&endDate=2024-01-31
```

#### Get Priority Statistics
```http
GET /api/enhancednotification/statistics/priority/High?startDate=2024-01-01&endDate=2024-01-31
```

### Bulk Operations

#### Send Bulk Notifications
```http
POST /api/enhancednotification/bulk/send
Content-Type: application/json

[
  {
    "type": "Email",
    "subject": "Welcome to CSET",
    "body": "Welcome to the CSET platform!",
    "recipients": [...]
  },
  {
    "type": "InApp",
    "subject": "System Maintenance",
    "body": "Scheduled maintenance in 2 hours.",
    "recipients": [...]
  }
]
```

#### Send Bulk Template Notifications
```http
POST /api/enhancednotification/bulk/send-template
Content-Type: application/json

{
  "templateName": "welcome-email",
  "recipients": [
    {
      "id": "user1",
      "name": "John Doe",
      "email": "john@example.com"
    },
    {
      "id": "user2",
      "name": "Jane Smith",
      "email": "jane@example.com"
    }
  ],
  "variables": {
    "platformName": "CSET",
    "supportEmail": "support@cset.inl.gov"
  }
}
```

### Health and Diagnostics

#### Test Connectivity
```http
GET /api/enhancednotification/health/test/Email
```

#### Get Health Status
```http
GET /api/enhancednotification/health/status
```

#### Validate Configuration
```http
GET /api/enhancednotification/health/validate
```

## Configuration

### EnhancedNotifications Section

```json
{
  "EnhancedNotifications": {
    "Enabled": true,
    "DefaultPriority": "Normal",
    "MaxRetries": 3,
    "RetryDelaySeconds": 60,
    "MaxRetryDelaySeconds": 3600,
    "TemplateCacheSize": 100,
    "PreferencesCacheSize": 1000,
    "DeliveryTrackingEnabled": true,
    "StatisticsEnabled": true,
    "BulkOperationsEnabled": true,
    "SchedulingEnabled": true,
    "Providers": {
      "Email": {
        "Enabled": true,
        "Priority": 1,
        "MaxConcurrentSends": 10,
        "RateLimitPerMinute": 60,
        "DefaultFromEmail": "no-reply@cset.inl.gov",
        "DefaultFromName": "CSET System",
        "RequireAuthentication": true,
        "UseSsl": false,
        "TimeoutSeconds": 30
      },
      "Sms": {
        "Enabled": false,
        "Priority": 2,
        "MaxConcurrentSends": 5,
        "RateLimitPerMinute": 10,
        "Provider": "Twilio",
        "DefaultFromNumber": "",
        "TimeoutSeconds": 30
      },
      "Push": {
        "Enabled": true,
        "Priority": 3,
        "MaxConcurrentSends": 20,
        "RateLimitPerMinute": 100,
        "Provider": "Firebase",
        "DefaultSound": "default",
        "DefaultBadge": 1,
        "TimeoutSeconds": 30
      },
      "Webhook": {
        "Enabled": false,
        "Priority": 4,
        "MaxConcurrentSends": 5,
        "RateLimitPerMinute": 30,
        "DefaultTimeoutSeconds": 30,
        "MaxRetries": 3,
        "RetryDelaySeconds": 60
      },
      "InApp": {
        "Enabled": true,
        "Priority": 5,
        "MaxConcurrentSends": 50,
        "RateLimitPerMinute": 200,
        "DefaultType": "info",
        "MaxHistoryPerUser": 1000,
        "AutoCleanupDays": 30
      }
    },
    "Templates": {
      "DefaultLanguage": "en-US",
      "SupportedLanguages": ["en-US", "es-ES", "fr-FR"],
      "AutoSaveDrafts": true,
      "VersionControl": true,
      "MaxTemplateSize": 1048576
    },
    "Preferences": {
      "DefaultEmailEnabled": true,
      "DefaultSmsEnabled": false,
      "DefaultPushEnabled": true,
      "DefaultWebhookEnabled": false,
      "DefaultInAppEnabled": true,
      "AllowUserOverride": true,
      "RequireConfirmation": false
    },
    "Scheduling": {
      "MaxScheduledDays": 365,
      "MaxScheduledPerUser": 100,
      "AllowRecurring": true,
      "MaxRecurringCount": 52,
      "TimeZoneSupport": true,
      "DefaultTimeZone": "UTC"
    },
    "Analytics": {
      "Enabled": true,
      "RetentionDays": 90,
      "AggregationInterval": "1h",
      "RealTimeMetrics": true,
      "CustomDimensions": ["source", "category", "priority", "template"]
    },
    "Security": {
      "RequireAuthentication": true,
      "RequireAuthorization": true,
      "RateLimitingEnabled": true,
      "MaxRequestsPerMinute": 100,
      "MaxBulkSize": 1000,
      "ContentValidation": true,
      "SanitizeHtml": true,
      "BlockedDomains": [],
      "AllowedDomains": []
    },
    "Monitoring": {
      "HealthCheckEnabled": true,
      "HealthCheckInterval": "5m",
      "MetricsEnabled": true,
      "MetricsInterval": "1m",
      "AlertingEnabled": true,
      "FailureThreshold": 5,
      "RecoveryThreshold": 3
    }
  }
}
```

## Data Models

### NotificationMessage
```csharp
public class NotificationMessage
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string? HtmlBody { get; set; }
    public List<NotificationRecipient> Recipients { get; set; }
    public string? TemplateName { get; set; }
    public Dictionary<string, object> TemplateVariables { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public RetryConfiguration RetryConfig { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
    public List<string> Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> RelatedEntities { get; set; }
}
```

### NotificationRecipient
```csharp
public class NotificationRecipient
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UserId { get; set; }
    public string? WebhookUrl { get; set; }
    public List<NotificationType> PreferredChannels { get; set; }
    public string Language { get; set; }
    public string Timezone { get; set; }
}
```

### NotificationTemplate
```csharp
public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public NotificationType Type { get; set; }
    public string SubjectTemplate { get; set; }
    public string BodyTemplate { get; set; }
    public string? HtmlBodyTemplate { get; set; }
    public List<TemplateVariable> Variables { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public List<string> Tags { get; set; }
    public NotificationPriority DefaultPriority { get; set; }
}
```

### NotificationPreferences
```csharp
public class NotificationPreferences
{
    public string UserId { get; set; }
    public EmailPreferences Email { get; set; }
    public SmsPreferences Sms { get; set; }
    public PushPreferences Push { get; set; }
    public WebhookPreferences Webhook { get; set; }
    public GlobalPreferences Global { get; set; }
    public Dictionary<string, CategoryPreferences> Categories { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

## Usage Examples

### Basic Email Notification
```csharp
var notificationService = serviceProvider.GetService<IEnhancedNotificationService>();

var message = new NotificationMessage
{
    Type = NotificationType.Email,
    Priority = NotificationPriority.Normal,
    Subject = "Welcome to CSET",
    Body = "Welcome to the CSET platform!",
    Recipients = new List<NotificationRecipient>
    {
        new NotificationRecipient
        {
            Id = "user123",
            Name = "John Doe",
            Email = "john.doe@example.com"
        }
    }
};

var response = await notificationService.SendNotificationAsync(message);
```

### Template-Based Notification
```csharp
var recipients = new List<NotificationRecipient>
{
    new NotificationRecipient
    {
        Id = "user123",
        Name = "John Doe",
        Email = "john.doe@example.com"
    }
};

var variables = new Dictionary<string, object>
{
    ["userName"] = "John Doe",
    ["assessmentName"] = "Critical Infrastructure Assessment",
    ["dueDate"] = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd")
};

var response = await notificationService.SendTemplateNotificationAsync(
    "assessment-invitation",
    recipients,
    variables,
    NotificationPriority.High);
```

### Bulk Notification
```csharp
var messages = new List<NotificationMessage>
{
    new NotificationMessage
    {
        Type = NotificationType.Email,
        Subject = "System Maintenance",
        Body = "Scheduled maintenance in 2 hours.",
        Recipients = new List<NotificationRecipient>
        {
            new NotificationRecipient { Email = "user1@example.com" },
            new NotificationRecipient { Email = "user2@example.com" }
        }
    }
};

var responses = await notificationService.SendBulkNotificationsAsync(messages);
```

### Scheduled Notification
```csharp
var message = new NotificationMessage
{
    Type = NotificationType.Email,
    Subject = "Assessment Reminder",
    Body = "Your assessment is due tomorrow.",
    Recipients = new List<NotificationRecipient>
    {
        new NotificationRecipient { Email = "user@example.com" }
    }
};

var scheduledTime = DateTime.UtcNow.AddDays(1).AddHours(9); // 9 AM tomorrow
var response = await notificationService.ScheduleNotificationAsync(message, scheduledTime);
```

## Best Practices

### Performance
- Use bulk operations for multiple recipients
- Leverage template caching for frequently used templates
- Implement appropriate rate limiting
- Use async/await patterns consistently

### Security
- Validate all input data
- Sanitize HTML content
- Implement proper authentication and authorization
- Use HTTPS for all external communications
- Monitor for suspicious activity

### Reliability
- Implement retry logic with exponential backoff
- Use circuit breakers for external services
- Monitor delivery success rates
- Implement dead letter queues for failed notifications

### User Experience
- Respect user preferences and quiet hours
- Provide clear error messages
- Allow users to manage their notification settings
- Group similar notifications when appropriate

## Monitoring and Troubleshooting

### Health Checks
- Monitor provider connectivity
- Track delivery success rates
- Monitor queue lengths and processing times
- Check template and preference cache hit rates

### Common Issues
- **SMTP Connection Failures**: Check SMTP server configuration and credentials
- **Template Rendering Errors**: Validate template syntax and required variables
- **Rate Limiting**: Monitor and adjust rate limits based on usage patterns
- **Memory Issues**: Monitor cache sizes and adjust as needed

### Logging
The system provides comprehensive logging at different levels:
- **Information**: Normal operations and successful deliveries
- **Warning**: Retry attempts and configuration issues
- **Error**: Failed deliveries and system errors

### Metrics
Key metrics to monitor:
- Delivery success rate by channel
- Average delivery time
- Template usage statistics
- User preference distribution
- System resource utilization

## Migration from Legacy System

The enhanced notification system is designed to work alongside the existing notification infrastructure. To migrate:

1. **Gradual Migration**: Start with new features using the enhanced system
2. **Template Migration**: Convert existing email templates to the new template system
3. **User Preferences**: Migrate user preferences to the new preference system
4. **Monitoring**: Compare performance and reliability between systems
5. **Full Migration**: Once validated, migrate all notifications to the enhanced system

## Support and Maintenance

### Regular Maintenance
- Monitor and clean up old delivery records
- Update templates and preferences as needed
- Review and adjust rate limits
- Update provider configurations

### Updates and Upgrades
- Keep provider SDKs updated
- Monitor for security updates
- Test new features in staging environment
- Plan for backward compatibility

### Documentation
- Keep API documentation updated
- Maintain troubleshooting guides
- Document custom configurations
- Provide user training materials

## Conclusion

The CSET Enhanced Notification System provides a robust, scalable, and feature-rich notification infrastructure suitable for enterprise deployments. With comprehensive API coverage, advanced features, and strong monitoring capabilities, it supports the complex notification requirements of modern cybersecurity assessment platforms.

For additional support or questions, please refer to the API documentation or contact the development team. 