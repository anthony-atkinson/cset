# Enhanced Notification System Guide

## Overview

The Enhanced Notification System provides comprehensive notification capabilities for CSET, supporting multiple channels including email, SMS, push notifications, webhooks, and in-app notifications. This system is designed for enterprise deployments with advanced features like templating, scheduling, delivery tracking, and user preferences.

## Features

### Multi-Channel Support
- **Email Notifications**: SMTP-based email delivery with HTML templates
- **SMS Notifications**: Support for Twilio and other SMS providers
- **Push Notifications**: Firebase Cloud Messaging integration
- **Webhook Notifications**: HTTP webhook delivery to external systems
- **In-App Notifications**: Real-time notifications within the application

### Advanced Features
- **Template System**: Reusable notification templates with variable substitution
- **Scheduling**: Send notifications at specific times
- **Delivery Tracking**: Monitor delivery status and retry failed deliveries
- **User Preferences**: Granular control over notification channels and timing
- **Bulk Operations**: Send notifications to multiple recipients efficiently
- **Rate Limiting**: Prevent notification spam and ensure fair usage
- **Health Monitoring**: Comprehensive health checks and diagnostics

## Configuration

### AppSettings Configuration

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
    }
  }
}
```

### Provider-Specific Configuration

#### Email Provider
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpSsl": true,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password"
  }
}
```

#### SMS Provider (Twilio)
```json
{
  "Sms": {
    "TwilioAccountSid": "your-account-sid",
    "TwilioAuthToken": "your-auth-token"
  }
}
```

#### Push Provider (Firebase)
```json
{
  "Push": {
    "FirebaseServerKey": "your-firebase-server-key",
    "FirebaseProjectId": "your-project-id"
  }
}
```

## API Usage

### Sending Notifications

#### Simple Notification
```csharp
// Send a simple email notification
var request = new CreateNotificationRequest
{
    Type = NotificationType.Email,
    Subject = "Assessment Complete",
    Body = "Your assessment has been completed successfully.",
    EmailRecipients = new List<string> { "user@example.com" }
};

var response = await notificationService.SendNotificationAsync(request);
```

#### Template-Based Notification
```csharp
// Send using a template
var response = await notificationService.SendTemplateNotificationAsync(
    "assessment_complete",
    new List<NotificationRecipient> 
    { 
        new NotificationRecipient { Email = "user@example.com" } 
    },
    new Dictionary<string, object>
    {
        ["assessmentName"] = "Critical Infrastructure Assessment",
        ["completionDate"] = DateTime.Now.ToString("MM/dd/yyyy")
    }
);
```

#### Bulk Notifications
```csharp
// Send to multiple recipients
var messages = new List<NotificationMessage>
{
    new NotificationMessage
    {
        Type = NotificationType.Email,
        Subject = "System Update",
        Body = "System maintenance scheduled for tonight.",
        Recipients = new List<NotificationRecipient>
        {
            new NotificationRecipient { Email = "user1@example.com" },
            new NotificationRecipient { Email = "user2@example.com" }
        }
    }
};

var responses = await notificationService.SendBulkNotificationsAsync(messages);
```

#### Scheduled Notifications
```csharp
// Schedule a notification for future delivery
var message = new NotificationMessage
{
    Type = NotificationType.Email,
    Subject = "Reminder: Assessment Due",
    Body = "Your assessment is due in 24 hours.",
    Recipients = new List<NotificationRecipient>
    {
        new NotificationRecipient { Email = "user@example.com" }
    }
};

var scheduledTime = DateTime.UtcNow.AddHours(24);
var response = await notificationService.ScheduleNotificationAsync(message, scheduledTime);
```

### Template Management

#### Creating Templates
```csharp
var template = new NotificationTemplate
{
    Name = "assessment_complete",
    Type = NotificationType.Email,
    SubjectTemplate = "Assessment Complete: {{assessmentName}}",
    BodyTemplate = "Dear {{userName}},\n\nYour assessment '{{assessmentName}}' has been completed on {{completionDate}}.",
    HtmlBodyTemplate = "<h2>Assessment Complete</h2><p>Dear {{userName}},</p><p>Your assessment '{{assessmentName}}' has been completed on {{completionDate}}.</p>",
    IsActive = true,
    Variables = new List<TemplateVariable>
    {
        new TemplateVariable { Name = "userName", Type = VariableType.String, Required = true },
        new TemplateVariable { Name = "assessmentName", Type = VariableType.String, Required = true },
        new TemplateVariable { Name = "completionDate", Type = VariableType.Date, Required = true }
    }
};

var createdTemplate = await notificationService.CreateTemplateAsync(template);
```

