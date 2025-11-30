using Constructs;
using HashiCorp.Cdktf.Providers.Google.ArtifactRegistryRepository;
using Flushy.Infrastructure.Config;

namespace Flushy.Infrastructure.Stacks.Gcp;

/// <summary>
/// Manages Google Artifact Registry for Docker image storage.
/// Provides a centralized Docker repository for all microservices.
/// </summary>
public class ArtifactRegistry
{
    /// <summary>
    /// Full URL to the Artifact Registry repository for Docker images.
    /// Format: {region}-docker.pkg.dev/{project-id}/{repository-id}
    /// </summary>
    public string RepositoryUrl { get; }

    public ArtifactRegistry(Construct scope, EnvironmentConfig config, string environment)
    {
        // Create Artifact Registry repository for Docker images
        var repository = new ArtifactRegistryRepository(scope, "flushy-artifacts", new ArtifactRegistryRepositoryConfig
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

        // Construct repository URL for use in Cloud Run image references
        RepositoryUrl = $"{config.Region}-docker.pkg.dev/{config.ProjectId}/flushy-services";
    }
}
