#!/bin/bash
# TerraForge Test Harness
# Minimal test script for server validation

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TEST_DIR="$SCRIPT_DIR/test"
SERVER_DIR="$SCRIPT_DIR/server/TerraForgeServer"
DOCKER_AVAILABLE=false
POSTGRES_CONTAINER="terraforge-test-postgres"
REDIS_CONTAINER="terraforge-test-redis"

# Logging helpers
log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Cleanup on exit
cleanup() {
    if [ "$DOCKER_AVAILABLE" = true ]; then
        log_info "Cleaning up Docker containers..."
        docker rm -f "$POSTGRES_CONTAINER" "$REDIS_CONTAINER" 2>/dev/null || true
    fi
}
trap cleanup EXIT

log_info "=== TerraForge Test Harness ==="
log_info "Starting test run at $(date)"
echo ""

# Step 1: Check Docker availability
log_info "Step 1: Checking Docker availability..."
if command -v docker &> /dev/null && docker info &> /dev/null; then
    DOCKER_AVAILABLE=true
    log_info "Docker is available"
else
    log_warn "Docker not available - will use mocks/simulation mode"
fi

# Step 2: Start PostgreSQL and Redis (if Docker available)
if [ "$DOCKER_AVAILABLE" = true ]; then
    log_info "Step 2: Starting PostgreSQL container..."
    docker rm -f "$POSTGRES_CONTAINER" 2>/dev/null || true
    docker run -d --name "$POSTGRES_CONTAINER" \
        -e POSTGRES_DB=terraforge_test \
        -e POSTGRES_USER=testuser \
        -e POSTGRES_PASSWORD=testpass \
        -p 5433:5432 \
        postgres:16-alpine > /dev/null 2>&1
    
    log_info "Step 3: Starting Redis container..."
    docker rm -f "$REDIS_CONTAINER" 2>/dev/null || true
    docker run -d --name "$REDIS_CONTAINER" \
        -p 6380:6379 \
        redis:7-alpine > /dev/null 2>&1
    
    log_info "Waiting for services to be ready..."
    sleep 5
    
    # Wait for PostgreSQL
    for i in {1..30}; do
        if docker exec "$POSTGRES_CONTAINER" pg_isready -U testuser -d terraforge_test > /dev/null 2>&1; then
            log_info "PostgreSQL is ready"
            break
        fi
        sleep 1
    done
    
    # Wait for Redis
    for i in {1..30}; do do
        if docker exec "$REDIS_CONTAINER" redis-cli ping > /dev/null 2>&1; then
            log_info "Redis is ready"
            break
        fi
        sleep 1
    done
else
    log_warn "Step 2/3: Skipping container startup (Docker unavailable)"
fi

# Step 4: Compile server code
log_info "Step 4: Compiling server code..."
cd "$SERVER_DIR"
if dotnet build -c Release -v quiet > /dev/null 2>&1; then
    log_info "Server compilation successful"
else
    log_error "Server compilation failed"
    exit 1
fi

# Step 5: Compile and run tests
log_info "Step 5: Compiling test suite..."
cd "$TEST_DIR"
if ! dotnet build -c Release -v quiet 2>&1; then
    log_error "Test compilation failed"
    exit 1
fi
log_info "Test compilation successful"

# Step 6: Run tests
log_info "Step 6: Running tests..."
echo ""

# Export test environment variables
if [ "$DOCKER_AVAILABLE" = true ]; then
    export TEST_DB_CONNECTION="Host=localhost;Port=5433;Database=terraforge_test;Username=testuser;Password=testpass"
    export TEST_REDIS_CONNECTION="localhost:6380,abortConnect=false"
else
    # Mock mode - tests will skip or simulate
    export TEST_DB_CONNECTION="mock"
    export TEST_REDIS_CONNECTION="mock"
fi

export TEST_MODE="$([ "$DOCKER_AVAILABLE" = true ] && echo "integration" || echo "mock")"

# Run the test executable
cd "$TEST_DIR"
if dotnet run -c Release --no-build 2>&1; then
    echo ""
    log_info "=== All tests passed ==="
    exit 0
else
    echo ""
    log_error "=== Some tests failed ==="
    exit 1
fi
