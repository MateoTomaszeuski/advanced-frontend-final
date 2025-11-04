#!/bin/bash

# Backend Pre-check Runner
# This script runs backend linting, unit tests, and integration tests

set -e

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

echo "🚀 Starting backend pre-check tests..."
echo "📁 Project root: ${PROJECT_ROOT}"

# Change to project root
cd "${PROJECT_ROOT}"

# Clean up any existing containers/images
echo "🧹 Initial cleanup..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v || true
docker rmi integration_test_container || true
docker rmi test_container || true
docker rmi linting_and_warns_container || true

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

# Backend Unit Tests
echo "🧪 Running backend unit tests..."
docker build \
    -f dockerfiles/tests.Dockerfile \
    -t test_container \
    .

docker run test_container

docker rmi test_container -f || true

# Backend Integration Tests
echo "🐳 Starting API and Database containers for backend integration tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml up -d --build

# Next line is if you get errors when running the script
# DOCKER_BUILDKIT=0 docker compose -f docker-compose/docker-compose.integration-tests.yml up -d --build

echo "🔨 Building C# integration tests container..."
docker build \
    -f dockerfiles/integration-tests.Dockerfile \
    -t integration_test_container \
    .

echo "🧪 Running C# integration tests..."
docker run --rm --network integration_tests_network integration_test_container

# Clean up backend
echo "🧹 Cleaning up backend tests..."
docker compose -f docker-compose/docker-compose.integration-tests.yml down -v
docker rmi integration_test_container || true
docker rmi linting_and_warns_container || true

echo ""
echo "✅ Backend pre-check tests completed successfully!"
