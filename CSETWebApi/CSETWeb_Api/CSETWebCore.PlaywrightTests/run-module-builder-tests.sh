#!/bin/bash

# CSET Module Builder E2E Tests Runner
# This script runs Playwright E2E tests for the Module Builder functionality

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Configuration
CONFIGURATION=${1:-"Debug"}
VERBOSITY=${2:-"normal"}
RUN_HEADLESS=${3:-"true"}
CAPTURE_SCREENSHOTS=${4:-"true"}

# Directories
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
TEST_RESULTS_DIR="$PROJECT_DIR/TestResults/ModuleBuilder"
COVERAGE_DIR="$PROJECT_DIR/Coverage/ModuleBuilder"

# Test settings
TEST_PROJECT="CSETWebCore.PlaywrightTests"
TEST_FILTER="TestCategory=ModuleBuilder"
TEST_ASSEMBLY="$PROJECT_DIR/bin/$CONFIGURATION/net7.0/$TEST_PROJECT.dll"

# Logging
LOG_FILE="$TEST_RESULTS_DIR/module-builder-tests.log"
TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")

# Functions
print_header() {
    echo -e "${BLUE}"
    echo "=========================================="
    echo "  CSET Module Builder E2E Tests Runner"
    echo "=========================================="
    echo -e "${NC}"
}

print_config() {
    echo -e "${YELLOW}Configuration:${NC}"
    echo "  Configuration: $CONFIGURATION"
    echo "  Verbosity: $VERBOSITY"
    echo "  Headless: $RUN_HEADLESS"
    echo "  Screenshots: $CAPTURE_SCREENSHOTS"
    echo "  Test Results: $TEST_RESULTS_DIR"
    echo "  Coverage: $COVERAGE_DIR"
    echo ""
}

