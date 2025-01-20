namespace Coinpedia.Core.Domain;

public record CurrencyRates(
    IReadOnlyDictionary<CurrencySymbol, decimal> RatePerCurrency,
    CurrencySymbol BaseCurrency,
    DateTime UpdatedAt);