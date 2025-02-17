using System.Net;
using System.Text.Json;

using Coinpedia.Core.Errors;
using Coinpedia.FunctionalTests.Common;
using Coinpedia.WebApi.Errors;

using Xunit.Abstractions;

using static Coinpedia.WebApi.Handlers.CryptocurrencyHandlers;

namespace Coinpedia.FunctionalTests;

public class GetCryptocurrencyLatestQuotes(TestWebAppFactory app, ITestOutputHelper testOutputHelper) : BaseFuncionalTest(app)
{
    [Fact]
    public async Task ShouldReturn_Ok_When_BtcCryptocurrencyIsRequested()
    {
        // Arrange

        var baseCurrency = "EUR";
        var extraCurrency = "TTT";
        var baseCurrencyPrice = 935000.00775880407M;
        var extraRate = (Currency: extraCurrency, RateValue: 1.05M);

        var symbol = "BTC";

        App.TestSetup = new TestSetup
        {
            BaseCurrency = baseCurrency,
            ExtraCurrency = extraCurrency,

            ExchangeRatesApiClientResponseHandler = request =>
                HttpResponseMessages.OK(ApiClientJsonResponses.ExchangeRates.Ok(baseCurrency, extraRate)),

            CryptocurrencyQuoteApiClientResponseHandler = request =>
                HttpResponseMessages.OK(ApiClientJsonResponses.CoinMarketCap.Ok(symbol, baseCurrency, baseCurrencyPrice)),
        };

        // using var testScope = App.Services.CreateScope();
        //var settings = testScope.ServiceProvider.GetRequiredService<IOptions<Settings>>().Value;

        var httpClient = App.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/v1/cryptocurrencies/{symbol}/quotes/latest");
        var responseContent = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(responseContent);
        Assert.True(response.IsSuccessStatusCode, responseContent);

        var quotes = JsonSerializer.Deserialize<MultiCurrencyCryptocurrencyQuotesDto>(responseContent, JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(quotes);
        Assert.Equal(symbol, quotes.Cryptocurrency);
        Assert.Equal(baseCurrency, quotes.BaseCurrency);
        Assert.Equal(5 + 1 /* ExtraCurrency */, quotes.PricePerCurrency.Count);
        Assert.Equal(baseCurrencyPrice, quotes.PricePerCurrency[baseCurrency]);
        Assert.Equal(baseCurrencyPrice * extraRate.RateValue, quotes.PricePerCurrency[extraRate.Currency]);
    }

    [Fact]
    public async Task ShouldReturn_NotFound_When_RequestedUnknownCryptocurrency()
    {
        // Arrange
        var symbol = "NONE";
        var baseCurrency = "EUR";

        App.TestSetup = new TestSetup
        {
            BaseCurrency = baseCurrency,

            ExchangeRatesApiClientResponseHandler = request =>
                HttpResponseMessages.OK(ApiClientJsonResponses.ExchangeRates.Ok(baseCurrency)),

            CryptocurrencyQuoteApiClientResponseHandler = request =>
                HttpResponseMessages.OK(ApiClientJsonResponses.CoinMarketCap.NotFound(symbol)),
        };

        var httpClient = App.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/v1/cryptocurrencies/{symbol}/quotes/latest");
        var responseContent = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(responseContent);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = JsonSerializer.Deserialize<ErrorDto>(responseContent, JsonSerializerOptions.Web);

        // Assert
        AssertError(error, expecteStatusCode: HttpStatusCode.NotFound, expectedError: nameof(NotFound));
    }

    [Fact]
    public async Task ShouldReturn_None_When_RatesAreUnavailable()
    {
        // Arrange
        var symbol = "BTC";

        App.TestSetup = new TestSetup
        {
            ExchangeRatesApiClientResponseHandler = request =>
                HttpResponseMessages.InternalServerError(),

            CryptocurrencyQuoteApiClientResponseHandler = request =>
                HttpResponseMessages.OK(ApiClientJsonResponses.CoinMarketCap.Ok(symbol, baseCurrency: "EUR", baseCurrencyPrice: 123)),
        };

        var httpClient = App.CreateClient();

        // Act
        var response = await httpClient.GetAsync($"/v1/cryptocurrencies/{symbol}/quotes/latest");
        var responseContent = await response.Content.ReadAsStringAsync();
        testOutputHelper.WriteLine(responseContent);
        Assert.Equal(HttpStatusCode.FailedDependency, response.StatusCode);

        var error = JsonSerializer.Deserialize<ErrorDto>(responseContent, JsonSerializerOptions.Web);

        // Assert
        AssertError(error, expecteStatusCode: HttpStatusCode.FailedDependency, expectedError: nameof(FailedDependency));
    }

    private static void AssertError(ErrorDto? error, HttpStatusCode expecteStatusCode, string expectedError)
    {
        Assert.NotNull(error);
        Assert.Equal((int)expecteStatusCode, error.StatusCode);
        Assert.Equal(expectedError, error.Error);
        Assert.NotNull(error.ErrorMessage);
    }
}
