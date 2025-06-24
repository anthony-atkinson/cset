#!/bin/bash

# CSET E2E Tests Execution Script
# This script runs only E2E tests from the CSET solution

# Default parameters
CONFIGURATION="Debug"
VERBOSITY="normal"
COVERAGE=false
PARALLEL=false
INSTALL_PLAYWRIGHT=true

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --verbosity)
            VERBOSITY="$2"
            shift 2
            ;;
        --coverage)
            COVERAGE=true
            shift
            ;;
        --parallel)
            PARALLEL=true
            shift
            ;;
        --no-install-playwright)
            INSTALL_PLAYWRIGHT=false
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--configuration Debug|Release] [--verbosity quiet|minimal|normal|detailed|diagnostic] [--coverage] [--parallel] [--no-install-playwright]"
            exit 1
            ;;
    esac
done

echo "🌐 Running CSET E2E Tests..."
echo "Configuration: $CONFIGURATION"
echo "Verbosity: $VERBOSITY"

# Install Playwright if requested
if [ "$INSTALL_PLAYWRIGHT" = true ]; then
    echo "📦 Installing Playwright..."
    if ! dotnet tool install --global Microsoft.Playwright.CLI; then
        echo "⚠️  Playwright installation failed, attempting to use existing installation"
    fi
    
    echo "🌐 Installing Playwright browsers..."
    if ! playwright install; then
        echo "⚠️  Browser installation failed, tests may not run properly"
    fi
fi

# Build arguments
ARGS=("test" "CSETWeb_Api.sln" "--filter" "TestCategory=E2E" "--configuration" "$CONFIGURATION" "--verbosity" "$VERBOSITY" "--no-restore")

# Add coverage if requested (note: E2E tests typically don't collect code coverage)
if [ "$COVERAGE" = true ]; then
    ARGS+=("--collect:XPlat Code Coverage")
    echo "Code coverage collection enabled"
fi

# Add parallel execution if requested (E2E tests typically run sequentially)
if [ "$PARALLEL" = true ]; then
    ARGS+=("--maxcpucount:1")
    echo "Parallel execution enabled (max 1 CPU for E2E)"
fi

# Add output directory for test results
ARGS+=("--logger" "trx;LogFileName=E2ETests.trx" "--results-directory" "TestResults")

echo "Executing: dotnet ${ARGS[*]}"

# Execute the test command
dotnet "${ARGS[@]}"
RESULT=$?

# Check exit code
if [ $RESULT -eq 0 ]; then
    echo "✅ E2E tests completed successfully!"
else
    echo "❌ E2E tests failed with exit code: $RESULT"
    exit $RESULT
fi

echo "📊 Test results saved to: TestResults/E2ETests.trx" 