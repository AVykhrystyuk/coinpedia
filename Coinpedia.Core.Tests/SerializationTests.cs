using System.Text.Json;

using Coinpedia.Core.Domain;
using Coinpedia.Infrastructure.ApiClients;

namespace Coinpedia.Core.Tests;

public class SerializationTests
{
    private readonly CurrencySymbol EUR = new("EUR");

    [Fact]
    public void Deserialize_CurrencyRates()
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

        Assert.Equal(EUR, currencyRates.BaseCurrency);
        Assert.Equal(1, currencyRates.RatePerCurrency[EUR]);
    }

    [Fact]
    public void Deserialize_ExchangeRatesApiClient_ErrorResponseContent_1()
    {
        var json = @"
            {
                ""error"": {
                    ""code"": ""invalid_base_currency"",
                    ""message"": ""An unexpected error ocurred. [Technical Support: support@apilayer.com]""
                }
            }
        ";

        var errorResponseContent = JsonSerializer.Deserialize<ExchangeRatesApiClient.ErrorResponseContent>(json)!;

        Assert.Null(errorResponseContent.Success);
        Assert.Equal("invalid_base_currency", errorResponseContent?.Error?.Code);
        Assert.Equal("An unexpected error ocurred. [Technical Support: support@apilayer.com]", errorResponseContent?.Error?.Message);
    }

    [Fact]
    public void Deserialize_ExchangeRatesApiClient_ErrorResponseContent_2()
    {
        var json = @"
            {
                ""success"": false,
                ""error"": {
                    ""code"": 104,
                    ""info"": ""Your monthly API request volume has been reached. Please upgrade your plan.""
                }
            }
        ";

        var errorResponseContent = JsonSerializer.Deserialize<ExchangeRatesApiClient.ErrorResponseContent>(json)!;

        Assert.False(errorResponseContent.Success);
        Assert.Equal(104, errorResponseContent?.Error?.Code);
        Assert.Equal("Your monthly API request volume has been reached. Please upgrade your plan.", errorResponseContent?.Error?.Info);
    }
}