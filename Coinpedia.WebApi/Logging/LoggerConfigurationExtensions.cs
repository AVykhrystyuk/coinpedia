using Coinpedia.Core.Errors;
using Coinpedia.WebApi.Config;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;

namespace Coinpedia.WebApi.Logging;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureBootstrap(this LoggerConfiguration loggerConfig)
    {
        return loggerConfig
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    }

    public static LoggerConfiguration WriteToOpenTelemetry(this LoggerConfiguration loggerConfig, IServiceProvider services)
    {
        var environment = services.GetRequiredService<IHostEnvironment>();
        var configuration = services.GetRequiredService<IConfiguration>();

        var settings = configuration.GetSection(SeqSettings.SectionKey).Get<SeqSettings>() 
            ?? throw new Exception($"{SeqSettings.SectionKey} configuration section is missing");

        return loggerConfig.WriteTo.OpenTelemetry(options => options.ConfigureSeq(environment, settings));
    }

    public static LoggerConfiguration Configure(this LoggerConfiguration loggerConfig, IServiceProvider services) =>
        loggerConfig
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning);
            //.MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            //.MinimumLevel.Override("Microsoft.AspNetCore.Http", LogEventLevel.Warning)

    public static T Error<T>(this IDiagnosticContext context, T error)
        where T: Error
    {
        if (error.Exception is { } ex)
        {
            context.SetException(ex);
        }

        context.Set("Error", error, destructureObjects: true);
        return error;
    }
}