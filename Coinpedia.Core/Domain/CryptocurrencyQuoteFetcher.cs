using System.ComponentModel.DataAnnotations;

using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Errors;

using CSharpFunctionalExtensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coinpedia.Core.Domain;

public class CryptocurrencyQuoteFetcher(
    ICryptocurrencyQuoteApiClient cryptocurrencyQuoteApiClient,
    IExchangeRatesApiClient exchangeRatesApiClient,
    IOptions<ICryptocurrencyQuoteFetcherSettings> settings,
    ILogger<CryptocurrencyQuoteFetcher> logger
) : ICryptocurrencyQuoteFetcher
{
    public async Task<Result<MultiCurrencyCryptocurrencyQuotes, Error>> FetchCryptocurrencyQuote(
        CryptocurrencySymbol symbol,
        CancellationToken cancellationToken
    )
    {
        using var _ = logger.BeginAttributesScope(settings.Value);

        var (_, _, baseCurrency, baseCurrencyErr) = CurrencySymbol.TryCreate(settings.Value.BaseCurrency);
        if (baseCurrencyErr is not null)
        {
            return baseCurrencyErr;
        }

        var (_, _, cryptocurrencyQuote, cryptocurrencyFailure) = await cryptocurrencyQuoteApiClient.GetCryptocurrencyQuote(
            searchQuery: new CryptocurrencyQuoteSearchQuery(symbol, BaseCurrency: baseCurrency),
            cancellationToken);

        if (cryptocurrencyFailure is not null)
        {
            return cryptocurrencyFailure;
        }

        // TODO: run both requests parallel

        var requiredCurrencies = settings.Value.RequiredCurrencies
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Select(currency => CurrencySymbol.TryCreate(currency)
                .TapError(err => logger.LogWarning("Failed to create a currencySymbol: {@Error}", err))
            )
            .Where(result => result.IsSuccess)
            .Select(result => result.Value)
            .ToArray();

        var (_, _, exchangeRates, exchangeRatesFailure) = await exchangeRatesApiClient.GetCurrencyRates(
            new GetCurrencyRatesQuery(baseCurrency, requiredCurrencies),
            cancellationToken);

        return cryptocurrencyQuote.Apply(exchangeRates);
    }
}

public interface ICryptocurrencyQuoteFetcherSettings
{
    string BaseCurrency { get; }

    /// <summary>
    /// USD,EUR,GBP,...
    /// </summary>
    [Required]
    string RequiredCurrencies { get; }
}