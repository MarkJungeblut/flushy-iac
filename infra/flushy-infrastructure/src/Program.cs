using Constructs;
using HashiCorp.Cdktf;
using Flushy.Infrastructure.Stacks;

namespace Flushy.Infrastructure;

class Program
{
    public static void Main(string[] args)
    {
        App app = new App();

        // Create GCP stack for the specified environment
        string environment = GetEnvironment(args);
        new GcpStack(app, $"flushy-{environment}", environment);

        app.Synth();
        Console.WriteLine($"CDKTF synthesis complete for environment: {environment}");
    }

    private static string GetEnvironment(string[] args)
    {
        // Check for environment variable or command line arg
        string? env = Environment.GetEnvironmentVariable("FLUSHY_ENV");

        if (string.IsNullOrEmpty(env) && args.Length > 0)
        {
            env = args[0];
        }

        // Default to 'dev' if not specified
        return env?.ToLowerInvariant() ?? "dev";
    }
}
