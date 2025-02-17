using System.ComponentModel.DataAnnotations;

using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coinpedia.Core.Domain;

public class CryptocurrencyQuoteFetcher(
    ICryptocurrencyQuoteApiClient cryptocurrencyQuoteApiClient,
    ICurrencyRatesApiClient currencyRatesApiClient,
    IOptions<ICryptocurrencyQuoteFetcherSettings> settings,
    ILogger<CryptocurrencyQuoteFetcher> logger
) : ICryptocurrencyQuoteFetcher
{
    public async Task<Result<MultiCurrencyCryptocurrencyQuotes, Error>> FetchCryptocurrencyQuote(
        CryptocurrencySymbol symbol,
        CancellationToken cancellationToken
    )
    {
        using var _ = logger.BeginAttributedScope(settings.Value);

        var (_, _, baseCurrency, baseCurrencyErr) = CurrencySymbol.TryCreate(settings.Value.BaseCurrency);
        if (baseCurrencyErr is not null)
        {
            return baseCurrencyErr;
        }

        // NOTE: uses cache (decorated)
        // TODO: [memory-consumption-mitigation]: LRU cache? since there are so many different cryptocurrencies
        var cryptocurrencyQuoteTask = cryptocurrencyQuoteApiClient.GetCryptocurrencyQuote(
            searchQuery: new CryptocurrencyQuoteSearchQuery(symbol, BaseCurrency: baseCurrency),
            cancellationToken);

        var currencyRatesTask = GetCurrencyRates(baseCurrency, cancellationToken);

        await Task.WhenAll(cryptocurrencyQuoteTask, currencyRatesTask);

        var (_, _, cryptocurrencyQuote, cryptocurrencyFailure) = await cryptocurrencyQuoteTask;
        if (cryptocurrencyFailure is not null)
        {
            return cryptocurrencyFailure;
        }

        var (_, _, currencyRates, currencyRatesFailure) = await currencyRatesTask;
        if (currencyRatesFailure is not null)
        {
            // TODO: should we not fail when currency rates are unavailable? should we just fallback to only showing the base currency rate?
            return currencyRatesFailure;
        }

        return cryptocurrencyQuote.Apply(currencyRates);
    }

    private async Task<Result<CurrencyRates, Error>> GetCurrencyRates(CurrencySymbol baseCurrency, CancellationToken cancellationToken)
    {
        var rawRequiredCurrencies = settings.Value.RequiredCurrencies
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .ToArray();

        if (rawRequiredCurrencies.Length == 0)
        {
            return new InternalError
            {
                Message = "Required currencies are not provided. They must be configured in settings",
                Context = new { settings.Value.RequiredCurrencies, rawRequiredCurrencies }
            };
        }

        using var _ = logger.BeginAttributedScope(rawRequiredCurrencies);

        var requiredCurrencies = rawRequiredCurrencies
            .Select(currency => CurrencySymbol.TryCreate(currency)
                .TapError(err => logger.LogWarning("Failed to create a currencySymbol: {@Error}", err))
            )
            .Where(result => result.IsSuccess)
            .Select(result => result.Value)
            .ToArray();

        // NOTE: uses cache (decorated)
        return await currencyRatesApiClient.GetCurrencyRates(
            new GetCurrencyRatesQuery(baseCurrency, requiredCurrencies),
            cancellationToken);
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