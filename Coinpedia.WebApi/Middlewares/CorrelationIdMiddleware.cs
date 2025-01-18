using Coinpedia.Core;

namespace Coinpedia.WebApi.Middlewares;

public class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        CorrelationId.Value = context.Request.Headers[WellKnownHeaders.CorrelationId].FirstOrDefault()!;

        var correlationId = CorrelationId.Value;

        context.Items[WellKnownHeaders.CorrelationId] = correlationId;

        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
        });

        context.Response.Headers[WellKnownHeaders.CorrelationId] = correlationId;

        await next(context);
    }
}
