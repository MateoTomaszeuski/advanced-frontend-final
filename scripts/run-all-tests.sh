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

# Clean up any existing containers/images
echo "🧹 Initial cleanup..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v  || true
docker rmi integration_test_container  || true
docker rmi test_container  || true
docker rmi linting_and_warns_container  || true
docker rmi client_test_container  || true

echo ""
echo "=========================================="
echo "🔧 BACKEND TESTS"
echo "=========================================="
echo ""

# Backend Linting and Warns
echo "📋 Backend Linting and Warns..."
docker build \
    -f dockerfiles/check.Dockerfile \
    -t linting_and_warns_container \
.

echo "✅ Backend linting passed"

# Backend Unit Tests
echo "� Running backend unit tests..."
docker build \
    -f dockerfiles/tests.Dockerfile \
    -t test_container \
.

docker run --rm test_container

docker rmi test_container -f  || true

# Backend Integration Tests
echo "� Starting API and Database containers for backend integration tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml up -d --build

echo "🔨 Building C# integration tests container..."
docker build \
    -f dockerfiles/integration-tests.Dockerfile \
    -t integration_test_container \
.

echo "🧪 Running C# integration tests..."
docker run --rm --network docker-compose_integration_tests_network integration_test_container

# Clean up backend
echo "🧹 Cleaning up backend tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v
docker rmi integration_test_container  || true
docker rmi linting_and_warns_container  || true

echo ""
echo "=========================================="
echo "⚛️  FRONTEND TESTS"
echo "=========================================="
echo ""

# Frontend Linting
echo "📋 Frontend Linting..."
cd app/client
pnpm i
pnpm run lint:ci
cd "${PROJECT_ROOT}"

# Frontend Integration Tests
echo "🐳 Starting API and Database containers for frontend integration tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml up -d --build

echo "� Building frontend integration tests container..."
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
echo "✅ All pre-check tests completed successfully!"