print_step() {
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

check_prerequisites() {
    print_step "Checking prerequisites..."
    
    # Check if .NET is installed
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed or not in PATH"
        exit 1
    fi
    
    # Check if Playwright is installed
    if ! command -v playwright &> /dev/null; then
        print_warning "Playwright is not installed. Installing..."
        dotnet tool install --global Microsoft.Playwright.CLI
        playwright install
    fi
    
    # Check if test project exists
    if [ ! -f "$TEST_ASSEMBLY" ]; then
        print_error "Test assembly not found: $TEST_ASSEMBLY"
        print_step "Building test project..."
        dotnet build "$PROJECT_DIR/$TEST_PROJECT.csproj" --configuration "$CONFIGURATION"
    fi
    
    print_success "Prerequisites check completed"
}

create_directories() {
    print_step "Creating output directories..."
    
    mkdir -p "$TEST_RESULTS_DIR"
    mkdir -p "$COVERAGE_DIR"
    
    print_success "Directories created"
}

setup_environment() {
    print_step "Setting up test environment..."
    
    # Set environment variables for tests
    export CSET_TEST_ENVIRONMENT="E2E"
    export CSET_TEST_CONFIGURATION="$CONFIGURATION"
    export CSET_TEST_HEADLESS="$RUN_HEADLESS"
    export CSET_TEST_SCREENSHOTS="$CAPTURE_SCREENSHOTS"
    export CSET_TEST_TIMEOUT="30000"
    export CSET_TEST_RETRIES="2"
    
    # Set Playwright environment variables
    export PLAYWRIGHT_BROWSERS_PATH="$PROJECT_DIR/.playwright"
    export PLAYWRIGHT_HEADLESS="$RUN_HEADLESS"
    
    print_success "Environment setup completed"
}

run_tests() {
    print_step "Running Module Builder E2E tests..."
    
    local test_args=(
        "test"
        "$TEST_ASSEMBLY"
        "--filter" "$TEST_FILTER"
        "--configuration" "$CONFIGURATION"
        "--verbosity" "$VERBOSITY"
        "--logger" "console;verbosity=normal"
        "--logger" "trx;LogFileName=ModuleBuilderTests_$TIMESTAMP.trx"
        "--results-directory" "$TEST_RESULTS_DIR"
        "--collect" "XPlat Code Coverage"
        "--settings" "$PROJECT_DIR/testsettings.json"
    )
    
    # Add coverage options if available
    if command -v dotnet-coverage &> /dev/null; then
        test_args+=("--collect" "XPlat Code Coverage")
    fi
    
    # Run the tests
    local exit_code=0
    dotnet "${test_args[@]}" || exit_code=$?
    
    if [ $exit_code -eq 0 ]; then
        print_success "Module Builder tests completed successfully"
    else
        print_error "Module Builder tests failed with exit code $exit_code"
    fi
    
    return $exit_code
}

generate_reports() {
    print_step "Generating test reports..."
    
    # Generate HTML report if trx file exists
    local trx_file="$TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.trx"
    if [ -f "$trx_file" ]; then
        print_step "Converting TRX to HTML report..."
        dotnet tool install --global trx2html
        trx2html "$trx_file" -o "$TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.html"
        print_success "HTML report generated: $TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.html"
    fi
    
    # Generate coverage report if available
    local coverage_file="$TEST_RESULTS_DIR/coverage.cobertura.xml"
    if [ -f "$coverage_file" ]; then
        print_step "Generating coverage report..."
        dotnet tool install --global dotnet-reportgenerator-globaltool
        reportgenerator -reports:"$coverage_file" -targetdir:"$COVERAGE_DIR" -reporttypes:Html
        print_success "Coverage report generated: $COVERAGE_DIR/index.html"
    fi
    
    print_success "Report generation completed"
}

create_summary() {
    print_step "Creating test summary..."
    
    local summary_file="$TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.summary.md"
    
    cat > "$summary_file" << EOF
# Module Builder E2E Tests Summary

**Test Run:** $TIMESTAMP  
**Configuration:** $CONFIGURATION  
**Headless:** $RUN_HEADLESS  
**Screenshots:** $CAPTURE_SCREENSHOTS  

## Test Results

EOF
    
    # Parse TRX file for summary
    local trx_file="$TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.trx"
    if [ -f "$trx_file" ]; then
        local total_tests=$(grep -c "<UnitTest" "$trx_file" || echo "0")
        local passed_tests=$(grep -c 'outcome="Passed"' "$trx_file" || echo "0")
        local failed_tests=$(grep -c 'outcome="Failed"' "$trx_file" || echo "0")
        local skipped_tests=$(grep -c 'outcome="NotExecuted"' "$trx_file" || echo "0")
        
        cat >> "$summary_file" << EOF
- **Total Tests:** $total_tests
- **Passed:** $passed_tests
- **Failed:** $failed_tests
- **Skipped:** $skipped_tests

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

- **Test Results:** [TRX File]($TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.trx)
- **HTML Report:** [HTML Report]($TEST_RESULTS_DIR/ModuleBuilderTests_$TIMESTAMP.html)
- **Coverage Report:** [Coverage Report]($COVERAGE_DIR/index.html)

## Logs

- **Test Log:** [Test Log]($LOG_FILE)

EOF
    fi
    
    print_success "Summary created: $summary_file"
}

cleanup() {
    print_step "Cleaning up temporary files..."
    
    # Remove temporary test files
    find "$TEST_RESULTS_DIR" -name "*.tmp" -delete 2>/dev/null || true
    
    print_success "Cleanup completed"
}

main() {
    print_header
    print_config
    
    # Redirect output to log file
    exec > >(tee -a "$LOG_FILE")
    exec 2>&1
    
    print_step "Starting Module Builder E2E tests at $TIMESTAMP"
    
    check_prerequisites
    create_directories
    setup_environment
    
    local test_exit_code=0
    run_tests || test_exit_code=$?
    
    generate_reports
    create_summary
    cleanup
    
    print_step "Module Builder E2E tests completed at $(date)"
    
    if [ $test_exit_code -eq 0 ]; then
        print_success "All Module Builder tests passed!"
        echo -e "${GREEN}"
        echo "=========================================="
        echo "  Module Builder Tests: PASSED"
        echo "=========================================="
        echo -e "${NC}"
    else
        print_error "Some Module Builder tests failed!"
        echo -e "${RED}"
        echo "=========================================="
        echo "  Module Builder Tests: FAILED"
        echo "=========================================="
        echo -e "${NC}"
    fi
    
    exit $test_exit_code
}

# Handle script interruption
trap 'print_error "Script interrupted by user"; exit 1' INT TERM

# Run main function
main "$@" 