#!/bin/bash

# Test Coverage Script for Students API
# Runs tests with coverage collection and generates HTML reports

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"
RESULTS_DIR="$PROJECT_DIR/TestResults"
COVERAGE_THRESHOLD=80

echo "=========================================="
echo "Students API - Test Coverage Report"
echo "=========================================="
echo ""

# Clean previous results
echo "🧹 Cleaning previous test results..."
rm -rf "$RESULTS_DIR"
mkdir -p "$RESULTS_DIR"

# Install ReportGenerator if not already installed
if ! dotnet tool list -g | grep -q "dotnet-reportgenerator-globaltool"; then
    echo "📦 Installing ReportGenerator tool..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
else
    echo "✅ ReportGenerator already installed"
fi

echo ""
echo "🧪 Running tests with coverage collection..."
echo ""

# Run tests with coverage using Coverlet (settings from .csproj)
dotnet test "$PROJECT_DIR/Students.Tests.csproj" \
    --configuration Release \
    --logger "console;verbosity=minimal" \
    /p:CollectCoverage=true

# Check if coverage file was generated
if [ ! -f "$RESULTS_DIR/coverage.cobertura.xml" ]; then
    echo ""
    echo "❌ Error: Coverage file not generated!"
    exit 1
fi

echo ""
echo "📊 Generating HTML coverage report..."

# Generate HTML report
reportgenerator \
    -reports:"$RESULTS_DIR/coverage.cobertura.xml" \
    -targetdir:"$RESULTS_DIR/coverage-report" \
    -reporttypes:"Html;TextSummary;Badges" \
    -classfilters:"-*.Tests.*;-*.Program;-*Migrations*"

echo ""
echo "=========================================="
echo "Coverage Summary"
echo "=========================================="

# Display text summary
cat "$RESULTS_DIR/coverage-report/Summary.txt"

echo ""
echo "=========================================="

# Extract line coverage percentage
LINE_COVERAGE=$(grep -oP 'Line coverage: \K[0-9.]+' "$RESULTS_DIR/coverage-report/Summary.txt" || echo "0")

echo ""
echo "📍 Line Coverage: ${LINE_COVERAGE}%"
echo "🎯 Threshold: ${COVERAGE_THRESHOLD}%"
echo ""

# Check threshold
if (( $(echo "$LINE_COVERAGE < $COVERAGE_THRESHOLD" | bc -l) )); then
    echo "⚠️  Warning: Coverage is below threshold!"
    echo "   Current: ${LINE_COVERAGE}% | Required: ${COVERAGE_THRESHOLD}%"
    echo ""
    echo "📄 Full report: $RESULTS_DIR/coverage-report/index.html"
    exit 0  # Don't fail build, just warn
else
    echo "✅ Coverage meets threshold requirement!"
    echo ""
fi

echo "📄 Full HTML report generated at:"
echo "   $RESULTS_DIR/coverage-report/index.html"
echo ""
echo "🌐 Open in browser:"
echo "   xdg-open $RESULTS_DIR/coverage-report/index.html"
echo ""
echo "=========================================="
