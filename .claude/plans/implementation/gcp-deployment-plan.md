# GCP Deployment Plan

**Priority**: Medium
**Status**: 🔄 Pending

## Overview
Deploy the complete infrastructure and services to Google Cloud Platform using CDKTF, optimized for GCP free tier with zero-cost operation.

## Goals
- Set up GCP project and required APIs
- Create service accounts with minimal permissions
- Deploy infrastructure via CDKTF
- Build and push container images
- Deploy services to Cloud Run
- Validate monitoring and logging
- Test deployed services
- Stay within GCP free tier limits

## Prerequisites (Manual Setup)

### 1. Create GCP Project
```bash
# Via Google Cloud Console or gcloud
gcloud projects create YOUR_PROJECT_ID --name="Flushy IaC"
gcloud config set project YOUR_PROJECT_ID
```

### 2. Enable Required APIs
```bash
gcloud services enable run.googleapis.com
gcloud services enable artifactregistry.googleapis.com
gcloud services enable logging.googleapis.com
gcloud services enable monitoring.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable cloudtrace.googleapis.com
```

### 3. Create Service Account for CDKTF
```bash
# Create service account
gcloud iam service-accounts create cdktf-deployer \
  --display-name="CDKTF Deployment Service Account"

# Grant necessary roles (least privilege)
gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/run.admin"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/artifactregistry.admin"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/logging.admin"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/monitoring.admin"

gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
  --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
  --role="roles/secretmanager.admin"

# Download service account key
gcloud iam service-accounts keys create cdktf-key.json \
  --iam-account=cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com

# DO NOT commit this key! Add to .gitignore
```

### 4. Set Environment Variables
```bash
export GOOGLE_APPLICATION_CREDENTIALS=/path/to/cdktf-key.json
export GCP_PROJECT_ID=YOUR_PROJECT_ID
export GCP_REGION=us-central1
```

## Deployment Steps

### 1. Test CDKTF Locally First
```bash
# Always test locally before deploying!
make infra-test

# Preview what will be created
make infra-diff
```

### 2. Deploy Infrastructure with CDKTF
```bash
# This creates:
# - Artifact Registry repository
# - Cloud Run services (scaled to zero initially)
# - Service accounts
# - Cloud Monitoring dashboards
# - Alert policies
# - Log sinks

make deploy
# or: cd infra/Flushy.Infrastructure && cdktf deploy
```

**Expected resources created**:
- Artifact Registry: `flushy-containers`
- Cloud Run services: `flushy-api-service`, `flushy-grpc-service`
- Service accounts: `flushy-api-sa`, `flushy-grpc-sa`
- Dashboards: Error rates, latency, costs
- Alerts: Error rate >5%, latency >1s, budget alerts

### 3. Build and Push Container Images
```bash
# Configure Docker for Artifact Registry
gcloud auth configure-docker ${GCP_REGION}-docker.pkg.dev

# Build and push API service
cd services/flushy-api-service
docker build -t ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-api-service:latest .
docker push ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-api-service:latest

# Build and push gRPC service
cd ../flushy-grpc-service
docker build -t ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-grpc-service:latest .
docker push ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-grpc-service:latest
```

### 4. Deploy Services to Cloud Run
CDKTF should have already created the services, but if manual deployment is needed:

```bash
# Deploy API service
gcloud run deploy flushy-api-service \
  --image ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-api-service:latest \
  --platform managed \
  --region ${GCP_REGION} \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 1 \
  --port 8080

# Deploy gRPC service
gcloud run deploy flushy-grpc-service \
  --image ${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/flushy-containers/flushy-grpc-service:latest \
  --platform managed \
  --region ${GCP_REGION} \
  --allow-unauthenticated \
  --memory 256Mi \
  --cpu 1 \
  --min-instances 0 \
  --max-instances 1 \
  --port 8080 \
  --use-http2
```

### 5. Verify Deployment
```bash
# Get service URLs
gcloud run services list --region ${GCP_REGION}

# Test API service
SERVICE_URL=$(gcloud run services describe flushy-api-service --region ${GCP_REGION} --format 'value(status.url)')
curl ${SERVICE_URL}/health
curl ${SERVICE_URL}/api/weather

# Test gRPC service (requires grpcurl)
GRPC_URL=$(gcloud run services describe flushy-grpc-service --region ${GCP_REGION} --format 'value(status.url)')
grpcurl -plaintext ${GRPC_URL#https://} greeter.Greeter/SayHello -d '{"name":"World"}'
```

