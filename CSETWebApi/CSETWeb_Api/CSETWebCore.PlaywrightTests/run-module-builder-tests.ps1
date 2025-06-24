# CSET Module Builder E2E Tests Runner (PowerShell)
# This script runs Playwright E2E tests for the Module Builder functionality

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [bool]$RunHeadless = $true,
    [bool]$CaptureScreenshots = $true
)

# Error handling
$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = Split-Path -Parent $ScriptDir
$TestResultsDir = Join-Path $ProjectDir "TestResults\ModuleBuilder"
$CoverageDir = Join-Path $ProjectDir "Coverage\ModuleBuilder"

# Test settings
$TestProject = "CSETWebCore.PlaywrightTests"
$TestFilter = "TestCategory=ModuleBuilder"
$TestAssembly = Join-Path $ProjectDir "bin\$Configuration\net7.0\$TestProject.dll"

# Logging
$LogFile = Join-Path $TestResultsDir "module-builder-tests.log"
$Timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"

# Functions
function Write-Header {
    Write-Host "==========================================" -ForegroundColor Blue
    Write-Host "  CSET Module Builder E2E Tests Runner" -ForegroundColor Blue
    Write-Host "==========================================" -ForegroundColor Blue
    Write-Host ""
}

function Write-Config {
    Write-Host "Configuration:" -ForegroundColor Yellow
    Write-Host "  Configuration: $Configuration"
    Write-Host "  Verbosity: $Verbosity"
    Write-Host "  Headless: $RunHeadless"
    Write-Host "  Screenshots: $CaptureScreenshots"
    Write-Host "  Test Results: $TestResultsDir"
    Write-Host "  Coverage: $CoverageDir"
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Test-Prerequisites {
    Write-Step "Checking prerequisites..."
    
    # Check if .NET is installed
    try {
        $null = Get-Command dotnet -ErrorAction Stop
    }
    catch {
        Write-Error ".NET SDK is not installed or not in PATH"
        exit 1
    }
    
    # Check if Playwright is installed
    try {
        $null = Get-Command playwright -ErrorAction Stop
    }
    catch {
        Write-Warning "Playwright is not installed. Installing..."
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install
    }
    
    # Check if test project exists
    if (-not (Test-Path $TestAssembly)) {
        Write-Error "Test assembly not found: $TestAssembly"
        Write-Step "Building test project..."
        dotnet build (Join-Path $ProjectDir "$TestProject.csproj") --configuration $Configuration
    }
    
    Write-Success "Prerequisites check completed"
}

function New-Directories {
    Write-Step "Creating output directories..."
    
    if (-not (Test-Path $TestResultsDir)) {
        New-Item -ItemType Directory -Path $TestResultsDir -Force | Out-Null
    }
    
    if (-not (Test-Path $CoverageDir)) {
        New-Item -ItemType Directory -Path $CoverageDir -Force | Out-Null
    }
    
    Write-Success "Directories created"
}

function Set-Environment {
    Write-Step "Setting up test environment..."
    
    # Set environment variables for tests
    $env:CSET_TEST_ENVIRONMENT = "E2E"
    $env:CSET_TEST_CONFIGURATION = $Configuration
    $env:CSET_TEST_HEADLESS = $RunHeadless.ToString().ToLower()
    $env:CSET_TEST_SCREENSHOTS = $CaptureScreenshots.ToString().ToLower()
    $env:CSET_TEST_TIMEOUT = "30000"
    $env:CSET_TEST_RETRIES = "2"
    
    # Set Playwright environment variables
    $env:PLAYWRIGHT_BROWSERS_PATH = Join-Path $ProjectDir ".playwright"
    $env:PLAYWRIGHT_HEADLESS = $RunHeadless.ToString().ToLower()
    
    Write-Success "Environment setup completed"
}

function Invoke-Tests {
    Write-Step "Running Module Builder E2E tests..."
    
    $testArgs = @(
        "test",
        $TestAssembly,
        "--filter", $TestFilter,
        "--configuration", $Configuration,
        "--verbosity", $Verbosity,
        "--logger", "console;verbosity=normal",
        "--logger", "trx;LogFileName=ModuleBuilderTests_$Timestamp.trx",
        "--results-directory", $TestResultsDir,
        "--collect", "XPlat Code Coverage",
        "--settings", (Join-Path $ProjectDir "testsettings.json")
    )
    
    # Run the tests
    try {
        dotnet $testArgs
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq 0) {
            Write-Success "Module Builder tests completed successfully"
        }
        else {
            Write-Error "Module Builder tests failed with exit code $exitCode"
        }
        
        return $exitCode
    }
    catch {
        Write-Error "Error running tests: $_"
        return 1
    }
}