#### Using Templates
```csharp
var variables = new Dictionary<string, object>
{
    ["userName"] = "John Doe",
    ["assessmentName"] = "Critical Infrastructure Assessment",
    ["completionDate"] = DateTime.Now.ToString("MM/dd/yyyy")
};

var response = await notificationService.SendTemplateNotificationAsync(
    "assessment_complete",
    recipients,
    variables
);
```

### User Preferences

#### Getting User Preferences
```csharp
var preferences = await notificationService.GetUserPreferencesAsync(userId);
```

#### Updating User Preferences
```csharp
var preferences = new NotificationPreferences
{
    Email = new EmailPreferences
    {
        Enabled = true,
        EmailAddress = "user@example.com",
        Format = EmailFormat.Html,
        MaxPerDay = 50,
        QuietHoursStart = new TimeSpan(22, 0, 0), // 10 PM
        QuietHoursEnd = new TimeSpan(8, 0, 0),    // 8 AM
        Timezone = "America/New_York"
    },
    Sms = new SmsPreferences
    {
        Enabled = false,
        PhoneNumber = "+1234567890",
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
    Global = new GlobalPreferences
    {
        NotificationsEnabled = true,
        DefaultChannels = new List<NotificationType> { NotificationType.Email, NotificationType.InApp },
        Language = "en-US",
        Timezone = "America/New_York"
    }
};

var updatedPreferences = await notificationService.UpdateUserPreferencesAsync(userId, preferences);
```

### Delivery Tracking

#### Getting Delivery Status
```csharp
var deliveryStatuses = await notificationService.GetDeliveryStatusAsync(notificationId);
```

#### Retrying Failed Deliveries
```csharp
var success = await notificationService.RetryFailedDeliveriesAsync(notificationId);
```

### Statistics and Analytics

#### Getting Statistics
```csharp
var statistics = await notificationService.GetStatisticsAsync(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow
);
```

#### Type-Specific Statistics
```csharp
var emailStats = await notificationService.GetTypeStatisticsAsync(
    NotificationType.Email,
    startDate: DateTime.UtcNow.AddDays(-7),
    endDate: DateTime.UtcNow
);
```

## Health and Diagnostics

### Testing Connectivity
```csharp
// Test email connectivity
var emailConnected = await notificationService.TestConnectivityAsync(NotificationType.Email);

// Test SMS connectivity
var smsConnected = await notificationService.TestConnectivityAsync(NotificationType.Sms);
```

### Getting Health Status
```csharp
var healthStatus = await notificationService.GetHealthStatusAsync();
```

### Validating Configuration
```csharp
var validationResults = await notificationService.ValidateConfigurationAsync();
```

## Frontend Integration

### Angular Service Example
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = '/api/enhancednotification';

  constructor(private http: HttpClient) {}

  sendNotification(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-simple`, request);
  }

  sendTemplateNotification(templateName: string, recipients: any[], variables: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/send-template`, {
      templateName,
      recipients,
      variables
    });
  }

  getUserPreferences(): Observable<any> {
    return this.http.get(`${this.apiUrl}/preferences`);
  }

  updateUserPreferences(preferences: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/preferences`, preferences);
  }

  getNotifications(includeRead = false, limit = 50): Observable<any> {
    return this.http.get(`${this.apiUrl}/inapp/notifications`, {
      params: { includeRead: includeRead.toString(), limit: limit.toString() }
    });
  }

  markAsRead(notificationId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/inapp/notifications/${notificationId}/read`, {});
  }
}
```

### React Hook Example
```typescript
import { useState, useEffect } from 'react';
import { notificationService } from '../services/notificationService';

