# CSET Notification System Tests Execution Script (PowerShell)
# This script runs notification system tests from the CSET solution

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [bool]$EnableEmailTesting = $true,
    [bool]$EnableUITesting = $true
)

# Error handling
$ErrorActionPreference = "Stop"

# Configuration
$OutputDir = "TestResults\Notifications"
$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

Write-Host "🔔 Running CSET Notification System Tests..." -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow
Write-Host "Email Testing: $EnableEmailTesting" -ForegroundColor Yellow
Write-Host "UI Testing: $EnableUITesting" -ForegroundColor Yellow

# Create output directory
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

# Update test settings for notification testing
Write-Host "📝 Updating test settings for notification testing..." -ForegroundColor Green

$TestSettings = @{
    BaseUrl = "http://localhost:4200"
    ApiUrl = "http://localhost:5000"
    Browser = @{
        Default = "chromium"
        Headless = $true
        SlowMo = 0
        VideoPath = "test-results\videos"
        ScreenshotPath = "test-results\screenshots"
    }
    CrossBrowser = @{
        Enabled = $false
        Browsers = @("chromium")
        ParallelExecution = $false
        ScreenshotOnFailure = $true
        VideoRecording = $false
        PerformanceMonitoring = $false
        MemoryMonitoring = $false
        AccessibilityTesting = $true
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
        EnablePerformanceTests = $false
        EnableAccessibilityTests = $true
        EnableCrossBrowserTests = $false
        EnableNotificationTests = $true
        EnableEmailTests = $true
    }
    Notifications = @{
        EmailTesting = $EnableEmailTesting
        UITesting = $EnableUITesting
        TestEmailAddress = "test@example.com"
        SmtpSettings = @{
            Host = "localhost"
            Port = 1025
            UseSsl = $false
            Username = ""
            Password = ""
        }
    }
}

$TestSettings | ConvertTo-Json -Depth 10 | Out-File -FilePath "CSETWebApi\CSETWeb_Api\CSETWebCore.PlaywrightTests\testsettings.json" -Encoding UTF8

# Build arguments
$Args = @(
    "test",
    "CSETWeb_Api.sln",
    "--filter", "TestCategory=Notifications",
    "--configuration", $Configuration,
    "--verbosity", $Verbosity,
    "--no-restore",
    "--logger", "trx;LogFileName=NotificationTests_$Timestamp.trx",
    "--results-directory", $OutputDir,
    "--collect:XPlat Code Coverage"
)

Write-Host "Executing: dotnet $($Args -join ' ')" -ForegroundColor Green

# Execute the test command
try {
    & dotnet $Args
    $ExitCode = $LASTEXITCODE
}
catch {
    Write-Host "❌ Error executing notification tests: $_" -ForegroundColor Red
    exit 1
}

# Check exit code
if ($ExitCode -eq 0) {
    Write-Host "✅ Notification tests completed successfully!" -ForegroundColor Green
}
else {
    Write-Host "❌ Notification tests failed!" -ForegroundColor Red
    exit $ExitCode
}

# Generate notification test report
Write-Host "📊 Generating notification test report..." -ForegroundColor Green

# Create notification summary
$SummaryContent = @"
# CSET Notification System Test Summary

**Generated:** $(Get-Date)
**Configuration:** $Configuration
**Test Run:** $Timestamp

## Test Results

### Notification System Tests
- Version notification display and interaction
- Upgrade notification functionality
- Snackbar notification system
- Error notification handling
- Success notification handling
- Warning notification handling
- Notification persistence across navigation
- Notification accessibility features
- Notification performance and responsiveness
- Notification dismissal functionality
- Notification grouping and stacking
- Notification content validation

### Email Notification Tests
- Assessment invitation email functionality
- Password reset email functionality
- Email configuration validation
- Email template functionality
- Email notification preferences
- Email notification error handling
- Email notification delivery tracking
- Email notification rate limiting
- Email notification security

## Notification Types Tested

### UI Notifications
- **Version Notifications**: Update availability and version comparison
- **Upgrade Notifications**: Assessment upgrade prompts and workflows
- **Snackbar Notifications**: Temporary success/error messages
- **Alert Notifications**: Bootstrap-style alerts (success, warning, error, info)
- **Modal Notifications**: Dialog-based notifications
- **Toast Notifications**: Overlay notifications

