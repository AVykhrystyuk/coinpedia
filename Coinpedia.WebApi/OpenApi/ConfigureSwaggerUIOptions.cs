using Swashbuckle.AspNetCore.SwaggerUI;

namespace Coinpedia.WebApi.OpenApi;

public static class ConfigureSwaggerUIOptions
{
    public static void Configure(SwaggerUIOptions options, WebApplication provider)
    {
        foreach (var description in provider.DescribeApiVersions())
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }

        options.EnableTryItOutByDefault();
    }
}