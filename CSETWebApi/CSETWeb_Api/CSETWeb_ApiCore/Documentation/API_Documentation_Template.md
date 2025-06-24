# CSET API Documentation Template

## Overview
This document provides a template for documenting CSET API endpoints. Use this template to ensure consistent and comprehensive documentation across all controllers.

## Controller Documentation Template

### Class-Level Documentation
```csharp
/// <summary>
/// Provides endpoints for [specific functionality] in CSET.
/// [Brief description of what this controller handles]
/// </summary>
[ApiController]
[CsetAuthorize] // If authentication required
public class [ControllerName]Controller : ControllerBase
```

### Constructor Documentation
```csharp
/// <summary>
/// Initializes a new instance of the [ControllerName]Controller.
/// </summary>
/// <param name="service1">Description of service 1</param>
/// <param name="service2">Description of service 2</param>
public [ControllerName]Controller(IService1 service1, IService2 service2)
```

### Method Documentation Template

#### GET Endpoints
```csharp
/// <summary>
/// [Brief description of what the endpoint does]
/// </summary>
/// <param name="param1">Description of parameter 1</param>
/// <param name="param2">Description of parameter 2 (optional)</param>
/// <returns>
/// 200 OK with [return type] if successful
/// 400 Bad Request if [specific error condition]
/// 401 Unauthorized if user is not authenticated
/// 404 Not Found if [resource not found]
/// </returns>
/// <remarks>
/// [Detailed description of the endpoint functionality]
/// 
/// [Usage examples and scenarios]
/// 
/// Sample request:
///     GET /api/[endpoint]?param1=value1&param2=value2
/// 
/// [Additional notes about behavior, side effects, etc.]
/// </remarks>
[HttpGet]
[Route("api/[endpoint]")]
[ProducesResponseType(typeof([ReturnType]), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(401)]
[ProducesResponseType(404)]
public IActionResult [MethodName]([parameters])
```

#### POST Endpoints
```csharp
/// <summary>
/// [Brief description of what the endpoint does]
/// </summary>
/// <param name="model">The [model type] containing the data to [action]</param>
/// <returns>
/// 200 OK with [return type] if successful
/// 201 Created with [return type] if resource created
/// 400 Bad Request if [validation errors or invalid data]
/// 401 Unauthorized if user is not authenticated
/// 409 Conflict if [conflict condition]
/// </returns>
/// <remarks>
/// [Detailed description of the endpoint functionality]
/// 
/// [Usage examples and scenarios]
/// 
/// Sample request:
///     POST /api/[endpoint]
///     {
///         "property1": "value1",
///         "property2": "value2"
///     }
/// 
/// [Additional notes about behavior, side effects, etc.]
/// </remarks>
[HttpPost]
[Route("api/[endpoint]")]
[ProducesResponseType(typeof([ReturnType]), 200)]
[ProducesResponseType(typeof([ReturnType]), 201)]
[ProducesResponseType(typeof([ErrorType]), 400)]
[ProducesResponseType(401)]
[ProducesResponseType(409)]
public IActionResult [MethodName]([FromBody] [ModelType] model)
```

#### PUT Endpoints
```csharp
/// <summary>
/// [Brief description of what the endpoint does]
/// </summary>
/// <param name="id">The unique identifier of the resource to update</param>
/// <param name="model">The [model type] containing the updated data</param>
/// <returns>
/// 200 OK with [return type] if successful
/// 400 Bad Request if [validation errors or invalid data]
/// 401 Unauthorized if user is not authenticated
/// 404 Not Found if resource not found
/// </returns>
/// <remarks>
/// [Detailed description of the endpoint functionality]
/// 
/// [Usage examples and scenarios]
/// 
/// Sample request:
///     PUT /api/[endpoint]/123
///     {
///         "property1": "updated value1",
///         "property2": "updated value2"
///     }
/// 
/// [Additional notes about behavior, side effects, etc.]
/// </remarks>
[HttpPut]
[Route("api/[endpoint]/{id}")]
[ProducesResponseType(typeof([ReturnType]), 200)]
[ProducesResponseType(typeof([ErrorType]), 400)]
[ProducesResponseType(401)]
[ProducesResponseType(404)]
public IActionResult [MethodName](int id, [FromBody] [ModelType] model)
```

