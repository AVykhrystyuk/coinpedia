using Coinpedia.Core;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
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

    public static async Task<Results<Ok<MultiCurrencyCryptocurrencyQuotesDto>, JsonHttpResult<Error>>> GetLatestQuote(
        [FromRoute(Name = "symbol")] string symbolRaw,
        [FromServices] ICryptocurrencyQuoteFetcher cryptocurrencyQuoteFetcher,
        [FromServices] ILogger<Logs> logger,
        [FromServices] IDiagnosticContext diagnosticContext,
        CancellationToken cancellationToken)
    {
        using var _ = logger.BeginAttributedScope(symbolRaw, arg1Name: "symbol");

        var (_, _, symbol, err) = CryptocurrencySymbol.TryCreate(symbolRaw);
        if (err is not null)
        {
            return TypedResults.Json(err, statusCode: StatusCodes.Status400BadRequest);
        }

        var searchResult = await cryptocurrencyQuoteFetcher.FetchCryptocurrencyQuote(symbol, cancellationToken);
        if (searchResult.IsSuccess)
        {
            return TypedResults.Ok(searchResult.Value.ToDto());
        }
        else // Failure
        {
            var error = searchResult.Error;

            diagnosticContext.Error(error); // logged during SerilogRequestLogging

            return error switch
            {
                Core.Errors.NotFound => Json(error, StatusCodes.Status404NotFound),
                None => Json(error, StatusCodes.Status424FailedDependency),
                TooManyRequests => Json(error, StatusCodes.Status429TooManyRequests),
                InvalidInput => Json(error, StatusCodes.Status400BadRequest),
                RequestCancelled => Json(error, StatusCodes.Status499ClientClosedRequest),
                _ => Json(error, StatusCodes.Status500InternalServerError),
            };
        }

        static JsonHttpResult<Error> Json(Error error, int statusCode) =>
            TypedResults.Json(error, statusCode: statusCode);
    }

    public record MultiCurrencyCryptocurrencyQuotesDto(
        string Cryptocurrency,
        DateTime CryptocurrencyUpdatedAt,
        DateTime CurrencyRatesUpdatedAt,
        IReadOnlyDictionary<string, decimal> PricePerCurrency,
        string BaseCurrency);

    private static MultiCurrencyCryptocurrencyQuotesDto ToDto(this MultiCurrencyCryptocurrencyQuotes quotes) =>
        new(quotes.Cryptocurrency.Value,
            quotes.CryptocurrencyUpdatedAt,
            quotes.CurrencyRatesUpdatedAt,
            quotes.PricePerCurrency.ToDictionary(p => p.Key.Value, p => p.Value),
            quotes.BaseCurrency.Value);
}
