using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;

namespace Coinpedia.Infrastructure.ApiClients;

public static class ExchangeRatesApiClientExtensions
{
    public static Result<CurrencyRates, Error> ToCurrencyRates(
        this ExchangeRatesApiClient.ResponseContent responseContent,
        GetCurrencyRatesQuery ratesQuery,
        ILogger logger
    )
    {
        var ratePerCurrency = new Dictionary<CurrencySymbol, decimal>();

        foreach (var currency in ratesQuery.ForCurrencies)
        {
            if (!responseContent.Rates.TryGetValue(currency.Value, out var rate))
            {
                logger.LogError("[ER]: Unexpected response - missing rate for currency '{MissingCurrency}'. Skipping this currency", currency.Value);
                continue;
            }

            ratePerCurrency.Add(currency, rate);
        }

        var updatedAt = DateTimeOffset.FromUnixTimeSeconds(responseContent.UnixTimestamp).UtcDateTime;

        return new CurrencyRates(ratesQuery.BaseCurrency, ratePerCurrency, updatedAt);
    }
}
