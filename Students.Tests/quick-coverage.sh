#!/bin/bash

# Quick Coverage Check - Minimal Output
# For detailed report, use ./run-coverage.sh

set -e

cd "$(dirname "${BASH_SOURCE[0]}")"

echo "Running tests with coverage..."

# Run tests quietly with coverage
dotnet test \
    --configuration Release \
    --verbosity quiet \
    --nologo \
    /p:CollectCoverage=true 2>&1 | grep -E "(Passed|Failed|Total|coverage)"

echo ""
echo "✅ For detailed HTML report, run: ./run-coverage.sh"
