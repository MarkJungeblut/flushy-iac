using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Google.Provider;
using HashiCorp.Cdktf.Providers.Google.ArtifactRegistryRepository;
using HashiCorp.Cdktf.Providers.Google.ServiceAccount;
using HashiCorp.Cdktf.Providers.Google.ProjectIamMember;
using Flushy.Infrastructure.Config;
using Flushy.Infrastructure.Constructs;

namespace Flushy.Infrastructure.Stacks;

/// <summary>
/// Main GCP infrastructure stack for Flushy services
/// </summary>
public class GcpStack : TerraformStack
{
    public GcpStack(Construct scope, string id, string environment) : base(scope, id)
    {
        // Load environment-specific configuration
        var config = EnvironmentConfig.ForEnvironment(environment);

        // Configure GCP provider
        new GoogleProvider(this, "google", new GoogleProviderConfig
        {
            Project = config.ProjectId,
            Region = config.Region
        });

        // Create Artifact Registry repository for Docker images
        var artifactRegistry = new ArtifactRegistryRepository(this, "flushy-artifacts", new ArtifactRegistryRepositoryConfig
        {
            Location = config.Region,
            RepositoryId = "flushy-services",
            Description = "Docker repository for Flushy microservices",
            Format = "DOCKER",
            Labels = new Dictionary<string, string>
            {
                { "environment", environment },
                { "managed-by", "cdktf" }
            }
        });

        // Create service account for API service
        var apiServiceAccount = new ServiceAccount(this, "api-service-account", new ServiceAccountConfig
        {
            AccountId = $"flushy-api-{environment}",
            DisplayName = $"Flushy API Service ({environment})",
            Description = "Service account for Flushy REST API service"
        });

        // Grant minimal IAM roles to API service account
        new ProjectIamMember(this, "api-logging-writer", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/logging.logWriter",
            Member = $"serviceAccount:{apiServiceAccount.Email}"
        });

        new ProjectIamMember(this, "api-monitoring-writer", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/monitoring.metricWriter",
            Member = $"serviceAccount:{apiServiceAccount.Email}"
        });

        new ProjectIamMember(this, "api-trace-agent", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/cloudtrace.agent",
            Member = $"serviceAccount:{apiServiceAccount.Email}"
        });

        // Create service account for gRPC service
        var grpcServiceAccount = new ServiceAccount(this, "grpc-service-account", new ServiceAccountConfig
        {
            AccountId = $"flushy-grpc-{environment}",
            DisplayName = $"Flushy gRPC Service ({environment})",
            Description = "Service account for Flushy gRPC service"
        });

        // Grant minimal IAM roles to gRPC service account
        new ProjectIamMember(this, "grpc-logging-writer", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/logging.logWriter",
            Member = $"serviceAccount:{grpcServiceAccount.Email}"
        });

        new ProjectIamMember(this, "grpc-monitoring-writer", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/monitoring.metricWriter",
            Member = $"serviceAccount:{grpcServiceAccount.Email}"
        });

        new ProjectIamMember(this, "grpc-trace-agent", new ProjectIamMemberConfig
        {
            Project = config.ProjectId,
            Role = "roles/cloudtrace.agent",
            Member = $"serviceAccount:{grpcServiceAccount.Email}"
        });

        // Create Cloud Run service for REST API
        var apiService = new CloudRunConstruct(this, "api-service", new CloudRunConstructConfig
        {
            ServiceName = $"flushy-api-{environment}",
            Environment = environment,
            Region = config.Region,
            Image = $"{config.Region}-docker.pkg.dev/{config.ProjectId}/flushy-services/flushy-api:latest",
            ServiceAccountEmail = apiServiceAccount.Email,
            CloudRunConfig = config.CloudRun,
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ASPNETCORE_ENVIRONMENT", ToPascalCase(environment) },
                { "GCP_PROJECT_ID", config.ProjectId }
            }
        });

        // Create Cloud Run service for gRPC
        var grpcService = new CloudRunConstruct(this, "grpc-service", new CloudRunConstructConfig
        {
            ServiceName = $"flushy-grpc-{environment}",
            Environment = environment,
            Region = config.Region,
            Image = $"{config.Region}-docker.pkg.dev/{config.ProjectId}/flushy-services/flushy-grpc:latest",
            ServiceAccountEmail = grpcServiceAccount.Email,
            CloudRunConfig = config.CloudRun,
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ASPNETCORE_ENVIRONMENT", ToPascalCase(environment) },
                { "GCP_PROJECT_ID", config.ProjectId }
            }
        });

        // Outputs
        new TerraformOutput(this, "artifact_registry_url", new TerraformOutputConfig
        {
            Value = $"{config.Region}-docker.pkg.dev/{config.ProjectId}/flushy-services",
            Description = "Artifact Registry URL for Docker images"
        });

        new TerraformOutput(this, "api_service_url", new TerraformOutputConfig
        {
            Value = apiService.ServiceUrl,
            Description = "Cloud Run URL for REST API service"
        });

        new TerraformOutput(this, "grpc_service_url", new TerraformOutputConfig
        {
            Value = grpcService.ServiceUrl,
            Description = "Cloud Run URL for gRPC service"
        });
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}
