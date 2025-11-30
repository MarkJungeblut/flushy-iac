using Constructs;
using HashiCorp.Cdktf.Providers.Google.ServiceAccount;
using HashiCorp.Cdktf.Providers.Google.ProjectIamMember;
using Flushy.Infrastructure.Config;

namespace Flushy.Infrastructure.Stacks.Gcp;

/// <summary>
/// Manages Google Cloud service accounts and IAM bindings for microservices.
/// Creates service accounts with minimal necessary permissions following least-privilege principle.
/// </summary>
public class ServiceAccounts
{
    /// <summary>
    /// Service account email for the REST API service
    /// </summary>
    public string ApiServiceAccountEmail { get; }

    /// <summary>
    /// Service account email for the gRPC service
    /// </summary>
    public string GrpcServiceAccountEmail { get; }

    public ServiceAccounts(Construct scope, EnvironmentConfig config, string environment)
    {
        // Create service account for API service
        var apiServiceAccount = CreateServiceAccount(
            scope,
            id: "api-service-account",
            accountId: $"flushy-api-{environment}",
            displayName: $"Flushy API Service ({environment})",
            description: "Service account for Flushy REST API service"
        );

        // Grant minimal IAM roles to API service account
        GrantObservabilityPermissions(scope, config.ProjectId, apiServiceAccount.Email, "api");

        // Create service account for gRPC service
        var grpcServiceAccount = CreateServiceAccount(
            scope,
            id: "grpc-service-account",
            accountId: $"flushy-grpc-{environment}",
            displayName: $"Flushy gRPC Service ({environment})",
            description: "Service account for Flushy gRPC service"
        );

        // Grant minimal IAM roles to gRPC service account
        GrantObservabilityPermissions(scope, config.ProjectId, grpcServiceAccount.Email, "grpc");

        // Store emails for use in Cloud Run services
        ApiServiceAccountEmail = apiServiceAccount.Email;
        GrpcServiceAccountEmail = grpcServiceAccount.Email;
    }

    /// <summary>
    /// Creates a Google Cloud service account
    /// </summary>
    private static ServiceAccount CreateServiceAccount(
        Construct scope,
        string id,
        string accountId,
        string displayName,
        string description)
    {
        return new ServiceAccount(scope, id, new ServiceAccountConfig
        {
            AccountId = accountId,
            DisplayName = displayName,
            Description = description
        });
    }

    /// <summary>
    /// Grants observability permissions (logging, monitoring, tracing) to a service account.
    /// These are the minimal permissions required for Cloud Run services to report metrics and logs.
    /// </summary>
    private static void GrantObservabilityPermissions(
        Construct scope,
        string projectId,
        string serviceAccountEmail,
        string servicePrefix)
    {
        // Cloud Logging - allows service to write logs
        new ProjectIamMember(scope, $"{servicePrefix}-logging-writer", new ProjectIamMemberConfig
        {
            Project = projectId,
            Role = "roles/logging.logWriter",
            Member = $"serviceAccount:{serviceAccountEmail}"
        });

        // Cloud Monitoring - allows service to write custom metrics
        new ProjectIamMember(scope, $"{servicePrefix}-monitoring-writer", new ProjectIamMemberConfig
        {
            Project = projectId,
            Role = "roles/monitoring.metricWriter",
            Member = $"serviceAccount:{serviceAccountEmail}"
        });

        // Cloud Trace - allows service to send trace data
        new ProjectIamMember(scope, $"{servicePrefix}-trace-agent", new ProjectIamMemberConfig
        {
            Project = projectId,
            Role = "roles/cloudtrace.agent",
            Member = $"serviceAccount:{serviceAccountEmail}"
        });
    }
}