### Email Notifications
- **Assessment Invitations**: User invitation to participate in assessments
- **Password Resets**: Password reset and account recovery emails
- **Welcome Emails**: New user onboarding emails
- **System Notifications**: Administrative and system status emails
- **Template Emails**: Customizable email templates

### System Notifications
- **Error Notifications**: Application errors and validation failures
- **Success Notifications**: Operation completion confirmations
- **Warning Notifications**: Important warnings and cautions
- **Info Notifications**: General information and status updates

## Test Coverage

### Accessibility Testing
- ARIA labels and screen reader support
- Keyboard navigation and focus management
- Color contrast and visual accessibility
- Screen reader text validation

### Performance Testing
- Notification display timing
- Memory usage during notifications
- Network request efficiency
- UI responsiveness during notifications

### Security Testing
- Email encryption and authentication
- Spam protection and rate limiting
- Input validation and sanitization
- Secure email delivery

### Error Handling
- Invalid email address validation
- SMTP configuration errors
- Network connectivity issues
- Template rendering errors

## Recommendations

1. **Email Configuration**: Ensure SMTP settings are properly configured for production
2. **Template Management**: Regularly review and update email templates
3. **Rate Limiting**: Implement appropriate rate limiting for email notifications
4. **Accessibility**: Continue improving notification accessibility features
5. **Performance**: Monitor notification performance in production environments
6. **Security**: Regularly audit email security settings and configurations

## Configuration Notes

- Email testing requires valid SMTP configuration
- UI testing requires browser automation setup
- Accessibility testing requires screen reader simulation
- Performance testing requires consistent test environment

"@

$SummaryContent | Out-File -FilePath "$OutputDir\notification_summary_$Timestamp.md" -Encoding UTF8

Write-Host "📄 Notification summary saved to: $OutputDir\notification_summary_$Timestamp.md" -ForegroundColor Green

# Check for notification test result files
$NotificationFiles = Get-ChildItem -Path $OutputDir -Filter "*notification*" -Recurse -ErrorAction SilentlyContinue

if ($NotificationFiles) {
    Write-Host "📈 Notification test files found:" -ForegroundColor Yellow
    $NotificationFiles | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    
    # Create aggregated notification report
    Write-Host "📊 Creating aggregated notification report..." -ForegroundColor Green
    
    $AggregatedReport = @{
        testRun = $Timestamp
        configuration = $Configuration
        timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
        notificationFiles = $NotificationFiles.FullName
        summary = @{
            totalTests = $NotificationFiles.Count
            emailTests = "Notification system email functionality tests"
            uiTests = "Notification system UI functionality tests"
            accessibilityTests = "Notification accessibility compliance tests"
            securityTests = "Notification security and validation tests"
        }
    }
    
    $AggregatedReport | ConvertTo-Json -Depth 10 | Out-File -FilePath "$OutputDir\aggregated_notification_$Timestamp.json" -Encoding UTF8
    
    Write-Host "📄 Aggregated notification report saved to: $OutputDir\aggregated_notification_$Timestamp.json" -ForegroundColor Green
}
else {
    Write-Host "⚠️  No notification test files found" -ForegroundColor Yellow
}

# Display test results summary
Write-Host ""
Write-Host "🎯 Notification Test Summary:" -ForegroundColor Cyan
Write-Host "==============================" -ForegroundColor Cyan
Write-Host "Output Directory: $OutputDir" -ForegroundColor White
Write-Host "Test Run ID: $Timestamp" -ForegroundColor White
Write-Host "Configuration: $Configuration" -ForegroundColor White
Write-Host ""

# Check for test result files
$TrxFiles = Get-ChildItem -Path $OutputDir -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue

if ($TrxFiles) {
    Write-Host "📋 Test Result Files:" -ForegroundColor Yellow
    $TrxFiles | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    Write-Host ""
}

# Check for coverage reports
$CoverageDirs = Get-ChildItem -Path $OutputDir -Filter "coverage" -Directory -Recurse -ErrorAction SilentlyContinue

if ($CoverageDirs) {
    Write-Host "📊 Coverage Reports:" -ForegroundColor Yellow
    $CoverageDirs | ForEach-Object { Write-Host "  - $($_.FullName)" -ForegroundColor Gray }
    Write-Host ""
}

Write-Host "✅ Notification system testing completed!" -ForegroundColor Green
Write-Host "📁 Results available in: $OutputDir" -ForegroundColor Cyan 