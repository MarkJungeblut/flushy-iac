# Foundation Setup Plan

**Priority**: Critical
**Status**: ✅ Completed

## Overview
Establish the foundational structure for the monorepo including directory layout, code quality tooling, and developer workflow automation.

## Goals
- Create complete directory structure for infra, services, and shared libraries
- Set up code quality enforcement (EditorConfig, StyleCop, analyzers)
- Configure SonarQube for continuous code quality inspection
- Establish developer workflow with Makefile
- Document project setup and usage

## Tasks

### 1. Create Directory Structure
```bash
infra/flushy-infrastructure/src/{Stacks,Constructs,Config}/
services/flushy-api-service/src/{Api/Controllers,Application/{Services,Interfaces},Domain/Models}/
services/flushy-api-service/tests/Flushy.Api.Tests/{Controllers,Services}/
services/flushy-grpc-service/src/{Api/Grpc,Application/{Services,Interfaces},Domain/Models,Protos}/
services/flushy-grpc-service/tests/Flushy.Grpc.Tests/{Grpc,Services}/
services/shared/Flushy.Shared.Configuration/
```

### 2. Initialize Git with .gitignore
Create comprehensive .gitignore excluding:
- .NET build artifacts (bin/, obj/)
- Terraform/CDKTF state files
- Docker volumes
- Environment files (.env)
- IDE files
- Secrets and credentials

### 3. Create Code Quality Configuration Files

#### .editorconfig
- Consistent code style across IDEs
- C# naming conventions (PascalCase, IPascalCase for interfaces)
- Indentation rules (4 spaces for C#, 2 for JSON/YAML)

#### Directory.Build.props
- Warnings treated as errors (`TreatWarningsAsErrors=true`)
- Nullable reference types enabled
- Latest C# language version
- .NET analyzers enabled (AnalysisMode=All)
- Code analyzers:
  - Microsoft.CodeAnalysis.NetAnalyzers
  - StyleCop.Analyzers
  - SonarAnalyzer.CSharp

#### sonar-project.properties
- Project configuration for SonarQube
- Coverage report paths
- Test exclusions
- Source directories

### 4. Create Makefile with Quality Targets
Developer convenience commands:
- `make build` - Build all services
- `make test` - Run tests with coverage
- `make quality` - Run code quality checks
- `make format` - Format code
- `make sonar-up/down` - Manage SonarQube
- `make sonar` - Run SonarQube analysis
- `make install-tools` - Install .NET global tools
- `make infra-test` - Test CDKTF locally (no costs)
- `make infra-synth` - Generate Terraform JSON
- `make infra-diff` - Preview infrastructure changes
- `make deploy` - Deploy to GCP (with cost warning)

### 5. Create .env.example
Template for local development environment variables:
- GCP_PROJECT_ID
- GCP_REGION
- ASPNETCORE_ENVIRONMENT
- SONAR_HOST_URL
- SONAR_TOKEN

### 6. Create docker-compose.sonar.yml
Local SonarQube setup for code quality scanning:
- SonarQube community edition
- Persistent volumes for data/logs/extensions
- Health checks
- Network configuration

### 7. Create README.md
Comprehensive documentation:
- Project overview and architecture
- Technology stack
- Prerequisites and installation
- Getting started guide
- Development workflow
- Makefile commands reference
- GCP deployment instructions
- Troubleshooting

### 8. Commit Initial Structure
Clean commit message following conventional commits format (no attribution/generation notes).

## Success Criteria
- ✅ All directories created
- ✅ Git initialized with comprehensive .gitignore
- ✅ Code quality tools configured
- ✅ Makefile with all common commands
- ✅ Documentation complete
- ✅ Initial structure committed to Git

## Files Created
- `.editorconfig`
- `Directory.Build.props`
- `stylecop.json`
- `sonar-project.properties`
- `Makefile`
- `.env.example`
- `docker-compose.sonar.yml`
- `.gitignore`
- `README.md`
- `CLAUDE.md` (project-specific AI instructions)

## Next Step
→ [CDKTF Infrastructure Plan](cdktf-infrastructure-plan.md)
