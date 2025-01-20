using System.ComponentModel.DataAnnotations;

using Coinpedia.Core.Domain;
using Coinpedia.Infrastructure.ApiClients;

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

public class ExchangeRatesSettings : IExchangeRatesSettings
{
    public const string SectionKey = "ExchangeRates";

    [Required]
    public required string BaseUrl { get; init; }

    [Required]
    public required string ApiKey { get; init; }
}

public static class SettingsExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<Settings>()
            .Bind(configuration.GetSection(Settings.SectionKey))
            .ValidateDataAnnotations();

        services
             .AddOptionsWithValidateOnStart<SeqSettings>()
             .Bind(configuration.GetSection(SeqSettings.SectionKey))
             .ValidateDataAnnotations();

        services
            .AddOptionsWithValidateOnStart<CoinMarketCapSettings>()
            .Bind(configuration.GetSection(CoinMarketCapSettings.SectionKey))
            .ValidateDataAnnotations();

        services
            .AddOptionsWithValidateOnStart<ExchangeRatesSettings>()
            .Bind(configuration.GetSection(ExchangeRatesSettings.SectionKey))
            .ValidateDataAnnotations();

        return services;
    }
}
