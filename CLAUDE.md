# Claude Code Instructions for Flushy IaC Project

This file contains project-specific instructions and preferences for Claude Code when working on this repository.

## Git Commit Guidelines

**IMPORTANT**: Do NOT add the following to commit messages:
- ❌ "🤖 Generated with [Claude Code](https://claude.com/claude-code)"
- ❌ "Co-Authored-By: Claude <noreply@anthropic.com>"

Keep commit messages clean and focused on the actual changes.

### Commit Message Format

Use conventional commits format:
```
feat: Add feature description
fix: Fix bug description
docs: Update documentation
refactor: Refactor code
test: Add tests
chore: Maintenance tasks
```

### Commit Message Structure
```
<type>: <short description>

<detailed description with bullet points>
- Point 1
- Point 2

<any relevant context or breaking changes>
```

## Project-Specific Context

### Architecture
- **Hexagonal Architecture** (Ports & Adapters pattern) for all services
- `Api/` - Adapters (Controllers, gRPC services)
- `Application/` - Use cases, business logic, interfaces
- `Domain/` - Core domain models and business rules

### Technology Stack
- **.NET 10** - Latest .NET version
- **CDKTF with C#** - Infrastructure as Code
- **GCP Cloud Run** - Deployment target
- **Docker Alpine** - Base images for minimal size
- **SonarQube** - Code quality scanning
- **Serilog** - Structured logging
- **OpenTelemetry** - Metrics, tracing, and telemetry
- **Jaeger** - Local distributed tracing (development)

### Code Quality Standards
- Warnings treated as errors (TreatWarningsAsErrors=true)
- Nullable reference types enabled
- StyleCop, .NET analyzers, and SonarAnalyzer all enabled
- Code coverage target: >80%
- All code must pass `make quality` before committing

### Testing Guidelines
- One test class per production class
- Test organization mirrors source structure
- Example: `WeatherController.cs` → `WeatherControllerTests.cs`
- Use xUnit for testing framework
- Mock external dependencies

### Naming Conventions
- Services: `flushy-*` (e.g., flushy-api-service, flushy-grpc-service)
- C# files: PascalCase
- Interfaces: IPascalCase (prefix with I)
- Test classes: {ClassName}Tests

### Development Workflow
1. Make changes
2. Run `make test` to verify tests pass
3. Run `make quality` to check code quality
4. Run `make format` if formatting issues exist
5. Commit with clean commit messages (no attribution/generation notes)

### File Organization
- Keep hexagonal architecture structure
- Controllers/gRPC services in `Api/`
- Business logic in `Application/Services/`
- Interfaces in `Application/Interfaces/`
- Domain models in `Domain/Models/`

### Security
- Never commit secrets, credentials, or keys
- Use Google Secret Manager for production secrets
- Use .env files for local development (gitignored)
- Service accounts should have least privilege IAM roles

### Documentation
- XML comments for public APIs
- README.md kept up to date
- Inline comments only when logic isn't self-evident
- Avoid over-commenting obvious code

### Observability & Telemetry
- **Logging**: Serilog with structured logging (JSON format)
  - Enrichers: Correlation IDs, request context, environment info
  - Sinks: Console (local), Google Cloud Logging (production)
- **Tracing**: OpenTelemetry with automatic instrumentation
  - ASP.NET Core, HttpClient, gRPC automatic tracing
  - Export to GCP Cloud Trace (production) and Jaeger (local dev)
- **Metrics**: OpenTelemetry metrics
  - Custom business metrics + automatic runtime metrics
  - Export to GCP Cloud Monitoring
- **Correlation**: Request correlation IDs propagated across services
- **Local Development**: Jaeger UI for viewing traces (http://localhost:16686)

## Implementation Progress

### ✅ Completed
- [x] Step 1: Foundation Setup

### 🔄 In Progress
- [ ] Step 2: CDKTF Infrastructure

### 📋 Pending
- [ ] Step 3: Shared Libraries
- [ ] Step 4: REST API Service
- [ ] Step 5: gRPC Service
- [ ] Step 6: Docker Compose
- [ ] Step 7: Configuration & Testing
- [ ] Step 8: Documentation
- [ ] Step 9: GCP Deployment

## Common Commands

```bash
make help          # Show all available commands
make build         # Build all services
make test          # Run tests with coverage
make quality       # Run code quality checks
make format        # Format code
make sonar-up      # Start SonarQube locally
make sonar         # Run SonarQube analysis
make up            # Start services locally
make down          # Stop services

# Infrastructure (CDKTF) - Local Testing
make infra-test    # Test CDKTF locally (no GCP, no costs)
make infra-synth   # Generate Terraform JSON
make infra-diff    # Preview changes (requires GCP creds, read-only)
make deploy        # Deploy to GCP (WARNING: costs!)
```

## Implementation Plans

This project follows modular implementation plans located in `.claude/plans/implementation/`:

1. **[Foundation Plan](/.claude/plans/implementation/foundation-plan.md)** ✅ - Directory structure, code quality tools, Makefile
2. **[CDKTF Infrastructure Plan](/.claude/plans/implementation/cdktf-infrastructure-plan.md)** 🔄 - GCP infrastructure as code
3. **[Shared Libraries Plan](/.claude/plans/implementation/shared-libraries-plan.md)** 🔄 - Configuration + Observability libraries
4. **[REST API Service Plan](/.claude/plans/implementation/rest-api-service-plan.md)** 🔄 - .NET 10 REST API with hexagonal architecture
5. **[gRPC Service Plan](/.claude/plans/implementation/grpc-service-plan.md)** 🔄 - .NET 10 gRPC service
6. **[Docker Compose Plan](/.claude/plans/implementation/docker-compose-plan.md)** 🔄 - Local development orchestration
7. **[Configuration & Testing Plan](/.claude/plans/implementation/configuration-testing-plan.md)** 🔄 - Environment configs, testing, SonarQube
8. **[Documentation Plan](/.claude/plans/implementation/documentation-plan.md)** 🔄 - README, architecture docs, troubleshooting
9. **[GCP Deployment Plan](/.claude/plans/implementation/gcp-deployment-plan.md)** 🔄 - Production deployment to GCP Cloud Run

**Master Plan**: [steady-wondering-cloud.md](/.claude/plans/steady-wondering-cloud.md) (overview and architecture)

## Important Notes

- Always reference the relevant plan for detailed requirements
- Plans are modular - update individual files as needed
- Maintain hexagonal architecture throughout
- Code quality gates must pass before merging
- Test coverage must remain above 80%
- **ZERO COST REQUIREMENT**: This project should incur NO costs. Use GCP free tier only, minimal resources, and be cost-conscious in all infrastructure decisions
