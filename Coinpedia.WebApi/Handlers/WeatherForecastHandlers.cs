using Coinpedia.WebApi.Config;
using Microsoft.Extensions.Options;

namespace Coinpedia.WebApi.Handlers;

public static class WeatherForecastHandlers
{
    public static IEndpointRouteBuilder MapWeatherForecast(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", (IOptions<Settings> settings) => $"obsolete: {settings.Value.Secret}")
            .WithName("obsolete")
            .WithOpenApi()
            .HasApiVersion(1)
            .MapToApiVersion(1);

        builder.MapGet("/", GetAllForecasts)
            .WithName(nameof(GetAllForecasts))            
            .WithOpenApi()
            .HasApiVersion(2)
            .MapToApiVersion(2);

        builder.MapGet("/today", GetTodaysForecast)
            .WithName(nameof(GetTodaysForecast))
            .WithOpenApi()
            .HasApiVersion(2);

        return builder;
    }

    public static WeatherForecast[] GetAllForecasts()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        return forecast;
    }

    public static WeatherForecast GetTodaysForecast()
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        return new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}