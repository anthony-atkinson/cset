# CSET Developer Documentation

## Overview
This guide provides comprehensive documentation for developers working on the CSET (Cyber Security Evaluation Tool) project, including architecture overview, development environment setup, testing guidelines, and contribution guidelines.

## Table of Contents
1. [Architecture Overview](#architecture-overview)
2. [Development Environment Setup](#development-environment-setup)
3. [Project Structure](#project-structure)
4. [Development Workflow](#development-workflow)
5. [Testing Guidelines](#testing-guidelines)
6. [Code Standards](#code-standards)
7. [API Development](#api-development)
8. [Frontend Development](#frontend-development)
9. [Database Development](#database-development)
10. [Security Guidelines](#security-guidelines)
11. [Performance Guidelines](#performance-guidelines)
12. [Deployment](#deployment)
13. [Troubleshooting](#troubleshooting)

---

## Architecture Overview

### System Architecture
CSET follows a modern, layered architecture pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Frontend (Angular)                       │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Components│ │   Services  │ │   Guards    │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ HTTP/WebSocket
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (.NET Core)                    │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │ Controllers │ │ Middleware  │ │   Filters   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Dependency Injection
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  Business Logic Layer                       │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Services  │ │   Managers  │ │   Helpers   │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Entity Framework
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                   Data Access Layer                         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │  Repositories│ │   Context   │ │   Migrations│           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ SQL Server
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Database Layer                           │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │   Tables    │ │   Views     │ │  Procedures │           │
│  └─────────────┘ └─────────────┘ └─────────────┘           │
└─────────────────────────────────────────────────────────────┘
```

### Key Components

#### Frontend (Angular)
- **Components**: Reusable UI components
- **Services**: Business logic and API communication
- **Guards**: Route protection and authentication
- **Interceptors**: HTTP request/response handling
- **Pipes**: Data transformation

#### Backend (.NET Core)
- **Controllers**: API endpoints and request handling
- **Services**: Business logic implementation
- **Repositories**: Data access abstraction
- **Middleware**: Cross-cutting concerns
- **Filters**: Request/response processing

#### Data Layer
- **Entity Framework**: ORM for database operations
- **Migrations**: Database schema management
- **Stored Procedures**: Complex database operations

### Technology Stack

#### Frontend
- **Framework**: Angular 19
- **Language**: TypeScript 5.8+
- **UI Library**: Angular Material
- **Styling**: SCSS with Bootstrap 5.3+
- **Testing**: Jasmine/Karma, Playwright
- **Build Tool**: Angular CLI

#### Backend
- **Framework**: .NET 7+
- **Language**: C# 11+
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Testing**: MSTest, NUnit, xUnit
- **Documentation**: Swagger/OpenAPI

#### Database
- **Primary**: SQL Server 2022
- **Development**: LocalDB
- **Caching**: Redis 6.0+
- **Migrations**: Entity Framework Migrations

#### DevOps
- **CI/CD**: GitHub Actions
- **Containerization**: Docker
- **Monitoring**: Application Insights
- **Security**: SonarQube, OWASP ZAP

---

## Development Environment Setup

### Prerequisites

#### Required Software
```bash
# .NET 7.0+ SDK
dotnet --version  # Should be 7.0.0 or higher

# Node.js 18+ and npm
node --version    # Should be 18.0.0 or higher
npm --version     # Should be 9.0.0 or higher

# Git
git --version     # Should be 2.30.0 or higher

# SQL Server (LocalDB for development)
sqllocaldb info   # Should show LocalDB instances

# Redis (for caching and real-time features)
redis-server --version  # Should be 6.0.0 or higher
```

#### IDE Requirements
- **Visual Studio 2022** (Windows) or **VS Code** (Cross-platform)
- **SQL Server Management Studio** or **Azure Data Studio**
- **Postman** or **Insomnia** for API testing

### Step 1: Clone Repository
```bash
# Clone the repository
git clone https://github.com/cisagov/cset.git
cd cset

# Checkout development branch
git checkout develop
```

### Step 2: Backend Setup

#### Install Dependencies
```bash
# Navigate to API project
cd CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run database migrations
dotnet ef database update
```

#### Configure Development Settings
```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "CSETWeb": "Server=(localdb)\\mssqllocaldb;Database=CSET_Development;Trusted_Connection=true;MultipleActiveResultSets=true",
    "Redis": "localhost:6379"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Security": {
    "JwtSecret": "development-secret-key-change-in-production",
    "JwtExpirationHours": 24
  }
}
```

### Step 3: Frontend Setup

#### Install Dependencies
```bash
# Navigate to frontend project
cd CSETWebNg

# Install npm packages
npm install

# Install Angular CLI globally (if not already installed)
npm install -g @angular/cli
```

#### Configure Development Settings
```json
// src/environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  signalRUrl: 'https://localhost:5001/hubs',
  enableDebug: true
};
```

### Step 4: Database Setup

#### LocalDB Setup
```bash
# Start LocalDB
sqllocaldb start "MSSQLLocalDB"

# Create development database
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE CSET_Development"
```

#### Seed Data
```bash
# Run seed data script
dotnet run --project CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore --seed-data
```

### Step 5: Redis Setup

#### Windows
```powershell
# Install Redis via Chocolatey
choco install redis-64

# Start Redis service
redis-server
```

#### macOS/Linux
```bash
# Install Redis
sudo apt install redis-server  # Ubuntu/Debian
brew install redis            # macOS

# Start Redis
redis-server
```

### Step 6: Verify Setup

#### Test Backend
```bash
# Run the API
cd CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore
dotnet run

# Test API endpoints
curl https://localhost:5001/api/health
curl https://localhost:5001/api-docs
```

#### Test Frontend
```bash
# Run the frontend
cd CSETWebNg
ng serve

# Open browser
open http://localhost:4200
```

---

## Project Structure

### Backend Structure
```
CSETWebApi/
├── CSETWeb_Api/
│   ├── CSETWeb_ApiCore/           # Main API project
│   │   ├── Controllers/           # API controllers
│   │   ├── Models/                # Data models and DTOs
│   │   ├── Services/              # Business services
│   │   ├── Middleware/            # Custom middleware
│   │   ├── Filters/               # Action filters
│   │   ├── Extensions/            # Extension methods
│   │   ├── Startup.cs             # Application startup
│   │   └── Program.cs             # Entry point
│   ├── CSETWebCore.Business/      # Business logic layer
│   ├── CSETWebCore.DataLayer/     # Data access layer
│   ├── CSETWebCore.Model/         # Entity models
│   ├── CSETWebCore.Interfaces/    # Service interfaces
│   └── CSETWebCore.Helpers/       # Utility classes
├── DatabaseScripts/               # Database scripts
└── Tests/                         # Test projects
```

### Frontend Structure
```
CSETWebNg/
├── src/
│   ├── app/                       # Application code
│   │   ├── components/            # Reusable components
│   │   ├── services/              # Business services
│   │   ├── guards/                # Route guards
│   │   ├── interceptors/          # HTTP interceptors
│   │   ├── pipes/                 # Custom pipes
│   │   └── models/                # TypeScript interfaces
│   ├── assets/                    # Static assets
│   ├── environments/              # Environment configurations
│   └── styles/                    # Global styles
├── e2e/                           # End-to-end tests
└── dist/                          # Build output
```

---

## Development Workflow

### Git Workflow

#### Branch Naming Convention
```
feature/feature-name              # New features
bugfix/bug-description           # Bug fixes
hotfix/critical-fix              # Critical fixes
release/version-number           # Release preparation
```

#### Commit Message Format
```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes
- `refactor`: Code refactoring
- `test`: Test changes
- `chore`: Build/tool changes

**Examples:**
```
feat(auth): add multi-factor authentication support

fix(api): resolve database connection timeout issue

docs(readme): update installation instructions
```

### Development Process

#### 1. Feature Development
```bash
# Create feature branch
git checkout -b feature/new-feature

# Make changes and commit
git add .
git commit -m "feat(api): implement new endpoint"

# Push to remote
git push origin feature/new-feature

# Create pull request
# GitHub will create PR automatically
```

#### 2. Code Review Process
1. **Self-Review**: Review your own code before submitting
2. **Peer Review**: At least one other developer must review
3. **Automated Checks**: CI/CD pipeline must pass
4. **Approval**: Maintainer approval required for merge

#### 3. Testing Requirements
- **Unit Tests**: >80% code coverage
- **Integration Tests**: All API endpoints covered
- **E2E Tests**: Critical user workflows covered
- **Performance Tests**: Load testing for new features

---

## Testing Guidelines

### Backend Testing

#### Unit Testing (MSTest)
```csharp
[TestClass]
public class AssessmentServiceTests
{
    private IAssessmentService _service;
    private Mock<IAssessmentRepository> _mockRepository;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IAssessmentRepository>();
        _service = new AssessmentService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task CreateAssessment_ValidData_ReturnsAssessment()
    {
        // Arrange
        var request = new CreateAssessmentRequest
        {
            AssessmentName = "Test Assessment",
            Framework = "NIST_CSF"
        };

        _mockRepository.Setup(r => r.CreateAsync(It.IsAny<Assessment>()))
            .ReturnsAsync(new Assessment { Id = 1, AssessmentName = "Test Assessment" });

        // Act
        var result = await _service.CreateAssessmentAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Assessment", result.AssessmentName);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<Assessment>()), Times.Once);
    }
}
```

#### Integration Testing
```csharp
[TestClass]
public class AssessmentControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("ConnectionStrings:CSETWeb", 
                    "Server=(localdb)\\mssqllocaldb;Database=CSET_Test;Trusted_Connection=true;");
            });
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetAssessments_ReturnsAssessments()
    {
        // Act
        var response = await _client.GetAsync("/api/assessment");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.IsTrue(content.Contains("assessments"));
    }
}
```

### Frontend Testing

#### Unit Testing (Jasmine/Karma)
```typescript
describe('AssessmentService', () => {
  let service: AssessmentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AssessmentService]
    });
    service = TestBed.inject(AssessmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('should create assessment', () => {
    const assessment = { assessmentName: 'Test', framework: 'NIST_CSF' };
    
    service.createAssessment(assessment).subscribe(result => {
      expect(result.assessmentName).toBe('Test');
    });

    const req = httpMock.expectOne('/api/assessment');
    expect(req.request.method).toBe('POST');
    req.flush({ id: 1, assessmentName: 'Test', framework: 'NIST_CSF' });
  });
});
```

#### E2E Testing (Playwright)
```typescript
import { test, expect } from '@playwright/test';

test('create assessment workflow', async ({ page }) => {
  await page.goto('/assessment');
  
  await page.click('#create-assessment-btn');
  await page.fill('#assessment-name', 'E2E Test Assessment');
  await page.fill('#assessment-description', 'Test description');
  await page.selectOption('#framework-select', 'NIST_CSF');
  
  await page.click('#save-assessment-btn');
  
  await expect(page.locator('.success-message')).toBeVisible();
  await expect(page.locator('.success-message')).toContainText('Assessment created successfully');
});
```

### Performance Testing

#### Load Testing
```csharp
[TestClass]
public class LoadTests
{
    [TestMethod]
    public async Task ConcurrentUsers_AssessmentCreation()
    {
        var tasks = new List<Task>();
        var results = new List<bool>();

        // Simulate 50 concurrent users
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    using var client = new HttpClient();
                    var response = await client.PostAsync("/api/assessment", 
                        new StringContent("{\"assessmentName\":\"Load Test\"}", Encoding.UTF8, "application/json"));
                    results.Add(response.IsSuccessStatusCode);
                }
                catch
                {
                    results.Add(false);
                }
            }));
        }

        await Task.WhenAll(tasks);
        var successRate = results.Count(r => r) / (double)results.Count;
        Assert.IsTrue(successRate > 0.95, "Success rate should be >95%");
    }
}
```

---

## Code Standards

### C# Coding Standards

#### Naming Conventions
```csharp
// Classes and Interfaces
public class AssessmentService { }
public interface IAssessmentService { }

// Methods and Properties
public async Task<Assessment> CreateAssessmentAsync(CreateAssessmentRequest request) { }
public string AssessmentName { get; set; }

// Constants
public const string DefaultFramework = "NIST_CSF";

// Private fields
private readonly IAssessmentRepository _repository;
```

#### Code Organization
```csharp
public class AssessmentService : IAssessmentService
{
    // 1. Private fields
    private readonly IAssessmentRepository _repository;
    private readonly ILogger<AssessmentService> _logger;

    // 2. Constructor
    public AssessmentService(IAssessmentRepository repository, ILogger<AssessmentService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // 3. Public methods
    public async Task<Assessment> CreateAssessmentAsync(CreateAssessmentRequest request)
    {
        // Implementation
    }

    // 4. Private methods
    private async Task ValidateRequestAsync(CreateAssessmentRequest request)
    {
        // Validation logic
    }
}
```

### TypeScript Coding Standards

#### Naming Conventions
```typescript
// Interfaces
interface AssessmentService { }
interface CreateAssessmentRequest { }

// Classes
export class AssessmentComponent { }

// Methods and Properties
async createAssessment(request: CreateAssessmentRequest): Promise<Assessment> { }
assessmentName: string;

// Constants
const DEFAULT_FRAMEWORK = 'NIST_CSF';

// Private fields
private assessmentService: AssessmentService;
```

#### Code Organization
```typescript
@Component({
  selector: 'app-assessment',
  templateUrl: './assessment.component.html'
})
export class AssessmentComponent implements OnInit, OnDestroy {
  // Public properties
  public assessments: Assessment[] = [];
  public loading = false;
  public error: string | null = null;

  // Private properties
  private destroy$ = new Subject<void>();

  // Constructor
  constructor(
    private assessmentService: AssessmentService,
    private messageService: MessageService
  ) { }

  // Lifecycle hooks
  ngOnInit(): void {
    this.loadAssessments();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Public methods
  public async createAssessment(request: CreateAssessmentRequest): Promise<void> {
    try {
      this.loading = true;
      const assessment = await this.assessmentService.createAssessment(request).toPromise();
      this.assessments.push(assessment);
      this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Assessment created' });
    } catch (error) {
      this.error = 'Failed to create assessment';
      this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
    } finally {
      this.loading = false;
    }
  }

  // Private methods
  private async loadAssessments(): Promise<void> {
    try {
      this.loading = true;
      this.assessments = await this.assessmentService.getAssessments().toPromise();
    } catch (error) {
      this.error = 'Failed to load assessments';
    } finally {
      this.loading = false;
    }
  }
}
```

---

## API Development

### Controller Guidelines

#### Standard Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;
    private readonly ILogger<AssessmentController> _logger;

    public AssessmentController(IAssessmentService assessmentService, ILogger<AssessmentController> logger)
    {
        _assessmentService = assessmentService ?? throw new ArgumentNullException(nameof(assessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new assessment
    /// </summary>
    /// <param name="request">The assessment creation request</param>
    /// <returns>The created assessment</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Assessment), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentRequest request)
    {
        try
        {
            var assessment = await _assessmentService.CreateAssessmentAsync(request);
            return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id }, assessment);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for assessment creation");
            return BadRequest(new ValidationProblemDetails(ex.Errors));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assessment");
            return StatusCode(500, "An error occurred while creating the assessment");
        }
    }
}
```

#### Response Types
```csharp
// Success responses
return Ok(result);                    // 200 OK
return CreatedAtAction(...);          // 201 Created
return NoContent();                   // 204 No Content

// Error responses
return BadRequest(validationErrors);  // 400 Bad Request
return Unauthorized();                // 401 Unauthorized
return Forbid();                      // 403 Forbidden
return NotFound();                    // 404 Not Found
return Conflict(conflictInfo);        // 409 Conflict
return StatusCode(500, errorMessage); // 500 Internal Server Error
```

### Model Validation

#### Data Annotations
```csharp
public class CreateAssessmentRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string AssessmentName { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [EnumDataType(typeof(Framework))]
    public string Framework { get; set; }

    [Range(1, 5)]
    public int MaturityLevel { get; set; } = 1;
}
```

#### Custom Validation
```csharp
public class AssessmentNameUniqueAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var assessmentService = (IAssessmentService)validationContext.GetService(typeof(IAssessmentService));
        var assessmentName = value as string;

        if (assessmentService.IsNameUnique(assessmentName))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult("Assessment name must be unique");
    }
}
```

---

## Frontend Development

### Component Guidelines

#### Standard Component Structure
```typescript
@Component({
  selector: 'app-assessment',
  templateUrl: './assessment.component.html',
  styleUrls: ['./assessment.component.scss']
})
export class AssessmentComponent implements OnInit, OnDestroy {
  // Public properties
  public assessments: Assessment[] = [];
  public loading = false;
  public error: string | null = null;

  // Private properties
  private destroy$ = new Subject<void>();

  // Constructor
  constructor(
    private assessmentService: AssessmentService,
    private messageService: MessageService
  ) { }

  // Lifecycle hooks
  ngOnInit(): void {
    this.loadAssessments();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Public methods
  public async createAssessment(request: CreateAssessmentRequest): Promise<void> {
    try {
      this.loading = true;
      const assessment = await this.assessmentService.createAssessment(request).toPromise();
      this.assessments.push(assessment);
      this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Assessment created' });
    } catch (error) {
      this.error = 'Failed to create assessment';
      this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
    } finally {
      this.loading = false;
    }
  }

  // Private methods
  private async loadAssessments(): Promise<void> {
    try {
      this.loading = true;
      this.assessments = await this.assessmentService.getAssessments().toPromise();
    } catch (error) {
      this.error = 'Failed to load assessments';
    } finally {
      this.loading = false;
    }
  }
}
```

### Service Guidelines

#### Standard Service Structure
```typescript
@Injectable({
  providedIn: 'root'
})
export class AssessmentService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  public getAssessments(): Observable<Assessment[]> {
    return this.http.get<Assessment[]>(`${this.apiUrl}/assessment`)
      .pipe(
        catchError(this.handleError)
      );
  }

  public createAssessment(request: CreateAssessmentRequest): Observable<Assessment> {
    return this.http.post<Assessment>(`${this.apiUrl}/assessment`, request)
      .pipe(
        catchError(this.handleError)
      );
  }

  public updateAssessment(id: number, request: UpdateAssessmentRequest): Observable<Assessment> {
    return this.http.put<Assessment>(`${this.apiUrl}/assessment/${id}`, request)
      .pipe(
        catchError(this.handleError)
      );
  }

  public deleteAssessment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/assessment/${id}`)
      .pipe(
        catchError(this.handleError)
      );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred';
    
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = error.error.message;
    } else {
      // Server-side error
      errorMessage = error.error?.message || `Error Code: ${error.status}`;
    }
    
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
```

---

## Database Development

### Entity Framework Guidelines

#### Entity Structure
```csharp
[Table("Assessments")]
public class Assessment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string AssessmentName { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Framework { get; set; }

    public int MaturityLevel { get; set; } = 1;

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    [StringLength(100)]
    public string ModifiedBy { get; set; }

    // Navigation properties
    public virtual ICollection<Question> Questions { get; set; }
    public virtual ICollection<Document> Documents { get; set; }
}
```

#### Repository Pattern
```csharp
public interface IAssessmentRepository
{
    Task<Assessment> GetByIdAsync(int id);
    Task<IEnumerable<Assessment>> GetAllAsync();
    Task<Assessment> CreateAsync(Assessment assessment);
    Task<Assessment> UpdateAsync(Assessment assessment);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public class AssessmentRepository : IAssessmentRepository
{
    private readonly CSETContext _context;

    public AssessmentRepository(CSETContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Assessment> GetByIdAsync(int id)
    {
        return await _context.Assessments
            .Include(a => a.Questions)
            .Include(a => a.Documents)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Assessment>> GetAllAsync()
    {
        return await _context.Assessments
            .Include(a => a.Questions)
            .OrderByDescending(a => a.CreatedDate)
            .ToListAsync();
    }

    public async Task<Assessment> CreateAsync(Assessment assessment)
    {
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync();
        return assessment;
    }

    public async Task<Assessment> UpdateAsync(Assessment assessment)
    {
        _context.Entry(assessment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return assessment;
    }

    public async Task DeleteAsync(int id)
    {
        var assessment = await _context.Assessments.FindAsync(id);
        if (assessment != null)
        {
            _context.Assessments.Remove(assessment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Assessments.AnyAsync(a => a.Id == id);
    }
}
```

### Migration Guidelines

#### Creating Migrations
```bash
# Add new migration
dotnet ef migrations add AddAssessmentTable

# Update database
dotnet ef database update

# Remove last migration (if needed)
dotnet ef migrations remove
```

#### Migration Best Practices
```csharp
public partial class AddAssessmentTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Assessments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AssessmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                Framework = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                MaturityLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Assessments", x => x.Id);
            });

        // Add indexes for performance
        migrationBuilder.CreateIndex(
            name: "IX_Assessments_CreatedDate",
            table: "Assessments",
            column: "CreatedDate");

        migrationBuilder.CreateIndex(
            name: "IX_Assessments_Framework",
            table: "Assessments",
            column: "Framework");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Assessments");
    }
}
```

---

## Security Guidelines

### Authentication and Authorization

#### JWT Token Implementation
```csharp
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Security:JwtSecret"]);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

#### Authorization Attributes
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AssessmentController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAssessments()
    {
        // Implementation
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentRequest request)
    {
        // Implementation
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAssessment(int id)
    {
        // Implementation
    }
}
```

### Input Validation

#### Model Validation
```csharp
public class CreateAssessmentRequest
{
    [Required(ErrorMessage = "Assessment name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Assessment name must be between 1 and 200 characters")]
    [RegularExpression(@"^[a-zA-Z0-9\s\-_]+$", ErrorMessage = "Assessment name contains invalid characters")]
    public string AssessmentName { get; set; }

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Framework is required")]
    [EnumDataType(typeof(Framework), ErrorMessage = "Invalid framework selection")]
    public string Framework { get; set; }

    [Range(1, 5, ErrorMessage = "Maturity level must be between 1 and 5")]
    public int MaturityLevel { get; set; } = 1;
}
```

#### Custom Validation
```csharp
public class AssessmentNameUniqueAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var assessmentService = (IAssessmentService)validationContext.GetService(typeof(IAssessmentService));
        var assessmentName = value.ToString();

        // Check if name is unique
        if (!assessmentService.IsNameUnique(assessmentName))
        {
            return new ValidationResult("Assessment name must be unique");
        }

        return ValidationResult.Success;
    }
}
```

---

## Performance Guidelines

### Caching Strategy

#### Redis Caching
```csharp
public class CachedAssessmentService : IAssessmentService
{
    private readonly IAssessmentService _assessmentService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedAssessmentService> _logger;

    public CachedAssessmentService(
        IAssessmentService assessmentService,
        IDistributedCache cache,
        ILogger<CachedAssessmentService> logger)
    {
        _assessmentService = assessmentService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Assessment> GetByIdAsync(int id)
    {
        var cacheKey = $"assessment:{id}";
        var cachedAssessment = await _cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedAssessment))
        {
            return JsonSerializer.Deserialize<Assessment>(cachedAssessment);
        }

        var assessment = await _assessmentService.GetByIdAsync(id);
        
        if (assessment != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(assessment), options);
        }

        return assessment;
    }
}
```

### Database Optimization

#### Query Optimization
```csharp
public async Task<IEnumerable<Assessment>> GetAssessmentsWithDetailsAsync()
{
    return await _context.Assessments
        .Include(a => a.Questions)
        .Include(a => a.Documents)
        .AsNoTracking()  // For read-only queries
        .OrderByDescending(a => a.CreatedDate)
        .Take(100)  // Limit results
        .ToListAsync();
}
```

#### Pagination
```csharp
public async Task<PagedResult<Assessment>> GetAssessmentsPagedAsync(int page, int pageSize)
{
    var totalCount = await _context.Assessments.CountAsync();
    
    var assessments = await _context.Assessments
        .Include(a => a.Questions)
        .OrderByDescending(a => a.CreatedDate)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<Assessment>
    {
        Data = assessments,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
    };
}
```

---

## Deployment

### Development Deployment

#### Local Development
```bash
# Backend
cd CSETWebApi/CSETWeb_Api/CSETWeb_ApiCore
dotnet run

# Frontend
cd CSETWebNg
ng serve

# Database
sqllocaldb start "MSSQLLocalDB"

# Redis
redis-server
```

#### Docker Development
```bash
# Build and run with Docker Compose
docker-compose up --build

# Or run individual services
docker run -d --name cset-api -p 5001:5001 cset-api:latest
docker run -d --name cset-frontend -p 4200:4200 cset-frontend:latest
docker run -d --name cset-db -p 1433:1433 mcr.microsoft.com/mssql/server:2019-latest
docker run -d --name cset-redis -p 6379:6379 redis:6.2-alpine
```

### Production Deployment

#### Environment Configuration
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "CSETWeb": "Server=prod-db-server;Database=CSET_Production;User Id=cset_app;Password=SecurePassword123!;",
    "Redis": "prod-redis-server:6379,password=your-redis-password"
  },
  "Security": {
    "JwtSecret": "your-production-jwt-secret",
    "JwtExpirationHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

#### Deployment Scripts
```bash
#!/bin/bash
# deploy.sh

# Build application
dotnet build --configuration Release

# Run database migrations
dotnet ef database update --environment Production

# Deploy to server
rsync -avz --exclude 'node_modules' --exclude '.git' ./ user@server:/opt/cset/

# Restart services
ssh user@server "sudo systemctl restart cset-api"
ssh user@server "sudo systemctl restart cset-frontend"
```

---

## Troubleshooting

### Common Issues

#### Database Connection Issues
```bash
# Check database connectivity
telnet your-db-server 1433

# Test connection string
sqlcmd -S your-db-server -U username -P password -Q "SELECT 1"

# Check connection pool
SELECT * FROM sys.dm_exec_connections
```

#### Redis Connection Issues
```bash
# Check Redis connectivity
redis-cli -h your-redis-server ping

# Check Redis memory usage
redis-cli -h your-redis-server info memory

# Monitor Redis connections
redis-cli -h your-redis-server monitor
```

#### Performance Issues
```bash
# Check application performance
curl -X GET "https://your-api/api/health/detailed"

# Monitor system resources
htop
iotop
nethogs

# Check application logs
tail -f /opt/cset/logs/app.log | grep ERROR
```

### Debugging Tools

#### Backend Debugging
```csharp
// Add logging
_logger.LogInformation("Processing request: {RequestId}", requestId);
_logger.LogWarning("Validation failed: {Errors}", validationErrors);
_logger.LogError(exception, "Error processing request: {RequestId}", requestId);

// Use debugger
#if DEBUG
    System.Diagnostics.Debugger.Break();
#endif
```

#### Frontend Debugging
```typescript
// Add console logging
console.log('Processing request:', request);
console.warn('Validation failed:', errors);
console.error('Error processing request:', error);

// Use browser dev tools
debugger;
```

### Performance Monitoring

#### Application Insights
```csharp
// Custom telemetry
_telemetryClient.TrackEvent("AssessmentCreated", new Dictionary<string, string>
{
    ["AssessmentId"] = assessment.Id.ToString(),
    ["Framework"] = assessment.Framework
});

// Custom metrics
_telemetryClient.GetMetric("AssessmentCreationTime").TrackValue(stopwatch.ElapsedMilliseconds);
```

#### Health Checks
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Test database connection
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}
```

---

## Support and Resources

### Documentation
- [CSET API Documentation](https://docs.cset.gov/api)
- [Angular Documentation](https://angular.io/docs)
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Documentation](https://docs.microsoft.com/en-us/ef/)

### Community
- [CSET GitHub Repository](https://github.com/cisagov/cset)
- [CSET Community Forum](https://community.cset.gov)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/cset)

### Tools
- [Visual Studio](https://visualstudio.microsoft.com/)
- [VS Code](https://code.visualstudio.com/)
- [Postman](https://www.postman.com/)
- [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/)

### Training
- [C# Fundamentals](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [Angular Tutorial](https://angular.io/tutorial)
- [Entity Framework Tutorial](https://docs.microsoft.com/en-us/ef/core/get-started/)

---

This comprehensive developer documentation provides all the information needed to contribute to the CSET project effectively, following best practices and maintaining code quality standards. 