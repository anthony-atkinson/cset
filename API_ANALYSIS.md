# CSET API Analysis for Blazor Migration

## Project Overview
This document analyzes the current CSET Web API structure to ensure compatibility with the Blazor Server migration. The API is built on .NET 7+ with Entity Framework Core and provides RESTful endpoints for the CSET application.

## Current API Architecture

### Technology Stack
- **Framework**: ASP.NET Core 7+ Web API
- **Database**: Microsoft SQL Server 2022 with Entity Framework Core
- **Authentication**: JWT tokens with custom authorization
- **Real-time**: SignalR for real-time communication
- **Documentation**: Swagger/OpenAPI

### Project Structure
```
CSETWeb_Api/
├── CSETWeb_ApiCore/           # Main API controllers and configuration
├── CSETWebCore.Business/      # Business logic layer
├── CSETWebCore.DataLayer/     # Data access layer
├── CSETWebCore.Model/         # Entity models and DTOs
├── CSETWebCore.Interfaces/    # Interface definitions
└── CSETWebCore.Helpers/       # Utility classes
```

## API Controllers Analysis

### Authentication & Authorization
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `AuthController` | User authentication and JWT management | `/api/auth/login`, `/api/auth/token`, `/api/auth/islocal` | Critical |
| `UserController` | User management and profiles | User CRUD operations | High |
| `MfaController` | Multi-factor authentication | MFA setup and verification | High |

### Assessment Management
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `AssessmentController` | Core assessment operations | `/api/createassessment/gallery`, `/api/assessmentsforuser` | Critical |
| `QuestionsController` | Question management and answers | Question CRUD, answer submission | Critical |
| `DemographicsController` | Assessment demographics | Demographics CRUD | High |

### Analytics & Reporting
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `AnalyticsController` | Basic analytics | Analytics data retrieval | High |
| `AdvancedAnalyticsController` | Advanced analytics | Complex analytics queries | Medium |
| `ReportsController` | Report generation | PDF/Excel report generation | High |
| `AggregationController` | Assessment aggregation | Multi-assessment analysis | Medium |

### Standards & Frameworks
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `StandardsController` | Standards management | Standards CRUD | High |
| `FrameworkController` | Framework operations | Framework management | Medium |
| `SetsController` | Question sets | Set management | High |

### File Operations
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `FileUploadController` | File uploads | File upload handling | Medium |
| `FileDownloadController` | File downloads | File download handling | Medium |
| `AssessmentImportController` | Assessment import | Import functionality | Medium |
| `AssessmentExportController` | Assessment export | Export functionality | Medium |

### Real-time Communication
| Controller | Purpose | Key Endpoints | Migration Priority |
|------------|---------|---------------|-------------------|
| `CollaborationController` | Real-time collaboration | Collaboration features | Medium |
| SignalR Hubs | Real-time updates | Assessment updates, notifications | High |

## Key API Endpoints

### Authentication Endpoints
```http
POST /api/auth/login                    # Enterprise login
POST /api/auth/login/standalone         # Standalone login
GET  /api/auth/islocal                  # Check installation type
GET  /api/auth/token                    # Issue new JWT token
POST /api/auth/istokenvalid             # Validate token
GET  /api/auth/accesskey                # Get access key
```

### Assessment Endpoints
```http
GET  /api/createassessment/gallery      # Create assessment from gallery
GET  /api/assessmentsforuser            # Get user's assessments
GET  /api/assessmentdetail              # Get assessment details
POST /api/assessmentdetail              # Update assessment details
GET  /api/assessmentdocuments           # Get assessment documents
GET  /api/lastmodified                  # Get last modified date
```

### Question Endpoints
```http
GET  /api/questions                     # Get questions
POST /api/questions                     # Submit answers
GET  /api/questions/requirements        # Get requirements
GET  /api/questions/maturity            # Get maturity questions
```

### Analytics Endpoints
```http
GET  /api/analytics/dashboard           # Get dashboard data
GET  /api/analytics/trends              # Get trend analysis
GET  /api/analytics/comparison          # Get comparison data
```

### Report Endpoints
```http
GET  /api/reports/executive             # Generate executive report
GET  /api/reports/detailed              # Generate detailed report
GET  /api/reports/csv                   # Export to CSV
GET  /api/reports/excel                 # Export to Excel
```

## Data Models

### Core Assessment Model
```csharp
public class AssessmentDetail
{
    public int Id { get; set; }
    public string AssessmentName { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatorId { get; set; }
    public string CreatorName { get; set; }
    public string FacilitatorName { get; set; }
    public bool SelfAssessment { get; set; }
    public DateTime? AssessmentDate { get; set; }
    public string FacilityName { get; set; }
    public string CityOrSiteName { get; set; }
    public string StateProvRegion { get; set; }
    public string PostalCode { get; set; }
    public int? RegionCode { get; set; }
    public string Charter { get; set; }
    public string CreditUnion { get; set; }
    public long Assets { get; set; }
    public string DiagramMarkup { get; set; }
    public string DiagramImage { get; set; }
    public int? SectorId { get; set; }
    public int? IndustryId { get; set; }
    public bool UseStandard { get; set; }
    public bool UseDiagram { get; set; }
    public bool UseMaturity { get; set; }
    public string Workflow { get; set; }
    public Guid? GalleryItemGuid { get; set; }
    public Guid AssessmentGuid { get; set; }
    public string Origin { get; set; }
    public bool AssessorMode { get; set; }
    public MaturityModel MaturityModel { get; set; }
    public List<string> Standards { get; set; }
    public string ApplicationMode { get; set; }
    public QuestionRequirementCounts QuestionRequirementCounts { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string AdditionalNotesAndComments { get; set; }
    public string AssessmentDescription { get; set; }
    public string ExecutiveSummary { get; set; }
    public string TypeTitle { get; set; }
    public string TypeDescription { get; set; }
    public string PciiNumber { get; set; }
}
```