function New-Reports {
    Write-Step "Generating test reports..."
    
    # Generate HTML report if trx file exists
    $trxFile = Join-Path $TestResultsDir "ModuleBuilderTests_$Timestamp.trx"
    if (Test-Path $trxFile) {
        Write-Step "Converting TRX to HTML report..."
        try {
            dotnet tool install --global trx2html
            trx2html $trxFile -o (Join-Path $TestResultsDir "ModuleBuilderTests_$Timestamp.html")
            Write-Success "HTML report generated: $(Join-Path $TestResultsDir "ModuleBuilderTests_$Timestamp.html")"
        }
        catch {
            Write-Warning "Failed to generate HTML report: $_"
        }
    }
    
    # Generate coverage report if available
    $coverageFile = Join-Path $TestResultsDir "coverage.cobertura.xml"
    if (Test-Path $coverageFile) {
        Write-Step "Generating coverage report..."
        try {
            dotnet tool install --global dotnet-reportgenerator-globaltool
            reportgenerator -reports:$coverageFile -targetdir:$CoverageDir -reporttypes:Html
            Write-Success "Coverage report generated: $(Join-Path $CoverageDir "index.html")"
        }
        catch {
            Write-Warning "Failed to generate coverage report: $_"
        }
    }
    
    Write-Success "Report generation completed"
}

function New-Summary {
    Write-Step "Creating test summary..."
    
    $summaryFile = Join-Path $TestResultsDir "ModuleBuilderTests_$Timestamp.summary.md"
    
    $summaryContent = @"
# Module Builder E2E Tests Summary

**Test Run:** $Timestamp  
**Configuration:** $Configuration  
**Headless:** $RunHeadless  
**Screenshots:** $CaptureScreenshots  

## Test Results

"@
    
    # Parse TRX file for summary
    $trxFile = Join-Path $TestResultsDir "ModuleBuilderTests_$Timestamp.trx"
    if (Test-Path $trxFile) {
        $trxContent = Get-Content $trxFile -Raw
        $totalTests = ([regex]::Matches($trxContent, '<UnitTest')).Count
        $passedTests = ([regex]::Matches($trxContent, 'outcome="Passed"')).Count
        $failedTests = ([regex]::Matches($trxContent, 'outcome="Failed"')).Count
        $skippedTests = ([regex]::Matches($trxContent, 'outcome="NotExecuted"')).Count
        
        $summaryContent += @"

- **Total Tests:** $totalTests
- **Passed:** $passedTests
- **Failed:** $failedTests
- **Skipped:** $skippedTests

## Test Categories

### Module Management Tests
- Module list navigation and display
- New module creation workflow
- Module details editing and validation
- Module cloning functionality
- Module deletion with confirmation

### Requirements Management Tests
- Adding new requirements with categories and subcategories
- Editing existing requirements
- Deleting requirements with confirmation
- Requirement search functionality
- Requirement filtering by category
- Requirement sorting by title
- Bulk operations on requirements

### Questions Management Tests
- Adding new questions with categories and subcategories
- Editing existing questions
- Deleting questions with confirmation
- Question search functionality
- Question filtering and sorting

### Module Builder Features
- Clone from existing modules functionality
- Document management integration
- Category and subcategory management
- Validation and error handling
- Search and filtering capabilities
- Bulk operations support
- Export and import functionality
- Accessibility compliance
- Performance monitoring
- Error handling and recovery
- Data integrity validation
- Concurrency handling

## Reports

- **Test Results:** [TRX File]($TestResultsDir\ModuleBuilderTests_$Timestamp.trx)
- **HTML Report:** [HTML Report]($TestResultsDir\ModuleBuilderTests_$Timestamp.html)
- **Coverage Report:** [Coverage Report]($CoverageDir\index.html)

## Logs

- **Test Log:** [Test Log]($LogFile)

"@
    }
    
    $summaryContent | Out-File -FilePath $summaryFile -Encoding UTF8
    Write-Success "Summary created: $summaryFile"
}

function Remove-TempFiles {
    Write-Step "Cleaning up temporary files..."
    
    # Remove temporary test files
    Get-ChildItem -Path $TestResultsDir -Filter "*.tmp" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue
    
    Write-Success "Cleanup completed"
}

# Main execution
try {
    Write-Header
    Write-Config
    
    # Redirect output to log file
    Start-Transcript -Path $LogFile -Append
    
    Write-Step "Starting Module Builder E2E tests at $Timestamp"
    
    Test-Prerequisites
    New-Directories
    Set-Environment
    
    $testExitCode = Invoke-Tests
    
    New-Reports
    New-Summary
    Remove-TempFiles
    
    Write-Step "Module Builder E2E tests completed at $(Get-Date)"
    
    if ($testExitCode -eq 0) {
        Write-Success "All Module Builder tests passed!"
        Write-Host "==========================================" -ForegroundColor Green
        Write-Host "  Module Builder Tests: PASSED" -ForegroundColor Green
        Write-Host "==========================================" -ForegroundColor Green
    }
    else {
        Write-Error "Some Module Builder tests failed!"
        Write-Host "==========================================" -ForegroundColor Red
        Write-Host "  Module Builder Tests: FAILED" -ForegroundColor Red
        Write-Host "==========================================" -ForegroundColor Red
    }
    
    exit $testExitCode
}
catch {
    Write-Error "Script error: $_"
    exit 1
}
finally {
    Stop-Transcript
} 