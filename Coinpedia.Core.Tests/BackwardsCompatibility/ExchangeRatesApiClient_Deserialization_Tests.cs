using System.Text.Json;

using Coinpedia.Infrastructure.ApiClients;

namespace Coinpedia.Core.Tests.BackwardsCompatibility;

public class ExchangeRatesApiClient_Deserialization_Tests
{
    [Fact]
    public void ResponseContent()
    {
        var json = """
            {
                "success": true,
                "timestamp": 1738511955,
                "base": "EUR",
                "date": "2025-02-02",
                "rates": {
                    "EUR": 1,
                    "USD": 1.036215,
                    "BRL": 6.053612,
                    "GBP": 0.836177,
                    "AUD": 1.6614
                }
            }
        """;

        var responseContent = JsonSerializer.Deserialize<ExchangeRatesApiClient.ResponseContent>(json)!;

        Assert.Equal("EUR", responseContent.Base);
        Assert.Equal(1, responseContent.Rates["EUR"]);
        Assert.Equal(5, responseContent.Rates.Count);
    }

    [Fact]
    public void ErrorResponseContent_1()
    {
        var json = """
            {
                "error": {
                    "code": "invalid_base_currency",
                    "message": "An unexpected error ocurred. [Technical Support: support@apilayer.com]"
                }
            }
        """;

        var errorResponseContent = JsonSerializer.Deserialize<ExchangeRatesApiClient.ErrorResponseContent>(json)!;

        Assert.Null(errorResponseContent.Success);
        Assert.Equal("invalid_base_currency", errorResponseContent?.Error?.Code);
        Assert.Equal("An unexpected error ocurred. [Technical Support: support@apilayer.com]", errorResponseContent?.Error?.Message);
    }

    [Fact]
    public void ErrorResponseContent_2()
    {
        var json = """
            {
                "success": false,
                "error": {
                    "code": 104,
                    "info": "Your monthly API request volume has been reached. Please upgrade your plan."
                }
            }
        """;

        var errorResponseContent = JsonSerializer.Deserialize<ExchangeRatesApiClient.ErrorResponseContent>(json)!;

        Assert.False(errorResponseContent.Success);
        Assert.Equal(104, errorResponseContent?.Error?.Code);
        Assert.Equal("Your monthly API request volume has been reached. Please upgrade your plan.", errorResponseContent?.Error?.Info);
    }
}