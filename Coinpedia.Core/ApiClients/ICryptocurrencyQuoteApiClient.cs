using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using CSharpFunctionalExtensions;

namespace Coinpedia.Core.ApiClients;

public interface ICryptocurrencyQuoteApiClient
{
    /// <returns>
    /// CryptocurrencyQuote | None | PaymentRequired | TooManyRequests | InternalError
    /// </returns>
    Task<Result<CryptocurrencyQuote, Error>> GetCryptocurrencyQuote(
        CryptocurrencyQuoteSearchQuery searchQuery,
        CancellationToken cancellationToken = default);
}

public record CryptocurrencyQuoteSearchQuery(CryptocurrencySymbol Symbol, string BaseCurrency);

public record CryptocurrencyQuote(
    CryptocurrencySymbol Symbol,
    DateTime UpdatedAt,
    decimal Price,
    string Currency);