export const useNotifications = (userId: string) => {
  const [notifications, setNotifications] = useState([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadNotifications();
  }, [userId]);

  const loadNotifications = async () => {
    try {
      setLoading(true);
      const [notificationsData, unreadCountData] = await Promise.all([
        notificationService.getNotifications(userId, false, 50),
        notificationService.getUnreadCount(userId)
      ]);
      setNotifications(notificationsData);
      setUnreadCount(unreadCountData);
    } catch (error) {
      console.error('Error loading notifications:', error);
    } finally {
      setLoading(false);
    }
  };

  const markAsRead = async (notificationId: string) => {
    try {
      await notificationService.markAsRead(userId, notificationId);
      await loadNotifications(); // Refresh the list
    } catch (error) {
      console.error('Error marking notification as read:', error);
    }
  };

  return {
    notifications,
    unreadCount,
    loading,
    markAsRead,
    refresh: loadNotifications
  };
};
```

## Best Practices

### Performance Optimization
1. **Use Templates**: Create reusable templates instead of sending individual notifications
2. **Batch Operations**: Use bulk notification methods for multiple recipients
3. **Rate Limiting**: Respect rate limits to avoid overwhelming providers
4. **Caching**: Leverage the built-in template and preferences caching

### Security Considerations
1. **Input Validation**: Always validate notification content and recipient information
2. **Authentication**: Use proper authentication for webhook endpoints
3. **Encryption**: Use HTTPS for all external communications
4. **Rate Limiting**: Implement rate limiting to prevent abuse

### Error Handling
1. **Retry Logic**: The system includes automatic retry logic for failed deliveries
2. **Monitoring**: Monitor delivery statistics and error rates
3. **Fallback**: Configure multiple notification channels for critical messages
4. **Logging**: Review logs for delivery issues and system health

### User Experience
1. **Preferences**: Respect user notification preferences and quiet hours
2. **Localization**: Use appropriate language and timezone settings
3. **Accessibility**: Ensure notifications are accessible to all users
4. **Testing**: Test notifications in development before production deployment

## Troubleshooting

### Common Issues

#### Email Notifications Not Sending
1. Check SMTP configuration in appsettings.json
2. Verify SMTP credentials and authentication
3. Test connectivity using the health check endpoint
4. Check firewall and network connectivity

#### SMS Notifications Failing
1. Verify Twilio credentials and account status
2. Check phone number format (should include country code)
3. Ensure SMS provider is enabled in configuration
4. Review rate limits and account balance

#### Push Notifications Not Working
1. Verify Firebase configuration and server key
2. Check device token registration
3. Ensure push provider is enabled
4. Test with mock provider first

#### Webhook Delivery Issues
1. Verify webhook URL is accessible
2. Check authentication headers if required
3. Review webhook endpoint response codes
4. Monitor webhook timeout settings

### Debugging Tools

#### Health Check Endpoints
- `GET /api/enhancednotification/health/status` - Overall system health
- `GET /api/enhancednotification/health/test/{type}` - Test specific provider
- `GET /api/enhancednotification/health/validate` - Validate configuration

#### Logging
Enable detailed logging for notification providers:
```json
{
  "Logging": {
    "LogLevel": {
      "CSETWebCore.Business.Notification": "Debug",
      "CSETWebCore.Business.Notification.Providers": "Debug"
    }
  }
}
```

#### Statistics
Monitor notification statistics:
- `GET /api/enhancednotification/statistics` - Overall statistics
- `GET /api/enhancednotification/statistics/type/{type}` - Type-specific statistics
- `GET /api/enhancednotification/statistics/priority/{priority}` - Priority-specific statistics

## Migration from Legacy System

The Enhanced Notification System is designed to work alongside the existing notification system. To migrate:

1. **Gradual Migration**: Start with new features using the enhanced system
2. **Dual Support**: Both systems can run simultaneously during transition
3. **Template Migration**: Convert existing email templates to the new template system
4. **User Preferences**: Migrate user preferences to the new preference system
5. **Monitoring**: Monitor both systems during transition period

## Support and Maintenance

### Regular Maintenance Tasks
1. **Template Review**: Regularly review and update notification templates
2. **Provider Monitoring**: Monitor provider health and performance
3. **Statistics Review**: Review notification statistics and trends
4. **Configuration Updates**: Update provider configurations as needed

### Backup and Recovery
1. **Template Backup**: Regularly backup notification templates
2. **Configuration Backup**: Backup notification configuration settings
3. **Database Backup**: Ensure notification data is included in database backups
4. **Recovery Procedures**: Document recovery procedures for notification system

### Updates and Upgrades
1. **Provider Updates**: Keep notification providers updated
2. **Security Patches**: Apply security patches promptly
3. **Feature Updates**: Review and implement new features as needed
4. **Compatibility**: Ensure compatibility with CSET updates

## Conclusion

The Enhanced Notification System provides a comprehensive, enterprise-ready notification solution for CSET. With support for multiple channels, advanced features like templating and scheduling, and robust monitoring and diagnostics, it meets the needs of both standalone and enterprise deployments.

For additional support or questions, refer to the CSET documentation or contact the development team. 