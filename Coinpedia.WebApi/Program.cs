using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Handlers;
using Coinpedia.WebApi.Logging;
using Coinpedia.WebApi.Middlewares;
using Coinpedia.WebApi.OpenApi;

using Microsoft.Extensions.Options;

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
}
finally
{
    Log.CloseAndFlush();
}

static void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddSerilog((services, loggerConfig) => loggerConfig.Configure(builder, services));
    builder.Services.AddScoped<CorrelationIdMiddleware>();

    builder.Services.AddTransient<Settings>(sp => sp.GetRequiredService<IOptions<Settings>>().Value);

    builder.Services.AddScoped<ICryptocurrencyQuoteApiClient, CoinMarketCapCryptocurrencyQuoteApiClient>();
    builder.Services.AddHttpClient<ICryptocurrencyQuoteApiClient, CoinMarketCapCryptocurrencyQuoteApiClient>((sp, client) =>
    {
        var settings = sp.GetRequiredService<Settings>();
        client.DefaultRequestHeaders.Add("X-Coinpedia-Correlation-ID", CorrelationId.Value);
        client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.CoinMarketCapApiKey);
        client.BaseAddress = new Uri(settings.CoinMarketCapBaseUrl);
    }).AddPolicyHandler(HttpRetryPolicies.Default());

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