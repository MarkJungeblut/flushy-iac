# Flushy IaC - CDKTF + .NET Monorepo Implementation Plan

## Overview
Build a production-ready monorepo with CDKTF infrastructure provisioning .NET 10 services (REST API and gRPC) deployable to GCP Cloud Run, with complete local development environment using Docker Compose.

---

## Architecture Summary

### Technology Stack
- **Infrastructure**: CDKTF (C#) targeting GCP
- **Services**: .NET 10 (REST API + gRPC)
- **Local Dev**: Docker Compose
- **Cloud Platform**: GCP Cloud Run
- **Monitoring**: Cloud Logging + Cloud Monitoring + Cloud Trace (error rates, latency, cost)
- **Container Base**: .NET 10 Alpine images
- **Naming Convention**: flushy-* (e.g., flushy-api-service, flushy-grpc-service)

### Monorepo Structure
```
flushy-iac/
├── infra/
│   └── flushy-infrastructure/      # CDKTF Infrastructure
│       ├── src/                    # Infrastructure code (C#)
│       │   ├── Program.cs          # CDKTF entry point
│       │   ├── Stacks/
│       │   │   └── GcpStack.cs     # GCP resource stack
│       │   ├── Constructs/
│       │   │   └── CloudRunConstruct.cs # Reusable components
│       │   ├── Config/
│       │   │   └── EnvironmentConfig.cs
│       │   ├── Flushy.Infrastructure.csproj
│       │   └── cdktf.json
│       └── tests/                  # Infrastructure tests
├── services/
│   ├── flushy-api-service/         # REST API (.NET 10)
│   │   ├── src/                    # Application code
│   │   └── tests/                  # Unit tests
│   ├── flushy-grpc-service/        # gRPC service (.NET 10)
│   │   ├── src/                    # Application code
│   │   └── tests/                  # Unit tests
│   └── shared/                     # Shared libraries
│       └── Flushy.Shared.Configuration/  # Config & Secret Manager helpers
├── .editorconfig                   # Code style consistency
├── Directory.Build.props           # Shared MSBuild properties
├── sonar-project.properties        # SonarQube configuration
├── docker-compose.yml              # Local orchestration
├── docker-compose.sonar.yml        # SonarQube local setup
├── Makefile                        # Common tasks
└── README.md                       # Setup & documentation
```

---

## Phase 1: Foundation Setup

### 1.1 Directory Structure
Create complete monorepo structure with:
- Root-level configuration files (.gitignore, .env.example)
- Infrastructure directories (infra/cdktf, infra/docker)
- Services directories (api-service, grpc-service, shared)
- Documentation structure

### 1.2 Root Configuration Files

**Makefile** - Developer convenience commands:
```makefile
.PHONY: up down logs build test clean deploy sonar sonar-up sonar-down quality

up:           # Start local services with docker-compose
	docker-compose up -d

down:         # Stop local services
	docker-compose down

logs:         # Tail service logs
	docker-compose logs -f

build:        # Build all .NET services
	dotnet build

test:         # Run all tests with coverage
	dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

clean:        # Clean build artifacts
	dotnet clean
	find . -name "bin" -o -name "obj" | xargs rm -rf

deploy:       # Deploy to GCP via CDKTF
	cd infra/flushy-infrastructure/src && cdktf deploy

sonar-up:     # Start SonarQube locally
	docker-compose -f docker-compose.sonar.yml up -d

sonar-down:   # Stop SonarQube
	docker-compose -f docker-compose.sonar.yml down

sonar:        # Run SonarQube analysis
	dotnet sonarscanner begin /k:"flushy-iac" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="$$SONAR_TOKEN"
	dotnet build
	dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
	dotnet sonarscanner end /d:sonar.login="$$SONAR_TOKEN"

quality:      # Run code quality checks
	dotnet format --verify-no-changes
	dotnet build /warnaserror
```

**.gitignore** - Exclude:
- Terraform state files (*.tfstate*)
- .NET build artifacts (bin/, obj/)
- Docker volumes
- Environment files (.env, except .env.example)
- IDE files

**.env.example** - Template for local development:
```
GCP_PROJECT_ID=your-project-id
GCP_REGION=us-central1
ASPNETCORE_ENVIRONMENT=Development
SONAR_HOST_URL=http://localhost:9000
SONAR_TOKEN=your-sonar-token
```

**.editorconfig** - Enforce consistent coding styles:
```
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
end_of_line = lf
trim_trailing_whitespace = true
insert_final_newline = true

[*.cs]
# C# code style rules
dotnet_sort_system_directives_first = true
csharp_new_line_before_open_brace = all
csharp_prefer_braces = true
```

**Directory.Build.props** - Shared MSBuild configuration:
```xml
<Project>
  <PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisMode>All</AnalysisMode>
  </PropertyGroup>
</Project>
```

**sonar-project.properties** - SonarQube analysis config:
```
sonar.projectKey=flushy-iac
sonar.projectName=Flushy Infrastructure
sonar.sources=services/,infra/
sonar.tests=services/
sonar.test.inclusions=**/*Tests.cs
sonar.exclusions=**/bin/**,**/obj/**,**/Migrations/**
sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml
sonar.coverage.exclusions=**/*Tests.cs,**/Program.cs
```

---

## Phase 2: CDKTF Infrastructure

### 2.1 CDKTF Project Initialization
```bash
cd infra
npm install -g cdktf-cli
cdktf init --template=csharp --local --project-name=Flushy.Infrastructure
```

### 2.2 GCP Resources Stack

**infra/flushy-infrastructure/src/Program.cs** - Core infrastructure:
- Service Accounts (per service with minimal IAM)
- Artifact Registry (Docker repository)
- Cloud Run Services (api-service, grpc-service)
- Cloud Logging (log sinks, log-based metrics)
- Cloud Monitoring (dashboards, alert policies)
- Secret Manager (environment secrets)

**Key CDKTF Patterns**:
- Separate constructs for reusability (CloudRunService construct)
- Environment-based configurations (dev/staging/prod)
- GCS backend for Terraform state
- Output service URLs and endpoints

### 2.3 Cloud Run Configuration
Each service gets:
- Memory: 512MB (adjust after load testing)
- CPU: 1
- Timeout: 60s
- Health check endpoints (/health, /ready)
- Environment variables from Secret Manager
- Automatic HTTPS with custom domain support

---

## Phase 3: Shared .NET Libraries

### 3.1 Shared.Configuration
**Purpose**: Configuration management and Secret Manager integration

**Features**:
- Load appsettings.json per environment
- Google Secret Manager integration (production)
- Type-safe configuration objects
- Environment variable overrides

**Note**: Logging and Telemetry shared libraries will be designed in a later phase when we tackle observability implementation.

---

## Phase 4: REST API Service

### 4.1 Project Structure (Hexagonal Architecture)
```
services/flushy-api-service/
├── src/
│   ├── Program.cs                          # Startup, DI configuration
│   ├── Api/                                # Primary adapters (inbound)
│   │   └── Controllers/
│   │       ├── HealthController.cs         # /health, /ready endpoints
│   │       └── WeatherController.cs        # Simple example endpoint
│   ├── Application/                        # Application layer (use cases)
│   │   ├── Services/
│   │   │   └── WeatherService.cs           # Business logic
│   │   └── Interfaces/
│   │       └── IWeatherService.cs
│   └── Domain/                             # Domain layer (entities, value objects)
│       └── Models/
│           └── WeatherForecast.cs          # Domain model
├── tests/
│   └── Flushy.Api.Tests/
│       ├── Controllers/
│       │   ├── HealthControllerTests.cs    # Tests for HealthController
│       │   └── WeatherControllerTests.cs   # Tests for WeatherController
│       └── Services/
│           └── WeatherServiceTests.cs      # Tests for WeatherService
├── Dockerfile                              # Multi-stage build
├── Flushy.Api.csproj
└── appsettings.{env}.json
```

### 4.2 Key Features
- **Hexagonal Architecture** (Ports & Adapters):
  - `Api/` - Primary adapters (Controllers)
  - `Application/` - Use cases and interfaces
  - `Domain/` - Core business logic and models
- ASP.NET Core Minimal APIs for simplicity
- **Simple example**: Weather forecast endpoint (GET /api/weather)
- Health check endpoints (liveness + readiness)
- OpenAPI/Swagger (development only)
- Environment-aware configuration
- Graceful shutdown handling for Cloud Run
- **Unit tests organized by component** (one test class per controller/service)

### 4.3 Dockerfile Pattern
```dockerfile
# Multi-stage build
FROM mcr.microsoft.com/dotnet:10-alpine AS base
FROM mcr.microsoft.com/dotnet:10-sdk-alpine AS builder
# Build and publish
FROM base
# Copy published artifacts
EXPOSE 8080
```

**Benefits**: Minimal image size (~70MB), fast builds, security hardening

---

## Phase 5: gRPC Service

### 5.1 Project Structure (Hexagonal Architecture)
```
services/flushy-grpc-service/
├── src/
│   ├── Program.cs                          # Startup, DI configuration
│   ├── Api/                                # Primary adapters (inbound)
│   │   └── Grpc/
│   │       └── GreeterService.cs           # gRPC service implementation
│   ├── Application/                        # Application layer (use cases)
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
│       │   └── GreeterServiceTests.cs      # Tests for gRPC service
│       └── Services/
│           └── GreetingServiceTests.cs     # Tests for business logic
├── Dockerfile
├── Flushy.Grpc.csproj
└── appsettings.{env}.json
```

### 5.2 Key Features
- **Hexagonal Architecture** (Ports & Adapters):
  - `Api/Grpc/` - gRPC service adapters
  - `Application/` - Business logic and interfaces
  - `Domain/` - Core domain models
- Protocol Buffer definitions (contract-first)
- Health check service (gRPC standard)
- Reflection enabled (development only)
- HTTP/2 support for Cloud Run
- **Unit tests organized by component** (separate test classes for gRPC services and business logic)

### 5.3 Example Proto Definition
**Simple Greeter Service**:
```protobuf
syntax = "proto3";
option csharp_namespace = "Flushy.Grpc";

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

---

## Phase 6: Local Development Environment

### 6.1 Docker Compose Setup

**docker-compose.yml** (Main services):
```yaml
version: '3.8'
services:
  flushy-api-service:
    build:
      context: .
      dockerfile: services/flushy-api-service/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    volumes:
      - ./services/flushy-api-service:/app/src
    networks:
      - flushy-network

  flushy-grpc-service:
    build:
      context: .
      dockerfile: services/flushy-grpc-service/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    networks:
      - flushy-network

networks:
  flushy-network:
    driver: bridge
```

**docker-compose.sonar.yml** (Code quality tools):
```yaml
version: '3.8'
services:
  sonarqube:
    image: sonarqube:community
    ports:
      - "9000:9000"
    environment:
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_logs:/opt/sonarqube/logs
      - sonarqube_extensions:/opt/sonarqube/extensions
    networks:
      - flushy-network

volumes:
  sonarqube_data:
  sonarqube_logs:
  sonarqube_extensions:

networks:
  flushy-network:
    driver: bridge
```

### 6.2 Development Workflow
1. **Start**: `make up` or `docker-compose up`
2. **Access**:
   - REST API: http://localhost:5000
   - gRPC Service: http://localhost:5001
   - Swagger UI: http://localhost:5000/swagger
3. **Watch Logs**: `make logs`
4. **Stop**: `make down`

### 6.3 Hot Reload Support
- Volume mounts for code directories
- .NET watch mode for automatic rebuilds
- Rapid iteration without container rebuilds

---

## Phase 7: Configuration Management

### 7.1 Environment-Specific Config
**appsettings.json** (defaults):
```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "Monitoring": { "Enabled": true, "SamplingRate": 0.1 }
}
```

**appsettings.Development.json**:
```json
{
  "Logging": { "LogLevel": { "Default": "Debug" } },
  "EnableSwagger": true,
  "Monitoring": { "SamplingRate": 1.0 }
}
```

**appsettings.Production.json**:
```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "EnableSwagger": false
}
```

### 7.2 Secret Management
**Local**: Use .env files (gitignored)
**Production**: Google Secret Manager via CDKTF

```csharp
// Production: Load from Secret Manager
if (!builder.Environment.IsDevelopment()) {
    var secretClient = new SecretManagerServiceClient();
    var secret = secretClient.AccessSecretVersion(...);
    builder.Configuration["ConnectionString"] = secret;
}
```

---

## Phase 8: Monitoring (CDKTF Provisioning)

### 8.1 Cloud Monitoring Setup
**CDKTF provisions** (all monitoring features):
- **Error Rate Alerts**: Alert when error rate > 5%
- **Latency Tracking**: P50, P95, P99 latency metrics with alerts (> 1s)
- **Cost Monitoring**: Budget alerts and cost tracking dashboards
- **Dashboards**: Comprehensive dashboards for latency, error rate, throughput, and costs
- **Log-based metrics**: Custom metrics from structured logs
- **Uptime checks**: Health endpoint monitoring

**Note**: Detailed logging and telemetry implementation (Serilog, correlation IDs, OpenTelemetry) will be planned and implemented in a later phase.

---

## Implementation Sequence

### Step 1: Foundation (Priority: Critical)
1. Create directory structure
2. Initialize Git with .gitignore
3. Create code quality configuration files:
   - .editorconfig (consistent code style)
   - Directory.Build.props (shared MSBuild props, warnings as errors, analyzers)
   - sonar-project.properties (SonarQube config)
4. Create Makefile with quality targets
5. Create .env.example
6. Create docker-compose.sonar.yml for local SonarQube
7. Create README.md
8. Commit initial structure

### Step 2: CDKTF Infrastructure (Priority: Critical)
1. Initialize CDKTF project with C# template in infra/
2. Create Program.cs with CDKTF app initialization
3. Create GcpStack.cs with Cloud Run, monitoring, and logging resources
4. Create CloudRunConstruct.cs for reusable Cloud Run service pattern
5. Add comprehensive monitoring: error alerts, latency tracking, cost budgets
6. Test with `cdktf synth` (validate, don't deploy yet)
7. Commit CDKTF code

### Step 3: Shared Libraries (Priority: High)
1. Create Flushy.Shared.Configuration project
   - Config builders
   - Secret Manager client (basic setup)
2. Build and test locally
3. Commit shared libraries

**Note**: Logging and Telemetry libraries will be added in a later phase.

### Step 4: REST API Service (Priority: Critical)
1. Create Flushy.Api.csproj with dependencies
2. Implement hexagonal architecture structure:
   - `Api/Controllers/` folder
   - `Application/Services/` and `Application/Interfaces/` folders
   - `Domain/Models/` folder
3. Implement Program.cs with DI container configuration (basic setup, no logging/telemetry yet)
4. Add HealthController (/health, /ready) in Api/Controllers/
5. Add WeatherService (business logic) in Application/Services/
6. Add WeatherController in Api/Controllers/
7. Create WeatherForecast domain model in Domain/Models/
8. Create Dockerfile (multi-stage Alpine build)
9. Test Docker build locally
10. Add unit tests (Flushy.Api.Tests project):
    - HealthControllerTests.cs
    - WeatherControllerTests.cs
    - WeatherServiceTests.cs
11. Commit API service

### Step 5: gRPC Service (Priority: High)
1. Create Flushy.Grpc.csproj
2. Define greeter.proto (simple SayHello RPC)
3. Implement hexagonal architecture structure:
   - `Api/Grpc/` folder with gRPC services
   - `Application/Services/` and `Application/Interfaces/` folders
   - `Domain/Models/` folder
4. Implement GreetingService (business logic) in Application/Services/
5. Implement GreeterService (gRPC adapter) in Api/Grpc/
6. Create Dockerfile
7. Test Docker build locally
8. Add unit tests (Flushy.Grpc.Tests project):
   - GreeterServiceTests.cs
   - GreetingServiceTests.cs
9. Commit gRPC service

### Step 6: Docker Compose (Priority: Critical)
1. Create docker-compose.yml
2. Add both services with proper networking
3. Create docker-compose.override.yml for dev settings
4. Test full stack locally (`make up`)
5. Verify logs and endpoints
6. Commit Docker Compose config

### Step 7: Configuration & Testing (Priority: High)
1. Create appsettings files per environment
2. Test environment switching locally
3. Implement Secret Manager integration (mock locally)
4. Run full test suite with coverage (`make test`)
5. Run code quality checks (`make quality`)
6. Start SonarQube locally (`make sonar-up`)
7. Run SonarQube analysis (`make sonar`)
8. Verify code quality metrics in SonarQube dashboard
9. Document configuration approach
10. Commit configs

### Step 8: Documentation (Priority: Medium)
1. Create comprehensive README
   - Quick start guide
   - Local development setup
   - Deployment instructions
2. Add architecture diagrams
3. Create troubleshooting guide
4. Document common tasks
5. Commit documentation

### Step 9: GCP Deployment (Priority: Medium)

**Prerequisites** (user performs manually):
1. Create GCP project in Console
2. Enable required APIs:
   - Cloud Run API
   - Artifact Registry API
   - Cloud Logging API
   - Cloud Monitoring API
   - Secret Manager API
3. Create service account for CDKTF with required permissions
4. Download service account key JSON

**Deployment Steps**:
1. Set GCP credentials environment variable
2. Run `cdktf deploy` to provision infrastructure
3. Build and push Docker images to Artifact Registry
4. Deploy services to Cloud Run
5. Verify all monitoring features (error alerts, latency, costs)
6. Test deployed services
7. Validate logs in Cloud Logging

---

## Critical Files to Create

### Must Create First (Blocking)
1. **/.editorconfig** - Code style enforcement
2. **/Directory.Build.props** - Shared MSBuild configuration
3. **/sonar-project.properties** - SonarQube configuration
4. **/infra/flushy-infrastructure/src/Program.cs** - CDKTF C# entry point
5. **/infra/flushy-infrastructure/src/Stacks/GcpStack.cs** - GCP resources stack
6. **/services/flushy-api-service/Program.cs** - REST API service (basic setup)
7. **/services/shared/Flushy.Shared.Configuration/** - Configuration helpers
8. **/docker-compose.yml** - Local dev orchestration
9. **/docker-compose.sonar.yml** - SonarQube setup
10. **/Makefile** - Developer workflow with quality targets

### High Priority
7. **/services/flushy-grpc-service/Program.cs** - gRPC service
8. **/services/flushy-api-service/Api/Controllers/HealthController.cs** - Health checks
9. **/services/flushy-api-service/Application/Services/WeatherService.cs** - Business logic
10. **/services/flushy-api-service/Api/Controllers/WeatherController.cs** - REST endpoint
11. **/services/flushy-api-service/Domain/Models/WeatherForecast.cs** - Domain model
12. **/services/flushy-grpc-service/Protos/greeter.proto** - gRPC contract
13. **/services/flushy-grpc-service/Application/Services/GreetingService.cs** - Business logic
14. **/services/flushy-grpc-service/Api/Grpc/GreeterService.cs** - gRPC adapter
15. **/services/flushy-api-service/Dockerfile** - API containerization
16. **/services/flushy-grpc-service/Dockerfile** - gRPC containerization
17. **/README.md** - Documentation with GCP setup instructions

---

## Best Practices & Considerations

### Cloud Run Specific
- Container must listen on PORT environment variable (defaults to 8080)
- Startup time must be under 4 minutes
- Graceful shutdown handling for SIGTERM
- Health checks mandatory (/health for liveness, /ready for readiness)
- Stateless services only

### Security
- Least privilege IAM roles per service
- No secrets in code or environment variables (use Secret Manager)
- Alpine base images for minimal attack surface
- No root user in containers
- Regular dependency updates

### Scalability
- All services must be stateless
- Use external state stores (Cloud Storage, Cloud SQL)
- Implement health checks for proper load balancing
- Circuit breakers for external dependencies
- Monitoring thresholds: error rate > 5%, latency P99 > 1s

### Local Development
- Hot reload with volume mounts
- Debug logging enabled locally
- Swagger UI for API exploration (dev only)
- gRPC reflection for service discovery (dev only)
- Fast feedback loop with `make` commands

### Architecture
- **Hexagonal Architecture** (Ports & Adapters pattern):
  - `Api/` - Adapters (Controllers, gRPC services)
  - `Application/` - Use cases, business logic, interfaces
  - `Domain/` - Core domain models and business rules
  - Benefits: Testability, maintainability, clear separation of concerns
- **Test Organization**: One test class per production class (e.g., WeatherController → WeatherControllerTests)
- **Observability**: Logging and telemetry architecture to be designed in a later phase

### Code Quality & Consistency
- **EditorConfig**: Enforces consistent coding styles across IDEs
- **Directory.Build.props**: Centralized MSBuild configuration
  - Warnings as errors
  - Nullable reference types enabled
  - Latest C# language version
  - All analyzers enabled
- **SonarQube**: Continuous code quality inspection
  - Code smells detection
  - Security vulnerabilities scanning
  - Code coverage tracking
  - Technical debt monitoring
- **Static Analyzers**: StyleCop, .NET analyzers, SonarAnalyzer
- **Code Formatting**: dotnet format for consistent formatting

### CDKTF Patterns
- Separate constructs for reusability
- Environment configs for dev/staging/prod differences
- GCS backend for state management
- Regular `cdktf plan` to detect drift
- Document manual GCP changes

---

## Dependencies

### Required Software
- Node.js 18+ (CDKTF)
- .NET 10 SDK
- Docker Desktop
- cdktf-cli (npm install -g)
- GCP CLI (gcloud) for deployment
- dotnet-sonarscanner (dotnet tool install --global dotnet-sonarscanner)
- dotnet-format (dotnet tool install --global dotnet-format)

### NuGet Packages (per service - initial phase)
- Microsoft.AspNetCore.App (runtime)
- Google.Cloud.SecretManager.V1 (configuration)
- Grpc.AspNetCore (gRPC service only)

**Note**: Logging/telemetry packages (Serilog, OpenTelemetry, etc.) will be added in a later observability phase.

### Code Quality Packages
- coverlet.collector (code coverage)
- Microsoft.CodeAnalysis.NetAnalyzers (static analysis)
- StyleCop.Analyzers (style analysis)
- SonarAnalyzer.CSharp (SonarQube analyzer)

### NuGet Packages (CDKTF)
- HashiCorp.Cdktf
- HashiCorp.Cdktf.Providers.Google
- Constructs

---

## Success Criteria

### Local Development
- ✅ Both services start with `docker-compose up`
- ✅ API accessible at http://localhost:5000
- ✅ gRPC service accessible at http://localhost:5001
- ✅ Health endpoints return 200 OK
- ✅ Hot reload works for code changes
- ✅ Basic console logging works

### GCP Deployment
- ✅ CDKTF provisions all resources without errors
- ✅ Services deploy to Cloud Run successfully
- ✅ Public URLs accessible and returning responses
- ✅ Cloud Monitoring dashboards created
- ✅ Alert policies configured (error rate, latency, cost)
- ✅ No secrets exposed in configs

**Note**: Full logging integration (structured logs, correlation IDs, Cloud Logging) will be validated in a later observability phase.

### Code Quality
- ✅ All tests passing with >80% code coverage
- ✅ No compiler warnings (warnings treated as errors)
- ✅ SonarQube analysis passing (Quality Gate)
- ✅ Code formatting consistent (dotnet format)
- ✅ Static analysis passing (StyleCop, analyzers)
- ✅ Docker images build successfully
- ✅ No hardcoded secrets or credentials
- ✅ Comprehensive README documentation

---

## Future Enhancements (Not in Initial Scope)

- CI/CD pipelines (GitHub Actions or Cloud Build)
- Service-to-service communication
- Database integration (Cloud SQL)
- Message queues (Pub/Sub)
- API Gateway for unified entry point
- Terraform Cloud for state management
- Multi-region deployment
- Blue-green deployment strategy
- Load testing automation
- Integration with APM tools
