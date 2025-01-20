using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

using Serilog;

namespace Coinpedia.WebApi.Errors;

public class ProblemDetailsExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IDiagnosticContext diagnosticContext
) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        diagnosticContext.SetException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = exception.Message,
            Detail = exception.StackTrace,
            Type = "InternalServerError"
        };

        return problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
            });
    }
}