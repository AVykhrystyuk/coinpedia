using System.ComponentModel.DataAnnotations;

using Coinpedia.Core.Domain;
using Coinpedia.Infrastructure.ApiClients;
using Coinpedia.Infrastructure.ApiClients.Decorators;

namespace Coinpedia.WebApi.Config;

public class Settings : ICryptocurrencyQuoteFetcherSettings
{
    public const string SectionKey = "Settings";

    [Required]
    public required string BaseCurrency { get; init; }

    /// <summary>
    /// USD,EUR,GBP,...
    /// </summary>
    [Required]
    public required string RequiredCurrencies { get; init; }
}

public class SeqSettings
{
    public const string SectionKey = "Seq";

    [Required]
    public required string ApiKey { get; init; }

    [Required]
    public required string IngestionUrl { get; init; }
}

public class CoinMarketCapSettings
{
    public const string SectionKey = "CoinMarketCap";

    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ApiKey { get; init; }
}

public class ExchangeRatesSettings : IExchangeRatesSettings, ICurrencyRatesApiClientCacheSettings
{
    public const string SectionKey = "ExchangeRates";

    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ApiKey { get; init; }

    [Required]
    [RegularExpression("(?<days>\\d+):(?<hours>[0-1]?\\d|2[0-3]):(?<minutes>[0-5]?\\d):(?<seconds>[0-5]?\\d)")]
    public required string CacheDuration { get; init; }

    TimeSpan ICurrencyRatesApiClientCacheSettings.CacheDuration => TimeSpan.Parse(CacheDuration);
}

public class RedisSettings
{
    public const string SectionKey = "Redis";

    [Required]
    public required string ConnectionString { get; init; }
}

public static class SettingsExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Add<Settings>(Settings.SectionKey, configuration);
        services.Add<SeqSettings>(SeqSettings.SectionKey, configuration);
        services.Add<CoinMarketCapSettings>(CoinMarketCapSettings.SectionKey, configuration);
        services.Add<ExchangeRatesSettings>(ExchangeRatesSettings.SectionKey, configuration);
        services.Add<RedisSettings>(RedisSettings.SectionKey, configuration);

        return services;
    }

    private static IServiceCollection Add<T>(this IServiceCollection services, string sectionKey, IConfiguration configuration)
        where T : class
    {
        services
            .AddOptionsWithValidateOnStart<T>()
            .Bind(configuration.GetSection(sectionKey))
            .ValidateDataAnnotations();

        return services;
    }
}
