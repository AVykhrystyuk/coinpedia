using Coinpedia.WebApi;
using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Handlers;
using Coinpedia.WebApi.Logging;
using Coinpedia.WebApi.Middlewares;
using Coinpedia.WebApi.OpenApi;

using Serilog;

Log.Logger = new LoggerConfiguration()
    .ConfigureBootstrap()
    .CreateBootstrapLogger();

Log.Information("Starting application");

try
{
    var builder = WebApplication.CreateBuilder(args);

    ConfigureServices(builder);

    var app = builder.Build();

    ConfigureApp(app);

    app.Run();

    Log.Information("Application stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during bootstrapping. Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddSerilog((services, loggerConfig) => loggerConfig
        .Configure(services)
        .WriteToOpenTelemetry(services)
        .WriteTo.Console());
    builder.Services.AddScoped<CorrelationIdMiddleware>();

    builder.Services.AddCache(builder.Configuration);

    builder.Services.AddCryptocurrencyQuoteApiClient();
    builder.Services.AddCurrencyRatesApiClient();
    builder.Services.AddCryptocurrencyQuoteFetcher();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

    builder.Services.AddProblemDetails(options => options.Configure());
    builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

    builder.Services.AddSettings(builder.Configuration);

    builder.Services.AddApiVersioningAndExplorer();
}

static void ConfigureApp(WebApplication app)
{
    app.UseMiddleware<CorrelationIdMiddleware>();

    // if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options => options.Configure(app));
    }

    app.UseSerilogRequestLogging();

    var routeBuilder = app.NewVersionedRouteBuilder();

    // api/v1/cryptocurrencies/BTC/quotes/latest
    routeBuilder.MapGroup("/cryptocurrencies").MapCryptocurrencies().WithTags("Cryptocurrencies");

    app.UseExceptionHandler();
}

public partial class Program;