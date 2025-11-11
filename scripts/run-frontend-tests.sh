#!/bin/bash

# Frontend Pre-check Runner
# This script runs frontend checks only

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

echo "🚀 Starting frontend pre-check tests..."
echo "📁 Project root: ${PROJECT_ROOT}"

# Change to project root
cd "${PROJECT_ROOT}"

# Clean up any existing containers/images
echo "🧹 Initial cleanup..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v  || true
docker rmi client_test_container  || true

echo ""
echo "=========================================="
echo "⚛️  FRONTEND TESTS"
echo "=========================================="
echo ""

# Frontend Linting
echo "📋 Frontend Linting..."
cd app/client
pnpm i
pnpm run lint
cd "${PROJECT_ROOT}"

# Frontend Integration Tests
echo "🐳 Starting API and Database containers for frontend integration tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml up -d --build

echo "🔨 Building frontend integration tests container..."
docker build \
    -f dockerfiles/client-test.Dockerfile \
    -t client_test_container \
.

echo "🧪 Running frontend integration tests..."
docker run --rm --network docker-compose_integration_tests_network client_test_container

# Clean up frontend
echo "🧹 Cleaning up frontend tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v 
docker rmi client_test_container || true

echo ""
echo "✅ All frontend tests completed successfully!"
