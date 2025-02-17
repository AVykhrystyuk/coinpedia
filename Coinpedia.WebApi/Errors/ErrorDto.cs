namespace Coinpedia.WebApi.Errors;

public record ErrorDto(
    DateTime Timestamp,
    int StatusCode,
    string Error,
    string ErrorMessage
);
