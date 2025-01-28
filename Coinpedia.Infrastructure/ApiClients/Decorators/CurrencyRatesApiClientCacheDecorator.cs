using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
using Coinpedia.Infrastructure.Cache;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ZiggyCreatures.Caching.Fusion;

namespace Coinpedia.Infrastructure.ApiClients.Decorators;

public class CurrencyRatesApiClientCacheDecorator(
    ICurrencyRatesApiClient apiClient,
    IFusionCache cache,
    IOptions<ICurrencyRatesApiClientCacheSettings> settings,
    ILogger<CurrencyRatesApiClientCacheDecorator> logger
) : ICurrencyRatesApiClient
{
    public async Task<Result<CurrencyRates, Error>> GetCurrencyRates(GetCurrencyRatesQuery ratesQuery, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"currency-rates:{ratesQuery.BaseCurrency}";
        var tags = new[] { ratesQuery.BaseCurrency.Value };
        var cacheDuration = settings.Value.CacheDuration;

        var factoryGotCalled = false;

        var currencyRates = await cache.GetResultOrSetValueAsync<CurrencyRates, Error>(
            key: cacheKey,
            factory: Factory,
            setupAction: options => options.SetDuration(cacheDuration),
            tags: tags,
            token: cancellationToken
        );

        if (!factoryGotCalled)
        {
            logger.LogInformation("[Cache]: Currency rates are found in (and returned from) the cache, {cacheKey}", cacheKey);
        }

        return currencyRates;

        async Task<Result<CurrencyRates, Error>> Factory(FusionCacheFactoryExecutionContext<CurrencyRates> context, CancellationToken token)
        {
            factoryGotCalled = true;

            return await apiClient.GetCurrencyRates(ratesQuery, token);
        }
    }
}

public interface ICurrencyRatesApiClientCacheSettings
{
    TimeSpan CacheDuration { get; }
}
