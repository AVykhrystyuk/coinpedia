using Coinpedia.Core.ApiClients;
using Coinpedia.Core;
using Coinpedia.WebApi.Config;
using Microsoft.Extensions.Options;
using Coinpedia.Core.Domain;
using Coinpedia.Infrastructure.ApiClients;
using Coinpedia.Infrastructure.ApiClients.Decorators;
using ZiggyCreatures.Caching.Fusion;

namespace Coinpedia.WebApi;

public static class CoreInfrastructureExtensions
{
    public static IServiceCollection AddCryptocurrencyQuoteApiClient(this IServiceCollection services)
    {
        services.AddScoped<ICryptocurrencyQuoteApiClient, CoinMarketCapCryptocurrencyQuoteApiClient>();

        services.AddHttpClient<ICryptocurrencyQuoteApiClient, CoinMarketCapCryptocurrencyQuoteApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CoinMarketCapSettings>>().Value;
            client.DefaultRequestHeaders.Add("X-Coinpedia-Correlation-ID", CorrelationId.Value);
            client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.ApiKey);
            client.BaseAddress = new Uri(settings.BaseUrl);
        }).AddPolicyHandler(HttpRetryPolicies.Default());

        return services;
    }

    public static IServiceCollection AddCurrencyRatesApiClient(this IServiceCollection services)
    {
        services.AddTransient<IOptions<IExchangeRatesSettings>>(sp => sp.GetRequiredService<IOptions<ExchangeRatesSettings>>());

        services.AddScoped<ExchangeRatesApiClient>();

        services.AddHttpClient<ExchangeRatesApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExchangeRatesSettings>>().Value;
            client.DefaultRequestHeaders.Add("X-Coinpedia-Correlation-ID", CorrelationId.Value);
            // ApiKey is passed through queryString
            client.BaseAddress = new Uri(settings.BaseUrl);
        }).AddPolicyHandler(HttpRetryPolicies.Default());

        services.AddTransient<IOptions<ICurrencyRatesApiClientCacheSettings>>(sp => sp.GetRequiredService<IOptions<ExchangeRatesSettings>>());

        services.AddScoped<ICurrencyRatesApiClient, CurrencyRatesApiClientCacheDecorator>(sp => new CurrencyRatesApiClientCacheDecorator(
            apiClient: sp.GetRequiredService<ExchangeRatesApiClient>(),
            cache: sp.GetRequiredService<IFusionCache>(),
            settings: sp.GetRequiredService<IOptions<ICurrencyRatesApiClientCacheSettings>>()
        ));

        return services;
    }

    public static IServiceCollection AddCryptocurrencyQuoteFetcher(this IServiceCollection services)
    {
        services.AddTransient<IOptions<ICryptocurrencyQuoteFetcherSettings>>(sp => sp.GetRequiredService<IOptions<Settings>>());
        services.AddScoped<ICryptocurrencyQuoteFetcher, CryptocurrencyQuoteFetcher>();

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                // CACHE DURATION
                Duration = TimeSpan.FromMinutes(5)
            });

        return services;
    }
}
