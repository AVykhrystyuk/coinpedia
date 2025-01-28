using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
using Coinpedia.Infrastructure.Cache;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ZiggyCreatures.Caching.Fusion;

namespace Coinpedia.Infrastructure.ApiClients.Decorators;

public class CryptocurrencyQuoteApiClientCacheDecorator(
    ICryptocurrencyQuoteApiClient apiClient,
    IFusionCache cache,
    IOptions<ICryptocurrencyQuoteApiClientCacheSettings> settings,
    ILogger<CryptocurrencyQuoteApiClientCacheDecorator> logger
) : ICryptocurrencyQuoteApiClient
{
    public async Task<Result<CryptocurrencyQuote, Error>> GetCryptocurrencyQuote(CryptocurrencyQuoteSearchQuery searchQuery, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"cryptocurrency-quote:{searchQuery.BaseCurrency}:{searchQuery.Cryptocurrency}";
        var tags = new[] { searchQuery.BaseCurrency.Value, searchQuery.Cryptocurrency.Value };
        var cacheDuration = settings.Value.CacheDuration;

        var factoryGotCalled = false;

        var cryptocurrencyQuote = await cache.GetResultOrSetValueAsync<CryptocurrencyQuote, Error>(
            key: cacheKey,
            factory: Factory,
            setupAction: options => options.SetDuration(cacheDuration),
            tags: tags,
            token: cancellationToken
        );

        if (!factoryGotCalled)
        {
            logger.LogInformation("[Cache]: Cryptocurrency quote is found in (and returned from) the cache, {cacheKey}", cacheKey);
        }

        return cryptocurrencyQuote;

        async Task<Result<CryptocurrencyQuote, Error>> Factory(FusionCacheFactoryExecutionContext<CryptocurrencyQuote> context, CancellationToken token)
        {
            factoryGotCalled = true;

            return await apiClient.GetCryptocurrencyQuote(searchQuery, token);
        }
    }
}

public interface ICryptocurrencyQuoteApiClientCacheSettings
{
    TimeSpan CacheDuration { get; }
}
