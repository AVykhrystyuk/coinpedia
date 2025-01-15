using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Handlers;
using Coinpedia.WebApi.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();

builder.Services.AddProblemDetails(ConfigureProblemDetails.Configure);
builder.Services.AddExceptionHandler<GlobalProblemExceptionHandler>();

builder.Services.AddApiVersioningAndExplorer();

var app = builder.Build();

var routeBuilder = app.NewVersionedRouteBuilder();

routeBuilder.MapGroup("/weather-forecasts").MapWeatherForecast();

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => ConfigureSwaggerUIOptions.Configure(options, app));
}

app.UseExceptionHandler();

app.Run();
