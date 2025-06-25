# CSET API Documentation Template

## Overview
This template provides comprehensive documentation for CSET API endpoints, including usage examples, error handling, authentication, rate limiting, and webhook integration.

## Table of Contents
1. [Authentication](#authentication)
2. [Rate Limiting](#rate-limiting)
3. [Error Handling](#error-handling)
4. [Webhook Integration](#webhook-integration)
5. [Usage Examples](#usage-examples)
6. [Response Formats](#response-formats)
7. [Best Practices](#best-practices)

---

## Authentication

### JWT Bearer Token Authentication
CSET API uses JWT (JSON Web Token) Bearer token authentication for secure access.

#### Obtaining a Token
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-31T23:59:59Z",
  "user": {
    "id": 1,
    "username": "your_username",
    "email": "user@example.com",
    "roles": ["User"]
  }
}
```

#### Using the Token
Include the token in the Authorization header:
```http
GET /api/assessment
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Token Refresh
```http
POST /api/auth/refresh
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Multi-Factor Authentication (MFA)
For enhanced security, MFA can be enabled on user accounts.

#### Setup MFA
```http
POST /api/mfa/setup
Authorization: Bearer {token}
Content-Type: application/json

{
  "mfaType": "TOTP"
}
```

#### Verify MFA Code
```http
POST /api/mfa/verify
Authorization: Bearer {token}
Content-Type: application/json

{
  "mfaType": "TOTP",
  "code": "123456"
}
```

---

## Rate Limiting

### Rate Limit Headers
All API responses include rate limiting headers:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

### Rate Limit Tiers
- **Default Users**: 100 requests per hour
- **Admin Users**: 1000 requests per hour
- **API Clients**: 500 requests per hour

### Rate Limit Exceeded Response
```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json
Retry-After: 3600

{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 3600,
  "limit": 100,
  "remaining": 0,
  "reset": "2024-01-01T00:00:00Z"
}
```

### Bypassing Rate Limits
Admin users and privileged IPs can bypass rate limiting:
```http
X-RateLimit-Bypass: true
```

---

## Error Handling

### Standard Error Response Format
All API errors follow a consistent format:
```json
{
  "error": "Error Type",
  "message": "Human-readable error message",
  "correlationId": "abc123-def456-ghi789",
  "timestamp": "2024-01-01T12:00:00Z",
  "details": {
    "field": "additional error details"
  },
  "recovery": {
    "suggestion": "Suggested recovery action",
    "documentation": "https://docs.cset.gov/api/errors/validation"
  }
}
```

### Common HTTP Status Codes

#### 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "Invalid input data",
  "details": {
    "assessmentName": "Assessment name is required",
    "description": "Description must be less than 500 characters"
  }
}
```

#### 401 Unauthorized
```json
{
  "error": "AuthenticationError",
  "message": "Invalid or expired token",
  "recovery": {
    "suggestion": "Please re-authenticate using /api/auth/login"
  }
}
```

#### 403 Forbidden
```json
{
  "error": "AuthorizationError",
  "message": "Insufficient permissions",
  "details": {
    "requiredRole": "Admin",
    "userRole": "User"
  }
}
```

#### 404 Not Found
```json
{
  "error": "NotFoundError",
  "message": "Assessment with ID 123 not found",
  "correlationId": "abc123-def456-ghi789"
}
```

#### 409 Conflict
```json
{
  "error": "ConflictError",
  "message": "Assessment has been modified by another user",
  "details": {
    "lastModified": "2024-01-01T12:00:00Z",
    "modifiedBy": "other_user"
  }
}
```

#### 429 Too Many Requests
```json
{
  "error": "RateLimitError",
  "message": "Rate limit exceeded",
  "retryAfter": 3600
}
```

#### 500 Internal Server Error
```json
{
  "error": "InternalServerError",
  "message": "An unexpected error occurred",
  "correlationId": "abc123-def456-ghi789",
  "recovery": {
    "suggestion": "Please try again later or contact support"
  }
}
```

### Error Recovery Strategies

#### Retry Logic
For transient errors (5xx), implement exponential backoff:
```javascript
const retryWithBackoff = async (fn, maxRetries = 3) => {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      if (error.status < 500 || i === maxRetries - 1) {
        throw error;
      }
      await new Promise(resolve => 
        setTimeout(resolve, Math.pow(2, i) * 1000)
      );
    }
  }
};
```

#### Conflict Resolution
For 409 conflicts, implement conflict resolution:
```javascript
const handleConflict = async (assessmentId, data) => {
  try {
    return await updateAssessment(assessmentId, data);
  } catch (error) {
    if (error.status === 409) {
      // Get latest version and merge changes
      const latest = await getAssessment(assessmentId);
      const merged = mergeChanges(latest, data);
      return await updateAssessment(assessmentId, merged);
    }
    throw error;
  }
};
```

---

## Webhook Integration

### Webhook Configuration
Configure webhooks to receive real-time notifications:

#### Register Webhook
```http
POST /api/webhooks
Authorization: Bearer {token}
Content-Type: application/json

{
  "url": "https://your-app.com/webhooks/cset",
  "events": ["assessment.created", "assessment.updated", "report.generated"],
  "secret": "your-webhook-secret",
  "description": "Assessment notifications"
}
```

#### Webhook Events
Available webhook events:
- `assessment.created` - New assessment created
- `assessment.updated` - Assessment modified
- `assessment.deleted` - Assessment deleted
- `report.generated` - Report generation completed
- `user.login` - User login event
- `security.alert` - Security alert triggered

#### Webhook Payload Format
```json
{
  "event": "assessment.created",
  "timestamp": "2024-01-01T12:00:00Z",
  "data": {
    "assessmentId": 123,
    "assessmentName": "Security Assessment 2024",
    "createdBy": "user@example.com",
    "createdAt": "2024-01-01T12:00:00Z"
  },
  "signature": "sha256=abc123..."
}
```

#### Webhook Verification
Verify webhook authenticity using the signature:
```javascript
const verifyWebhook = (payload, signature, secret) => {
  const expectedSignature = crypto
    .createHmac('sha256', secret)
    .update(JSON.stringify(payload))
    .digest('hex');
  
  return crypto.timingSafeEqual(
    Buffer.from(signature.replace('sha256=', '')),
    Buffer.from(expectedSignature)
  );
};
```

#### Webhook Retry Logic
Webhooks are retried with exponential backoff:
- 1st retry: 1 minute
- 2nd retry: 5 minutes
- 3rd retry: 15 minutes
- 4th retry: 1 hour
- 5th retry: 4 hours

---

## Usage Examples

### Assessment Management

#### Create Assessment
```http
POST /api/assessment
Authorization: Bearer {token}
Content-Type: application/json

{
  "assessmentName": "Annual Security Review 2024",
  "description": "Comprehensive security assessment for Q1 2024",
  "framework": "NIST_CSF",
  "maturityModel": "CMMC_2_0",
  "facilityName": "Main Data Center",
  "facilityType": "DataCenter"
}
```

#### Update Assessment
```http
PUT /api/assessment/123
Authorization: Bearer {token}
Content-Type: application/json

{
  "assessmentName": "Updated Security Review 2024",
  "description": "Updated description with new requirements"
}
```

#### Get Assessment with Questions
```http
GET /api/assessment/123/questions?category=AC&maturityLevel=3
Authorization: Bearer {token}
```

#### Bulk Export Assessments
```http
POST /api/assessment/export/bulk
Authorization: Bearer {token}
Content-Type: application/json

{
  "assessmentIds": [123, 124, 125],
  "format": "JSON",
  "includeAttachments": true,
  "encrypt": true,
  "password": "secure_password"
}
```

### Real-time Collaboration

#### Join Assessment Session
```http
POST /api/collaboration/assessment/123/join
Authorization: Bearer {token}
Content-Type: application/json

{
  "userName": "John Doe",
  "role": "Assessor"
}
```

#### Send Collaboration Message
```http
POST /api/collaboration/assessment/123/message
Authorization: Bearer {token}
Content-Type: application/json

{
  "message": "Please review the access control questions",
  "type": "Comment",
  "questionId": 456
}
```

### Machine Learning Operations

#### Train ML Model
```http
POST /api/ml/models/train
Authorization: Bearer {token}
Content-Type: application/json

{
  "modelType": "RiskPrediction",
  "trainingData": {
    "startDate": "2023-01-01",
    "endDate": "2023-12-31",
    "frameworks": ["NIST_CSF", "CMMC_2_0"]
  },
  "parameters": {
    "algorithm": "RandomForest",
    "maxDepth": 10,
    "nEstimators": 100
  }
}
```

#### Get Predictions
```http
POST /api/ml/predict
Authorization: Bearer {token}
Content-Type: application/json

{
  "assessmentId": 123,
  "modelId": "risk-prediction-v1",
  "features": {
    "questionResponses": {...},
    "facilityType": "DataCenter",
    "industry": "Financial"
  }
}
```

### Performance Monitoring

#### Get Performance Metrics
```http
GET /api/performance/metrics?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer {token}
```

#### Get Health Status
```http
GET /api/health
Authorization: Bearer {token}
```

---

## Response Formats

### Pagination
List endpoints support pagination:
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

### Filtering and Sorting
```http
GET /api/assessment?page=1&pageSize=20&sortBy=createdAt&sortOrder=desc&filter=framework:NIST_CSF
```

### Bulk Operations
```json
{
  "results": [
    {
      "id": 123,
      "status": "success",
      "message": "Assessment created successfully"
    },
    {
      "id": 124,
      "status": "error",
      "message": "Validation failed",
      "details": {...}
    }
  ],
  "summary": {
    "total": 2,
    "successful": 1,
    "failed": 1
  }
}
```

---

## Best Practices

### Authentication
- Always use HTTPS in production
- Store tokens securely (not in localStorage for web apps)
- Implement token refresh logic
- Use MFA for sensitive operations

### Rate Limiting
- Implement exponential backoff for retries
- Cache responses when appropriate
- Use bulk endpoints for multiple operations
- Monitor rate limit headers

### Error Handling
- Always check HTTP status codes
- Implement proper retry logic for 5xx errors
- Log correlation IDs for debugging
- Provide user-friendly error messages

### Performance
- Use pagination for large datasets
- Implement client-side caching
- Use compression for large responses
- Monitor API response times

### Security
- Validate all input data
- Use parameterized queries
- Implement proper CORS policies
- Log security events

### Webhooks
- Verify webhook signatures
- Implement idempotency
- Handle webhook failures gracefully
- Use HTTPS for webhook URLs

---

## SDKs and Libraries

### JavaScript/TypeScript
```bash
npm install @cset/api-client
```

```javascript
import { CSETClient } from '@cset/api-client';

const client = new CSETClient({
  baseUrl: 'https://api.cset.gov',
  token: 'your-jwt-token'
});

const assessment = await client.assessments.create({
  assessmentName: 'My Assessment',
  framework: 'NIST_CSF'
});
```

### Python
```bash
pip install cset-api-client
```

```python
from cset_api import CSETClient

client = CSETClient(
    base_url='https://api.cset.gov',
    token='your-jwt-token'
)

assessment = client.assessments.create(
    assessment_name='My Assessment',
    framework='NIST_CSF'
)
```

### .NET
```bash
dotnet add package CSET.ApiClient
```

```csharp
using CSET.ApiClient;

var client = new CSETClient(
    baseUrl: "https://api.cset.gov",
    token: "your-jwt-token"
);

var assessment = await client.Assessments.CreateAsync(new AssessmentCreateRequest
{
    AssessmentName = "My Assessment",
    Framework = "NIST_CSF"
});
```

---

## Support and Resources

### Documentation
- [API Reference](https://docs.cset.gov/api)
- [SDK Documentation](https://docs.cset.gov/sdk)
- [Webhook Guide](https://docs.cset.gov/webhooks)
- [Authentication Guide](https://docs.cset.gov/auth)

### Support
- [API Status](https://status.cset.gov)
- [Support Portal](https://support.cset.gov)
- [Community Forum](https://community.cset.gov)

### Rate Limits
- [Rate Limit Guide](https://docs.cset.gov/rate-limits)
- [Rate Limit Calculator](https://docs.cset.gov/rate-limit-calculator)

### Security
- [Security Best Practices](https://docs.cset.gov/security)
- [Security Contact](mailto:security@cset.gov)
- [Vulnerability Disclosure](https://docs.cset.gov/security/vulnerability-disclosure) 