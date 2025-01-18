using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace Coinpedia.Core;

public static class LoggerExtensions
{
    public static IDisposable? BeginAttributesScope(
        this ILogger logger,
        object? expr1 = null,
        object? expr2 = null,
        object? expr3 = null,
        object? expr4 = null,
        [CallerArgumentExpression(nameof(expr1))] string arg1Name = "",
        [CallerArgumentExpression(nameof(expr2))] string arg2Name = "",
        [CallerArgumentExpression(nameof(expr3))] string arg3Name = "",
        [CallerArgumentExpression(nameof(expr4))] string arg4Name = ""
    )
    {
        var scopeAttributes = new Dictionary<string, object>();

        if (expr1 is not null)
        {
            scopeAttributes.Add("@" + arg1Name, expr1);
        }

        if (expr2 is not null)
        {
            scopeAttributes.Add("@" + arg2Name, expr2);
        }

        if (expr3 is not null)
        {
            scopeAttributes.Add("@" + arg3Name, expr3);
        }

        if (expr4 is not null)
        {
            scopeAttributes.Add("@" + arg4Name, expr4);
        }

        return logger.BeginScope(scopeAttributes);
    }
}
