using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Handlers;
using Coinpedia.WebApi.Middlewares;
using Coinpedia.WebApi.Logging;
using Coinpedia.WebApi.OpenApi;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);

var settings = builder.Configuration.GetSection(Settings.SectionKey).Get<Settings>() ?? 
    throw new Exception($"{Settings.SectionKey} configuration section is missing");

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .Enrich.WithExceptionDetails()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry(options => options.Configure(builder.Environment, settings))
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddScoped<CorrelationIdMiddleware>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

builder.Services.AddProblemDetails(options => options.Configure());
builder.Services.AddExceptionHandler<GlobalProblemExceptionHandler>();

builder.Services.AddSettings(builder.Configuration);

builder.Services.AddApiVersioningAndExplorer();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

var routeBuilder = app.NewVersionedRouteBuilder();

routeBuilder.MapGroup("/weather-forecasts").MapWeatherForecast().WithTags("Weather forecasts");

// api/v1/cryptocurrencies/BTC/quotes/latest
routeBuilder.MapGroup("/cryptocurrencies").MapCryptocurrencies().WithTags("Cryptocurrencies");

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.Configure(app));
}

app.UseExceptionHandler();

app.Run();
