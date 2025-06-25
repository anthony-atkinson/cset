#!/bin/bash

# CSET Coverage Runner Script
# This script runs test coverage for both backend and frontend components

set -e

echo "🧪 CSET Test Coverage Runner"
echo "=============================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

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

# Check if we're in the right directory
if [ ! -f "CSET_ENHANCEMENT_TASKS.md" ]; then
    print_error "Please run this script from the CSET root directory"
    exit 1
fi

# Parse command line arguments
BACKEND_ONLY=false
FRONTEND_ONLY=false
VERBOSE=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --backend-only)
            BACKEND_ONLY=true
            shift
            ;;
        --frontend-only)
            FRONTEND_ONLY=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --backend-only    Run only backend coverage"
            echo "  --frontend-only   Run only frontend coverage"
            echo "  --verbose         Enable verbose output"
            echo "  --help            Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                    # Run both backend and frontend coverage"
            echo "  $0 --backend-only     # Run only backend coverage"
            echo "  $0 --frontend-only    # Run only frontend coverage"
            echo "  $0 --verbose          # Run with verbose output"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Backend Coverage
if [ "$FRONTEND_ONLY" = false ]; then
    echo ""
    print_status "Running Backend Coverage (.NET)..."
    
    if [ ! -d "CSETWebApi/CSETWeb_Api" ]; then
        print_error "Backend directory not found: CSETWebApi/CSETWeb_Api"
        exit 1
    fi
    
    cd CSETWebApi/CSETWeb_Api
    
    # Check if .NET is available
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed or not in PATH"
        exit 1
    fi
    
    print_status "Restoring dependencies..."
    if [ "$VERBOSE" = true ]; then
        dotnet restore
    else
        dotnet restore --verbosity quiet
    fi
    
    print_status "Running tests with coverage..."
    if [ "$VERBOSE" = true ]; then
        dotnet test --collect:"XPlat Code Coverage" \
            --results-directory ./coverage \
            --logger trx \
            --verbosity normal \
            --configuration Release \
            --no-restore
    else
        dotnet test --collect:"XPlat Code Coverage" \
            --results-directory ./coverage \
            --logger trx \
            --verbosity minimal \
            --configuration Release \
            --no-restore
    fi
    
    # Check if coverage report was generated
    if [ -d "./coverage" ] && [ "$(ls -A ./coverage)" ]; then
        print_success "Backend coverage completed successfully!"
        print_status "Coverage reports available in: CSETWebApi/CSETWeb_Api/coverage/"
        
        # Try to generate HTML report if reportgenerator is available
        if command -v reportgenerator &> /dev/null; then
            print_status "Generating HTML coverage report..."
            reportgenerator \
                -reports:./coverage/*/cobertura.xml \
                -targetdir:./coverage/report \
                -reporttypes:Html;Cobertura;JsonSummary \
                -verbosity:Info
            print_success "HTML report generated: CSETWebApi/CSETWeb_Api/coverage/report/index.html"
        else
            print_warning "reportgenerator not found. Install with: dotnet tool install --global dotnet-reportgenerator-globaltool"
        fi
    else
        print_error "Backend coverage failed - no coverage data generated"
        exit 1
    fi
    
    cd ../..
fi

# Frontend Coverage
if [ "$BACKEND_ONLY" = false ]; then
    echo ""
    print_status "Running Frontend Coverage (Angular)..."
    
    if [ ! -d "CSETWebNg" ]; then
        print_error "Frontend directory not found: CSETWebNg"
        exit 1
    fi
    
    cd CSETWebNg
    
    # Check if Node.js is available
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed or not in PATH"
        exit 1
    fi
    
    # Check if npm is available
    if ! command -v npm &> /dev/null; then
        print_error "npm is not installed or not in PATH"
        exit 1
    fi
    
    print_status "Installing dependencies..."
    if [ "$VERBOSE" = true ]; then
        npm ci
    else
        npm ci --silent
    fi
    
    print_status "Running tests with coverage..."
    if [ "$VERBOSE" = true ]; then
        npm run test:coverage
    else
        npm run test:coverage --silent
    fi
    
    # Check if coverage report was generated
    if [ -d "./coverage" ] && [ "$(ls -A ./coverage)" ]; then
        print_success "Frontend coverage completed successfully!"
        print_status "Coverage reports available in: CSETWebNg/coverage/"
        
        if [ -f "./coverage/index.html" ]; then
            print_success "HTML report generated: CSETWebNg/coverage/index.html"
        fi
    else
        print_error "Frontend coverage failed - no coverage data generated"
        exit 1
    fi
    
    cd ..
fi

echo ""
print_success "Coverage run completed successfully!"
echo ""
print_status "Coverage Reports:"
if [ "$FRONTEND_ONLY" = false ]; then
    echo "  Backend: CSETWebApi/CSETWeb_Api/coverage/"
fi
if [ "$BACKEND_ONLY" = false ]; then
    echo "  Frontend: CSETWebNg/coverage/"
fi
echo ""
print_status "For more information, see: TEST_COVERAGE_GUIDE.md" 