### 6. Validate Monitoring
```bash
# Check Cloud Logging
gcloud logging read "resource.type=cloud_run_revision" --limit 10

# View in Cloud Console:
# - Cloud Logging: https://console.cloud.google.com/logs
# - Cloud Trace: https://console.cloud.google.com/traces
# - Cloud Monitoring: https://console.cloud.google.com/monitoring
```

**Verify**:
- ✅ Logs appearing in Cloud Logging
- ✅ Traces in Cloud Trace (with 1% sampling)
- ✅ Metrics in Cloud Monitoring
- ✅ Dashboards created
- ✅ Alert policies configured

### 7. Set Up Budget Alerts (IMPORTANT!)
```bash
# Create budget with $0 threshold
gcloud billing budgets create \
  --billing-account=YOUR_BILLING_ACCOUNT_ID \
  --display-name="Flushy Zero Cost Alert" \
  --budget-amount=0 \
  --threshold-rule=percent=50 \
  --threshold-rule=percent=90 \
  --threshold-rule=percent=100
```

**Alert thresholds**:
- 50% of $0 = $0
- 90% of $0 = $0
- 100% of $0 = $0
- Get notified immediately if ANY cost is incurred!

### 8. Test Complete Workflow
1. Call API endpoint → Check logs → Verify trace in Cloud Trace
2. Trigger error → Verify alert policy (if threshold exceeded)
3. Check latency metrics in dashboard
4. Verify correlation IDs in logs

### 9. Document Deployed URLs
Update README.md with:
- Service URLs
- Monitoring dashboard links
- Deployment date
- Configuration notes

## Free Tier Limits & Cost Optimization

**Cloud Run Free Tier** (per month):
- 2 million requests
- 360,000 GB-seconds of compute time
- 180,000 vCPU-seconds of compute time
- 1 GB outbound data

**Cloud Logging** (per month):
- 50 GB free

**Cloud Monitoring**:
- Free for GCP resources

**Cloud Trace** (per month):
- First 2.5 million spans free

**Optimization strategies**:
- Min instances: 0 (scale to zero)
- Memory: 256MB (minimum viable)
- Max instances: 1-2 (limit concurrent instances)
- Log sampling: Minimal verbosity in production
- Trace sampling: 1% (reduce trace volume)
- Log retention: 7-30 days (not long-term storage)

## Teardown (When Done)
```bash
# Destroy all resources
cd infra/Flushy.Infrastructure
cdktf destroy

# Or manually delete via Console:
# - Cloud Run services
# - Artifact Registry repository
# - Service accounts
# - Log sinks
# - Dashboards
# - Alert policies
```

## Success Criteria
- ✅ GCP project created and configured
- ✅ Required APIs enabled
- ✅ CDKTF deployment successful
- ✅ Services deployed to Cloud Run
- ✅ Public URLs accessible
- ✅ Health checks returning 200 OK
- ✅ Logs visible in Cloud Logging
- ✅ Traces in Cloud Trace
- ✅ Monitoring dashboards created
- ✅ Alert policies configured
- ✅ Budget alerts set ($0 threshold!)
- ✅ No unexpected costs
- ✅ Documentation updated

## Next Steps
After successful deployment:
- Monitor costs daily (should be $0)
- Review logs and traces
- Test service-to-service communication
- Optimize resource allocation based on metrics
- Plan future enhancements

## ⚠️ CRITICAL: Cost Monitoring
**Check costs daily**:
```bash
# View current costs
gcloud billing accounts list
gcloud billing projects describe YOUR_PROJECT_ID
```

**If costs appear**:
1. Immediately scale down or delete services
2. Review Cloud Billing reports
3. Check for unexpected resource usage
4. Consider using always-free resources only

## Notes
- **NEVER deploy without testing locally first** (`make infra-test`)
- **ALWAYS review `cdktf diff` before deploying**
- **SET UP BUDGET ALERTS IMMEDIATELY**
- Keep service accounts secure (never commit keys)
- Use least privilege IAM roles
- Monitor free tier usage regularly
- Test teardown process before actual deployment
