using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;

using Moq;

namespace Coinpedia.Core.Tests;

public static class SetupTest
{
    public static readonly CurrencySymbol EUR = new("EUR");
    public static readonly CurrencySymbol USD = new("USD");
    public static readonly CurrencySymbol GBP = new("GBP");

    public static readonly CryptocurrencySymbol Cryptocurrency = new("BTC");
    public static readonly CryptocurrencySymbol NonExistentCryptocurrency = new("_NONE_");

    public static CurrencyRates NewCurrencyRates(
        CurrencySymbol baseCurrency,
        IReadOnlyDictionary<CurrencySymbol, decimal>? ratePerCurrency = null
    ) => new(
        BaseCurrency: baseCurrency,
        UpdatedAt: DateTime.UtcNow,
        RatePerCurrency: ratePerCurrency ?? new Dictionary<CurrencySymbol, decimal>
        {
            [GBP] = 0.8M,
            [EUR] = 1.0M,
            [USD] = 1.5M,
        }
    );

    public static CryptocurrencyQuote NewCryptocurrencyQuote(CryptocurrencySymbol cryptocurrency, decimal price, CurrencySymbol currency) => new(
        cryptocurrency,
        UpdatedAt: DateTime.UtcNow,
        Price: price,
        Currency: currency
    );


    public static Mock<ILogger<CryptocurrencyQuoteFetcher>> MockLogger() => new();

    public static Mock<ICryptocurrencyQuoteFetcherSettings> MockCryptocurrencyQuoteFetcherSettings(CurrencySymbol baseCurrency, IReadOnlyList<CurrencySymbol> requiredCurrencies) =>
        MockCryptocurrencyQuoteFetcherSettings(
            baseCurrency.Value,
            requiredCurrencies: string.Join(",", requiredCurrencies.Select(c => c.Value))
        );

    public static Mock<ICryptocurrencyQuoteFetcherSettings> MockCryptocurrencyQuoteFetcherSettings(string baseCurrency, string requiredCurrencies)
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

    public static Mock<ICurrencyRatesApiClient> MockCurrencyRatesApiClient(CurrencyRates currencyRates)
    {
        var currencyRatesApiClientMock = new Mock<ICurrencyRatesApiClient>();

        currencyRatesApiClientMock
            .Setup(api => api.GetCurrencyRates(It.IsAny<GetCurrencyRatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Of<CurrencyRates, Error>(currencyRates));

        return currencyRatesApiClientMock;
    }

    public static Mock<ICurrencyRatesApiClient> MockCurrencyRatesApiClient(Error error)
    {
        var currencyRatesApiClientMock = new Mock<ICurrencyRatesApiClient>();

        currencyRatesApiClientMock
            .Setup(api => api.GetCurrencyRates(It.IsAny<GetCurrencyRatesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CurrencyRates, Error>(error));

        return currencyRatesApiClientMock;

    }
    public static Mock<ICryptocurrencyQuoteApiClient> MockCryptocurrencyQuoteApiClient(CryptocurrencyQuote cryptocurrencyQuote)
    {
        var cryptocurrencyQuoteApiClientMock = new Mock<ICryptocurrencyQuoteApiClient>();

        cryptocurrencyQuoteApiClientMock
            .Setup(api => api.GetCryptocurrencyQuote(It.IsAny<CryptocurrencyQuoteSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Of<CryptocurrencyQuote, Error>(cryptocurrencyQuote));

        return cryptocurrencyQuoteApiClientMock;
    }

    public static Mock<ICryptocurrencyQuoteApiClient> MockCryptocurrencyQuoteApiClient(Error error)
    {
        var cryptocurrencyQuoteApiClientMock = new Mock<ICryptocurrencyQuoteApiClient>();

        cryptocurrencyQuoteApiClientMock
            .Setup(api => api.GetCryptocurrencyQuote(It.IsAny<CryptocurrencyQuoteSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CryptocurrencyQuote, Error>(error));

        return cryptocurrencyQuoteApiClientMock;
    }
}