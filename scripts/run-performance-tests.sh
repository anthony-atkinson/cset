#!/bin/bash

# CSET Performance Tests Execution Script
# This script runs performance tests from the CSET solution

set -e

# Configuration
CONFIGURATION=${1:-"Debug"}
VERBOSITY=${2:-"normal"}
ENABLE_DETAILED_METRICS=${3:-"true"}
ENABLE_MEMORY_MONITORING=${4:-"true"}
ENABLE_NETWORK_MONITORING=${5:-"true"}
OUTPUT_DIR="TestResults/Performance"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "🚀 Running CSET Performance Tests..."
echo "Configuration: $CONFIGURATION"
echo "Verbosity: $VERBOSITY"
echo "Detailed Metrics: $ENABLE_DETAILED_METRICS"
echo "Memory Monitoring: $ENABLE_MEMORY_MONITORING"
echo "Network Monitoring: $ENABLE_NETWORK_MONITORING"

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Update test settings for performance testing
echo "📝 Updating test settings for performance testing..."
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
    "PerformanceMonitoring": true,
    "MemoryMonitoring": true,
    "AccessibilityTesting": false
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
    "EnablePerformanceTests": true,
    "EnableAccessibilityTests": false,
    "EnableCrossBrowserTests": false
  }
}
EOF

# Build arguments
ARGS=(
    "test"
    "CSETWeb_Api.sln"
    "--filter" "TestCategory=Performance"
    "--configuration" "$CONFIGURATION"
    "--verbosity" "$VERBOSITY"
    "--no-restore"
    "--logger" "trx;LogFileName=PerformanceTests_$TIMESTAMP.trx"
    "--results-directory" "$OUTPUT_DIR"
    "--collect:XPlat Code Coverage"
)

echo "Executing: dotnet ${ARGS[*]}"

# Execute the test command
dotnet "${ARGS[@]}"

# Check exit code
if [ $? -eq 0 ]; then
    echo "✅ Performance tests completed successfully!"
else
    echo "❌ Performance tests failed!"
    exit 1
fi

# Generate performance report
echo "📊 Generating performance report..."

# Create performance summary
cat > "$OUTPUT_DIR/performance_summary_$TIMESTAMP.md" << EOF
# CSET Performance Test Summary

**Generated:** $(date)
**Configuration:** $CONFIGURATION
**Test Run:** $TIMESTAMP

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

EOF

echo "📄 Performance summary saved to: $OUTPUT_DIR/performance_summary_$TIMESTAMP.md"

# Check for performance results files
PERFORMANCE_FILES=$(find "$OUTPUT_DIR" -name "performance_*.json" -type f 2>/dev/null || true)

if [ -n "$PERFORMANCE_FILES" ]; then
    echo "📈 Performance data files found:"
    echo "$PERFORMANCE_FILES"
    
    # Create aggregated performance report
    echo "📊 Creating aggregated performance report..."
    
    cat > "$OUTPUT_DIR/aggregated_performance_$TIMESTAMP.json" << EOF
{
  "testRun": "$TIMESTAMP",
  "configuration": "$CONFIGURATION",
  "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
  "performanceFiles": [
EOF

    for file in $PERFORMANCE_FILES; do
        echo "    \"$file\"," >> "$OUTPUT_DIR/aggregated_performance_$TIMESTAMP.json"
    done

    cat >> "$OUTPUT_DIR/aggregated_performance_$TIMESTAMP.json" << EOF
  ],
  "summary": {
    "totalTests": $(echo "$PERFORMANCE_FILES" | wc -l),
    "averageDuration": "Calculated from individual test results",
    "averageMemoryIncrease": "Calculated from individual test results",
    "averagePageLoadTime": "Calculated from individual test results"
  }
}
EOF

    echo "📄 Aggregated performance report saved to: $OUTPUT_DIR/aggregated_performance_$TIMESTAMP.json"
else
    echo "⚠️  No performance data files found"
fi

# Display test results summary
echo ""
echo "🎯 Performance Test Summary:"
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

echo "✅ Performance testing completed!"
echo "📁 Results available in: $OUTPUT_DIR" 