### Authentication Models
```csharp
public class Login
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; }
    public UserInfo User { get; set; }
    public bool IsPasswordExpired { get; set; }
    public string LinkerTime { get; set; }
}

public class UserInfo
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<string> Roles { get; set; }
}
```

## Authentication & Authorization

### JWT Token Structure
The API uses JWT tokens with the following claims:
- `UserId`: Current user ID
- `AssessmentId`: Current assessment context
- `AccessKey`: Access key for anonymous access
- `Expiration`: Token expiration time

### Authorization Attributes
- `[CsetAuthorize]`: Custom authorization attribute for CSET-specific logic
- `[AllowAnonymous]`: Allows anonymous access to specific endpoints
- Role-based authorization for admin functions

### Security Features
- JWT token validation
- Password expiration handling
- Multi-factor authentication support
- Access key authentication for anonymous users
- Rate limiting on sensitive endpoints

## Database Integration

### Entity Framework Context
```csharp
public class CSETContext : DbContext
{
    // Assessment-related entities
    public DbSet<ASSESSMENTS> ASSESSMENTS { get; set; }
    public DbSet<ASSESSMENT_CONTACTS> ASSESSMENT_CONTACTS { get; set; }
    public DbSet<DEMOGRAPHICS> DEMOGRAPHICS { get; set; }
    
    // Question-related entities
    public DbSet<NEW_QUESTION> NEW_QUESTION { get; set; }
    public DbSet<ANSWER> ANSWER { get; set; }
    public DbSet<REQUIREMENT_LEVELS> REQUIREMENT_LEVELS { get; set; }
    
    // Standards and frameworks
    public DbSet<STANDARD_SELECTION> STANDARD_SELECTION { get; set; }
    public DbSet<MATURITY_MODELS> MATURITY_MODELS { get; set; }
    
    // User management
    public DbSet<USERS> USERS { get; set; }
    public DbSet<USER_DETAIL_INFORMATION> USER_DETAIL_INFORMATION { get; set; }
}
```

### Key Database Tables
- `ASSESSMENTS`: Core assessment data
- `DEMOGRAPHICS`: Assessment demographics
- `NEW_QUESTION`: Question definitions
- `ANSWER`: User answers to questions
- `STANDARD_SELECTION`: Selected standards for assessments
- `MATURITY_MODELS`: Maturity model definitions
- `USERS`: User accounts
- `ASSESSMENT_CONTACTS`: Assessment contact information

## SignalR Integration

### Hubs
- `CSETHub`: Main hub for real-time communication
- Assessment updates
- Progress notifications
- Chart data updates
- User presence

### Real-time Features
- Assessment collaboration
- Live progress updates
- Real-time chart data
- User notifications
- Assessment state synchronization

## Migration Strategy

### Phase 1: Core API Compatibility
1. **Ensure API endpoints work with Blazor Server**
   - All current endpoints should work without changes
   - Update CORS configuration for Blazor Server
   - Verify authentication flow compatibility

2. **Update authentication for Blazor Server**
   - JWT tokens work with Blazor Server
   - SignalR authentication integration
   - User session management

3. **Database compatibility**
   - Entity Framework context works with Blazor Server
   - Database connection string configuration
   - Migration scripts compatibility

### Phase 2: Service Layer Integration
1. **Create Blazor services for API communication**
   - HTTP client services for each controller
   - SignalR client integration
   - Error handling and retry logic

2. **Data model mapping**
   - Convert API models to Blazor models
   - AutoMapper configuration
   - Validation integration

### Phase 3: Real-time Features
1. **SignalR integration**
   - Hub connection management
   - Real-time update handling
   - Connection state management

2. **Collaboration features**
   - Real-time assessment updates
   - User presence indicators
   - Progress synchronization

## Compatibility Assessment

### ✅ Compatible Features
- **RESTful API endpoints**: All endpoints work with Blazor Server
- **JWT authentication**: Compatible with ASP.NET Core Identity
- **Entity Framework**: Works seamlessly with Blazor Server
- **SignalR**: Built-in support in Blazor Server
- **File operations**: Standard .NET file handling
- **Database operations**: Direct compatibility

### ⚠️ Areas Requiring Attention
- **CORS configuration**: May need updates for Blazor Server
- **Session management**: Blazor Server uses different session model
- **Real-time connection limits**: Blazor Server has connection limits
- **Performance**: Server-side rendering may affect API performance

### 🔧 Required Changes
1. **Update CORS policy** for Blazor Server
2. **Configure SignalR** for Blazor Server integration
3. **Update authentication middleware** for Blazor Server
4. **Optimize database queries** for server-side rendering
5. **Implement caching** for better performance

## Success Criteria

### Technical Criteria
- [ ] All API endpoints accessible from Blazor Server
- [ ] Authentication working correctly
- [ ] Real-time features functional
- [ ] Database operations working
- [ ] File upload/download working
- [ ] Performance acceptable

### Functional Criteria
- [ ] Assessment creation and management
- [ ] Question answering and validation
- [ ] Report generation and export
- [ ] Analytics and dashboard
- [ ] User management and authentication
- [ ] Real-time collaboration

## Next Steps

1. **Verify API compatibility** with Blazor Server
2. **Create service layer** for API communication
3. **Implement authentication** integration
4. **Set up SignalR** for real-time features
5. **Test all endpoints** from Blazor Server
6. **Optimize performance** and caching

This analysis confirms that the current API architecture is compatible with Blazor Server migration, requiring minimal changes to the backend while providing a solid foundation for the frontend migration. 