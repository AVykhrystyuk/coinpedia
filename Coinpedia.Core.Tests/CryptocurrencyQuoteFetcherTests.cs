using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using CSharpFunctionalExtensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

namespace Coinpedia.Core.Tests;

public class CryptocurrencyQuoteFetcherTests
{
    private readonly CurrencySymbol EUR = new("EUR");
    private readonly CurrencySymbol USD = new("USD");
    private readonly CurrencySymbol GBP = new("GBP");

    private readonly CryptocurrencySymbol Cryptocurrency = new("TTT");


    [Fact]
    public async Task BasicHappyPath()
    {
        // Arrange
        var cryptocurrencyQuote = new CryptocurrencyQuote(
            Cryptocurrency,
            UpdatedAt: DateTime.UtcNow,
            Price: 100,
            Currency: EUR
        );

        var cryptocurrencyQuoteApiClientMock = MockCryptocurrencyQuoteApiClient(cryptocurrencyQuote);

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
        var currencyRatesApiClientMock = MockCurrencyRatesApiClient(currencyRates);

        var settingsMock = MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency: EUR.Value,
            requiredCurrencies: string.Join(",", new[] { EUR, USD, GBP }.Select(c => c.Value)));

        var loggerMock = new Mock<ILogger<CryptocurrencyQuoteFetcher>>();

        // Act
        var fetcher = new CryptocurrencyQuoteFetcher(
            cryptocurrencyQuoteApiClientMock.Object,
            currencyRatesApiClientMock.Object,
            Options.Create(settingsMock.Object),
            loggerMock.Object
        );

        var result = await fetcher.FetchCryptocurrencyQuote(Cryptocurrency, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        var prices = result.Value;

        Assert.Equal(prices.BaseCurrency, EUR);

        Assert.Equal(80, prices.PricePerCurrency[GBP]);
        Assert.Equal(100, prices.PricePerCurrency[EUR]);
        Assert.Equal(150, prices.PricePerCurrency[USD]);
    }

    private Mock<ICryptocurrencyQuoteFetcherSettings> MockCryptocurrencyQuoteFetcherSettings(string baseCurrency, string requiredCurrencies)
    {
        var settingsMock = new Mock<ICryptocurrencyQuoteFetcherSettings>();

        settingsMock
            .Setup(s => s.BaseCurrency)
            .Returns(baseCurrency);

        settingsMock
            .Setup(s => s.RequiredCurrencies)
            .Returns(requiredCurrencies);

        return settingsMock;
    }

    private static Mock<ICurrencyRatesApiClient> MockCurrencyRatesApiClient(CurrencyRates currencyRates)
    {
        var currencyRatesApiClientMock = new Mock<ICurrencyRatesApiClient>();

        currencyRatesApiClientMock
            .Setup(api => api.GetCurrencyRates(It.IsAny<GetCurrencyRatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Of<CurrencyRates, Error>(currencyRates));

        return currencyRatesApiClientMock;
    }

    private static Mock<ICryptocurrencyQuoteApiClient> MockCryptocurrencyQuoteApiClient(CryptocurrencyQuote cryptocurrencyQuote)
    {
        var cryptocurrencyQuoteApiClientMock = new Mock<ICryptocurrencyQuoteApiClient>();

        cryptocurrencyQuoteApiClientMock
            .Setup(api => api.GetCryptocurrencyQuote(It.IsAny<CryptocurrencyQuoteSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Of<CryptocurrencyQuote, Error>(cryptocurrencyQuote));

        return cryptocurrencyQuoteApiClientMock;
    }
}