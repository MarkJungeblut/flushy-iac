# Configuration & Testing Plan

**Priority**: High
**Status**: 🔄 Pending

## Overview
Configure environment-specific settings, test Secret Manager integration, run comprehensive test suite with code coverage, and validate code quality with SonarQube.

## Goals
- Create environment-specific configuration files
- Test Secret Manager integration (mocked locally)
- Run full test suite with >80% coverage
- Execute code quality checks
- Run SonarQube analysis
- Document configuration approach

## Tasks

### 1. Create Environment-Specific appsettings
For each service (API + gRPC):
- `appsettings.json` (base configuration)
- `appsettings.Development.json` (local dev settings)
- `appsettings.Production.json` (production settings)

**Configuration areas**:
- Logging levels
- Swagger enable/disable
- Jaeger vs Cloud Trace endpoints
- OTEL sampling rates (100% local, 1% prod)

### 2. Test Environment Switching
```bash
# Test Development
ASPNETCORE_ENVIRONMENT=Development dotnet run

# Test Production (without deploying)
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

### 3. Implement Secret Manager Integration
Add Secret Manager helper in `Flushy.Shared.Configuration`:
- Load secrets in production only
- Mock secrets in development/test environments
- Validate secret access permissions

### 4. Run Full Test Suite with Coverage
```bash
make test
# or: dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

**Coverage targets**:
- Overall: >80%
- Controllers: >80%
- Services: >90%
- Domain models: 100%

### 5. Run Code Quality Checks
```bash
make quality
# Checks:
# - Code formatting (dotnet format)
# - Build with warnings as errors
# - StyleCop rules
# - .NET analyzers
```

### 6. Start and Configure SonarQube
```bash
make sonar-up
# Wait 30-60 seconds for startup
# Visit http://localhost:9000
# Login: admin/admin (change on first login)
# Generate token: My Account → Security → Generate Tokens
```

### 7. Run SonarQube Analysis
```bash
export SONAR_TOKEN=your_token_here
make sonar
# Review results at http://localhost:9000
```

**SonarQube Quality Gate**:
- No critical/blocker issues
- <3% code duplication
- >80% code coverage
- Maintainability rating A or B

### 8. Verify All Quality Metrics
- ✅ All tests passing
- ✅ Code coverage >80%
- ✅ No compiler warnings
- ✅ SonarQube quality gate passing
- ✅ Code formatting consistent

### 9. Document Configuration Approach
Update README.md with:
- Environment configuration guide
- Secret Manager setup instructions
- Local testing procedures
- SonarQube usage

### 10. Commit Configuration & Documentation

## Configuration Files Structure
```
services/flushy-api-service/src/
├── appsettings.json              # Base
├── appsettings.Development.json  # Local dev
└── appsettings.Production.json   # Cloud Run

services/flushy-grpc-service/src/
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

## Secret Manager Usage (Production)
```csharp
if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddSecretManager(
        projectId: Environment.GetEnvironmentVariable("GCP_PROJECT_ID"),
        secretPrefix: "flushy-"
    );
}
```

## Success Criteria
- ✅ Environment-specific configs created
- ✅ Environment switching tested
- ✅ Secret Manager integration working (mocked locally)
- ✅ All tests passing with >80% coverage
- ✅ Code quality checks passing
- ✅ SonarQube analysis complete with passing quality gate
- ✅ Configuration documented
- ✅ Code committed

## Next Step
→ [Documentation Plan](documentation-plan.md)

## Notes
- Keep secrets out of appsettings files (use Secret Manager or .env)
- Use lowest log levels in production to minimize costs
- Test configuration loading in unit tests
- Validate SonarQube metrics before committing
