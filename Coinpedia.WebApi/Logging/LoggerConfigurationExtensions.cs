using Coinpedia.WebApi.Config;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;

namespace Coinpedia.WebApi.Logging;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration ConfigureBootstrap(this LoggerConfiguration config)
    {
        return config
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    }

    public static LoggerConfiguration Configure(this LoggerConfiguration config, WebApplicationBuilder builder, IServiceProvider services)
    {
        var settings = builder.Configuration.GetSection(Settings.SectionKey).Get<Settings>() 
            ?? throw new Exception($"{Settings.SectionKey} configuration section is missing");

        return config
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithSpan()
            .Enrich.WithExceptionDetails()
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(options => options.Configure(builder.Environment, settings));
    }
}