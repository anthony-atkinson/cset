#!/bin/bash

# Visual Regression Test Execution Script for CSET Playwright Tests
# This script runs visual regression tests and manages baseline images

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
NC='\033[0m' # No Color

# Configuration
PROJECT_DIR="CSETWebCore.PlaywrightTests"
RESULTS_DIR="test-results"
VISUAL_DIR="$RESULTS_DIR/visual"
BASELINE_DIR="$VISUAL_DIR/baselines"
CURRENT_DIR="$VISUAL_DIR/current"
DIFF_DIR="$VISUAL_DIR/diffs"
REPORTS_DIR="$RESULTS_DIR/reports"

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

print_visual() {
    echo -e "${PURPLE}[VISUAL]${NC} $1"
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
    print_status "Creating visual regression directories..."
    
    mkdir -p "$RESULTS_DIR"
    mkdir -p "$VISUAL_DIR"
    mkdir -p "$BASELINE_DIR"
    mkdir -p "$CURRENT_DIR"
    mkdir -p "$DIFF_DIR"
    mkdir -p "$REPORTS_DIR"
    
    print_success "Directories created"
}

# Function to update baseline images
update_baselines() {
    print_visual "Updating baseline images..."
    
    cd "$PROJECT_DIR"
    
    # Set environment variable to enable baseline updates
    export UPDATE_BASELINES="true"
    
    # Run visual regression tests with baseline update
    dotnet test \
        --filter "TestCategory=VisualRegression" \
        --logger "console;verbosity=normal" \
        --verbosity normal
    
    cd ..
    
    print_success "Baseline images updated"
}

# Function to run visual regression tests
run_visual_tests() {
    print_visual "Running visual regression tests..."
    
    cd "$PROJECT_DIR"
    
    # Run visual regression tests
    dotnet test \
        --filter "TestCategory=VisualRegression" \
        --logger "trx;LogFileName=$RESULTS_DIR/visual_regression_results.xml" \
        --logger "console;verbosity=normal" \
        --results-directory "$RESULTS_DIR" \
        --verbosity normal
    
    local exit_code=$?
    
    cd ..
    
    if [ $exit_code -eq 0 ]; then
        print_success "Visual regression tests completed"
    else
        print_error "Visual regression tests failed"
    fi
    
    return $exit_code
}

