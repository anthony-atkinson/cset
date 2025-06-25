# CSET API Rate Limiting and Throttling Guide

## Overview

This guide documents the implementation of comprehensive API rate limiting and throttling for the CSET (Cyber Security Evaluation Tool) application. The rate limiting system provides protection against API abuse, ensures fair resource usage, and supports both standalone and enterprise deployments.

## 🎯 Features Implemented

### ✅ Core Rate Limiting Features
- **Multi-level Rate Limiting**: General, client-specific, and endpoint-specific limits
- **Flexible Client Identification**: IP address, user ID, and custom client ID support
- **Admin Bypass**: Privileged users can bypass rate limiting
- **Real-time Monitoring**: Comprehensive statistics and analytics
- **Configurable Limits**: Easy configuration through appsettings.json
- **Rate Limit Headers**: Standard HTTP headers for client awareness
- **Graceful Degradation**: System continues to function even if rate limiting fails

### ✅ Advanced Features
- **Endpoint-specific Limits**: Different limits for different API endpoints
- **Client-specific Limits**: Custom limits for different client types
- **IP-based Rate Limiting**: Protection against IP-based abuse
- **User-based Rate Limiting**: Per-user limits for authenticated users
- **Admin Bypass**: Privileged users and IPs can bypass limits
- **Statistics and Analytics**: Real-time monitoring and reporting
- **Health Monitoring**: System health checks and status reporting

## 🏗️ Architecture

### Components

1. **RateLimitConfiguration** - Configuration models for rate limiting settings
2. **IRateLimitService** - Service interface for rate limiting operations
3. **RateLimitService** - Core rate limiting logic and implementation
4. **RateLimitMiddleware** - ASP.NET Core middleware for request processing
5. **RateLimitController** - API endpoints for monitoring and management
6. **AspNetCoreRateLimit** - Third-party library for advanced rate limiting features

### Data Flow

```
HTTP Request → RateLimitMiddleware → RateLimitService → Cache → Response
                ↓
            Check Limits → Apply Headers → Continue Pipeline
```

## 📋 Configuration

### appsettings.json Configuration

```json
{
  "RateLimiting": {
    "Enabled": true,
    "General": {
      "DefaultRequestsPerPeriod": 1000,
      "DefaultPeriodSeconds": 3600,
      "EnableForAllEndpoints": true,
      "ExcludedEndpoints": [
        "GET:/api-docs",
        "GET:/api-docs/*",
        "GET:/health",
        "GET:/health/*",
        "GET:/api/ratelimit/health"
      ]
    },
    "Client": {
      "Enabled": true,
      "ClientIdHeader": "X-ClientId",
      "UseIpAddress": true,
      "UseUserId": true,
      "ClientLimits": {
        "default": {
          "RequestsPerPeriod": 1000,
          "PeriodSeconds": 3600
        },
        "admin": {
          "RequestsPerPeriod": 5000,
          "PeriodSeconds": 3600
        },
        "api": {
          "RequestsPerPeriod": 2000,
          "PeriodSeconds": 3600
        }
      }
    },
    "Endpoints": {
      "Enabled": true,
      "EndpointLimits": {
        "POST:/api/assessment": {
          "RequestsPerPeriod": 100,
          "PeriodSeconds": 3600
        },
        "POST:/api/auth/login": {
          "RequestsPerPeriod": 10,
          "PeriodSeconds": 300
        }
      }
    },
    "AdminBypass": {
      "Enabled": true,
      "AdminRoles": ["Administrator", "Admin", "SystemAdmin"],
      "AdminUserIds": [],
      "AdminIpAddresses": ["127.0.0.1", "::1"]
    }
  }
}
```

### Configuration Options

#### General Settings
- **Enabled**: Enable/disable rate limiting globally
- **DefaultRequestsPerPeriod**: Default requests allowed per time period
- **DefaultPeriodSeconds**: Default time period in seconds
- **EnableForAllEndpoints**: Apply rate limiting to all endpoints
- **ExcludedEndpoints**: List of endpoints to exclude from rate limiting

