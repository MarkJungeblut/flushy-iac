# Shared Libraries Plan

**Priority**: High
**Status**: 🔄 Pending

## Overview
Create shared .NET libraries for configuration management and observability that can be reused across all microservices.

## Goals
- Centralize configuration management with Secret Manager integration
- Provide unified observability setup (Serilog + OpenTelemetry)
- Enable correlation ID propagation across services
- Simplify service setup with reusable components

## Projects to Create

### 1. Flushy.Shared.Configuration

**Purpose**: Configuration management and Secret Manager integration

**Features**:
- Load appsettings.json per environment (Development, Staging, Production)
- Google Secret Manager integration for production secrets
- Type-safe configuration objects
- Environment variable overrides
- Configuration validation

**Key Classes**:
- `ConfigurationBuilder` - Extension methods for configuration setup
- `SecretManagerConfigSource` - Custom configuration source for GCP Secret Manager
- `EnvironmentConfig` - Typed configuration model

**NuGet Dependencies**:
- `Google.Cloud.SecretManager.V1`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Configuration.EnvironmentVariables`

### 2. Flushy.Shared.Observability

**Purpose**: Centralized logging, tracing, and metrics configuration

**Features**:

#### Serilog Configuration
- Structured logging with JSON formatting
- **Console sink** (local development) with colored output
- **Google Cloud Logging sink** (production)
- **Enrichers**:
  - Correlation ID enricher
  - Environment enricher (machine name, environment)
  - Request context enricher (user, IP, etc.)
- Minimum log levels per environment
- Exception logging with stack traces

#### OpenTelemetry Setup
- **Tracing**:
  - Automatic instrumentation for ASP.NET Core, HttpClient, gRPC
  - Custom spans for business operations
  - **Jaeger exporter** (local development - http://localhost:16686)
  - **Google Cloud Trace exporter** (production)
  - Sampling configuration (100% local, 1% production for cost savings)
- **Metrics**:
  - Runtime metrics (CPU, memory, GC)
  - HTTP request metrics (duration, status codes, throughput)
  - Custom business metrics
  - Export to GCP Cloud Monitoring

#### Correlation ID Handling
- Generate correlation IDs for incoming requests
- Extract correlation IDs from headers (X-Correlation-ID)
- Propagate correlation IDs to downstream services
- Include in all log entries and trace spans
- Middleware for automatic correlation ID management

**Key Classes**:
- `SerilogConfiguration` - Extension methods for Serilog setup
- `OpenTelemetryConfiguration` - Extension methods for OTEL setup
- `CorrelationIdMiddleware` - ASP.NET Core middleware
- `CorrelationIdEnricher` - Serilog enricher
- `ObservabilityExtensions` - Combined setup helpers

**NuGet Dependencies**:
- **Serilog**:
  - `Serilog.AspNetCore`
  - `Serilog.Sinks.Console`
  - `Serilog.Sinks.GoogleCloudLogging`
  - `Serilog.Enrichers.Environment`
  - `Serilog.Enrichers.Thread`
  - `Serilog.Formatting.Compact`
- **OpenTelemetry**:
  - `OpenTelemetry.Extensions.Hosting`
  - `OpenTelemetry.Instrumentation.AspNetCore`
  - `OpenTelemetry.Instrumentation.Http`
  - `OpenTelemetry.Instrumentation.GrpcNetClient`
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol`
  - `OpenTelemetry.Exporter.Jaeger`
  - `Google.Cloud.Diagnostics.AspNetCore`

## Tasks

### 1. Create Flushy.Shared.Configuration Project
```bash
dotnet new classlib -n Flushy.Shared.Configuration -o services/shared/Flushy.Shared.Configuration
cd services/shared/Flushy.Shared.Configuration
dotnet add package Google.Cloud.SecretManager.V1
dotnet add package Microsoft.Extensions.Configuration
```

**Implement**:
- Configuration builder extensions
- Secret Manager integration
- Environment-based configuration loading

### 2. Create Flushy.Shared.Observability Project
```bash
dotnet new classlib -n Flushy.Shared.Observability -o services/shared/Flushy.Shared.Observability
cd services/shared/Flushy.Shared.Observability
```

**Add NuGet packages** (see dependencies above)

**Implement**:
- Serilog configuration with enrichers
- OpenTelemetry configuration (tracing + metrics)
- Correlation ID middleware
- Combined setup extension methods

### 3. Add Unit Tests
Create test projects:
- `Flushy.Shared.Configuration.Tests`
- `Flushy.Shared.Observability.Tests`

**Test coverage**:
- Configuration loading and validation
- Secret Manager integration (mocked)
- Correlation ID generation and propagation
- Serilog enrichers
- OpenTelemetry setup

### 4. Build and Test Locally
```bash
dotnet build services/shared/
dotnet test services/shared/
```

### 5. Commit Shared Libraries
Clean commit message (no attribution/generation notes).

## Usage Example

Services will use these libraries like this:

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add configuration with Secret Manager
builder.Configuration.AddSecretManager(builder.Environment);

// Add observability (Serilog + OpenTelemetry)
builder.AddObservability(new ObservabilityOptions
{
    ServiceName = "flushy-api-service",
    ServiceVersion = "1.0.0",
    JaegerEndpoint = "http://localhost:14268/api/traces", // local dev
    EnableCloudTrace = !builder.Environment.IsDevelopment()
});

var app = builder.Build();

// Use correlation ID middleware
app.UseCorrelationId();
```

## File Structure
```
services/shared/
├── Flushy.Shared.Configuration/
│   ├── ConfigurationExtensions.cs
│   ├── SecretManagerConfigSource.cs
│   ├── EnvironmentConfig.cs
│   └── Flushy.Shared.Configuration.csproj
├── Flushy.Shared.Observability/
│   ├── Serilog/
│   │   ├── SerilogConfiguration.cs
│   │   └── CorrelationIdEnricher.cs
│   ├── OpenTelemetry/
│   │   └── OpenTelemetryConfiguration.cs
│   ├── Middleware/
│   │   └── CorrelationIdMiddleware.cs
│   ├── ObservabilityExtensions.cs
│   └── Flushy.Shared.Observability.csproj
└── tests/
    ├── Flushy.Shared.Configuration.Tests/
    └── Flushy.Shared.Observability.Tests/
```

## Success Criteria
- ✅ Flushy.Shared.Configuration project created with Secret Manager integration
- ✅ Flushy.Shared.Observability project created with Serilog + OpenTelemetry
- ✅ Correlation ID middleware implemented
- ✅ Unit tests passing with >80% coverage
- ✅ `dotnet build` succeeds for all shared libraries
- ✅ Code committed to Git

## Next Step
→ [REST API Service Plan](rest-api-service-plan.md)

## Notes
- These libraries should have minimal dependencies to avoid version conflicts
- Use semantic versioning if publishing as NuGet packages
- Keep observability configuration flexible for different environments
- Optimize for GCP free tier (minimal log volume, low trace sampling in production)
