using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;

namespace Coinpedia.Infrastructure.ApiClients;

public static class CoinMarketCapCryptocurrencyQuoteApiClientExtensions
{
    public static Result<CryptocurrencyQuote, Error> ToCryptocurrencyQuote(
        this CoinMarketCapCryptocurrencyQuoteApiClient.ResponseContent responseContent,
        CryptocurrencyQuoteSearchQuery searchQuery,
        ILogger logger
    )
    {
        var cryptocurrenciesPerSymbol = responseContent.CryptocurrencyDataPerSymbol!;

        using var _1 = logger.BeginAttributedScope(cryptocurrenciesPerSymbol.Count);

        if (cryptocurrenciesPerSymbol.Count < 1)
        {
            logger.LogError("[CMC]: Unexpected response - 'data' collection is empty");
            return new None { Message = "[CryptocurrencyApi]: Unexpected response - data collection is empty", Context = Context() };
        }

        if (cryptocurrenciesPerSymbol.Count > 1)
        {
            logger.LogWarning("[CMC]: Data for more than one symbol is returned (while we only asked for one). Using the first one that matches the symbol");
        }

        if (!cryptocurrenciesPerSymbol.TryGetValue(searchQuery.Cryptocurrency.Value, out var cryptocurrencies))
        {
            logger.LogError("[CMC]: Unexpected response - cannot find cryptocurrencyData for requested symbol");
            return new None { Message = "[CryptocurrencyApi]: Unexpected response - cannot find cryptocurrencyData for requested symbol", Context = Context() };
        }

        using var _2 = logger.BeginAttributedScope(cryptocurrencies.Count);

        if (cryptocurrencies.Count < 1)
        {
            logger.LogError("[CMC]: Unexpected response - cryptocurrencies are empty for the symbol");
            return new NotFound { Message = "[CryptocurrencyApi]: Unexpected response - cryptocurrencies are empty for the symbol", Context = Context() };
        }

        var highestMarketCapCryptocurrency = cryptocurrencies.First();
        using var _3 = logger.BeginAttributedScope(highestMarketCapCryptocurrency.Id, highestMarketCapCryptocurrency.Name);

        if (cryptocurrencies.Count > 1)
        {
            var cryptocurrencyNames = cryptocurrencies.Select(c => c.Name).ToArray();
            logger.LogWarning("[CMC]: More than one cryptocurrencies are found for the symbol. Using the cryptocurrency with the highest market cap.");
        }

        var quotePerCurrency = highestMarketCapCryptocurrency.QuotePerCurrency;

        using var _4 = logger.BeginAttributedScope(quotePerCurrency.Count);

        if (quotePerCurrency.Count < 1)
        {
            logger.LogError("[CMC]: Unexpected response - 'quote' collection is empty");
            return new None { Message = "[CryptocurrencyApi]: Unexpected response - 'quote' collection is empty", Context = Context() };
        }

        if (quotePerCurrency.Count > 1)
        {
            logger.LogWarning("[CMC]: 'quote' has data for more than one currency (while we only asked for one). Using the one that matches base currency");
        }

        if (!quotePerCurrency.TryGetValue(searchQuery.BaseCurrency.Value, out var quote))
        {
            logger.LogError("[CMC]: Unexpected response - cannot find quoteData for base currency");
            return new None { Message = "[CryptocurrencyApi]: Unexpected response - cannot find quoteData for base currency", Context = Context() };
        }

        return new CryptocurrencyQuote(
            Cryptocurrency: searchQuery.Cryptocurrency,
            UpdatedAt: quote.LastUpdated,
            Price: quote.Price ?? 0.0M,
            Currency: searchQuery.BaseCurrency
        );

        object Context() => new { searchQuery };
    }
}
