using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using static Coinpedia.Core.Tests.SetupTest;

namespace Coinpedia.Core.Tests.Domain;

public class CryptocurrencyQuote_Apply_CurrencyRates_Tests
{
    [Fact]
    public void HappyPath()
    {
        // Arrange
        var cryptocurrencyQuote = NewCryptocurrencyQuote(Cryptocurrency, price: 100, currency: EUR);

        var currencyRates = NewCurrencyRates(
            baseCurrency: EUR, 
            ratePerCurrency: new Dictionary<CurrencySymbol, decimal>
            {
                [GBP] = 0.8M,
                [EUR] = 1.0M,
                [USD] = 1.5M,
            }
        );

        // Act
        var result = cryptocurrencyQuote.Apply(currencyRates);

        // Assert
        Assert.True(result.IsSuccess);

        var prices = result.Value;

        Assert.Equal(prices.BaseCurrency, currencyRates.BaseCurrency);
        Assert.Equal(prices.BaseCurrency, cryptocurrencyQuote.Currency);
        Assert.Equal(prices.Cryptocurrency, cryptocurrencyQuote.Cryptocurrency);

        Assert.Equal(80, prices.PricePerCurrency[GBP]);
        Assert.Equal(100, prices.PricePerCurrency[EUR]);
        Assert.Equal(150, prices.PricePerCurrency[USD]);
    }

    [Fact]
    public void CurrencyMismatch()
    {
        // Arrange
        var cryptocurrencyQuote = NewCryptocurrencyQuote(
            Cryptocurrency, 
            price: 100, 
            currency: EUR // !
        );

        var currencyRates = NewCurrencyRates(baseCurrency: USD); // not EUR !

        // Act
        var result = cryptocurrencyQuote.Apply(currencyRates);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<InvalidInput>(result.Error);
    }
}
