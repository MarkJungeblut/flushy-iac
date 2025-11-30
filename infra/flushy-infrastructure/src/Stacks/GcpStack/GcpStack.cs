using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Google.Provider;
using Flushy.Infrastructure.Config;
using Flushy.Infrastructure.Stacks.Gcp;

namespace Flushy.Infrastructure.Stacks;

/// <summary>
/// Main GCP infrastructure stack for Flushy services.
/// Orchestrates all GCP resources by delegating to specialized resource classes.
///
/// Architecture:
/// - ArtifactRegistry: Docker image storage
/// - ServiceAccounts: IAM identities with minimal permissions
/// - CloudRunServices: Containerized application deployment
/// </summary>
public class GcpStack : TerraformStack
{
    public GcpStack(Construct scope, string id, string environment) : base(scope, id)
    {
        // Load environment-specific configuration (dev/staging/prod)
        var config = EnvironmentConfig.ForEnvironment(environment);

        // Configure GCP provider with project and region
        new GoogleProvider(this, "google", new GoogleProviderConfig
        {
            Project = config.ProjectId,
            Region = config.Region
        });

        // Create Artifact Registry for Docker images
        var artifactRegistry = new ArtifactRegistry(this, config, environment);

        // Create service accounts with observability permissions
        var serviceAccounts = new ServiceAccounts(this, config, environment);

        // Create Cloud Run services (API and gRPC)
        var cloudRunServices = new CloudRunServices(
            this,
            config,
            environment,
            artifactRegistry.RepositoryUrl,
            serviceAccounts.ApiServiceAccountEmail,
            serviceAccounts.GrpcServiceAccountEmail
        );

        // Export important values as Terraform outputs
        new TerraformOutput(this, "artifact_registry_url", new TerraformOutputConfig
        {
            Value = artifactRegistry.RepositoryUrl,
            Description = "Artifact Registry URL for Docker images"
        });

        new TerraformOutput(this, "api_service_url", new TerraformOutputConfig
        {
            Value = cloudRunServices.ApiServiceUrl,
            Description = "Cloud Run URL for REST API service"
        });

        new TerraformOutput(this, "grpc_service_url", new TerraformOutputConfig
        {
            Value = cloudRunServices.GrpcServiceUrl,
            Description = "Cloud Run URL for gRPC service"
        });
    }
}