#### Client Settings
- **Enabled**: Enable client-specific rate limiting
- **ClientIdHeader**: HTTP header for client identification
- **UseIpAddress**: Use IP address for client identification
- **UseUserId**: Use user ID for client identification
- **ClientLimits**: Custom limits for different client types

#### Endpoint Settings
- **Enabled**: Enable endpoint-specific rate limiting
- **EndpointLimits**: Custom limits for specific endpoints

#### Admin Bypass Settings
- **Enabled**: Enable admin bypass functionality
- **AdminRoles**: User roles that can bypass rate limiting
- **AdminUserIds**: Specific user IDs that can bypass rate limiting
- **AdminIpAddresses**: IP addresses that can bypass rate limiting

## 🔧 API Endpoints

### Rate Limit Information

#### GET /api/ratelimit/info
Get current rate limit information for the requesting client.

**Response:**
```json
{
  "clientId": "ip:192.168.1.100",
  "currentCount": 45,
  "limit": 1000,
  "periodSeconds": 3600,
  "remainingSeconds": 1800,
  "isExceeded": false,
  "remainingRequests": 955
}
```

#### GET /api/ratelimit/statistics
Get rate limiting statistics and analytics (Admin only).

**Response:**
```json
{
  "totalRequests": 15000,
  "rateLimitedRequests": 150,
  "bypassedRequests": 50,
  "rateLimitPercentage": 1.0,
  "bypassPercentage": 0.33,
  "topRateLimitedClients": [
    {
      "clientId": "ip:192.168.1.100",
      "rateLimitedCount": 25,
      "totalRequests": 500,
      "rateLimitPercentage": 5.0
    }
  ],
  "topRateLimitedEndpoints": [
    {
      "endpoint": "/api/auth/login",
      "method": "POST",
      "rateLimitedCount": 50,
      "totalRequests": 200,
      "rateLimitPercentage": 25.0
    }
  ]
}
```

#### GET /api/ratelimit/client/{clientId}
Get rate limit information for a specific client (Admin only).

#### POST /api/ratelimit/client/{clientId}/reset
Reset rate limit counters for a specific client (Admin only).

#### GET /api/ratelimit/health
Get health status of the rate limiting system.

## 📊 Rate Limit Headers

The system adds the following headers to all API responses:

- **X-RateLimit-Limit**: Maximum requests allowed per period
- **X-RateLimit-Remaining**: Remaining requests in current period
- **X-RateLimit-Reset**: Time when the rate limit resets (ISO 8601 format)
- **X-RateLimit-Client**: Client identifier for the request

