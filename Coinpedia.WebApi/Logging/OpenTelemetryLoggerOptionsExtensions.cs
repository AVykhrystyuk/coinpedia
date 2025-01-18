using Coinpedia.WebApi.Config;

using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Coinpedia.WebApi.Logging;

public static class OpenTelemetryLoggerOptionsExtensions
{
    public static void Configure(this OpenTelemetryLoggerOptions options, IWebHostEnvironment environment, Settings settings)
    {
        options.SetResourceBuilder(
            ResourceBuilder.CreateEmpty()
                .AddService(environment.ApplicationName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = environment.EnvironmentName,
                })
        );

        options.IncludeScopes = true;
        options.IncludeFormattedMessage = true;

        options.AddOtlpExporter(exporter =>
        {
            exporter.Endpoint = new Uri(settings.SeqIngestionUrl);
            exporter.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            exporter.Headers = $"X-Seq-ApiKey={settings.SeqApiKey}";
        });

        options.AddConsoleExporter();
    }
}