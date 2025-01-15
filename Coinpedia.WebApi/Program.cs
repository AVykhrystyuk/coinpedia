using Coinpedia.WebApi.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.EnableTryItOutByDefault();
    });
}

app.MapGroup("/weather-forecasts")
    .MapGet("/", WeatherForecastHandlers.GetAllForecasts)
        .WithName("GetWeatherForecast")
        .WithOpenApi();

app.Run();

