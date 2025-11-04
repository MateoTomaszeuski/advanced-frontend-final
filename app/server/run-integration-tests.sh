#!/bin/bash

# Backend Pre-check Runner
# This script runs backend linting, unit tests, and integration tests

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

echo "🚀 Starting backend pre-check tests..."
echo "📁 Project root: ${PROJECT_ROOT}"

# Change to server directory
cd "${SCRIPT_DIR}"

# Clean up any existing containers
echo "🧹 Initial cleanup..."
docker compose -f "${PROJECT_ROOT}/docker-compose/docker-compose.integration-tests.yml" down -v || true

echo ""
echo "=========================================="
echo "🔧 BACKEND TESTS"
echo "=========================================="
echo ""

# Backend Build Check
echo "� Building backend..."
dotnet build

# Backend Unit Tests
echo "🧪 Running backend unit tests..."
dotnet test API.UnitTests/API.UnitTests.csproj --verbosity normal

# Backend Integration Tests
echo "🐳 Starting Database for integration tests..."
docker compose -f "${PROJECT_ROOT}/docker-compose/docker-compose.integration-tests.yml" up -d

echo "⏳ Waiting for database to be ready..."
sleep 5

echo "🧪 Running backend integration tests..."
dotnet test API.IntegrationTests/API.IntegrationTests.csproj --verbosity normal

# Clean up backend
echo "🧹 Cleaning up test containers..."
docker compose -f "${PROJECT_ROOT}/docker-compose/docker-compose.integration-tests.yml" down -v

echo ""
echo "✅ Backend pre-check tests completed successfully!"
