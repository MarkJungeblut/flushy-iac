# REST API Service Plan

**Priority**: Critical
**Status**: 🔄 Pending

## Overview
Build a production-ready REST API service using .NET 10 with hexagonal architecture, complete observability, health checks, and containerization for GCP Cloud Run deployment.

## Goals
- Implement hexagonal architecture (Ports & Adapters pattern)
- Create a simple Weather API as example/template
- Integrate Serilog and OpenTelemetry from shared libraries
- Implement health checks for Cloud Run
- Build optimized Docker container (Alpine-based)
- Achieve >80% test coverage

## Project Structure (Hexagonal Architecture)
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
│   └── Domain/                             # Domain layer
│       └── Models/
│           └── WeatherForecast.cs          # Domain model
├── tests/
│   └── Flushy.Api.Tests/
│       ├── Controllers/
│       │   ├── HealthControllerTests.cs
│       │   └── WeatherControllerTests.cs
│       └── Services/
│           └── WeatherServiceTests.cs
├── Dockerfile                              # Multi-stage Alpine build
├── Flushy.Api.csproj
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

## Key Features
- **Hexagonal Architecture**:
  - `Api/Controllers/` - HTTP adapters (inbound)
  - `Application/Services/` - Business logic and use cases
  - `Application/Interfaces/` - Port definitions
  - `Domain/Models/` - Core domain entities
- **ASP.NET Core** with Controllers
- **Weather API Example**: Simple GET endpoint for demonstration
- **Health Checks**: `/health` (liveness), `/ready` (readiness)
- **OpenAPI/Swagger**: Development environment only
- **Observability**: Serilog + OpenTelemetry integration
- **Graceful Shutdown**: Proper SIGTERM handling for Cloud Run

## Tasks

### 1. Create Project
```bash
cd services/flushy-api-service
dotnet new webapi -n Flushy.Api -o src
cd src
# Remove default WeatherForecast.cs and WeatherForecastController.cs
rm WeatherForecast.cs Controllers/WeatherForecastController.cs
```

### 2. Add Project References
```bash
dotnet add reference ../../shared/Flushy.Shared.Configuration/Flushy.Shared.Configuration.csproj
dotnet add reference ../../shared/Flushy.Shared.Observability/Flushy.Shared.Observability.csproj
```

### 3. Add NuGet Packages
```bash
dotnet add package Microsoft.AspNetCore.HealthChecks
dotnet add package Swashbuckle.AspNetCore
```

### 4. Implement Hexagonal Architecture

#### Domain Layer
**WeatherForecast.cs** - Domain model:
```csharp
namespace Flushy.Api.Domain.Models;

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

#### Application Layer
**IWeatherService.cs** - Port (interface):
```csharp
namespace Flushy.Api.Application.Interfaces;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(int days = 5);
}
```

**WeatherService.cs** - Business logic:
- Implement `IWeatherService`
- Generate random weather forecasts
- Add logging with Serilog

#### API Layer
**HealthController.cs**:
- `GET /health` - Liveness probe (always returns 200)
- `GET /ready` - Readiness probe (checks dependencies)

**WeatherController.cs**:
- `GET /api/weather` - Returns weather forecast
- Uses `IWeatherService` via dependency injection
- Logs requests and responses

### 5. Implement Program.cs with Observability
```csharp
using Flushy.Shared.Configuration;
using Flushy.Shared.Observability;

var builder = WebApplication.CreateBuilder(args);

// Configuration with Secret Manager
builder.Configuration.AddSecretManager(builder.Environment);

// Observability (Serilog + OpenTelemetry)
builder.AddObservability(new ObservabilityOptions
{
    ServiceName = "flushy-api-service",
    ServiceVersion = "1.0.0",
    JaegerEndpoint = "http://localhost:14268/api/traces",
    EnableCloudTrace = !builder.Environment.IsDevelopment()
});

// Services
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// Swagger (dev only)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

// Middleware
app.UseCorrelationId();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready");

app.Run();
```

### 6. Create Dockerfile (Multi-Stage Alpine Build)
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/Flushy.Api.csproj", "src/"]
COPY ["../shared/Flushy.Shared.Configuration/Flushy.Shared.Configuration.csproj", "shared/Flushy.Shared.Configuration/"]
COPY ["../shared/Flushy.Shared.Observability/Flushy.Shared.Observability.csproj", "shared/Flushy.Shared.Observability/"]
RUN dotnet restore "src/Flushy.Api.csproj"

# Copy source and build
COPY . .
WORKDIR "/src/src"
RUN dotnet build "Flushy.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Flushy.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app
EXPOSE 8080

# Non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

# Cloud Run uses PORT environment variable
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Flushy.Api.dll"]
```

### 7. Create appsettings Files
**appsettings.json** (base):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}
```

**appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### 8. Create Unit Tests
```bash
dotnet new xunit -n Flushy.Api.Tests -o tests/Flushy.Api.Tests
cd tests/Flushy.Api.Tests
dotnet add reference ../../src/Flushy.Api.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

**Test classes**:
- `HealthControllerTests.cs` - Test health endpoints
- `WeatherControllerTests.cs` - Test weather endpoint
- `WeatherServiceTests.cs` - Test business logic

**Test coverage target**: >80%

### 9. Test Docker Build Locally
```bash
cd services/flushy-api-service
docker build -t flushy-api-service:local .
docker run -p 8080:8080 flushy-api-service:local
```

Test endpoints:
- http://localhost:8080/health
- http://localhost:8080/ready
- http://localhost:8080/api/weather

### 10. Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### 11. Commit API Service
Clean commit message (no attribution/generation notes).

## Dockerfile Optimization (Alpine)
**Benefits**:
- Minimal image size (~70-90MB for .NET 10 Alpine)
- Fast builds with layer caching
- Security hardening (minimal attack surface)
- Non-root user execution

**Multi-stage build**:
1. SDK image for building
2. Runtime image for deployment
3. Copy only published artifacts

## Cloud Run Configuration
Optimized for GCP free tier:
- Memory: 256MB (minimum viable)
- CPU: 1 vCPU (shared)
- Port: 8080 (Cloud Run default)
- Startup timeout: <4 minutes
- Graceful shutdown: Handle SIGTERM

## Success Criteria
- ✅ Hexagonal architecture implemented
- ✅ Weather API endpoint working
- ✅ Health checks (`/health`, `/ready`) responding
- ✅ Serilog and OpenTelemetry integrated
- ✅ Swagger UI available in development
- ✅ Docker image builds successfully
- ✅ Unit tests passing with >80% coverage
- ✅ Local testing successful
- ✅ Code committed to Git

## Next Step
→ [gRPC Service Plan](grpc-service-plan.md)

## Notes
- Keep the Weather API simple - it's just an example/template
- Focus on architecture and patterns, not feature richness
- Ensure proper logging with correlation IDs
- Optimize for GCP free tier (minimal resources)
- Test locally before containerization
