using Coinpedia.Core.Errors;

namespace Coinpedia.Core.Domain;

public record CryptocurrencyQuote(
    CryptocurrencySymbol Cryptocurrency,
    DateTime UpdatedAt,
    decimal Price,
    CurrencySymbol Currency
)
{
    public Result<MultiCurrencyCryptocurrencyQuotes, Error> Apply(CurrencyRates currencyRates)
    {
        if (currencyRates.BaseCurrency != Currency)
        {
            return new InvalidInput
            { 
                Message = "BaseCurrency does not match", 
                Context = new { quoteCurrency = Currency, currencyRatesCurrency = currencyRates.BaseCurrency, quote = this, currencyRates } 
            };
        }

        var pricePerCurrency = new Dictionary<CurrencySymbol, decimal>();

        foreach (var (currency, rate) in currencyRates.RatePerCurrency)
        {
            pricePerCurrency[currency] = Price * rate;
        }

        return new MultiCurrencyCryptocurrencyQuotes(
            Cryptocurrency,
            CryptocurrencyUpdatedAt: UpdatedAt,
            CurrencyRatesUpdatedAt: currencyRates.UpdatedAt,
            pricePerCurrency,
            Currency
        );
    }
}