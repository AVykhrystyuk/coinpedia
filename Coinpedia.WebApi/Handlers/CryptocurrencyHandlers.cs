using Coinpedia.Core;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
using Coinpedia.WebApi.Errors;
using Coinpedia.WebApi.Logging;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Serilog;

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

        return builder;
    }

    public static async Task<Results<Ok<MultiCurrencyCryptocurrencyQuotesDto>, JsonHttpResult<ErrorDto>>> GetLatestQuote(
        [FromRoute(Name = "symbol")] string symbolRaw,
        [FromServices] ICryptocurrencyQuoteFetcher cryptocurrencyQuoteFetcher,
        [FromServices] ILogger<Logs> logger,
        [FromServices] IDiagnosticContext diagnosticContext,
        [FromServices] TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var timestamp = timeProvider.GetUtcNow();

        using var _ = logger.BeginAttributedScope(symbolRaw, arg1Name: "symbol");

        var (_, _, symbol, err) = CryptocurrencySymbol.TryCreate(symbolRaw);
        if (err is not null)
        {
            return Err(err, StatusCodes.Status400BadRequest, timestamp);
        }

        var searchResult = await cryptocurrencyQuoteFetcher.FetchCryptocurrencyQuote(symbol, cancellationToken);
        if (searchResult.IsSuccess)
        {
            return Ok(searchResult.Value.ToDto());
        }
        else // Failure
        {
            var error = searchResult.Error;

            diagnosticContext.Error(error); // logged during SerilogRequestLogging

            return error switch
            {
                Core.Errors.NotFound => Err(error, StatusCodes.Status404NotFound, timestamp),
                None => Err(error, StatusCodes.Status424FailedDependency, timestamp),
                FailedDependency => Err(error, StatusCodes.Status424FailedDependency, timestamp),
                TooManyRequests => Err(error, StatusCodes.Status429TooManyRequests, timestamp),
                InvalidInput => Err(error, StatusCodes.Status400BadRequest, timestamp),
                RequestCancelled => Err(error, StatusCodes.Status499ClientClosedRequest, timestamp),
                _ => Err(error, StatusCodes.Status500InternalServerError, timestamp),
            };
        }

        static Ok<MultiCurrencyCryptocurrencyQuotesDto> Ok(MultiCurrencyCryptocurrencyQuotesDto body) =>
            TypedResults.Ok(body);
        static JsonHttpResult<ErrorDto> Err(Error error, int statusCode, DateTimeOffset timestamp) =>
            TypedResults.Json(new ErrorDto(
                Timestamp: timestamp.UtcDateTime,
                StatusCode: statusCode,
                Error: error.GetType().Name,
                ErrorMessage: error.Message
            ), statusCode: statusCode);
    }

    public record MultiCurrencyCryptocurrencyQuotesDto(
        string Cryptocurrency,
        DateTime CryptocurrencyUpdatedAt,
        DateTime CurrencyRatesUpdatedAt,
        IReadOnlyDictionary<string, decimal> PricePerCurrency,
        string BaseCurrency);

    private static MultiCurrencyCryptocurrencyQuotesDto ToDto(this MultiCurrencyCryptocurrencyQuotes quotes) =>
        new(quotes.Cryptocurrency.Value,
            quotes.CryptocurrencyUpdatedAt.UtcDateTime,
            quotes.CurrencyRatesUpdatedAt.UtcDateTime,
            quotes.PricePerCurrency.ToDictionary(p => p.Key.Value, p => p.Value),
            quotes.BaseCurrency.Value);
}
