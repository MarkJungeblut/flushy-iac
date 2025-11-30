.PHONY: up down logs build test clean deploy infra-synth infra-diff infra-test sonar sonar-up sonar-down quality help

help:  ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

up:  ## Start local services with docker-compose
	docker-compose up -d

down:  ## Stop local services
	docker-compose down

logs:  ## Tail service logs
	docker-compose logs -f

build:  ## Build all .NET services
	dotnet build

test:  ## Run all tests with coverage
	dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

clean:  ## Clean build artifacts
	dotnet clean
	find . -type d -name "bin" -o -name "obj" -o -name "TestResults" | xargs rm -rf

infra-synth:  ## Generate Terraform JSON locally (no GCP required)
	cd infra/flushy-infrastructure/src && cdktf synth
	@echo "Generated Terraform files in infra/flushy-infrastructure/src/cdktf.out/"

infra-diff:  ## Preview infrastructure changes (requires GCP credentials, read-only)
	cd infra/flushy-infrastructure/src && cdktf diff

infra-test:  ## Test infrastructure code locally (synth + validate)
	@echo "Synthesizing CDKTF stack..."
	cd infra/flushy-infrastructure/src && cdktf synth
	@echo "✓ CDKTF synthesis successful"
	@echo "Review generated files in infra/flushy-infrastructure/src/cdktf.out/"

deploy:  ## Deploy to GCP via CDKTF (WARNING: may incur costs)
	@echo "WARNING: This will deploy to GCP and may incur costs!"
	@read -p "Are you sure? [y/N] " -n 1 -r; \
	echo; \
	if [[ $$REPLY =~ ^[Yy]$$ ]]; then \
		cd infra/flushy-infrastructure/src && cdktf deploy; \
	else \
		echo "Deployment cancelled."; \
	fi

sonar-up:  ## Start SonarQube locally
	docker-compose -f docker-compose.sonar.yml up -d
	@echo "SonarQube starting... Wait 30-60 seconds, then visit http://localhost:9000"
	@echo "Default credentials: admin/admin (you'll be prompted to change on first login)"

sonar-down:  ## Stop SonarQube
	docker-compose -f docker-compose.sonar.yml down

sonar:  ## Run SonarQube analysis (requires SONAR_TOKEN env var)
	@if [ -z "$(SONAR_TOKEN)" ]; then \
		echo "Error: SONAR_TOKEN environment variable is not set"; \
		echo "Get a token from http://localhost:9000/account/security"; \
		exit 1; \
	fi
	dotnet sonarscanner begin /k:"flushy-iac" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="$(SONAR_TOKEN)"
	dotnet build
	dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
	dotnet sonarscanner end /d:sonar.login="$(SONAR_TOKEN)"

quality:  ## Run code quality checks
	@echo "Checking code formatting..."
	dotnet format --verify-no-changes
	@echo "Building with warnings as errors..."
	dotnet build /warnaserror

restore:  ## Restore NuGet packages
	dotnet restore

format:  ## Format code using dotnet format
	dotnet format

install-tools:  ## Install required .NET tools
	dotnet tool install --global dotnet-sonarscanner || dotnet tool update --global dotnet-sonarscanner
	dotnet tool install --global dotnet-format || dotnet tool update --global dotnet-format
	@echo "Tools installed successfully"
