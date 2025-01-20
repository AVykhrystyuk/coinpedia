using Coinpedia.Core.Domain;

namespace Coinpedia.Core.Tests;

public class ApplicationOfCurrencyRatesUnitTests
{
    private readonly CurrencySymbol EUR = new("EUR");
    private readonly CurrencySymbol USD = new("USD");
    private readonly CurrencySymbol GBP = new("GBP");

    private readonly CryptocurrencySymbol Cryptocurrency = new("TTT");


    [Fact]
    public void BasicHappyPath()
    {
        var cryptocurrencyQuote = new CryptocurrencyQuote(
            Cryptocurrency,
            UpdatedAt: DateTime.UtcNow,
            Price: 100,
            Currency: EUR
        );

        var currencyRates = new CurrencyRates(
            BaseCurrency: EUR,
            UpdatedAt: DateTime.UtcNow,
            RatePerCurrency: new Dictionary<CurrencySymbol, decimal>
            {
                [GBP] = 0.8M,
                [EUR] = 1.0M,
                [USD] = 1.5M,
            }
        );

        var result = cryptocurrencyQuote.Apply(currencyRates);

        Assert.True(result.IsSuccess);

        var prices = result.Value;

        Assert.Equal(prices.BaseCurrency, EUR);

        Assert.Equal(80, prices.PricePerCurrency[GBP]);
        Assert.Equal(100, prices.PricePerCurrency[EUR]);
        Assert.Equal(150, prices.PricePerCurrency[USD]);
    }

    [Fact]
    public void BasicUnhappyPath()
    {
        var cryptocurrencyQuote = new CryptocurrencyQuote(
            Cryptocurrency,
            UpdatedAt: DateTime.UtcNow,
            Price: 100,
            Currency: EUR
        );

        var currencyRates = new CurrencyRates(
            BaseCurrency: USD,
            UpdatedAt: DateTime.UtcNow,
            RatePerCurrency: new Dictionary<CurrencySymbol, decimal>()
        );

        var result = cryptocurrencyQuote.Apply(currencyRates);

        Assert.True(result.IsFailure);
    }
}