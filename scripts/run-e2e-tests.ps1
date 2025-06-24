#!/usr/bin/env pwsh

# CSET E2E Tests Execution Script
# This script runs only E2E tests from the CSET solution

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [switch]$Coverage = $false,
    [switch]$Parallel = $false,
    [switch]$InstallPlaywright = $true
)

Write-Host "🌐 Running CSET E2E Tests..." -ForegroundColor Green
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
        Write-Host "⚠️  Browser installation failed, tests may not run properly" -ForegroundColor Yellow
    }
}

# Build arguments
$arguments = @(
    "test",
    "CSETWeb_Api.sln",
    "--filter", "TestCategory=E2E",
    "--configuration", $Configuration,
    "--verbosity", $Verbosity,
    "--no-restore"
)

# Add coverage if requested (note: E2E tests typically don't collect code coverage)
if ($Coverage) {
    $arguments += "--collect:XPlat Code Coverage"
    Write-Host "Code coverage collection enabled" -ForegroundColor Cyan
}

# Add parallel execution if requested (E2E tests typically run sequentially)
if ($Parallel) {
    $arguments += "--maxcpucount:1"
    Write-Host "Parallel execution enabled (max 1 CPU for E2E)" -ForegroundColor Cyan
}

# Add output directory for test results
$arguments += "--logger", "trx;LogFileName=E2ETests.trx"
$arguments += "--results-directory", "TestResults"

Write-Host "Executing: dotnet $($arguments -join ' ')" -ForegroundColor Gray

# Execute the test command
$result = & dotnet @arguments

# Check exit code
if ($result -eq 0) {
    Write-Host "✅ E2E tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ E2E tests failed with exit code: $result" -ForegroundColor Red
    exit $result
}

Write-Host "📊 Test results saved to: TestResults/E2ETests.trx" -ForegroundColor Cyan 