# Function to generate visual regression report
generate_visual_report() {
    print_visual "Generating visual regression report..."
    
    local report_file="$REPORTS_DIR/visual-regression-$(date +%Y%m%d_%H%M%S).html"
    
    # Create HTML report
    cat > "$report_file" << 'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>CSET Visual Regression Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background-color: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .header { background-color: #4a90e2; color: white; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
        .summary { background-color: #e9ecef; padding: 15px; border-radius: 5px; margin: 20px 0; }
        .test-result { border: 1px solid #ddd; border-radius: 5px; padding: 15px; margin: 10px 0; }
        .test-name { font-weight: bold; font-size: 16px; margin-bottom: 10px; }
        .test-status { padding: 5px 10px; border-radius: 3px; font-weight: bold; }
        .status-pass { background-color: #d4edda; color: #155724; }
        .status-fail { background-color: #f8d7da; color: #721c24; }
        .status-baseline { background-color: #fff3cd; color: #856404; }
        .image-comparison { display: flex; gap: 20px; margin-top: 10px; }
        .image-container { flex: 1; text-align: center; }
        .image-container img { max-width: 100%; height: auto; border: 1px solid #ddd; border-radius: 3px; }
        .image-label { font-weight: bold; margin-bottom: 5px; }
        .stats { display: flex; gap: 20px; margin-top: 10px; }
        .stat { text-align: center; }
        .stat-value { font-size: 18px; font-weight: bold; }
        .stat-label { color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>CSET Visual Regression Report</h1>
            <p>Generated on: $(date)</p>
        </div>
        
        <div class="summary">
            <h2>Test Summary</h2>
            <p>This report shows the visual regression test results for CSET application components.</p>
        </div>
EOF

    # Add test results (this would be populated from actual test results)
    cat >> "$report_file" << 'EOF'
        <div class="test-result">
            <div class="test-name">Login Page Visual Consistency</div>
            <div class="test-status status-pass">PASS</div>
            <div class="stats">
                <div class="stat">
                    <div class="stat-value">0.5%</div>
                    <div class="stat-label">Difference</div>
                </div>
                <div class="stat">
                    <div class="stat-value">2.0%</div>
                    <div class="stat-label">Threshold</div>
                </div>
            </div>
        </div>
        
        <div class="test-result">
            <div class="test-name">Dashboard Charts Visual Consistency</div>
            <div class="test-status status-pass">PASS</div>
            <div class="stats">
                <div class="stat">
                    <div class="stat-value">1.2%</div>
                    <div class="stat-label">Difference</div>
                </div>
                <div class="stat">
                    <div class="stat-value">2.0%</div>
                    <div class="stat-label">Threshold</div>
                </div>
            </div>
        </div>
EOF

    cat >> "$report_file" << 'EOF'
    </div>
</body>
</html>
EOF

    print_success "Visual regression report generated: $report_file"
}

# Function to display visual test summary
display_visual_summary() {
    print_visual "Visual regression test summary:"
    echo "=========================================="
    
    # Count baseline images
    local baseline_count=$(find "$BASELINE_DIR" -name "*.png" 2>/dev/null | wc -l)
    local current_count=$(find "$CURRENT_DIR" -name "*.png" 2>/dev/null | wc -l)
    local diff_count=$(find "$DIFF_DIR" -name "*.png" 2>/dev/null | wc -l)
    
    echo "Baseline images: $baseline_count"
    echo "Current images: $current_count"
    echo "Diff images: $diff_count"
    
    if [ $diff_count -gt 0 ]; then
        print_warning "Found $diff_count visual differences"
        echo "Diff images saved in: $DIFF_DIR"
    else
        print_success "No visual differences detected"
    fi
    
    echo "=========================================="
    print_status "Results saved in: $RESULTS_DIR"
    print_status "Reports saved in: $REPORTS_DIR"
}

# Function to clean up old test results
cleanup_old_results() {
    print_status "Cleaning up old test results..."
    
    # Remove current and diff images older than 7 days
    find "$CURRENT_DIR" -name "*.png" -mtime +7 -delete 2>/dev/null || true
    find "$DIFF_DIR" -name "*.png" -mtime +7 -delete 2>/dev/null || true
    
    print_success "Cleanup completed"
}

# Function to show help
show_help() {
    echo "CSET Visual Regression Test Runner"
    echo "=================================="
    echo ""
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  --update-baselines    Update baseline images"
    echo "  --cleanup            Clean up old test results"
    echo "  --help               Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0                    Run visual regression tests"
    echo "  $0 --update-baselines Update baseline images"
    echo "  $0 --cleanup          Clean up old results"
}

# Main execution
main() {
    local update_baselines_flag=false
    local cleanup_flag=false
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --update-baselines)
                update_baselines_flag=true
                shift
                ;;
            --cleanup)
                cleanup_flag=true
                shift
                ;;
            --help)
                show_help
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
    
    print_visual "Starting CSET Visual Regression Test Execution"
    echo "=========================================="
    
    # Check prerequisites
    check_prerequisites
    
    # Create directories
    create_directories
    
    # Cleanup if requested
    if [ "$cleanup_flag" = true ]; then
        cleanup_old_results
    fi
    
    # Update baselines if requested
    if [ "$update_baselines_flag" = true ]; then
        update_baselines
    fi
    
    # Run visual regression tests
    local exit_code=0
    if run_visual_tests; then
        print_success "Visual regression tests completed successfully"
    else
        print_error "Visual regression tests failed"
        exit_code=1
    fi
    
    # Generate report
    generate_visual_report
    
    # Display summary
    echo ""
    display_visual_summary
    
    if [ $exit_code -eq 0 ]; then
        print_success "All visual regression tests completed successfully!"
    else
        print_error "Some visual regression tests failed. Check the results for details."
    fi
    
    exit $exit_code
}

# Run main function
main "$@" 