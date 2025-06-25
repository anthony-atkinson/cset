# Enhanced Error Handling Guide

## Overview

The Enhanced Error Handling system provides comprehensive error management for the CSET application, including structured logging, error analytics, recovery suggestions, and correlation tracking. This system improves debugging capabilities, user experience, and operational monitoring.

## Features

### 🔍 **Correlation ID Tracking**
- Unique correlation IDs for every error
- End-to-end error tracking across requests
- Easy error investigation and debugging

### 📊 **Structured Error Logging**
- Comprehensive error context capture
- Request path, method, user ID, and timestamp
- Integration with NLog and Application Insights

### 🎯 **Error Type Classification**
- Specific error types for different scenarios
- Custom exception classes for business logic
- Intelligent error categorization

### 💡 **Recovery Suggestions**
- Context-aware recovery recommendations
- Automated recovery actions
- User-friendly error messages

### 📈 **Error Analytics**
- Error statistics and trends
- Most common error identification
- Performance impact analysis

## Architecture

### Core Components

#### 1. ErrorDetails Class
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Error/ErrorDetails.cs`

Enhanced error information model with:
- Correlation ID for tracking
- Timestamp and error type
- Recovery suggestions
- Request context (path, method, user ID)
- Development vs production detail handling

#### 2. Custom Exception Types
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Error/CustomExceptions.cs`

Specialized exception classes:
- `CSETException` - Base exception for CSET errors
- `AssessmentException` - Assessment-related errors
- `AuthenticationException` - Auth/authorization errors
- `ValidationException` - Data validation errors
- `BusinessLogicException` - Business rule violations
- `FileOperationException` - File system errors
- `ImportExportException` - Import/export errors

#### 3. Exception Middleware
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Error/ExceptionMiddlewareExtensions.cs`

Enhanced global exception handler with:
- Correlation ID generation
- Structured error logging
- Integration with analytics and recovery services
- Custom exception handling
- Development vs production error detail handling

#### 4. Error Analytics Service
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Services/ErrorAnalyticsService.cs`

Error tracking and analysis:
- Error occurrence tracking
- Statistics generation
- Trend analysis
- Common error identification
- Correlation ID lookup

#### 5. Error Recovery Service
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Services/ErrorRecoveryService.cs`

Intelligent recovery assistance:
- Context-aware recovery suggestions
- Automated recovery actions
- Error type-specific recommendations
- Risk assessment for automated actions

#### 6. Error Analytics Controller
**Location**: `CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore/Controllers/ErrorAnalyticsController.cs`

API endpoints for error monitoring:
- Error statistics retrieval
- Trend analysis
- Common error identification
- Error detail lookup by correlation ID
- Current error summary

## Usage

### Basic Error Handling

#### Throwing Custom Exceptions

```csharp
// Assessment-related error
throw new AssessmentException("Assessment not found", assessmentId: 123);

// Validation error with specific errors
var validationErrors = new Dictionary<string, string[]>
{
    ["email"] = new[] { "Invalid email format" },
    ["password"] = new[] { "Password must be at least 8 characters" }
};
throw new ValidationException("Validation failed", validationErrors);

// Business logic error
throw new BusinessLogicException("Cannot delete assessment with active questions", "AssessmentDeletionRule");

// File operation error
throw new FileOperationException("File not found", filePath: "/path/to/file", operation: "read");
```

#### Error Response Format

```json
{
  "statusCode": 400,
  "message": "Invalid request parameters provided.",
  "correlationId": "550e8400-e29b-41d4-a716-446655440000",
  "timestamp": "2024-01-15T10:30:00Z",
  "errorType": "ValidationError",
  "details": "System.ArgumentException: Invalid parameter...",
  "recoverySuggestions": [
    "Check the request parameters for validity",
    "Ensure all required fields are provided",
    "Verify data formats match expected types"
  ],
  "requestPath": "/api/assessment",
  "requestMethod": "POST",
  "userId": "12345"
}
```

### Error Analytics API

#### Get Error Statistics

```http
GET /api/erroranalytics/statistics?startDate=2024-01-01T00:00:00Z&endDate=2024-01-15T23:59:59Z
Authorization: Bearer <token>
```

Response:
```json
{
  "totalErrors": 150,
  "errorsByType": {
    "ValidationError": 45,
    "AuthenticationError": 12,
    "DatabaseError": 8,
    "BusinessLogicError": 25
  },
  "errorsByStatusCode": {
    "400": 70,
    "401": 12,
    "500": 68
  },
  "averageResponseTime": "00:00:00.250",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-15T23:59:59Z"
}
```

#### Get Error Trends

```http
GET /api/erroranalytics/trends?startDate=2024-01-01T00:00:00Z&endDate=2024-01-15T23:59:59Z
Authorization: Bearer <token>
```

#### Get Most Common Errors

```http
GET /api/erroranalytics/common?limit=10
Authorization: Bearer <token>
```

#### Get Error Details by Correlation ID

```http
GET /api/erroranalytics/details/550e8400-e29b-41d4-a716-446655440000
Authorization: Bearer <token>
```

#### Get Current Error Summary

```http
GET /api/erroranalytics/summary
Authorization: Bearer <token>
```

## Configuration

### Service Registration

The error handling services are automatically registered in `Startup.cs`:

```csharp
// Error Handling Services
services.AddScoped<Services.IErrorAnalyticsService, Services.ErrorAnalyticsService>();
services.AddScoped<Services.IErrorRecoveryService, Services.ErrorRecoveryService>();
```

### Middleware Configuration

The enhanced exception handler is configured in the request pipeline:

```csharp
app.ConfigureExceptionHandler();
```

### NLog Configuration

Enhanced NLog configuration for structured logging:

```xml
<target xsi:type="Database" name="db" 
        dbProvider="Microsoft.Data.SqlClient"
        connectionString="${configsetting:item=ConnectionStrings.CSET_DB}"
        commandText="INSERT INTO Nlogs ([Date], [Level], [Logger], [Message], [CorrelationId], [ErrorType], [StatusCode], [RequestPath], [UserId]) VALUES (@date, @lvl, @logger, @msg, @correlationId, @errorType, @statusCode, @requestPath, @userId)">
  <parameter name="@date" layout="${date}" dbType="SqlDbType.DateTime" />
  <parameter name="@lvl" layout="${level}" dbType="SqlDbType.VarChar" size="10" />
  <parameter name="@logger" layout="${logger}" dbType="SqlDbType.VarChar" size="255" />
  <parameter name="@msg" layout="${message}" dbType="SqlDbType.VarChar" size="4000" />
  <parameter name="@correlationId" layout="${event-properties:item=CorrelationId}" dbType="SqlDbType.VarChar" size="50" />
  <parameter name="@errorType" layout="${event-properties:item=ErrorType}" dbType="SqlDbType.VarChar" size="50" />
  <parameter name="@statusCode" layout="${event-properties:item=StatusCode}" dbType="SqlDbType.Int" />
  <parameter name="@requestPath" layout="${event-properties:item=RequestPath}" dbType="SqlDbType.VarChar" size="500" />
  <parameter name="@userId" layout="${event-properties:item=UserId}" dbType="SqlDbType.VarChar" size="50" />
