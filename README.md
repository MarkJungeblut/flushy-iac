# Flushy IaC - CDKTF + .NET Monorepo

Production-ready monorepo for deploying .NET 10 microservices (REST API and gRPC) to GCP Cloud Run using CDKTF (C#) for infrastructure provisioning.

## Overview

This project demonstrates a modern cloud-native architecture with:

- **Infrastructure as Code**: CDKTF with C# targeting Google Cloud Platform
- **Microservices**: .NET 10 REST API and gRPC services with hexagonal architecture
- **Local Development**: Docker Compose for rapid iteration
- **Code Quality**: Integrated SonarQube, StyleCop, and .NET analyzers
- **Monitoring**: Cloud Logging, Cloud Monitoring, and alerting

## Technology Stack

- **Infrastructure**: CDKTF (C#) → GCP Cloud Run
- **Services**: .NET 10 (ASP.NET Core, gRPC)
- **Container Runtime**: Docker with Alpine-based images
- **Code Quality**: SonarQube, StyleCop, .NET Analyzers
- **Local Dev**: Docker Compose

## Project Structure

```
flushy-iac/
├── infra/                              # Infrastructure code
│   └── Flushy.Infrastructure/          # CDKTF project (C#)
│       ├── Program.cs                  # CDKTF entry point
│       ├── Stacks/                     # Stack definitions
│       ├── Constructs/                 # Reusable components
│       └── Config/                     # Environment configs
├── services/
│   ├── flushy-api-service/             # REST API (.NET 10)
│   │   ├── src/
│   │   │   ├── Api/Controllers/        # Primary adapters
│   │   │   ├── Application/Services/   # Business logic
│   │   │   └── Domain/Models/          # Domain models
│   │   └── tests/
│   ├── flushy-grpc-service/            # gRPC service (.NET 10)
│   │   ├── src/
│   │   │   ├── Api/Grpc/               # gRPC adapters
│   │   │   ├── Application/Services/   # Business logic
│   │   │   └── Domain/Models/          # Domain models
│   │   └── tests/
│   └── shared/                         # Shared libraries
│       └── Flushy.Shared.Configuration/
├── .editorconfig                       # Code style rules
├── Directory.Build.props               # Shared MSBuild config
├── sonar-project.properties            # SonarQube config
├── docker-compose.yml                  # Local services
├── docker-compose.sonar.yml            # SonarQube setup
├── Makefile                            # Common tasks
└── README.md
```

## Prerequisites

### Required Software

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Node.js 18+](https://nodejs.org/) (for CDKTF)
- [Google Cloud CLI](https://cloud.google.com/sdk/docs/install) (for deployment)

### Optional Tools

- [Make](https://www.gnu.org/software/make/) (for convenience commands)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/)

## Getting Started

### 1. Clone and Setup

```bash
# Clone the repository
git clone <repository-url>
cd flushy-iac

# Copy environment template
cp .env.example .env

# Edit .env with your GCP project details
nano .env
```

### 2. Install Tools

```bash
# Install .NET global tools
make install-tools

# Or manually:
dotnet tool install --global dotnet-sonarscanner
dotnet tool install --global dotnet-format

# Install CDKTF CLI
npm install -g cdktf-cli@latest
```

### 3. Restore Dependencies

```bash
make restore
# Or: dotnet restore
```

## Local Development

### Start Services

```bash
# Start all services
make up

# View logs
make logs

# Stop services
make down
```

### Access Services

- REST API: http://localhost:5000
- gRPC Service: http://localhost:5001
- Swagger UI: http://localhost:5000/swagger (Development only)

### Development Workflow

1. Make code changes (hot reload enabled)
2. Run tests: `make test`
3. Check code quality: `make quality`
4. Format code: `make format`

## Code Quality

### Run Code Quality Checks

```bash
# All quality checks
make quality

# Individual checks
dotnet format --verify-no-changes
dotnet build /warnaserror
```

### SonarQube Analysis

```bash
# Start SonarQube
make sonar-up

# Wait 30-60 seconds, then visit http://localhost:9000
# Default credentials: admin/admin

# Generate a token at http://localhost:9000/account/security
export SONAR_TOKEN=your_token_here

# Run analysis
make sonar

# Stop SonarQube
make sonar-down
```

## Testing

```bash
# Run all tests with coverage
make test

# Or manually:
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

Coverage reports are generated in `TestResults/` directory.

## Infrastructure Deployment

### Prerequisites (GCP Setup)

1. Create a GCP project in the [Google Cloud Console](https://console.cloud.google.com/)
2. Enable required APIs:
   ```bash
   gcloud services enable run.googleapis.com
   gcloud services enable artifactregistry.googleapis.com
   gcloud services enable logging.googleapis.com
   gcloud services enable monitoring.googleapis.com
   gcloud services enable secretmanager.googleapis.com
   ```
3. Create a service account with required permissions:
   ```bash
   gcloud iam service-accounts create cdktf-deployer \
     --display-name="CDKTF Deployment Service Account"

   gcloud projects add-iam-policy-binding YOUR_PROJECT_ID \
     --member="serviceAccount:cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com" \
     --role="roles/run.admin"

   # Add additional roles as needed
   ```
4. Download service account key:
   ```bash
   gcloud iam service-accounts keys create key.json \
     --iam-account=cdktf-deployer@YOUR_PROJECT_ID.iam.gserviceaccount.com
   ```

### Deploy to GCP

```bash
# Set environment variable
export GOOGLE_APPLICATION_CREDENTIALS=/path/to/key.json

# Navigate to infrastructure
cd infra/flushy-infrastructure/src

# Initialize CDKTF
cdktf init

# Preview changes
cdktf plan

# Deploy infrastructure
cdktf deploy

# Or use Make from root:
make deploy
```

## Makefile Commands

| Command | Description |
|---------|-------------|
| `make help` | Show all available commands |
| `make up` | Start local services |
| `make down` | Stop local services |
| `make logs` | Tail service logs |
| `make build` | Build all .NET services |
| `make test` | Run tests with coverage |
| `make clean` | Clean build artifacts |
| `make quality` | Run code quality checks |
| `make format` | Format code |
| `make sonar-up` | Start SonarQube |
| `make sonar-down` | Stop SonarQube |
| `make sonar` | Run SonarQube analysis |
| `make deploy` | Deploy to GCP |
| `make install-tools` | Install .NET tools |

## Architecture

### Hexagonal Architecture (Ports & Adapters)

Services follow hexagonal architecture for maintainability and testability:

- **Api/**: Primary adapters (Controllers, gRPC services)
- **Application/**: Use cases and business logic
- **Domain/**: Core domain models and entities

### Cloud Run Configuration

Each service is configured with:
- Memory: 512MB (tunable)
- CPU: 1 vCPU
- Timeout: 60s
- Health checks: `/health` (liveness), `/ready` (readiness)
- Automatic HTTPS

## Monitoring & Observability

CDKTF provisions comprehensive monitoring:

- **Error Rate Alerts**: Trigger when error rate > 5%
- **Latency Tracking**: P50, P95, P99 metrics with alerts
- **Cost Monitoring**: Budget alerts and cost dashboards
- **Cloud Logging**: Centralized log aggregation
- **Uptime Checks**: Health endpoint monitoring

## Security Best Practices

- Least privilege IAM roles per service
- Secrets managed via Google Secret Manager
- No secrets in code or environment variables
- Alpine-based images for minimal attack surface
- Non-root container users
- Regular dependency updates

## Troubleshooting

### Build Errors

```bash
# Clean and rebuild
make clean
make build
```

### Docker Issues

```bash
# Reset Docker
make down
docker system prune -a
make up
```

### CDKTF Deployment Fails

```bash
# Check GCP credentials
gcloud auth list

# Verify APIs are enabled
gcloud services list --enabled

# Check CDKTF logs
cd infra/flushy-infrastructure/src
cdktf plan
```

### SonarQube Not Starting

```bash
# Check container logs
docker logs flushy-sonarqube

# Ensure sufficient memory (min 2GB)
docker stats

# Reset SonarQube data
make sonar-down
docker volume rm flushy-iac_sonarqube_data
make sonar-up
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Run `make quality` to ensure code standards
4. Run `make test` to ensure all tests pass
5. Submit a pull request

## License

Copyright (c) Flushy. All rights reserved.

## Support

For issues and questions, please open an issue in the repository.
