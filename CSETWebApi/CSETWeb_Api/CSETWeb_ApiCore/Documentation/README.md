# CSET API Documentation

## Overview
The CSET API provides comprehensive endpoints for managing cybersecurity assessments, user authentication, and compliance evaluation. This documentation is automatically generated using Swagger/OpenAPI and provides interactive testing capabilities.

## Accessing the API Documentation

### Development Environment
When running CSET in development mode, the API documentation is available at:
```
http://localhost:[port]/api-docs
```

### Production Environment
The API documentation is now available in all environments at:
```
https://[your-domain]/api-docs
```

## Features

### Interactive Documentation
- **Try It Out**: Test API endpoints directly from the browser
- **Request/Response Examples**: See sample requests and responses
- **Authentication**: Built-in JWT token authentication support
- **Parameter Validation**: Automatic validation of request parameters
- **Response Codes**: Complete list of possible HTTP status codes

### API Information
- **Title**: CSET API
- **Version**: v1
- **Description**: Cyber Security Evaluation Tool (CSET) API for cybersecurity assessments and compliance evaluation
- **Contact**: CISA CSET Team (cset_PMO@cisa.dhs.gov)
- **License**: MIT License

## Authentication

### JWT Token Authentication
Most CSET API endpoints require JWT token authentication. To use the interactive documentation:

1. Click the "Authorize" button at the top of the Swagger UI
2. Enter your JWT token in the format: `Bearer <your_token>`
3. Click "Authorize"
4. You can now test authenticated endpoints

### Getting a Token
To obtain a JWT token, use one of these endpoints:
- `POST /api/auth/login` - Enterprise authentication
- `POST /api/auth/login/standalone` - Standalone authentication
- `POST /api/auth/login/accesskey` - Access key authentication

## API Categories

### Authentication (`AuthController`)
- User login and authentication
- Token management and validation
- Access key generation
- Health check endpoints

### Assessments (`AssessmentController`)
- Assessment creation and management
- Assessment retrieval and updates
- Assessment lifecycle operations
- Document management

### Questions (`QuestionsController`)
- Question retrieval and management
- Answer submission and validation
- Question navigation and filtering

### Reports (`ReportsController`)
- Report generation and export
- Multiple report formats
- Custom report configuration

### Standards (`StandardsController`)
- Standards management
- Framework configuration
- Compliance mapping

### Demographics (`DemographicsController`)
- Demographic information management
- Organization details
- Assessment metadata

## Common Patterns

### Assessment Context
Many endpoints require an assessment context. This is typically provided through:
- Query parameters: `?assessmentId=123`
- Headers: `assessmentid: 123`
- JWT token payload

### Response Formats
- **JSON**: Primary response format for most endpoints
- **XML**: Available for some endpoints
- **File Downloads**: Reports and exports

### Error Handling
All endpoints return appropriate HTTP status codes:
- `200 OK`: Success
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Authentication required
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

## Development

### Adding New Endpoints
When adding new endpoints, follow the documentation template in `API_Documentation_Template.md`:

1. Add XML documentation comments to your controller and methods
2. Include `[ProducesResponseType]` attributes for all possible responses
3. Provide clear parameter descriptions
4. Include usage examples in the `<remarks>` section

### Building Documentation
The API documentation is automatically generated when you build the project. Ensure:
- XML documentation is enabled in the project file
- All public methods have XML comments
- Response types are properly documented

### Testing
Use the Swagger UI to test your endpoints:
1. Navigate to `/api-docs`
2. Find your endpoint
3. Click "Try it out"
4. Fill in the parameters
5. Execute the request
6. Review the response

## Troubleshooting

### Documentation Not Loading
- Ensure the application is running
- Check that Swagger is enabled in the configuration
- Verify XML documentation file is being generated

### Authentication Issues
- Ensure you have a valid JWT token
- Check token expiration
- Verify token format: `Bearer <token>`

### Missing Endpoints
- Ensure controllers are properly decorated with `[ApiController]`
- Check that methods have proper HTTP verb attributes
- Verify XML documentation is complete

## Support

For issues with the API documentation:
- Check the application logs for errors
- Verify Swagger configuration in `Startup.cs`
- Ensure all required packages are installed

For API usage questions:
- Contact the CISA CSET Team
- Review the API documentation template
- Check existing controller examples

## Related Files

- `Startup.cs` - Swagger configuration
- `Swagger/SwaggerDefaultValues.cs` - Custom Swagger filters
- `API_Documentation_Template.md` - Documentation standards
- Individual controller files - Endpoint implementations 