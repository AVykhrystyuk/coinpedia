using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

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

        var cachedCurrencyRates = cache.GetOrDefault<CurrencyRates>(cacheKey, token: cancellationToken);
        if (cachedCurrencyRates is not null)
        {
            logger.LogInformation("[Cache]: Currency rates are found in and returned from the cache, {cacheKey}", cacheKey);

            return cachedCurrencyRates;
        }

        var (_, _, currencyRates, error) = await apiClient.GetCurrencyRates(ratesQuery, cancellationToken);
        if (error is not null)
        {
            return error;
        }

        await cache.SetAsync(
            cacheKey,
            currencyRates,
            options => options.SetDuration(settings.Value.CacheDuration),
            token: cancellationToken
        );

        return currencyRates;
    }
}

public interface ICurrencyRatesApiClientCacheSettings
{
    TimeSpan CacheDuration { get; }
}