### Example Headers
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 955
X-RateLimit-Reset: 2024-01-15T10:30:00Z
X-RateLimit-Client: ip:192.168.1.100
```

## 🚫 Rate Limit Exceeded Response

When a rate limit is exceeded, the API returns:

**Status Code:** 429 (Too Many Requests)

**Headers:**
```
X-RateLimit-Limit: 1000
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 2024-01-15T10:30:00Z
Retry-After: 1800
```

**Response Body:**
```json
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Limit: 1000 requests per 3600 seconds.",
  "retryAfter": 1800,
  "limit": 1000,
  "remaining": 0,
  "reset": "2024-01-15T10:30:00Z"
}
```

## 🔐 Admin Bypass

### Bypass Conditions
Rate limiting can be bypassed for:

1. **Admin Roles**: Users with Administrator, Admin, or SystemAdmin roles
2. **Admin User IDs**: Specific user IDs configured in settings
3. **Admin IP Addresses**: Requests from configured IP addresses (localhost by default)

### Bypass Statistics
Bypassed requests are tracked separately and included in statistics but do not count against rate limits.

## 📈 Monitoring and Analytics

### Real-time Statistics
- Total requests processed
- Requests rate limited
- Requests bypassed
- Rate limiting percentage
- Top rate-limited clients
- Top rate-limited endpoints

### Logging
The system logs:
- Rate limit exceeded events
- Admin bypass events
- Configuration changes
- System errors

### Health Monitoring
- System status checks
- Configuration validation
- Service availability

## 🛠️ Implementation Details

### Files Modified

1. **CSETWebCore.Api.csproj** - Added AspNetCoreRateLimit package
2. **Models/RateLimiting/RateLimitConfiguration.cs** - Configuration models
3. **Interfaces/IRateLimitService.cs** - Service interface
4. **Services/RateLimitService.cs** - Core implementation
5. **Middleware/RateLimitMiddleware.cs** - Request processing middleware
6. **Controllers/RateLimitController.cs** - Management API endpoints
7. **Startup.cs** - Service registration and middleware configuration
8. **appsettings.json** - Configuration settings

### Dependencies
- **AspNetCoreRateLimit**: Advanced rate limiting features
- **Microsoft.Extensions.Caching.Distributed**: Distributed caching support
- **Newtonsoft.Json**: JSON serialization for cached data

## 🚀 Deployment Considerations

### Standalone Deployment
- Uses in-memory cache for rate limiting data
- Suitable for single-server deployments
- No additional infrastructure required

### Enterprise Deployment
- Can use Redis for distributed rate limiting
- Supports multiple server instances
- Provides centralized monitoring and management

### Performance Impact
- Minimal overhead (typically <1ms per request)
- Efficient caching strategies
- Graceful degradation on failures

## 🔧 Troubleshooting

### Common Issues

1. **Rate Limiting Not Working**
   - Check if rate limiting is enabled in configuration
   - Verify middleware is registered in Startup.cs
   - Check logs for configuration errors

2. **Admin Bypass Not Working**
   - Verify user roles are correctly assigned
   - Check admin IP addresses configuration
   - Ensure authentication is working properly

3. **High Memory Usage**
   - Monitor cache size and expiration settings
   - Consider using Redis for distributed deployments
   - Review rate limit periods and request counts

4. **Performance Issues**
   - Check cache performance and configuration
   - Monitor rate limiting overhead
   - Consider adjusting rate limit periods

### Debugging

Enable debug logging to troubleshoot issues:

```json
{
  "Logging": {
    "LogLevel": {
      "CSETWeb_ApiCore.Services.RateLimitService": "Debug",
      "CSETWeb_ApiCore.Middleware.RateLimitMiddleware": "Debug"
    }
  }
}
```

## 📚 Best Practices

### Configuration
1. **Start Conservative**: Begin with higher limits and adjust based on usage
2. **Monitor Usage**: Regularly review statistics and adjust limits
3. **Exclude Critical Endpoints**: Don't rate limit health checks and documentation
4. **Use Admin Bypass**: Configure bypass for legitimate administrative access

### Monitoring
1. **Track Statistics**: Monitor rate limiting statistics regularly
2. **Set Alerts**: Configure alerts for unusual rate limiting patterns
3. **Review Logs**: Check logs for rate limiting issues
4. **Performance Monitoring**: Monitor the impact on API performance

### Security
1. **Admin Bypass**: Use sparingly and monitor bypass usage
2. **IP Whitelisting**: Carefully manage admin IP addresses
3. **Role-based Access**: Use roles for admin bypass rather than user IDs
4. **Regular Review**: Periodically review and update configurations

## 🔄 Future Enhancements

### Planned Features
1. **Dynamic Rate Limiting**: Adjust limits based on server load
2. **Geographic Rate Limiting**: Different limits by geographic region
3. **Advanced Analytics**: More detailed reporting and trend analysis
4. **Rate Limit Templates**: Predefined configurations for different scenarios
5. **Integration with Monitoring**: Integration with Application Insights and other monitoring tools

### Customization Options
1. **Custom Rate Limit Strategies**: Implement custom rate limiting algorithms
2. **External Rate Limit Providers**: Support for external rate limiting services
3. **Rate Limit APIs**: RESTful APIs for dynamic rate limit management
4. **Webhook Notifications**: Notifications for rate limit events

## 📞 Support

For issues or questions related to rate limiting:

1. **Check Logs**: Review application logs for error messages
2. **Verify Configuration**: Ensure configuration is correct
3. **Test Endpoints**: Use the health and statistics endpoints
4. **Review Documentation**: Check this guide and API documentation

## 📄 License

This rate limiting implementation is part of the CSET project and follows the same licensing terms as the main application.

---

**Document Version**: 1.0  
**Last Updated**: January 2024  
**Next Review**: February 2024 