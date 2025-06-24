#!/usr/bin/env pwsh

# CSET Unit Tests Execution Script
# This script runs only unit tests from the CSET solution

param(
    [string]$Configuration = "Debug",
    [string]$Verbosity = "normal",
    [switch]$Coverage = $false,
    [switch]$Parallel = $true
)

Write-Host "🚀 Running CSET Unit Tests..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow

# Build arguments
$arguments = @(
    "test",
    "CSETWeb_Api.sln",
    "--filter", "TestCategory=Unit",
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
$arguments += "--logger", "trx;LogFileName=UnitTests.trx"
$arguments += "--results-directory", "TestResults"

Write-Host "Executing: dotnet $($arguments -join ' ')" -ForegroundColor Gray

# Execute the test command
$result = & dotnet @arguments

# Check exit code
if ($result -eq 0) {
    Write-Host "✅ Unit tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Unit tests failed with exit code: $result" -ForegroundColor Red
    exit $result
}

Write-Host "📊 Test results saved to: TestResults/UnitTests.trx" -ForegroundColor Cyan 