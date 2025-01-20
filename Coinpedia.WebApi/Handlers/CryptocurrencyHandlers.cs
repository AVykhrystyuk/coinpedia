﻿using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Logging;

using CSharpFunctionalExtensions;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    public static async Task<Results<Ok<CryptocurrencyQuote>, JsonHttpResult<Error>>> GetLatestQuote(
        [FromRoute(Name = "symbol")] string symbolRaw,
        IOptions<Settings> settings,
        [FromServices] ICryptocurrencyQuoteApiClient cryptocurrencyQuoteApiClient,
        [FromServices] ILogger<Logs> logger,
        [FromServices] IDiagnosticContext diagnosticContext,
        CancellationToken cancellationToken)
    {
        var baseCurrency = settings.Value.BaseCurrency;

        using var _ = logger.BeginAttributesScope(symbolRaw, baseCurrency, arg1Name: "symbol");

        var (_, _, symbol, err) = CryptocurrencySymbol.TryCreate(symbolRaw);
        if (err is not null)
        {
            return TypedResults.Json(err, statusCode: StatusCodes.Status400BadRequest);
        }

        var searchResult = await cryptocurrencyQuoteApiClient.GetCryptocurrencyQuote(
            searchQuery: new CryptocurrencyQuoteSearchQuery(symbol, BaseCurrency: baseCurrency), 
            cancellationToken);

        if (searchResult.IsSuccess)
        {
            return TypedResults.Ok(searchResult.Value); // TODO: DTO?
        }
        else // Failure
        {
            var error = searchResult.Error;

            diagnosticContext.Error(error);

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
}
