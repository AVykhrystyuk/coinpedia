using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using CSharpFunctionalExtensions;

namespace Coinpedia.Core.ApiClients;

public interface ICurrencyRatesApiClient
{
    /// <returns>
    /// CurrencyRates | RequestCancelled | None | TooManyRequests | InternalError
    /// </returns>
    Task<Result<CurrencyRates, Error>> GetCurrencyRates(GetCurrencyRatesQuery ratesQuery, CancellationToken cancellationToken = default);
}

public record GetCurrencyRatesQuery(
    CurrencySymbol BaseCurrency, 
    IReadOnlyList<CurrencySymbol> ForCurrencies);
