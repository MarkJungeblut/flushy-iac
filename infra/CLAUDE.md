# Infrastructure Context for Claude Code

This document provides context about the infrastructure setup for the Flushy IaC project.

## Overview

The infrastructure is defined using **CDKTF (Cloud Development Kit for Terraform)** in C#, targeting **Google Cloud Platform (GCP)** Cloud Run services.

## Architecture

### CDKTF Project Structure

```
infra/flushy-infrastructure/
├── src/
│   ├── Program.cs                      # CDKTF entry point
│   ├── Flushy.Infrastructure.csproj    # .NET 8.0 project file
│   ├── cdktf.json                      # CDKTF configuration
│   ├── Config/
│   │   └── EnvironmentConfig.cs        # Environment configs (dev/staging/prod)
│   ├── Constructs/
│   │   └── CloudRunConstruct.cs        # Reusable Cloud Run pattern
│   └── Stacks/
│       └── GcpStack/
│           ├── GcpStack.cs             # Main orchestrator stack
│           ├── ArtifactRegistry.cs     # Docker image repository
│           ├── ServiceAccounts.cs      # IAM service accounts + permissions
│           └── CloudRunServices.cs     # Cloud Run API + gRPC services
```

### Design Principles

#### Modular Resource Classes
The GCP stack is split into **single-responsibility classes** for better maintainability:

- **GcpStack.cs**: Main orchestrator that coordinates all resources
- **ArtifactRegistry.cs**: Manages Google Artifact Registry for Docker images
- **ServiceAccounts.cs**: Creates service accounts with minimal IAM permissions (logging, monitoring, tracing)
- **CloudRunServices.cs**: Deploys Cloud Run services for API and gRPC

**Benefits:**
- Clear separation of concerns
- Easy to add new GCP products (Pub/Sub, Cloud SQL, etc.)
- Reusable across environments
- Testable in isolation

#### Environment Configuration
Three environments supported: `dev`, `staging`, `prod`

Each environment has specific settings for:
- Cloud Run resources (memory, CPU, instances)
- Monitoring thresholds (error rates, latency)
- Observability sampling rates

#### Free-Tier Optimization
All infrastructure is optimized for **GCP free tier**:
- Memory: 256MB (minimum)
- CPU: 1 vCPU
- Min instances: 0 (scale to zero = no cost when idle)
- Max instances: 1-2 (within free tier: 2M requests/month)
- Trace sampling: 1% (dev) to reduce Cloud Trace costs
- Minimal log retention: 7-30 days

## Key Components

### 1. Artifact Registry
- Docker repository: `{region}-docker.pkg.dev/{project-id}/flushy-services`
- Stores container images for all microservices
- Tagged by environment

### 2. Service Accounts
Each service gets a dedicated service account with **least-privilege IAM roles**:
- `roles/logging.logWriter` - Write logs to Cloud Logging
- `roles/monitoring.metricWriter` - Write custom metrics
- `roles/cloudtrace.agent` - Send trace data to Cloud Trace

### 3. Cloud Run Services
Two services deployed:
- **flushy-api-{env}**: REST API service
- **flushy-grpc-{env}**: gRPC service

Configuration:
- Health checks: `/health` endpoint
- Environment variables: `ASPNETCORE_ENVIRONMENT`, `GCP_PROJECT_ID`
- Public access (unauthenticated)
- Auto-scaling based on traffic

## Local Development

### Prerequisites
- Node.js 18+ (for cdktf-cli)
- .NET 8 SDK
- cdktf-cli: `npm install -g cdktf-cli`

### Testing Infrastructure (Zero Cost)

```bash
# Synthesize CDKTF to Terraform JSON (no GCP required)
cd infra/flushy-infrastructure/src
cdktf synth

# Review generated Terraform
cat cdktf.out/stacks/flushy-dev/cdk.tf.json
```

### Environment Selection

Set environment via:
1. Environment variable: `export FLUSHY_ENV=staging`
2. Command line: `dotnet run staging`
3. Default: `dev`

## Deployment

**⚠️ WARNING**: Deployment to GCP may incur costs!

```bash
# Preview changes (requires GCP credentials, read-only)
cd infra/flushy-infrastructure/src
cdktf diff

# Deploy to GCP (interactive confirmation)
cdktf deploy

# Destroy resources
cdktf destroy
```

## Technology Stack

- **CDKTF**: 0.21.0
- **Provider**: HashiCorp.Cdktf.Providers.Google 16.0.0
- **.NET**: 8.0
- **Target**: GCP Cloud Run (fully managed)

## Notes

- **Code Quality**: Warnings suppressed for infrastructure code (`.editorconfig`)
- **State Management**: Local state file (`terraform.flushy-dev.tfstate`)
- **No Terraform CLI Required**: Uses pre-built providers from NuGet
- **Zero Cost Testing**: Can synthesize and validate locally without GCP credentials
