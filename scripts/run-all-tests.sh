#!/bin/bash

# Local Pre-check Runner
# This script runs the same checks as the GitHub Actions workflow

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "🚀 Starting local pre-check tests..."
echo "📁 Project root: ${PROJECT_ROOT}"

# Change to project root
cd "${PROJECT_ROOT}"

echo ""
echo "=========================================="
echo "🔧 BACKEND TESTS"
echo "=========================================="
echo ""

# Backend Tests
echo "📋 Backend: Restoring dependencies..."
cd app/server
dotnet restore

echo "🔨 Backend: Building..."
dotnet build --no-restore

echo "🧪 Backend: Running tests..."
dotnet test --no-build --verbosity normal

cd "${PROJECT_ROOT}"

echo ""
echo "=========================================="
echo "🐳 INTEGRATION TESTS"
echo "=========================================="
echo ""

# Clean up any existing containers
echo "🧹 Cleaning up existing containers..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v 2>/dev/null || true

# Start integration test environment
echo "🚀 Starting integration test environment..."
docker compose -f docker-compose/docker-compose.integration-tests.yml up -d

# Wait for services to be healthy
echo "⏳ Waiting for services to be ready..."
sleep 10

# Run integration tests (both backend and any API integration tests)
echo "🧪 Running integration tests..."
INTEGRATION_TEST_EXIT_CODE=0

# Backend integration tests
cd app/server
dotnet test --filter "FullyQualifiedName~IntegrationTests" --verbosity normal || INTEGRATION_TEST_EXIT_CODE=$?
cd "${PROJECT_ROOT}"

# Clean up integration test environment
echo "🧹 Cleaning up integration test environment..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v

# Check if integration tests failed
if [ $INTEGRATION_TEST_EXIT_CODE -ne 0 ]; then
  echo ""
  echo "❌ Integration tests failed!"
  exit $INTEGRATION_TEST_EXIT_CODE
fi

echo ""
echo "=========================================="
echo "⚛️  FRONTEND TESTS"
echo "=========================================="
echo ""

# Frontend Linting
echo "📋 Frontend: Installing dependencies..."
cd app/client
pnpm install --frozen-lockfile

echo "🔍 Frontend: Running linter..."
pnpm lint

echo "🧪 Frontend: Running tests..."
pnpm test --run

cd "${PROJECT_ROOT}"

echo ""
echo "✅ All pre-check tests completed successfully!"
echo ""
echo "You can now push your changes with confidence! 🚀"
