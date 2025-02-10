using System.Text.Json;

using Coinpedia.FunctionalTests.Common;
using Coinpedia.WebApi.Config;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using static Coinpedia.WebApi.Handlers.CryptocurrencyHandlers;

namespace Coinpedia.FunctionalTests;

public class GetCryptocurrencyLatestQuotes : BaseFuncionalTest
{
    public GetCryptocurrencyLatestQuotes(TestWebAppFactory app) : base(app)
    {
        App.BaseCurrency = "EUR";
        App.ExtraCurrency = "TTT";
    }

    [Fact]
    public async Task ShouldReturn_Ok_When_BtcCryptocurrencyIsRequested()
    {
        // Arrange
        using var scope = App.Services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>().Value;

        var baseCurrency = settings.BaseCurrency;
        var baseCurrencyPrice = 935000.00775880407M;
        var extraCurrency = App.ExtraCurrency ?? "NONE";
        var extraCurrencyRate = 1.05M;

        var symbol = "BTC";

        App.ExchangeRatesApiClientResponseHandler = request =>
            HttpResponseMessages.OK(ApiClientJsonResponses.ExchangeRates.Ok(baseCurrency, extraCurrency, extraCurrencyRate));

        App.CryptocurrencyQuoteApiClientResponseHandler = request =>
            HttpResponseMessages.OK(ApiClientJsonResponses.CoinMarketCap.Ok(symbol, baseCurrency, baseCurrencyPrice));

        // Act
        var response = await HttpClient.GetAsync($"/v1/cryptocurrencies/{symbol}/quotes/latest");
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, responseContent);

        var quotes = JsonSerializer.Deserialize<MultiCurrencyCryptocurrencyQuotesDto>(responseContent, JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(quotes);
        Assert.Equal(symbol, quotes.Cryptocurrency);
        Assert.Equal(baseCurrency, quotes.BaseCurrency);
        Assert.Equal(5 + 1 /* ExtraCurrency */, quotes.PricePerCurrency.Count);
        Assert.Equal(baseCurrencyPrice, quotes.PricePerCurrency[baseCurrency]);
        Assert.Equal(baseCurrencyPrice * extraCurrencyRate, quotes.PricePerCurrency[extraCurrency]);
    }
}
