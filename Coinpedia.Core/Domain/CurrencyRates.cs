namespace Coinpedia.Core.Domain;

public record CurrencyRates(
    CurrencySymbol BaseCurrency,
    IReadOnlyDictionary<CurrencySymbol, decimal> RatePerCurrency,
    DateTimeOffset UpdatedAt);