</target>
```

## Error Types and Recovery

### ValidationError
**Status Code**: 400
**Recovery Suggestions**:
- Check request parameters for validity
- Ensure all required fields are provided
- Verify data formats match expected types
- Review API documentation for correct parameter usage

### AuthenticationError
**Status Code**: 401
**Recovery Suggestions**:
- Log in with valid credentials
- Check if session has expired
- Verify required permissions
- Contact administrator if access issues persist

### BusinessLogicError
**Status Code**: 400
**Recovery Suggestions**:
- Verify current state allows this operation
- Check if all prerequisites are met
- Review business rules and constraints
- Contact support if issue persists

### DatabaseError
**Status Code**: 500
**Recovery Suggestions**:
- Try again in a few moments
- Check if database is accessible
- Verify database connection settings
- Contact support if issue persists

### FileOperationError
**Status Code**: 500
**Recovery Suggestions**:
- Check if file exists and is accessible
- Verify file permissions
- Ensure sufficient disk space
- Try again in a few moments

### ImportExportError
**Status Code**: 400
**Recovery Suggestions**:
- Verify file format is supported
- Check if file is not corrupted
- Ensure file size is within limits
- Try with different file or format

## Monitoring and Analytics

### Error Dashboard

The error analytics API provides endpoints for monitoring:
- **Statistics**: Overall error metrics and trends
- **Trends**: Error type patterns over time
- **Common Errors**: Most frequent error occurrences
- **Error Details**: Specific error investigation by correlation ID
- **Summary**: Current error activity overview

### Integration with Application Insights

The system integrates with Application Insights for:
- Error tracking and correlation
- Custom metrics and events
- Performance monitoring
- Alert configuration

### Log Analysis

Structured logging provides:
- Correlation ID tracking across requests
- Error type classification
- Request context capture
- User activity correlation

## Best Practices

### Error Handling Guidelines

1. **Use Custom Exceptions**: Throw specific exception types for different error scenarios
2. **Provide Context**: Include relevant information in error messages
3. **Log Appropriately**: Use structured logging with correlation IDs
4. **User-Friendly Messages**: Provide clear, actionable error messages
5. **Recovery Suggestions**: Include helpful recovery guidance

### Development vs Production

- **Development**: Full error details and stack traces
- **Production**: Sanitized error messages without sensitive information
- **Logging**: Comprehensive logging in both environments
- **Analytics**: Full analytics tracking in production

### Security Considerations

- Sanitize error messages in production
- Avoid exposing sensitive information
- Log security-relevant errors appropriately
- Monitor for suspicious error patterns

## Troubleshooting

### Common Issues

1. **Correlation ID Not Found**
   - Check if error was logged properly
   - Verify correlation ID format
   - Check database connectivity

2. **Recovery Suggestions Not Available**
   - Verify error type classification
   - Check error recovery service configuration
   - Review error context capture

3. **Analytics Data Missing**
   - Check Application Insights configuration
   - Verify telemetry service registration
   - Review error tracking implementation

### Debugging

1. **Check Logs**: Use correlation ID to trace error through logs
2. **Review Context**: Examine request path, method, and user information
3. **Analyze Trends**: Use analytics API to identify patterns
4. **Test Recovery**: Verify recovery suggestions and actions

## Future Enhancements

### Planned Features

1. **Real-time Error Alerts**: Configure alerts for specific error types
2. **Error Prediction**: ML-based error prediction and prevention
3. **Automated Recovery**: Enhanced automated recovery actions
4. **Error Reporting**: Scheduled error reports and notifications
5. **Performance Impact**: Error impact on application performance analysis

### Integration Opportunities

1. **External Monitoring**: Integration with external monitoring tools
2. **Slack/Teams Notifications**: Real-time error notifications
3. **Error Escalation**: Automated error escalation workflows
4. **User Feedback**: Error reporting and feedback collection

## Conclusion

The Enhanced Error Handling system provides a comprehensive solution for error management in the CSET application. It improves debugging capabilities, user experience, and operational monitoring while maintaining security and performance standards.

For questions or issues, contact the development team or refer to the API documentation at `/api-docs`. 