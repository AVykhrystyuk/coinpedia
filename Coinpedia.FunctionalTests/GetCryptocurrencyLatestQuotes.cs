using System.Text.Json;

using Coinpedia.FunctionalTests.Common;
using Coinpedia.WebApi.Config;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using static Coinpedia.WebApi.Handlers.CryptocurrencyHandlers;

namespace Coinpedia.FunctionalTests;

public class GetCryptocurrencyLatestQuotes(TestWebAppFactory app) : BaseFuncionalTest(app)
{
    [Fact]
    public async Task ShouldReturn_Ok_When_BtcCryptocurrencyIsRequested()
    {
        // Arrange
        using var scope = App.Services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<IOptions<Settings>>().Value;

        var currency = settings.BaseCurrency;
        var symbol = "BTC";

        App.ExchangeRatesApiClientResponseHandler = request =>
            HttpResponseMessages.OK(ApiClientJsonResponses.ExchangeRates.Ok(currency));
        App.CryptocurrencyQuoteApiClientResponseHandler = request =>
            HttpResponseMessages.OK(ApiClientJsonResponses.CoinMarketCap.Ok(symbol, currency));

        // Act
        var response = await HttpClient.GetAsync($"/v1/cryptocurrencies/{symbol}/quotes/latest");
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, responseContent);

        var quotes = JsonSerializer.Deserialize<MultiCurrencyCryptocurrencyQuotesDto>(responseContent, JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(quotes);
        Assert.Equal(symbol, quotes.Cryptocurrency);
        Assert.Equal(currency, quotes.BaseCurrency);
        Assert.Equal(5, quotes.PricePerCurrency.Count);
        Assert.Equal(93419.00775880407M, quotes.PricePerCurrency[currency]);
    }
}
