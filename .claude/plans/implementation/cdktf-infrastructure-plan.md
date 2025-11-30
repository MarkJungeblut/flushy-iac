# CDKTF Infrastructure Plan

**Priority**: Critical
**Status**: 🔄 Pending

## Overview
Initialize and configure CDKTF (Cloud Development Kit for Terraform) in C# to provision GCP infrastructure including Cloud Run services, monitoring, logging, and observability resources.

## Goals
- Initialize CDKTF project with C# template
- Create reusable infrastructure constructs
- Provision GCP Cloud Run services
- Set up monitoring and alerting
- Enable local testing without GCP costs

## Prerequisites
- Node.js 18+
- .NET 10 SDK
- cdktf-cli (`npm install -g cdktf-cli`)
- GCP CLI (for deployment only)

## Tasks

### 1. Initialize CDKTF Project
```bash
cd infra
cdktf init --template=csharp --local --project-name=Flushy.Infrastructure
```

### 2. Create Program.cs (CDKTF Entry Point)
Main application that:
- Initializes CDKTF app
- Creates GCP stack
- Configures outputs (service URLs, endpoints)

### 3. Create GcpStack.cs (Main Stack)
Core infrastructure resources:
- **Service Accounts**: Per-service with minimal IAM permissions
- **Artifact Registry**: Docker repository for container images
- **Cloud Run Services**:
  - flushy-api-service (REST API)
  - flushy-grpc-service (gRPC)
- **Cloud Logging**: Log sinks and log-based metrics
- **Cloud Monitoring**: Dashboards and alert policies
- **Secret Manager**: Environment secrets management

**Key CDKTF Patterns**:
- Separate constructs for reusability
- Environment-based configurations (dev/staging/prod)
- GCS backend for Terraform state (optional, for team collaboration)
- Output service URLs and endpoints

### 4. Create CloudRunConstruct.cs (Reusable Component)
Reusable construct for Cloud Run services with:
- **Resource Configuration** (FREE TIER OPTIMIZED):
  - Memory: 256MB (minimum for .NET Alpine)
  - CPU: 1 vCPU (shared)
  - Min instances: 0 (scale to zero for cost savings)
  - Max instances: 1-2 (stay within free tier)
  - Concurrency: 80 (default)
  - Timeout: 60s
- **Health Checks**:
  - Liveness: `/health`
  - Readiness: `/ready`
- **Environment Variables**: From Secret Manager
- **IAM**: Unauthenticated access (or authenticated as needed)
- **Logging**: Automatic log collection to Cloud Logging

### 5. Add Comprehensive Monitoring
**FREE TIER OPTIMIZED MONITORING**:
- **Error Rate Alerts**: Alert when error rate > 5% (basic alerting)
- **Latency Tracking**: P95, P99 latency metrics
- **Cost Monitoring**: Budget alerts (set to $0 with notification threshold)
- **Dashboards**: Basic dashboards for:
  - Request latency
  - Error rates
  - Request throughput
- **Log-based Metrics**: Custom metrics from structured logs (minimal)
- **Uptime Checks**: Health endpoint monitoring (limited frequency)
- **Trace Sampling**: 1% sampling rate (reduce Cloud Trace costs)

**Cost Optimization**:
- Minimal log retention (7-30 days)
- Reduced metrics frequency
- Low uptime check frequency
- Minimal trace sampling

### 6. Local Testing (No GCP Required, Zero Cost)
Test infrastructure code locally before deployment:

```bash
# Synthesize CDKTF to Terraform JSON
make infra-synth
# or: cd infra/Flushy.Infrastructure && cdktf synth

# Test infrastructure locally (synthesis + validation)
make infra-test

# Review generated Terraform
cat infra/Flushy.Infrastructure/cdktf.out/stacks/*/cdk.tf.json

# Preview changes (requires GCP credentials, read-only)
make infra-diff
# or: cd infra/Flushy.Infrastructure && cdktf diff
```

**Optional**: Unit tests for constructs using xUnit:
- Test construct creation
- Validate resource properties
- Check IAM configurations

### 7. Commit CDKTF Code
Commit with clean message (no attribution/generation notes).

## NuGet Packages Required
```xml
<PackageReference Include="HashiCorp.Cdktf" Version="latest" />
<PackageReference Include="HashiCorp.Cdktf.Providers.Google" Version="latest" />
<PackageReference Include="Constructs" Version="latest" />
```

## Cloud Run Configuration (FREE TIER)
Each service configuration:
- Memory: **256MB** (minimum viable)
- CPU: **1 vCPU** (shared)
- Min instances: **0** (scale to zero)
- Max instances: **1-2** (stay within free tier: 2M requests/month)
- Timeout: 60s
- Health checks: `/health`, `/ready`
- Container port: 8080
- Environment: From Secret Manager

**GCP Free Tier Limits**:
- Cloud Run: 2M requests/month, 360,000 GB-seconds compute time
- Cloud Logging: 50 GB/month
- Cloud Monitoring: Free for GCP resources
- Cloud Trace: First 2.5M spans/month free

## File Structure
```
infra/Flushy.Infrastructure/
├── Program.cs                 # CDKTF entry point
├── Stacks/
│   └── GcpStack.cs           # Main GCP resources stack
├── Constructs/
│   └── CloudRunConstruct.cs  # Reusable Cloud Run pattern
├── Config/
│   └── EnvironmentConfig.cs  # Environment-specific configs
├── Flushy.Infrastructure.csproj
└── cdktf.json                # CDKTF configuration
```

## Success Criteria
- ✅ CDKTF project initialized with C# template
- ✅ Program.cs and GcpStack.cs created
- ✅ CloudRunConstruct.cs for reusable patterns
- ✅ Monitoring and logging resources configured
- ✅ `cdktf synth` succeeds (generates valid Terraform)
- ✅ `cdktf diff` shows expected resources (read-only check)
- ✅ All configurations optimized for GCP free tier
- ✅ Code committed to Git

## Next Step
→ [Shared Libraries Plan](shared-libraries-plan.md)

## Notes
- **ZERO COST REQUIREMENT**: All configurations optimized for GCP free tier
- Do NOT run `cdktf deploy` until explicitly ready to deploy
- Always test locally with `make infra-test` first
- Review generated Terraform in `cdktf.out/` before deployment
- Use `cdktf destroy` to tear down resources when no longer needed
