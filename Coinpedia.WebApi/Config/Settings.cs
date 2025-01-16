using System.ComponentModel.DataAnnotations;

namespace Coinpedia.WebApi.Config;

public class Settings
{
    [Required]
    public required string Secret { get; init; }
};

public static class SettingsExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptionsWithValidateOnStart<Settings>()
            .Bind(configuration.GetSection("Settings"))
            .ValidateDataAnnotations();

        return services;
    }
}
