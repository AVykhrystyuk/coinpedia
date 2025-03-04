﻿using Coinpedia.WebApi.Config;

using Serilog.Sinks.OpenTelemetry;

namespace Coinpedia.WebApi.Logging;

public static class BatchedOpenTelemetrySinkOptionsExtensions
{
    public static void ConfigureSeq(this BatchedOpenTelemetrySinkOptions options, IHostEnvironment environment, SeqSettings settings)
    {
        options.Endpoint = settings.IngestionUrl;
        options.Protocol = OtlpProtocol.HttpProtobuf;
        options.Headers = new Dictionary<string, string>
        {
            ["X-Seq-ApiKey"] = settings.ApiKey,
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