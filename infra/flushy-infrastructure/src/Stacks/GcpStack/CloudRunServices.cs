using Constructs;
using Flushy.Infrastructure.Config;
using Flushy.Infrastructure.Constructs;

namespace Flushy.Infrastructure.Stacks.Gcp;

/// <summary>
/// Manages Google Cloud Run services for the application.
/// Creates and configures Cloud Run services for REST API and gRPC endpoints.
/// </summary>
public class CloudRunServices
{
    /// <summary>
    /// Cloud Run URL for the REST API service
    /// </summary>
    public string ApiServiceUrl { get; }

    /// <summary>
    /// Cloud Run URL for the gRPC service
    /// </summary>
    public string GrpcServiceUrl { get; }

    public CloudRunServices(
        Construct scope,
        EnvironmentConfig config,
        string environment,
        string artifactRegistryUrl,
        string apiServiceAccountEmail,
        string grpcServiceAccountEmail)
    {
        // Create Cloud Run service for REST API
        var apiService = new CloudRunConstruct(scope, "api-service", new CloudRunConstructConfig
        {
            ServiceName = $"flushy-api-{environment}",
            Environment = environment,
            Region = config.Region,
            Image = $"{artifactRegistryUrl}/flushy-api:latest",
            ServiceAccountEmail = apiServiceAccountEmail,
            CloudRunConfig = config.CloudRun,
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ASPNETCORE_ENVIRONMENT", ToPascalCase(environment) },
                { "GCP_PROJECT_ID", config.ProjectId }
            }
        });

        // Create Cloud Run service for gRPC
        var grpcService = new CloudRunConstruct(scope, "grpc-service", new CloudRunConstructConfig
        {
            ServiceName = $"flushy-grpc-{environment}",
            Environment = environment,
            Region = config.Region,
            Image = $"{artifactRegistryUrl}/flushy-grpc:latest",
            ServiceAccountEmail = grpcServiceAccountEmail,
            CloudRunConfig = config.CloudRun,
            EnvironmentVariables = new Dictionary<string, string>
            {
                { "ASPNETCORE_ENVIRONMENT", ToPascalCase(environment) },
                { "GCP_PROJECT_ID", config.ProjectId }
            }
        });

        // Store service URLs for stack outputs
        ApiServiceUrl = apiService.ServiceUrl;
        GrpcServiceUrl = grpcService.ServiceUrl;
    }

    /// <summary>
    /// Converts environment name to PascalCase for ASP.NET Core environment variable.
    /// Example: "dev" -> "Dev", "staging" -> "Staging"
    /// </summary>
    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }
}
