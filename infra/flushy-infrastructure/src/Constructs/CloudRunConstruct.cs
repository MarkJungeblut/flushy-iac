using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Google.CloudRunV2Service;
using HashiCorp.Cdktf.Providers.Google.CloudRunV2ServiceIamMember;
using Flushy.Infrastructure.Config;

namespace Flushy.Infrastructure.Constructs;

/// <summary>
/// Configuration for CloudRunConstruct
/// </summary>
public class CloudRunConstructConfig
{
    public required string ServiceName { get; init; }
    public required string Environment { get; init; }
    public required string Region { get; init; }
    public required string Image { get; init; }
    public required string ServiceAccountEmail { get; init; }
    public required CloudRunConfig CloudRunConfig { get; init; }
    public required Dictionary<string, string> EnvironmentVariables { get; init; }
}

/// <summary>
/// Reusable Cloud Run service construct with best practices for GCP free tier
/// </summary>
public class CloudRunConstruct : Construct
{
    public string ServiceUrl { get; }

    public CloudRunConstruct(Construct scope, string id, CloudRunConstructConfig config)
        : base(scope, id)
    {
        // Create Cloud Run v2 service
        var service = new CloudRunV2Service(this, "service", new CloudRunV2ServiceConfig
        {
            Name = config.ServiceName,
            Location = config.Region,
            Ingress = "INGRESS_TRAFFIC_ALL",
            Labels = new Dictionary<string, string>
            {
                { "environment", config.Environment },
                { "managed-by", "cdktf" }
            },
            Template = new CloudRunV2ServiceTemplate
            {
                ServiceAccount = config.ServiceAccountEmail,
                MaxInstanceRequestConcurrency = config.CloudRunConfig.Concurrency,
                Timeout = $"{config.CloudRunConfig.Timeout}s",
                Scaling = new CloudRunV2ServiceTemplateScaling
                {
                    MinInstanceCount = config.CloudRunConfig.MinInstances,
                    MaxInstanceCount = config.CloudRunConfig.MaxInstances
                },
                Containers = new[]
                {
                    new CloudRunV2ServiceTemplateContainers
                    {
                        Image = config.Image,
                        Resources = new CloudRunV2ServiceTemplateContainersResources
                        {
                            Limits = new Dictionary<string, string>
                            {
                                { "memory", config.CloudRunConfig.Memory },
                                { "cpu", config.CloudRunConfig.Cpu }
                            },
                            CpuIdle = true,
                            StartupCpuBoost = false
                        },
                        Ports = new CloudRunV2ServiceTemplateContainersPorts
                        {
                            Name = "http1",
                            ContainerPort = 8080
                        },
                        StartupProbe = new CloudRunV2ServiceTemplateContainersStartupProbe
                        {
                            InitialDelaySeconds = 0,
                            TimeoutSeconds = 1,
                            PeriodSeconds = 3,
                            FailureThreshold = 3,
                            HttpGet = new CloudRunV2ServiceTemplateContainersStartupProbeHttpGet
                            {
                                Path = "/health"
                            }
                        },
                        LivenessProbe = new CloudRunV2ServiceTemplateContainersLivenessProbe
                        {
                            InitialDelaySeconds = 0,
                            TimeoutSeconds = 1,
                            PeriodSeconds = 10,
                            FailureThreshold = 3,
                            HttpGet = new CloudRunV2ServiceTemplateContainersLivenessProbeHttpGet
                            {
                                Path = "/health"
                            }
                        },
                        Env = config.EnvironmentVariables.Select(kvp => new CloudRunV2ServiceTemplateContainersEnv
                        {
                            Name = kvp.Key,
                            Value = kvp.Value
                        }).ToArray()
                    }
                }
            }
        });

        // Allow unauthenticated access (adjust based on requirements)
        new CloudRunV2ServiceIamMember(this, "public-access", new CloudRunV2ServiceIamMemberConfig
        {
            Name = service.Name,
            Location = config.Region,
            Role = "roles/run.invoker",
            Member = "allUsers"
        });

        // Store service URL for output
        ServiceUrl = service.Uri;
    }
}
