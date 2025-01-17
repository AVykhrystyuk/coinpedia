using System.ComponentModel.DataAnnotations;

namespace Coinpedia.WebApi.Config;

public class Settings
{
    public const string SectionKey = "Settings";

    [Required]
    public required string SeqApiKey { get; init; }

    [Required]
    public required string SeqIngestionUrl { get; init; }
};

public static class SettingsExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<Settings>()
            .Bind(configuration.GetSection(Settings.SectionKey))
            .ValidateDataAnnotations();

        return services;
    }
}
