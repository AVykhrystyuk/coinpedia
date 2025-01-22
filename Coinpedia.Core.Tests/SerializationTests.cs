using System.Text.Json;

using Coinpedia.Core.Domain;

namespace Coinpedia.Core.Tests;

public class SerializationTests
{
    private readonly CurrencySymbol EUR = new("EUR");

    [Fact]
    public void DeserializeCurrencyRates()
    {
        var json = @"
            {
                ""BaseCurrency"": ""EUR"",
                ""RatePerCurrency"": {
                    ""USD"": 1.041802,
                    ""EUR"": 1,
                    ""BRL"": 6.275821,
                    ""GBP"": 0.843899,
                    ""AUD"": 1.661701
                },
                ""UpdatedAt"": ""2025-01-21T01:19:17Z""     
            }
        ";

        var currencyRates = JsonSerializer.Deserialize<CurrencyRates>(json)!;

        Assert.Equal(currencyRates.BaseCurrency, EUR);
        Assert.Equal(1, currencyRates.RatePerCurrency[EUR]);
    }
}