using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Handlers;
using Coinpedia.WebApi.Logging;
using Coinpedia.WebApi.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection(Settings.SectionKey).Get<Settings>() ?? throw new Exception("Setting are missing");

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(options => ConfigureOpenTelemetryLoggerOptions.Configure(options, builder.Environment, settings));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

builder.Services.AddProblemDetails(ConfigureProblemDetails.Configure);
builder.Services.AddExceptionHandler<GlobalProblemExceptionHandler>();

builder.Services.AddSettings(builder.Configuration);

builder.Services.AddApiVersioningAndExplorer();

var app = builder.Build();

var routeBuilder = app.NewVersionedRouteBuilder();

routeBuilder.MapGroup("/weather-forecasts").MapWeatherForecast().WithTags("Weather forecasts");

// api/v1/cryptocurrencies/BTC/quotes/latest
routeBuilder.MapGroup("/cryptocurrencies").MapCryptocurrencies().WithTags("Cryptocurrencies");

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => ConfigureSwaggerUIOptions.Configure(options, app));
}

app.UseExceptionHandler();

app.Run();
