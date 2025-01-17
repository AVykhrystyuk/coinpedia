using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Mvc;

namespace Coinpedia.WebApi.Handlers;

public static class CryptocurrencyHandlers
{
    public class Logs { };

    public static RouteGroupBuilder MapCryptocurrencies(this RouteGroupBuilder builder)
    {
        builder.MapGet("/{symbol}/quotes/latest", GetLatestQuote)
            .WithName($"{nameof(CryptocurrencyHandlers)}_{nameof(GetLatestQuote)}")
            .WithOpenApi()
            .HasApiVersion(1);

        builder.MapPost("/test", (CryptocurrencyQuote q) => q.Symbol)
            .WithName($"{nameof(CryptocurrencyHandlers)}_test")
            .WithOpenApi()
            .HasApiVersion(1);

        return builder;
    }

    public static CryptocurrencyQuote GetLatestQuote(string symbol, [FromServices] ILogger<Logs> logger)
    {
        logger.LogInformation("GetLatestQuote was called. x={X}, y={Y}, symbol={Symbol}", 1, 2, symbol);

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CustomerId"] = 12345,
            ["OrderId"] = 54,
            ["symbol"] = symbol
        }))
        {
            logger.LogInformation("GetLatestQuote was called second time. x={X}, y={Y}, symbol={Symbol}", 3, 4, symbol);
        }

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        return new CryptocurrencyQuote
        (
            symbol,
            DateOnly.FromDateTime(DateTime.Now),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
    }
}

[JsonConverter(typeof(JsonConverter))]
public record CryptocurrencySymbol(string Symbol)
{
    //public static bool TryParse(string rawValue, out CryptocurrencySymbol result)
    //{
    //    result = new CryptocurrencySymbol(rawValue);
    //    return true;
    //}

    public class JsonConverter : JsonConverter<CryptocurrencySymbol>
    {
        public override CryptocurrencySymbol Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new CryptocurrencySymbol(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, CryptocurrencySymbol symbol, JsonSerializerOptions options) =>
            writer.WriteStringValue(symbol.Symbol);
    }
}

public record CryptocurrencyQuote(string Symbol, DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}