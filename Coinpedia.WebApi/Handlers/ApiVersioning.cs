using Asp.Versioning;

namespace Coinpedia.WebApi.Handlers;

public static class ApiVersioning
{
    public static IEndpointRouteBuilder NewVersionedRouteBuilder(this IEndpointRouteBuilder builder)
    {
        var apiVersionSet = builder.NewApiVersionSet()
            // .HasApiVersion(new ApiVersion(1))
            // .HasApiVersion(new ApiVersion(2))
            .Build();

        return builder
            .MapGroup("/v{apiVersion:apiVersion}")
            .WithApiVersionSet(apiVersionSet);
    }

    public static void AddApiVersioningAndExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}
