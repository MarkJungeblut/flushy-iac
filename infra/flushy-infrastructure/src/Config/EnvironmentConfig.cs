namespace Flushy.Infrastructure.Config;

/// <summary>
/// Environment-specific configuration for infrastructure deployment
/// </summary>
public class EnvironmentConfig
{
    public string ProjectId { get; init; }
    public string Region { get; init; }
    public CloudRunConfig CloudRun { get; init; }
    public MonitoringConfig Monitoring { get; init; }

    public EnvironmentConfig(
        string projectId,
        string region,
        CloudRunConfig cloudRun,
        MonitoringConfig monitoring)
    {
        ProjectId = projectId;
        Region = region;
        CloudRun = cloudRun;
        Monitoring = monitoring;
    }

    /// <summary>
    /// Get environment-specific configuration
    /// </summary>
    public static EnvironmentConfig ForEnvironment(string environment)
    {
        return environment.ToLowerInvariant() switch
        {
            "dev" => CreateDevConfig(),
            "staging" => CreateStagingConfig(),
            "prod" => CreateProdConfig(),
            _ => throw new ArgumentException($"Unknown environment: {environment}", nameof(environment))
        };
    }

    private static EnvironmentConfig CreateDevConfig()
    {
        string projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "flushy-dev";

        return new EnvironmentConfig(
            projectId: projectId,
            region: "us-central1",
            cloudRun: new CloudRunConfig
            {
                Memory = "256Mi",
                Cpu = "1",
                MinInstances = 0,
                MaxInstances = 1,
                Timeout = 60,
                Concurrency = 80
            },
            monitoring: new MonitoringConfig
            {
                ErrorRateThreshold = 0.05,
                LatencyP95Threshold = 2000,
                LatencyP99Threshold = 5000,
                TraceSamplingRate = 0.01,
                LogRetentionDays = 7,
                UptimeCheckIntervalSeconds = 300
            }
        );
    }

    private static EnvironmentConfig CreateStagingConfig()
    {
        string projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "flushy-staging";

        return new EnvironmentConfig(
            projectId: projectId,
            region: "us-central1",
            cloudRun: new CloudRunConfig
            {
                Memory = "256Mi",
                Cpu = "1",
                MinInstances = 0,
                MaxInstances = 2,
                Timeout = 60,
                Concurrency = 80
            },
            monitoring: new MonitoringConfig
            {
                ErrorRateThreshold = 0.05,
                LatencyP95Threshold = 1500,
                LatencyP99Threshold = 3000,
                TraceSamplingRate = 0.05,
                LogRetentionDays = 30,
                UptimeCheckIntervalSeconds = 180
            }
        );
    }

    private static EnvironmentConfig CreateProdConfig()
    {
        string projectId = Environment.GetEnvironmentVariable("GCP_PROJECT_ID") ?? "flushy-prod";

        return new EnvironmentConfig(
            projectId: projectId,
            region: "us-central1",
            cloudRun: new CloudRunConfig
            {
                Memory = "256Mi",
                Cpu = "1",
                MinInstances = 0,
                MaxInstances = 2,
                Timeout = 60,
                Concurrency = 80
            },
            monitoring: new MonitoringConfig
            {
                ErrorRateThreshold = 0.02,
                LatencyP95Threshold = 1000,
                LatencyP99Threshold = 2000,
                TraceSamplingRate = 0.1,
                LogRetentionDays = 90,
                UptimeCheckIntervalSeconds = 60
            }
        );
    }
}

/// <summary>
/// Cloud Run service configuration
/// </summary>
public class CloudRunConfig
{
    public required string Memory { get; init; }
    public required string Cpu { get; init; }
    public required int MinInstances { get; init; }
    public required int MaxInstances { get; init; }
    public required int Timeout { get; init; }
    public required int Concurrency { get; init; }
}

/// <summary>
/// Monitoring and observability configuration
/// </summary>
public class MonitoringConfig
{
    public required double ErrorRateThreshold { get; init; }
    public required int LatencyP95Threshold { get; init; }
    public required int LatencyP99Threshold { get; init; }
    public required double TraceSamplingRate { get; init; }
    public required int LogRetentionDays { get; init; }
    public required int UptimeCheckIntervalSeconds { get; init; }
}
