namespace Coinpedia.Core.Domain;

public record CryptocurrencyQuote(
    CryptocurrencySymbol Symbol,
    DateTime UpdatedAt,
    decimal Price,
    CurrencySymbol Currency
)
{
    public MultiCurrencyCryptocurrencyQuotes Apply(CurrencyRates currencyRates)
    {
        var pricePerCurrency = new Dictionary<CurrencySymbol, decimal>();

        foreach (var (currency, rate) in currencyRates.RatePerCurrency)
        {
            pricePerCurrency[currency] = Price * rate;
        }

        return new MultiCurrencyCryptocurrencyQuotes(
            Symbol,
            CryptocurrencyUpdatedAt: UpdatedAt,
            CurrencyRatesUpdatedAt: currencyRates.UpdatedAt,
            pricePerCurrency,
            Currency
        );
    }
}