#### DELETE Endpoints
```csharp
/// <summary>
/// [Brief description of what the endpoint does]
/// </summary>
/// <param name="id">The unique identifier of the resource to delete</param>
/// <returns>
/// 200 OK if successful
/// 204 No Content if successful (no response body)
/// 401 Unauthorized if user is not authenticated
/// 404 Not Found if resource not found
/// </returns>
/// <remarks>
/// [Detailed description of the endpoint functionality]
/// 
/// [Usage examples and scenarios]
/// 
/// Sample request:
///     DELETE /api/[endpoint]/123
/// 
/// [Additional notes about behavior, side effects, etc.]
/// </remarks>
[HttpDelete]
[Route("api/[endpoint]/{id}")]
[ProducesResponseType(200)]
[ProducesResponseType(204)]
[ProducesResponseType(401)]
[ProducesResponseType(404)]
public IActionResult [MethodName](int id)
```

## Common Response Types

### Success Responses
- **200 OK**: Request successful, returns data
- **201 Created**: Resource created successfully
- **204 No Content**: Request successful, no response body

### Error Responses
- **400 Bad Request**: Invalid request data or validation errors
- **401 Unauthorized**: Authentication required or failed
- **403 Forbidden**: Authenticated but not authorized
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflict (e.g., duplicate data)
- **500 Internal Server Error**: Server error

## Authentication and Authorization

### JWT Token Authentication
Most CSET API endpoints require JWT token authentication. Include the token in the Authorization header:

```
Authorization: Bearer <jwt_token>
```

### Standalone vs Enterprise
- **Standalone**: Local installation, simplified authentication
- **Enterprise**: Multi-user installation, full authentication required

## Common Parameters

### Query Parameters
- `assessmentId`: Assessment identifier (integer)
- `galleryGuid`: Gallery item identifier (GUID)
- `workflow`: Workflow type (string)
- `csn`: Custom Set Name (string, optional)

### Headers
- `Authorization`: JWT Bearer token
- `assessmentid`: Assessment ID for context
- `aggregationid`: Aggregation ID for context

## Model Documentation

### Request Models
Document all properties of request models:

```csharp
/// <summary>
/// Represents a [model purpose]
/// </summary>
public class [ModelName]
{
    /// <summary>
    /// Gets or sets the [property description]
    /// </summary>
    /// <example>Example value</example>
    public string PropertyName { get; set; }
    
    /// <summary>
    /// Gets or sets the [property description]
    /// </summary>
    /// <example>123</example>
    public int NumericProperty { get; set; }
}
```

### Response Models
Document all properties of response models:

```csharp
/// <summary>
/// Represents the response from [endpoint]
/// </summary>
public class [ResponseModelName]
{
    /// <summary>
    /// Gets or sets the [property description]
    /// </summary>
    public string PropertyName { get; set; }
    
    /// <summary>
    /// Gets or sets the [property description]
    /// </summary>
    public List<SubModel> Items { get; set; }
}
```

## Best Practices

### Documentation Standards
1. **Be Clear and Concise**: Use simple, clear language
2. **Include Examples**: Provide sample requests and responses
3. **Document All Parameters**: Explain what each parameter does
4. **List All Response Codes**: Document all possible HTTP status codes
5. **Include Usage Notes**: Explain when and how to use the endpoint

### Code Organization
1. **Group Related Endpoints**: Use consistent naming and organization
2. **Use Meaningful Names**: Choose descriptive method and parameter names
3. **Follow REST Conventions**: Use appropriate HTTP methods and status codes
4. **Validate Input**: Include proper validation and error handling

### Security Considerations
1. **Document Authentication**: Clearly indicate which endpoints require authentication
2. **Explain Authorization**: Document any role-based access requirements
3. **Security Headers**: Document any required security headers
4. **Rate Limiting**: Note any rate limiting restrictions

## Example Implementation

See the `AuthController.cs` and `AssessmentController.cs` files for examples of properly documented endpoints following this template.

## Maintenance

- Update documentation when endpoints change
- Review documentation for accuracy regularly
- Ensure examples are current and working
- Keep security information up to date 