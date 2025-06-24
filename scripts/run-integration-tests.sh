#!/bin/bash

# CSET Integration Tests Execution Script
# This script runs only integration tests from the CSET solution

# Default parameters
CONFIGURATION="Debug"
VERBOSITY="normal"
COVERAGE=false
PARALLEL=false

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
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--configuration Debug|Release] [--verbosity quiet|minimal|normal|detailed|diagnostic] [--coverage] [--parallel]"
            exit 1
            ;;
    esac
done

echo "🔗 Running CSET Integration Tests..."
echo "Configuration: $CONFIGURATION"
echo "Verbosity: $VERBOSITY"

# Build arguments
ARGS=("test" "CSETWeb_Api.sln" "--filter" "TestCategory=Integration" "--configuration" "$CONFIGURATION" "--verbosity" "$VERBOSITY" "--no-restore")

# Add coverage if requested
if [ "$COVERAGE" = true ]; then
    ARGS+=("--collect:XPlat Code Coverage")
    echo "Code coverage collection enabled"
fi

# Add parallel execution if requested (integration tests typically run sequentially)
if [ "$PARALLEL" = true ]; then
    ARGS+=("--maxcpucount:2")
    echo "Parallel execution enabled (max 2 CPUs)"
fi

# Add output directory for test results
ARGS+=("--logger" "trx;LogFileName=IntegrationTests.trx" "--results-directory" "TestResults")

echo "Executing: dotnet ${ARGS[*]}"

# Execute the test command
dotnet "${ARGS[@]}"
RESULT=$?

# Check exit code
if [ $RESULT -eq 0 ]; then
    echo "✅ Integration tests completed successfully!"
else
    echo "❌ Integration tests failed with exit code: $RESULT"
    exit $RESULT
fi

echo "📊 Test results saved to: TestResults/IntegrationTests.trx" 