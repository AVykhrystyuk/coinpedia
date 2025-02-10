using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Infrastructure.ApiClients;
using Coinpedia.Infrastructure.ApiClients.Decorators;
using Coinpedia.WebApi.Config;

using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using ZiggyCreatures.Caching.Fusion;

namespace Coinpedia.WebApi;

public static class CoreInfrastructureExtensions
{
    public static IServiceCollection AddCryptocurrencyQuoteApiClient(this IServiceCollection services)
    {
        services.AddHttpClient<CoinMarketCapApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<CoinMarketCapSettings>>().Value;
            client.DefaultRequestHeaders.Add("X-Coinpedia-Correlation-ID", CorrelationId.Value);
            client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.ApiKey);
            client.BaseAddress = new Uri(settings.BaseUrl);
        }).AddPolicyHandler(HttpRetryPolicies.Default());

        services.AddTransient<IOptions<ICryptocurrencyQuoteApiClientCacheSettings>>(sp => sp.GetRequiredService<IOptions<CoinMarketCapSettings>>());

        services.AddScoped<ICryptocurrencyQuoteApiClient, CryptocurrencyQuoteApiClientCacheDecorator>(sp => new CryptocurrencyQuoteApiClientCacheDecorator(
            apiClient: sp.GetRequiredService<CoinMarketCapApiClient>(),
            cache: sp.GetRequiredService<IFusionCache>(),
            settings: sp.GetRequiredService<IOptions<ICryptocurrencyQuoteApiClientCacheSettings>>(),
            logger: sp.GetRequiredService<ILogger<CryptocurrencyQuoteApiClientCacheDecorator>>()
        ));

        return services;
    }

    public static IServiceCollection AddCurrencyRatesApiClient(this IServiceCollection services)
    {
        // ExchangeRatesApiClient requires something from settings, but settings currently live at the highest level (WebApi)
        services.AddTransient<IOptions<IExchangeRatesSettings>>(sp => sp.GetRequiredService<IOptions<ExchangeRatesSettings>>());

        services.AddHttpClient<ExchangeRatesApiClient>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<ExchangeRatesSettings>>().Value;
            client.DefaultRequestHeaders.Add("X-Coinpedia-Correlation-ID", CorrelationId.Value);
            // NOTE: ApiKey is passed through QueryString
            client.BaseAddress = new Uri(settings.BaseUrl);
        })
        .AddPolicyHandler(HttpRetryPolicies.Default());

        // CurrencyRatesApiClientCacheDecorator requires something from settings, but settings currently live at the highest level (WebApi)
        services.AddTransient<IOptions<ICurrencyRatesApiClientCacheSettings>>(sp => sp.GetRequiredService<IOptions<ExchangeRatesSettings>>());

        services.AddScoped<ICurrencyRatesApiClient, CurrencyRatesApiClientCacheDecorator>(sp => new CurrencyRatesApiClientCacheDecorator(
            apiClient: sp.GetRequiredService<ExchangeRatesApiClient>(),
            cache: sp.GetRequiredService<IFusionCache>(),
            settings: sp.GetRequiredService<IOptions<ICurrencyRatesApiClientCacheSettings>>(),
            logger: sp.GetRequiredService<ILogger<CurrencyRatesApiClientCacheDecorator>>()
        ));

        return services;
    }

    public static IServiceCollection AddCryptocurrencyQuoteFetcher(this IServiceCollection services)
    {
        // CryptocurrencyQuoteFetcher requires something from settings, but settings currently live at the highest level (WebApi)
        services.AddTransient<IOptions<ICryptocurrencyQuoteFetcherSettings>>(sp => sp.GetRequiredService<IOptions<Settings>>());
        services.AddScoped<ICryptocurrencyQuoteFetcher, CryptocurrencyQuoteFetcher>();

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisSettings = configuration.GetSection(RedisSettings.SectionKey).Get<RedisSettings>()
            ?? throw new Exception($"{RedisSettings.SectionKey} configuration section is missing");

        services.AddMemoryCache();
        services.AddFusionCache()
            .WithOptions(options =>
            {
                options.FactorySyntheticTimeoutsLogLevel = LogLevel.Debug; // most likely will be ignored
                options.FactoryErrorsLogLevel = LogLevel.Error;
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(1),
                JitterMaxDuration = TimeSpan.FromSeconds(0.5)
            })
            .WithSystemTextJsonSerializer()
            .WithDistributedCache(
                new RedisCache(new RedisCacheOptions
                {
                    ConfigurationOptions = new ConfigurationOptions
                    {
                        EndPoints = { redisSettings.ConnectionString }
                    }
                })
            );

        return services;
    }
}
