using Coinpedia.Infrastructure.ApiClients;
using Coinpedia.WebApi.Config;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.NullObjects;

namespace Coinpedia.FunctionalTests.Common;

public static class TestExtensions
{
    public static IServiceCollection TurnOffFusionCache(this IServiceCollection services)
    {
        services.RemoveAll<IFusionCache>();
        services.AddSingleton<IFusionCache, NullFusionCache>();
        return services;
    }

    public static IServiceCollection MockExchangeRatesApiClient(this IServiceCollection services, Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
    {
        services.RemoveOptions<ExchangeRatesSettings>();
        services.AddSingleton(_ => Options.Create(new ExchangeRatesSettings
        {
            BaseUrl = "https://mock-api__exchange-rates__/",
            ApiKey = "ApiKey",
            CacheDuration = TimeSpan.Zero.ToString(),
        }));

        services.AddHttpClient<ExchangeRatesApiClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler(responseFunc));

        return services;
    }

    public static IServiceCollection MockCryptocurrencyQuoteApiClient(this IServiceCollection services, Func<HttpRequestMessage, HttpResponseMessage> responseFunc)
    {
        services.RemoveOptions<CoinMarketCapSettings>();
        services.AddSingleton(_ => Options.Create(new CoinMarketCapSettings
        {
            BaseUrl = "https://mock-api__coin-market-cap__/",
            ApiKey = "ApiKey",
            CacheDuration = TimeSpan.Zero.ToString(),
        }));

        services.AddHttpClient<CoinMarketCapApiClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new MockHttpMessageHandler(responseFunc));

        return services;
    }

    public static IServiceCollection RemoveOptions<T>(this IServiceCollection services)
        where T : class
    {
        services.RemoveAll<IOptions<T>>();
        services.RemoveAll<IOptionsSnapshot<T>>();
        services.RemoveAll<IOptionsMonitor<T>>();
        services.RemoveAll<IValidateOptions<T>>();
        return services;
    }
}