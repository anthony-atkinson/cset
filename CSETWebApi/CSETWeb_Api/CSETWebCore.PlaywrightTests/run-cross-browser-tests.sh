#!/bin/bash

# Cross-Browser Test Execution Script for CSET Playwright Tests
# This script runs tests across multiple browsers and generates compatibility reports

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="CSETWebCore.PlaywrightTests"
RESULTS_DIR="test-results"
REPORTS_DIR="$RESULTS_DIR/reports"
BROWSERS=("chromium" "firefox" "webkit")
BROWSER_NAMES=("Chrome" "Firefox" "Safari")

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to check prerequisites
check_prerequisites() {
    print_status "Checking prerequisites..."
    
    if ! command_exists dotnet; then
        print_error "dotnet CLI is not installed"
        exit 1
    fi
    
    if ! command_exists pwsh; then
        print_warning "PowerShell Core not found, using bash for test execution"
    fi
    
    print_success "Prerequisites check completed"
}

# Function to create directories
create_directories() {
    print_status "Creating result directories..."
    
    mkdir -p "$RESULTS_DIR"
    mkdir -p "$REPORTS_DIR"
    mkdir -p "$RESULTS_DIR/screenshots"
    mkdir -p "$RESULTS_DIR/videos"
    
    print_success "Directories created"
}

# Function to install Playwright browsers
install_browsers() {
    print_status "Installing Playwright browsers..."
    
    cd "$PROJECT_DIR"
    
    # Install browsers if not already installed
    if ! dotnet tool list --global | grep -q "microsoft.playwright.cli"; then
        print_status "Installing Playwright CLI..."
        dotnet tool install --global Microsoft.Playwright.CLI
    fi
    
    print_status "Installing browsers..."
    playwright install
    
    cd ..
    
    print_success "Browsers installed"
}

# Function to run tests for a specific browser
run_browser_tests() {
    local browser=$1
    local browser_name=$2
    
    print_status "Running tests for $browser_name..."
    
    cd "$PROJECT_DIR"
    
    # Set browser-specific environment variables
    export PLAYWRIGHT_BROWSER="$browser"
    export PLAYWRIGHT_HEADLESS="true"
    
    # Run tests with browser-specific filter
    local test_filter="TestCategory=CrossBrowser"
    local result_file="$RESULTS_DIR/${browser}_results.xml"
    
    print_status "Executing tests with filter: $test_filter"
    
    dotnet test \
        --filter "$test_filter" \
        --logger "trx;LogFileName=$result_file" \
        --logger "console;verbosity=normal" \
        --results-directory "$RESULTS_DIR" \
        --verbosity normal
    
    local exit_code=$?
    
    cd ..
    
    if [ $exit_code -eq 0 ]; then
        print_success "Tests completed for $browser_name"
    else
        print_error "Tests failed for $browser_name"
    fi
    
    return $exit_code
}

