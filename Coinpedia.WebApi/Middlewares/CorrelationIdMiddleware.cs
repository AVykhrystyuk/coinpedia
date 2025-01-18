using Coinpedia.Core;

namespace Coinpedia.WebApi.Middlewares;

public class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var existingCorrelationId =
            context.Request.Headers[WellKnownHeaders.CorrelationId].FirstOrDefault() ??
            context.Response.Headers[WellKnownHeaders.CorrelationId].FirstOrDefault(); // to keep using and avoid overriding it below

        CorrelationId.Value = existingCorrelationId!;

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
