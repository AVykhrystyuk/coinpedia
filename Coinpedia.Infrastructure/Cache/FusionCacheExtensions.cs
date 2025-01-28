using ZiggyCreatures.Caching.Fusion;

namespace Coinpedia.Infrastructure.Cache;

public static class FusionCacheExtensions
{
    // helps to convert standart IFusionCache.GetOrSetAsync(...) from exception-based flow to Result-based flow
    public static async Task<Result<TValue, TError>> GetResultOrSetValueAsync<TValue, TError>(
        this IFusionCache cache,
        string key,
        Func<FusionCacheFactoryExecutionContext<TValue>, CancellationToken, Task<Result<TValue, TError>>> factory,
        Action<FusionCacheEntryOptions> setupAction,
        IEnumerable<string>? tags = null,
        CancellationToken token = default
    ) where TValue : notnull
    {
        try
        {
            // https://github.com/ZiggyCreatures/FusionCache/blob/main/src/ZiggyCreatures.FusionCache/FusionCache_Async.cs#L89

            var value = await cache.GetOrSetAsync<TValue>(
                key: key, 
                factory: FactoryWrapper, 
                setupAction: setupAction,
                tags: tags,
                token: token);

            return Result.Of<TValue, TError>(value);
        }
        catch (FusionCacheFactoryInterruptedException<TValue, TError> ex)
        {
            return ex.FailedResult;
        }

        async Task<TValue> FactoryWrapper(FusionCacheFactoryExecutionContext<TValue> context, CancellationToken token)
        {
            var result = await factory(context, token);

            if (result.IsSuccess)
            {
                return result.Value;
            }
            else // Failure
            {
                throw new FusionCacheFactoryInterruptedException<TValue, TError> { FailedResult = result };
            }
        }
    }

    // HACK: inheriting from SyntheticTimeoutException helps to skip logging of our custom exception
    // for more details find "ProcessFactoryError" method in the source code: https://github.com/ZiggyCreatures/FusionCache/blob/707b594ab4ea3e44e4ee93ee545c8cf21bcc070c/src/ZiggyCreatures.FusionCache/FusionCache.cs#L583
    private class FusionCacheFactoryInterruptedException<TValue, TError>() : SyntheticTimeoutException
        where TValue : notnull
    {
        public required Result<TValue, TError> FailedResult { get; init; }
    }
}
