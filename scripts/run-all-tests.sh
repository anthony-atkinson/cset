#!/bin/bash

# CSET All Tests Execution Script
# This script runs all tests from the CSET solution

# Default parameters
CONFIGURATION="Debug"
VERBOSITY="normal"
COVERAGE=true
PARALLEL=true
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
        --no-coverage)
            COVERAGE=false
            shift
            ;;
        --no-parallel)
            PARALLEL=false
            shift
            ;;
        --no-install-playwright)
            INSTALL_PLAYWRIGHT=false
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--configuration Debug|Release] [--verbosity quiet|minimal|normal|detailed|diagnostic] [--no-coverage] [--no-parallel] [--no-install-playwright]"
            exit 1
            ;;
    esac
done

echo "🎯 Running CSET All Tests..."
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
        echo "⚠️  Browser installation failed, E2E tests may not run properly"
    fi
fi

# Build arguments
ARGS=("test" "CSETWeb_Api.sln" "--configuration" "$CONFIGURATION" "--verbosity" "$VERBOSITY" "--no-restore")

# Add coverage if requested
if [ "$COVERAGE" = true ]; then
    ARGS+=("--collect:XPlat Code Coverage")
    echo "Code coverage collection enabled"
fi

# Add parallel execution if requested
if [ "$PARALLEL" = true ]; then
    ARGS+=("--maxcpucount:4")
    echo "Parallel execution enabled (max 4 CPUs)"
fi

# Add output directory for test results
ARGS+=("--logger" "trx;LogFileName=AllTests.trx" "--results-directory" "TestResults")

echo "Executing: dotnet ${ARGS[*]}"

# Execute the test command
dotnet "${ARGS[@]}"
RESULT=$?

# Check exit code
if [ $RESULT -eq 0 ]; then
    echo "✅ All tests completed successfully!"
else
    echo "❌ Tests failed with exit code: $RESULT"
    exit $RESULT
fi

echo "📊 Test results saved to: TestResults/AllTests.trx"

# Generate coverage report if coverage was collected
if [ "$COVERAGE" = true ]; then
    echo "📈 Generating coverage report..."
    if dotnet tool install --global dotnet-reportgenerator-globaltool; then
        reportgenerator "-reports:TestResults/**/coverage.opencover.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:Html;TextSummary"
        echo "📊 Coverage report generated: TestResults/CoverageReport/index.html"
    else
        echo "⚠️  Failed to install report generator tool"
    fi
fi 