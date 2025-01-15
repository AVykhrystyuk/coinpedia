using Microsoft.AspNetCore.Http.Features;

namespace Coinpedia.WebApi.Errors;

public static class ConfigureProblemDetails
{
    public static void Configure(ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var request = context.HttpContext.Request;

            context.ProblemDetails.Instance = $"[{request.Method}] {request.Path}";

            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

            var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
            context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);

            if (context.ProblemDetails.Status is { } problemStatus)
            {
                context.HttpContext.Response.StatusCode = problemStatus;
            }
            else if (context.HttpContext.Response is { StatusCode: > 0 } response)
            {
                context.ProblemDetails.Status = response.StatusCode;
            }
        };
    }
}