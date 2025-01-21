using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

namespace Coinpedia.Core;

public interface ICryptocurrencyQuoteFetcher
{
    Task<Result<MultiCurrencyCryptocurrencyQuotes, Error>> FetchCryptocurrencyQuote(CryptocurrencySymbol symbol, CancellationToken cancellationToken);
}

public record MultiCurrencyCryptocurrencyQuotes(
    CryptocurrencySymbol Cryptocurrency,
    DateTime CryptocurrencyUpdatedAt,
    DateTime CurrencyRatesUpdatedAt,
    IReadOnlyDictionary<CurrencySymbol, decimal> PricePerCurrency,
    CurrencySymbol BaseCurrency);
