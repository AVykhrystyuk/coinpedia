using Coinpedia.Core;

using Microsoft.AspNetCore.Http.Features;

using Serilog.Enrichers.Span;

namespace Coinpedia.WebApi.Errors;

public static class ProblemDetailsOptionsExtensions
{
    public static void Configure(this ProblemDetailsOptions options)
    {
        options.CustomizeProblemDetails = context =>
        {
            var request = context.HttpContext.Request;
            var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;

            context.ProblemDetails.Instance = $"[{request.Method}] {request.Path}";

            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Extensions.TryAdd("activityId", activity?.Id);
            context.ProblemDetails.Extensions.TryAdd("traceId", activity?.GetTraceId()); // activity?.TraceId.ToString());
            context.ProblemDetails.Extensions.TryAdd("spanId", activity?.GetSpanId()); // activity?.SpanId.ToString());
            context.ProblemDetails.Extensions.TryAdd(WellKnownHeaders.CorrelationId, CorrelationId.Value);

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