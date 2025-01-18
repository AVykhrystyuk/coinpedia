using Coinpedia.WebApi.Config;

using Serilog.Sinks.OpenTelemetry;

namespace Coinpedia.WebApi.Logging;

public static class BatchedOpenTelemetrySinkOptionsExtensions
{
    public static void Configure(this BatchedOpenTelemetrySinkOptions options, IWebHostEnvironment environment, Settings settings)
    {
        options.Endpoint = settings.SeqIngestionUrl;
        options.Protocol = OtlpProtocol.HttpProtobuf;
        options.Headers = new Dictionary<string, string>
        {
            ["X-Seq-ApiKey"] = settings.SeqApiKey,
        };
        options.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = environment.ApplicationName,
            ["service.instance.id"] = Guid.NewGuid(),
            ["deployment.environment"] = environment.EnvironmentName,
            ["machineName"] = Environment.MachineName,
        };
    }
}