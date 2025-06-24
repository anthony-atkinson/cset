# CSET Test Execution Scripts

This directory contains scripts for running different types of tests in the CSET project. These scripts provide a convenient way to execute tests selectively based on their category and type.

## 📁 Scripts Overview

### PowerShell Scripts (Windows/macOS)
- `run-unit-tests.ps1` - Run only unit tests
- `run-integration-tests.ps1` - Run only integration tests  
- `run-e2e-tests.ps1` - Run only E2E tests
- `run-all-tests.ps1` - Run all tests

### Bash Scripts (Linux/macOS)
- `run-unit-tests.sh` - Run only unit tests
- `run-integration-tests.sh` - Run only integration tests
- `run-e2e-tests.sh` - Run only E2E tests
- `run-all-tests.sh` - Run all tests

## 🚀 Quick Start

### Windows/macOS (PowerShell)
```powershell
# Run unit tests only
.\scripts\run-unit-tests.ps1

# Run integration tests only
.\scripts\run-integration-tests.ps1

# Run E2E tests only
.\scripts\run-e2e-tests.ps1

# Run all tests
.\scripts\run-all-tests.ps1
```

### Linux/macOS (Bash)
```bash
# Run unit tests only
./scripts/run-unit-tests.sh

# Run integration tests only
./scripts/run-integration-tests.sh

# Run E2E tests only
./scripts/run-e2e-tests.sh

# Run all tests
./scripts/run-all-tests.sh
```

## ⚙️ Script Options

### PowerShell Scripts
```powershell
# Run with custom configuration
.\scripts\run-unit-tests.ps1 -Configuration Release

# Run with coverage
.\scripts\run-unit-tests.ps1 -Coverage

# Run without parallel execution
.\scripts\run-unit-tests.ps1 -Parallel:$false

# Run with detailed verbosity
.\scripts\run-unit-tests.ps1 -Verbosity detailed
```

### Bash Scripts
```bash
# Run with custom configuration
./scripts/run-unit-tests.sh --configuration Release

# Run with coverage
./scripts/run-unit-tests.sh --coverage

# Run without parallel execution
./scripts/run-unit-tests.sh --no-parallel

# Run with detailed verbosity
./scripts/run-unit-tests.sh --verbosity detailed
```

## 📊 Test Categories

### Unit Tests
- **Projects**: `CSETWebCore.BusinessTests`, `CSETWebCore.HelpersTests`
- **Filter**: `TestCategory=Unit`
- **Execution Time**: <30 seconds
- **Parallel**: Yes (4 CPUs)
- **Coverage**: Recommended

### Integration Tests
- **Projects**: `CSETWebCore.ApiTests`, `CSETWebCore.DatabaseManagerTests1`
- **Filter**: `TestCategory=Integration`
- **Execution Time**: <2 minutes
- **Parallel**: No (sequential)
- **Coverage**: Recommended

### E2E Tests
- **Projects**: `CSETWebCore.PlaywrightTests`
- **Filter**: `TestCategory=E2E`
- **Execution Time**: <10 minutes
- **Parallel**: No (sequential)
- **Coverage**: Not recommended
- **Dependencies**: Playwright browsers

## 🎯 Use Cases

### Development Workflow
```bash
# Quick feedback during development
./scripts/run-unit-tests.sh --coverage

# Before committing code
./scripts/run-integration-tests.sh

# Full validation before push
./scripts/run-all-tests.sh
```

### CI/CD Pipeline
```bash
# Fast feedback in PR builds
./scripts/run-unit-tests.sh --coverage --parallel

# Integration validation
./scripts/run-integration-tests.sh --coverage

# Full validation in main branch
./scripts/run-all-tests.sh --coverage
```

### Debugging
```bash
# Run specific test type with detailed output
./scripts/run-unit-tests.sh --verbosity detailed

# Run without parallel execution for easier debugging
./scripts/run-unit-tests.sh --no-parallel
```

## 📈 Output and Results

### Test Results
- **Format**: TRX (Visual Studio Test Results)
- **Location**: `TestResults/`
- **Files**: 
  - `UnitTests.trx`
  - `IntegrationTests.trx`
  - `E2ETests.trx`
  - `AllTests.trx`

### Coverage Reports
- **Format**: HTML + Text Summary
- **Location**: `TestResults/CoverageReport/`
- **Files**: 
  - `index.html` (HTML report)
  - `Summary.txt` (Text summary)

### Log Files
- **Verbosity Levels**: quiet, minimal, normal, detailed, diagnostic
- **Default**: normal
- **Recommendation**: Use `detailed` for debugging

## 🔧 Configuration

### Environment Variables
```bash
# Set custom configuration
export CSET_TEST_CONFIGURATION=Release

# Set custom verbosity
export CSET_TEST_VERBOSITY=detailed

# Disable Playwright installation
export CSET_SKIP_PLAYWRIGHT_INSTALL=true
```

### Script Parameters

#### PowerShell Parameters
- `Configuration` (string): Debug, Release (default: Debug)
- `Verbosity` (string): quiet, minimal, normal, detailed, diagnostic (default: normal)
- `Coverage` (switch): Enable code coverage collection
- `Parallel` (switch): Enable parallel execution

#### Bash Parameters
- `--configuration`: Debug, Release (default: Debug)
- `--verbosity`: quiet, minimal, normal, detailed, diagnostic (default: normal)
- `--coverage`: Enable code coverage collection
- `--parallel` / `--no-parallel`: Control parallel execution
- `--no-install-playwright`: Skip Playwright installation

## 🚨 Troubleshooting

### Common Issues

#### Playwright Installation Fails
```bash
# Manual installation
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

#### Test Execution Fails
```bash
# Check .NET version
dotnet --version

# Restore packages
dotnet restore CSETWeb_Api.sln

# Clean and rebuild
dotnet clean CSETWeb_Api.sln
dotnet build CSETWeb_Api.sln
```

#### Coverage Report Not Generated
```bash
# Install report generator manually
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate report manually
reportgenerator "-reports:TestResults/**/coverage.opencover.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:Html;TextSummary"
```

### Performance Optimization

#### For Faster Execution
```bash
# Run only fast tests
dotnet test --filter "TestExecutionType=Fast"

# Use more CPUs
dotnet test --maxcpucount:8

# Skip restore
dotnet test --no-restore
```

#### For Better Coverage
```bash
# Run with coverage
./scripts/run-unit-tests.sh --coverage

# Generate detailed coverage report
./scripts/run-all-tests.sh --coverage
```

## 📚 Related Documentation

- [UNIT_TEST_IMPLEMENTATION_TASKS.md](../UNIT_TEST_IMPLEMENTATION_TASKS.md) - Unit test implementation plan
- [PLAYWRIGHT_TESTING_TASKS.md](../PLAYWRIGHT_TESTING_TASKS.md) - Comprehensive test implementation tracker

## 🤝 Contributing

When adding new test scripts:

1. **Follow naming convention**: `run-{test-type}-tests.{ext}`
2. **Include help text**: Document all parameters
3. **Add error handling**: Check exit codes and provide meaningful messages
4. **Support both platforms**: Create both PowerShell and Bash versions
5. **Update this README**: Document new scripts and options 