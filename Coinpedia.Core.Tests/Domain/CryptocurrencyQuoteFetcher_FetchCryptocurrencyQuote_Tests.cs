using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Options;

using Moq;

using static Coinpedia.Core.Tests.SetupTest;

namespace Coinpedia.Core.Tests.Domain;

public class CryptocurrencyQuoteFetcher_FetchCryptocurrencyQuote_Tests
{
    [Fact]
    public async Task HappyPath()
    {
        // Arrange
        var cryptocurrencyQuote = NewCryptocurrencyQuote(
            Cryptocurrency,
            price: 100,
            currency: EUR
        );

        var cryptocurrencyQuoteApiClientMock = MockCryptocurrencyQuoteApiClient(cryptocurrencyQuote);

        var currencyRates = NewCurrencyRates(
            baseCurrency: EUR,
            ratePerCurrency: new Dictionary<CurrencySymbol, decimal>
            {
                [GBP] = 0.8M,
                [EUR] = 1.0M,
                [USD] = 1.5M,
            }
        );
        var currencyRatesApiClientMock = MockCurrencyRatesApiClient(currencyRates);

        var settingsMock = MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency: EUR,
            requiredCurrencies: [EUR, USD, GBP]
        );

        // Act
        var fetcher = NewCryptocurrencyQuoteFetcher(cryptocurrencyQuoteApiClientMock, currencyRatesApiClientMock, settingsMock);

        var result = await fetcher.FetchCryptocurrencyQuote(Cryptocurrency, CancellationToken.None);

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
    public async Task CurrencyMismatch()
    {
        // Arrange
        var cryptocurrencyQuoteApiClientMock = MockCryptocurrencyQuoteApiClient(NewCryptocurrencyQuote(
            Cryptocurrency,
            price: 100,
            currency: EUR // !
        ));

        var currencyRatesApiClientMock = MockCurrencyRatesApiClient(NewCurrencyRates(
            baseCurrency: USD // not EUR!
        ));

        var settingsMock = MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency: EUR,
            requiredCurrencies: [EUR, USD, GBP]
        );

        // Act
        var fetcher = NewCryptocurrencyQuoteFetcher(cryptocurrencyQuoteApiClientMock, currencyRatesApiClientMock, settingsMock);

        var result = await fetcher.FetchCryptocurrencyQuote(Cryptocurrency, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<InvalidInput>(result.Error);
    }

    [Fact]
    public async Task Error_from_CryptocurrencyQuoteApiClient()
    {
        // Arrange
        var cryptocurrencyQuoteApiClientMock = MockCryptocurrencyQuoteApiClient(new NotFound { Message = "cryptocurrency does not exist", Context = NonExistentCryptocurrency });

        var currencyRatesApiClientMock = MockCurrencyRatesApiClient(NewCurrencyRates(
            baseCurrency: EUR
        ));

        var settingsMock = MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency: EUR,
            requiredCurrencies: [EUR, USD, GBP]
        );

        // Act
        var fetcher = NewCryptocurrencyQuoteFetcher(cryptocurrencyQuoteApiClientMock, currencyRatesApiClientMock, settingsMock);

        var result = await fetcher.FetchCryptocurrencyQuote(Cryptocurrency, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<NotFound>(result.Error);
    }

    [Fact]
    public async Task Error_from_CurrencyRatesApiClient()
    {
        // Arrange
        var cryptocurrencyQuoteApiClientMock = MockCryptocurrencyQuoteApiClient(NewCryptocurrencyQuote(
            Cryptocurrency,
            price: 100,
            currency: EUR
        ));

        var currencyRatesApiClientMock = MockCurrencyRatesApiClient(new TooManyRequests { Message = "TooManyRequests", Context = "some context" });

        var settingsMock = MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency: EUR,
            requiredCurrencies: [EUR, USD, GBP]
        );

        // Act
        var fetcher = NewCryptocurrencyQuoteFetcher(cryptocurrencyQuoteApiClientMock, currencyRatesApiClientMock, settingsMock);

        var result = await fetcher.FetchCryptocurrencyQuote(Cryptocurrency, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.IsType<TooManyRequests>(result.Error);
    }

    private static CryptocurrencyQuoteFetcher NewCryptocurrencyQuoteFetcher(
        Mock<ICryptocurrencyQuoteApiClient> cryptocurrencyQuoteApiClientMock,
        Mock<ICurrencyRatesApiClient> currencyRatesApiClientMock,
        Mock<ICryptocurrencyQuoteFetcherSettings> settingsMock
    ) => new(
        cryptocurrencyQuoteApiClientMock.Object,
        currencyRatesApiClientMock.Object,
        Options.Create(settingsMock.Object),
        MockLogger().Object
    );
}