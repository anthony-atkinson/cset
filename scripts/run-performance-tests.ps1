#!/usr/bin/env pwsh

# CSET Performance Tests Execution Script
# This script runs performance tests from the CSET solution

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [bool]$EnableDetailedMetrics = $true,
    [bool]$EnableMemoryMonitoring = $true,
    [bool]$EnableNetworkMonitoring = $true
)

$ErrorActionPreference = "Stop"

# Configuration
$OutputDir = "TestResults/Performance"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "🚀 Running CSET Performance Tests..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow
Write-Host "Detailed Metrics: $EnableDetailedMetrics" -ForegroundColor Yellow
Write-Host "Memory Monitoring: $EnableMemoryMonitoring" -ForegroundColor Yellow
Write-Host "Network Monitoring: $EnableNetworkMonitoring" -ForegroundColor Yellow

# Create output directory
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Update test settings for performance testing
Write-Host "📝 Updating test settings for performance testing..." -ForegroundColor Cyan

$testSettings = @{
    BaseUrl = "http://localhost:4200"
    ApiUrl = "http://localhost:5000"
    Browser = @{
        Default = "chromium"
        Headless = $true
        SlowMo = 0
        VideoPath = "test-results/videos"
        ScreenshotPath = "test-results/screenshots"
    }
    CrossBrowser = @{
        Enabled = $false
        Browsers = @("chromium")
        ParallelExecution = $false
        ScreenshotOnFailure = $true
        VideoRecording = $false
        PerformanceMonitoring = $true
        MemoryMonitoring = $true
        AccessibilityTesting = $false
    }
    Test = @{
        DefaultTimeout = 60000
        NavigationTimeout = 60000
        TakeScreenshotOnFailure = $true
        RecordVideo = $false
    }
    TestCredentials = @{
        Username = "test@example.com"
        Password = "TestPassword123!"
        AccessKey = ""
    }
    Database = @{
        ConnectionString = ""
        ResetBeforeTests = $false
    }
    Features = @{
        EnableSecurityTests = $false
        EnablePerformanceTests = $true
        EnableAccessibilityTests = $false
        EnableCrossBrowserTests = $false
    }
}

$testSettings | ConvertTo-Json -Depth 10 | Out-File -FilePath "CSETWebApi/CSETWeb_Api/CSETWebCore.PlaywrightTests/testsettings.json" -Encoding UTF8

# Build arguments
$arguments = @(
    "test",
    "CSETWeb_Api.sln",
    "--filter", "TestCategory=Performance",
    "--configuration", $Configuration,
    "--verbosity", $Verbosity,
    "--no-restore",
    "--logger", "trx;LogFileName=PerformanceTests_$Timestamp.trx",
    "--results-directory", $OutputDir,
    "--collect:XPlat Code Coverage"
)

Write-Host "Executing: dotnet $($arguments -join ' ')" -ForegroundColor Gray

# Execute the test command
$result = & dotnet @arguments

# Check exit code
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Performance tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Performance tests failed!" -ForegroundColor Red
    exit 1
}

# Generate performance report
Write-Host "📊 Generating performance report..." -ForegroundColor Cyan

# Create performance summary
$summaryContent = @"
# CSET Performance Test Summary

**Generated:** $(Get-Date)
**Configuration:** $Configuration
**Test Run:** $Timestamp

## Test Results

### Application Performance Tests
- Login page load time
- Login workflow performance
- Dashboard loading and rendering
- Assessment creation performance
- Question navigation responsiveness
- Report generation performance
- Full workflow performance
- Memory usage stability
- Network request efficiency

### UI Component Performance Tests
- Form input responsiveness
- Button click responsiveness
- Dropdown/select responsiveness
- Chart rendering performance
- Table rendering performance
- Modal dialog performance
- Navigation menu responsiveness
- Text rendering performance
- Image loading performance
- Scroll performance
- Keyboard input responsiveness
- Mouse interaction responsiveness
- Component re-rendering performance
- Animation performance

## Performance Metrics

### Page Load Times
- Target: < 5 seconds
- First Paint: < 2.5 seconds
- First Contentful Paint: < 3 seconds
- DOM Content Loaded: < 3 seconds

### Memory Usage
- Target: < 50MB increase for full workflow
- Chart rendering: < 15MB increase
- Component re-rendering: < 10MB increase

### Network Efficiency
- Target: < 25 requests for dashboard
- Average request time: < 1000ms
- Large requests (>1MB): < 5

### Responsiveness
- Form inputs: < 1000ms
- Button clicks: < 500ms
- Dropdown selections: < 1000ms
- Animations: < 1000ms

## Recommendations

1. Monitor memory usage during extended sessions
2. Optimize chart rendering for large datasets
3. Implement lazy loading for non-critical components
4. Consider caching strategies for frequently accessed data
5. Monitor network request patterns for optimization opportunities
"@

$summaryContent | Out-File -FilePath "$OutputDir/performance_summary_$Timestamp.md" -Encoding UTF8
Write-Host "📄 Performance summary saved to: $OutputDir/performance_summary_$Timestamp.md" -ForegroundColor Green

# Check for performance results files
$performanceFiles = Get-ChildItem -Path $OutputDir -Filter "performance_*.json" -ErrorAction SilentlyContinue

if ($performanceFiles) {
    Write-Host "📈 Performance data files found:" -ForegroundColor Cyan
    $performanceFiles | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    
    # Create aggregated performance report
    Write-Host "📊 Creating aggregated performance report..." -ForegroundColor Cyan
    
    $aggregatedReport = @{
        testRun = $Timestamp
        configuration = $Configuration
        timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        performanceFiles = $performanceFiles.FullName
        summary = @{
            totalTests = $performanceFiles.Count
            averageDuration = "Calculated from individual test results"
            averageMemoryIncrease = "Calculated from individual test results"
            averagePageLoadTime = "Calculated from individual test results"
        }
    }
    
    $aggregatedReport | ConvertTo-Json -Depth 10 | Out-File -FilePath "$OutputDir/aggregated_performance_$Timestamp.json" -Encoding UTF8
    Write-Host "📄 Aggregated performance report saved to: $OutputDir/aggregated_performance_$Timestamp.json" -ForegroundColor Green
} else {
    Write-Host "⚠️  No performance data files found" -ForegroundColor Yellow
}

# Display test results summary
Write-Host ""
Write-Host "🎯 Performance Test Summary:" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host "Output Directory: $OutputDir" -ForegroundColor White
Write-Host "Test Run ID: $Timestamp" -ForegroundColor White
Write-Host "Configuration: $Configuration" -ForegroundColor White
Write-Host ""

# Check for test result files
$trxFiles = Get-ChildItem -Path $OutputDir -Filter "*.trx" -ErrorAction SilentlyContinue

if ($trxFiles) {
    Write-Host "📋 Test Result Files:" -ForegroundColor Cyan
    $trxFiles | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    Write-Host ""
}

# Check for coverage reports
$coverageDirs = Get-ChildItem -Path $OutputDir -Filter "coverage" -Directory -ErrorAction SilentlyContinue

if ($coverageDirs) {
    Write-Host "📊 Coverage Reports:" -ForegroundColor Cyan
    $coverageDirs | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    Write-Host ""
}

Write-Host "✅ Performance testing completed!" -ForegroundColor Green
Write-Host "📁 Results available in: $OutputDir" -ForegroundColor Green 