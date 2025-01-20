namespace Coinpedia.Core.Errors;

public record Error
{
    public required string Message { get; init; }
    public required object Context { get; init; }
    public Error? InnerError { get; init; }
    public Exception? Exception { get; init; }
}

public record InvalidInput : Error;

public record InternalError : Error;

public record None : Error;

public record NotFound : Error;

public record Conflict : Error;

public record Forbidden : Error;

public record FailedDependency : Error;

public record RequestCancelled : Error;

public record TooManyRequests : Error;