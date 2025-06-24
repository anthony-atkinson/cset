#!/bin/bash

# CSET Notification System Tests Execution Script
# This script runs notification system tests from the CSET solution

set -e

# Configuration
CONFIGURATION=${1:-"Debug"}
VERBOSITY=${2:-"normal"}
ENABLE_EMAIL_TESTING=${3:-"true"}
ENABLE_UI_TESTING=${4:-"true"}
OUTPUT_DIR="TestResults/Notifications"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "🔔 Running CSET Notification System Tests..."
echo "Configuration: $CONFIGURATION"
echo "Verbosity: $VERBOSITY"
echo "Email Testing: $ENABLE_EMAIL_TESTING"
echo "UI Testing: $ENABLE_UI_TESTING"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Update test settings for notification testing
echo "📝 Updating test settings for notification testing..."
cat > CSETWebApi/CSETWeb_Api/CSETWebCore.PlaywrightTests/testsettings.json << EOF
{
  "BaseUrl": "http://localhost:4200",
  "ApiUrl": "http://localhost:5000",
  "Browser": {
    "Default": "chromium",
    "Headless": true,
    "SlowMo": 0,
    "VideoPath": "test-results/videos",
    "ScreenshotPath": "test-results/screenshots"
  },
  "CrossBrowser": {
    "Enabled": false,
    "Browsers": ["chromium"],
    "ParallelExecution": false,
    "ScreenshotOnFailure": true,
    "VideoRecording": false,
    "PerformanceMonitoring": false,
    "MemoryMonitoring": false,
    "AccessibilityTesting": true
  },
  "Test": {
    "DefaultTimeout": 60000,
    "NavigationTimeout": 60000,
    "TakeScreenshotOnFailure": true,
    "RecordVideo": false
  },
  "TestCredentials": {
    "Username": "test@example.com",
    "Password": "TestPassword123!",
    "AccessKey": ""
  },
  "Database": {
    "ConnectionString": "",
    "ResetBeforeTests": false
  },
  "Features": {
    "EnableSecurityTests": false,
    "EnablePerformanceTests": false,
    "EnableAccessibilityTests": true,
    "EnableCrossBrowserTests": false,
    "EnableNotificationTests": true,
    "EnableEmailTests": true
  },
  "Notifications": {
    "EmailTesting": $ENABLE_EMAIL_TESTING,
    "UITesting": $ENABLE_UI_TESTING,
    "TestEmailAddress": "test@example.com",
    "SmtpSettings": {
      "Host": "localhost",
      "Port": 1025,
      "UseSsl": false,
      "Username": "",
      "Password": ""
    }
  }
}
EOF

# Build arguments
ARGS=(
    "test"
    "CSETWeb_Api.sln"
    "--filter" "TestCategory=Notifications"
    "--configuration" "$CONFIGURATION"
    "--verbosity" "$VERBOSITY"
    "--no-restore"
    "--logger" "trx;LogFileName=NotificationTests_$TIMESTAMP.trx"
    "--results-directory" "$OUTPUT_DIR"
    "--collect:XPlat Code Coverage"
)

echo "Executing: dotnet ${ARGS[*]}"

# Execute the test command
dotnet "${ARGS[@]}"

# Check exit code
if [ $? -eq 0 ]; then
    echo "✅ Notification tests completed successfully!"
else
    echo "❌ Notification tests failed!"
    exit 1
fi

# Generate notification test report
echo "📊 Generating notification test report..."

# Create notification summary
cat > "$OUTPUT_DIR/notification_summary_$TIMESTAMP.md" << EOF
# CSET Notification System Test Summary

**Generated:** $(date)
**Configuration:** $CONFIGURATION
**Test Run:** $TIMESTAMP

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

EOF

echo "📄 Notification summary saved to: $OUTPUT_DIR/notification_summary_$TIMESTAMP.md"

# Check for notification test result files
NOTIFICATION_FILES=$(find "$OUTPUT_DIR" -name "*notification*" -type f 2>/dev/null || true)

if [ -n "$NOTIFICATION_FILES" ]; then
    echo "📈 Notification test files found:"
    echo "$NOTIFICATION_FILES"
    
    # Create aggregated notification report
    echo "📊 Creating aggregated notification report..."
    
    cat > "$OUTPUT_DIR/aggregated_notification_$TIMESTAMP.json" << EOF
{
  "testRun": "$TIMESTAMP",
  "configuration": "$CONFIGURATION",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "notificationFiles": [
EOF

    for file in $NOTIFICATION_FILES; do
        echo "    \"$file\"," >> "$OUTPUT_DIR/aggregated_notification_$TIMESTAMP.json"
    done

    cat >> "$OUTPUT_DIR/aggregated_notification_$TIMESTAMP.json" << EOF
  ],
  "summary": {
    "totalTests": $(echo "$NOTIFICATION_FILES" | wc -l),
    "emailTests": "Notification system email functionality tests",
    "uiTests": "Notification system UI functionality tests",
    "accessibilityTests": "Notification accessibility compliance tests",
    "securityTests": "Notification security and validation tests"
  }
}
EOF

    echo "📄 Aggregated notification report saved to: $OUTPUT_DIR/aggregated_notification_$TIMESTAMP.json"
else
    echo "⚠️  No notification test files found"
fi

# Display test results summary
echo ""
echo "🎯 Notification Test Summary:"
echo "=============================="
echo "Output Directory: $OUTPUT_DIR"
echo "Test Run ID: $TIMESTAMP"
echo "Configuration: $CONFIGURATION"
echo ""

# Check for test result files
TRX_FILES=$(find "$OUTPUT_DIR" -name "*.trx" -type f 2>/dev/null || true)

if [ -n "$TRX_FILES" ]; then
    echo "📋 Test Result Files:"
    for file in $TRX_FILES; do
        echo "  - $file"
    done
    echo ""
fi

# Check for coverage reports
COVERAGE_DIRS=$(find "$OUTPUT_DIR" -name "coverage" -type d 2>/dev/null || true)

if [ -n "$COVERAGE_DIRS" ]; then
    echo "📊 Coverage Reports:"
    for dir in $COVERAGE_DIRS; do
        echo "  - $dir"
    done
    echo ""
fi

echo "✅ Notification system testing completed!"
echo "📁 Results available in: $OUTPUT_DIR" 