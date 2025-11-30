# Docker Compose Plan

**Priority**: Critical
**Status**: 🔄 Pending

## Overview
Create Docker Compose configuration for local development with hot reload, service networking, and Jaeger for distributed tracing.

## Goals
- Orchestrate all services locally
- Enable hot reload for rapid development
- Set up service networking
- Add Jaeger for local trace visualization
- Provide simple commands via Makefile

## Services to Include
- `flushy-api-service` (REST API on port 5000)
- `flushy-grpc-service` (gRPC on port 5001)
- `jaeger` (Tracing UI on port 16686)

## Tasks

### 1. Create docker-compose.yml
```yaml
version: '3.8'

services:
  flushy-api-service:
    build:
      context: ./services/flushy-api-service
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - JAEGER_AGENT_HOST=jaeger
      - JAEGER_AGENT_PORT=6831
    volumes:
      - ./services/flushy-api-service/src:/app/src
    networks:
      - flushy-network
    depends_on:
      - jaeger

  flushy-grpc-service:
    build:
      context: ./services/flushy-grpc-service
      dockerfile: Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - JAEGER_AGENT_HOST=jaeger
      - JAEGER_AGENT_PORT=6831
    networks:
      - flushy-network
    depends_on:
      - jaeger

  jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: flushy-jaeger
    ports:
      - "16686:16686"  # UI
      - "14268:14268"  # Collector HTTP
      - "6831:6831/udp" # Agent
    environment:
      - COLLECTOR_ZIPKIN_HOST_PORT=:9411
    networks:
      - flushy-network

networks:
  flushy-network:
    driver: bridge
```

### 2. Create docker-compose.override.yml (Optional)
Development-specific overrides for hot reload and debugging.

### 3. Test Full Stack
```bash
make up
make logs
# Visit http://localhost:5000/swagger
# Visit http://localhost:16686 (Jaeger UI)
make down
```

### 4. Commit Docker Compose Config

## Success Criteria
- ✅ All services start with `make up`
- ✅ REST API accessible at http://localhost:5000
- ✅ gRPC service accessible at http://localhost:5001
- ✅ Jaeger UI at http://localhost:16686
- ✅ Services can communicate
- ✅ Hot reload working
- ✅ Code committed

## Next Step
→ [Configuration & Testing Plan](configuration-testing-plan.md)
