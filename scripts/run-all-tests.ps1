#!/usr/bin/env pwsh

# CSET All Tests Execution Script
# This script runs all tests from the CSET solution

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [switch]$Coverage = $true,
    [switch]$Parallel = $true,
    [switch]$InstallPlaywright = $true
)

Write-Host "🎯 Running CSET All Tests..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow

# Install Playwright if requested
if ($InstallPlaywright) {
    Write-Host "📦 Installing Playwright..." -ForegroundColor Cyan
    $playwrightResult = & dotnet tool install --global Microsoft.Playwright.CLI
    if ($playwrightResult -ne 0) {
        Write-Host "⚠️  Playwright installation failed, attempting to use existing installation" -ForegroundColor Yellow
    }
    
    Write-Host "🌐 Installing Playwright browsers..." -ForegroundColor Cyan
    $browserResult = & playwright install
    if ($browserResult -ne 0) {
        Write-Host "⚠️  Browser installation failed, E2E tests may not run properly" -ForegroundColor Yellow
    }
}

# Build arguments
$arguments = @(
    "test",
    "CSETWeb_Api.sln",
    "--configuration", $Configuration,
    "--verbosity", $Verbosity,
    "--no-restore"
)

# Add coverage if requested
if ($Coverage) {
    $arguments += "--collect:XPlat Code Coverage"
    Write-Host "Code coverage collection enabled" -ForegroundColor Cyan
}

# Add parallel execution if requested
if ($Parallel) {
    $arguments += "--maxcpucount:4"
    Write-Host "Parallel execution enabled (max 4 CPUs)" -ForegroundColor Cyan
}

# Add output directory for test results
$arguments += "--logger", "trx;LogFileName=AllTests.trx"
$arguments += "--results-directory", "TestResults"

Write-Host "Executing: dotnet $($arguments -join ' ')" -ForegroundColor Gray

# Execute the test command
$result = & dotnet @arguments

# Check exit code
if ($result -eq 0) {
    Write-Host "✅ All tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Tests failed with exit code: $result" -ForegroundColor Red
    exit $result
}

Write-Host "📊 Test results saved to: TestResults/AllTests.trx" -ForegroundColor Cyan

# Generate coverage report if coverage was collected
if ($Coverage) {
    Write-Host "📈 Generating coverage report..." -ForegroundColor Cyan
    $coverageResult = & dotnet tool install --global dotnet-reportgenerator-globaltool
    if ($coverageResult -eq 0) {
        & reportgenerator "-reports:TestResults/**/coverage.opencover.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:Html;TextSummary"
        Write-Host "📊 Coverage report generated: TestResults/CoverageReport/index.html" -ForegroundColor Cyan
    }
} 