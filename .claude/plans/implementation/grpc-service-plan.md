# gRPC Service Plan

**Priority**: High
**Status**: 🔄 Pending

## Overview
Build a production-ready gRPC service using .NET 10 with hexagonal architecture, Protocol Buffers, observability, and containerization for GCP Cloud Run deployment.

## Goals
- Implement hexagonal architecture for gRPC service
- Create simple Greeter service as example
- Define Protocol Buffer contracts
- Integrate Serilog and OpenTelemetry
- Implement gRPC health checks
- Build optimized Docker container
- Achieve >80% test coverage

## Project Structure (Hexagonal Architecture)
```
services/flushy-grpc-service/
├── src/
│   ├── Program.cs                          # Startup, DI configuration
│   ├── Api/                                # Primary adapters (inbound)
│   │   └── Grpc/
│   │       └── GreeterService.cs           # gRPC service implementation
│   ├── Application/                        # Application layer
│   │   ├── Services/
│   │   │   └── GreetingService.cs          # Business logic
│   │   └── Interfaces/
│   │       └── IGreetingService.cs
│   ├── Domain/                             # Domain layer
│   │   └── Models/
│   │       └── Greeting.cs                 # Domain model
│   └── Protos/
│       └── greeter.proto                   # gRPC contract
├── tests/
│   └── Flushy.Grpc.Tests/
│       ├── Grpc/
│       │   └── GreeterServiceTests.cs
│       └── Services/
│           └── GreetingServiceTests.cs
├── Dockerfile
├── Flushy.Grpc.csproj
└── appsettings.{env}.json
```

## Key Features
- **Hexagonal Architecture**:
  - `Api/Grpc/` - gRPC service adapters
  - `Application/Services/` - Business logic
  - `Domain/Models/` - Core domain
- **Protocol Buffers**: Contract-first design
- **gRPC Health Checks**: Standard health service
- **Reflection**: Development only
- **HTTP/2**: Cloud Run support
- **Observability**: Serilog + OpenTelemetry

## Tasks

### 1. Create Project
```bash
cd services/flushy-grpc-service
dotnet new grpc -n Flushy.Grpc -o src
```

### 2. Add Project References
```bash
cd src
dotnet add reference ../../shared/Flushy.Shared.Configuration/Flushy.Shared.Configuration.csproj
dotnet add reference ../../shared/Flushy.Shared.Observability/Flushy.Shared.Observability.csproj
```

### 3. Add NuGet Packages
```bash
dotnet add package Grpc.AspNetCore
dotnet add package Grpc.HealthCheck
dotnet add package Grpc.AspNetCore.Server.Reflection
```

### 4. Define Proto Contract
**Protos/greeter.proto**:
```protobuf
syntax = "proto3";

option csharp_namespace = "Flushy.Grpc";

package greeter;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

### 5. Implement Hexagonal Architecture
- **Domain**: `Greeting` model
- **Application**: `IGreetingService` interface and implementation
- **API**: `GreeterService` gRPC adapter

### 6. Create Dockerfile
Multi-stage Alpine build similar to REST API.

### 7. Add Unit Tests
Test gRPC service and business logic with xUnit + Moq.

### 8. Test Locally
```bash
docker build -t flushy-grpc-service:local .
docker run -p 8080:8080 flushy-grpc-service:local
```

### 9. Commit gRPC Service

## Success Criteria
- ✅ Hexagonal architecture implemented
- ✅ gRPC service working
- ✅ Protocol Buffers defined
- ✅ Health checks responding
- ✅ Observability integrated
- ✅ Docker build successful
- ✅ Tests passing (>80% coverage)
- ✅ Code committed

## Next Step
→ [Docker Compose Plan](docker-compose-plan.md)