# Function to generate compatibility report
generate_report() {
    print_status "Generating cross-browser compatibility report..."
    
    local report_file="$REPORTS_DIR/cross-browser-compatibility-$(date +%Y%m%d_%H%M%S).html"
    
    # Create HTML report
    cat > "$report_file" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>CSET Cross-Browser Compatibility Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background-color: #f5f5f5; padding: 20px; border-radius: 5px; }
        .browser-results { margin: 20px 0; }
        .browser-card { border: 1px solid #ddd; border-radius: 5px; padding: 15px; margin: 10px 0; }
        .browser-name { font-weight: bold; font-size: 18px; margin-bottom: 10px; }
        .pass-rate { color: #28a745; }
        .fail-rate { color: #dc3545; }
        .stats { display: flex; gap: 20px; }
        .stat { text-align: center; }
        .stat-value { font-size: 24px; font-weight: bold; }
        .stat-label { color: #666; }
        .summary { background-color: #e9ecef; padding: 15px; border-radius: 5px; margin: 20px 0; }
    </style>
</head>
<body>
    <div class="header">
        <h1>CSET Cross-Browser Compatibility Report</h1>
        <p>Generated on: $(date)</p>
    </div>
    
    <div class="summary">
        <h2>Test Summary</h2>
        <p>This report shows the compatibility of CSET application across different browsers.</p>
    </div>
EOF

    # Add browser results
    for i in "${!BROWSERS[@]}"; do
        local browser="${BROWSERS[$i]}"
        local browser_name="${BROWSER_NAMES[$i]}"
        local result_file="$RESULTS_DIR/${browser}_results.xml"
        
        if [ -f "$result_file" ]; then
            # Parse test results (simplified - in real implementation, you'd use a proper XML parser)
            local total_tests=$(grep -c "TestResult" "$result_file" || echo "0")
            local passed_tests=$(grep -c 'outcome="Passed"' "$result_file" || echo "0")
            local failed_tests=$(grep -c 'outcome="Failed"' "$result_file" || echo "0")
            
            local pass_rate=0
            if [ "$total_tests" -gt 0 ]; then
                pass_rate=$((passed_tests * 100 / total_tests))
            fi
            
            cat >> "$report_file" << EOF
    <div class="browser-results">
        <div class="browser-card">
            <div class="browser-name">$browser_name</div>
            <div class="stats">
                <div class="stat">
                    <div class="stat-value">$total_tests</div>
                    <div class="stat-label">Total Tests</div>
                </div>
                <div class="stat">
                    <div class="stat-value pass-rate">$passed_tests</div>
                    <div class="stat-label">Passed</div>
                </div>
                <div class="stat">
                    <div class="stat-value fail-rate">$failed_tests</div>
                    <div class="stat-label">Failed</div>
                </div>
                <div class="stat">
                    <div class="stat-value">${pass_rate}%</div>
                    <div class="stat-label">Pass Rate</div>
                </div>
            </div>
        </div>
    </div>
EOF
        fi
    done
    
    cat >> "$report_file" << 'EOF'
</body>
</html>
EOF

    print_success "Compatibility report generated: $report_file"
}

# Function to display summary
display_summary() {
    print_status "Cross-browser test execution summary:"
    echo "=========================================="
    
    for i in "${!BROWSERS[@]}"; do
        local browser="${BROWSERS[$i]}"
        local browser_name="${BROWSER_NAMES[$i]}"
        local result_file="$RESULTS_DIR/${browser}_results.xml"
        
        if [ -f "$result_file" ]; then
            local total_tests=$(grep -c "TestResult" "$result_file" || echo "0")
            local passed_tests=$(grep -c 'outcome="Passed"' "$result_file" || echo "0")
            local failed_tests=$(grep -c 'outcome="Failed"' "$result_file" || echo "0")
            
            echo "$browser_name: $passed_tests/$total_tests passed"
        else
            echo "$browser_name: No results found"
        fi
    done
    
    echo "=========================================="
    print_status "Results saved in: $RESULTS_DIR"
    print_status "Reports saved in: $REPORTS_DIR"
}

# Main execution
main() {
    print_status "Starting CSET Cross-Browser Test Execution"
    echo "=========================================="
    
    # Check prerequisites
    check_prerequisites
    
    # Create directories
    create_directories
    
    # Install browsers
    install_browsers
    
    # Run tests for each browser
    local overall_exit_code=0
    
    for i in "${!BROWSERS[@]}"; do
        local browser="${BROWSERS[$i]}"
        local browser_name="${BROWSER_NAMES[$i]}"
        
        echo ""
        print_status "Testing $browser_name ($browser)..."
        
        if run_browser_tests "$browser" "$browser_name"; then
            print_success "$browser_name tests completed successfully"
        else
            print_error "$browser_name tests failed"
            overall_exit_code=1
        fi
    done
    
    # Generate report
    generate_report
    
    # Display summary
    echo ""
    display_summary
    
    if [ $overall_exit_code -eq 0 ]; then
        print_success "All cross-browser tests completed successfully!"
    else
        print_error "Some cross-browser tests failed. Check the results for details."
    fi
    
    exit $overall_exit_code
}

# Run main function
main "$@" 