using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

namespace Coinpedia.Core.ApiClients;

public interface ICryptocurrencyQuoteApiClient
{
    /// <returns>
    /// CryptocurrencyQuote | RequestCancelled | None | NotFound | TooManyRequests | InternalError
    /// </returns>
    Task<Result<CryptocurrencyQuote, Error>> GetCryptocurrencyQuote(
        CryptocurrencyQuoteSearchQuery searchQuery,
        CancellationToken cancellationToken = default);
}

public record CryptocurrencyQuoteSearchQuery(
    CryptocurrencySymbol Cryptocurrency,
    CurrencySymbol BaseCurrency);
