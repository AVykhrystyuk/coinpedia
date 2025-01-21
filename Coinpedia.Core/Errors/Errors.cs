using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coinpedia.Core.Errors;

public record Error
{
    public required string Message { get; init; }
    public required object Context { get; init; }
    public Error? InnerError { get; init; }

    [JsonConverter(typeof(ExceptionJsonConverter))]
    public Exception? Exception { get; init; }

    /// <summary>
    /// Write-only JsonConverter that helps to serialize sometimes not serializable exceptions
    /// https://github.com/dotnet/runtime/issues/43026
    /// </summary>
    public class ExceptionJsonConverter : JsonConverter<Exception>
    {
        public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, Exception ex, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Message", ex.Message);
            // writer.WriteString("StackTrace", ex.StackTrace);
            writer.WriteEndObject();
        }
    }
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