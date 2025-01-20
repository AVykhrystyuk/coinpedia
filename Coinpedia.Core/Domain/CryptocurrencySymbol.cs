using System.Text.Json;
using System.Text.Json.Serialization;

using Coinpedia.Core.Errors;

using CSharpFunctionalExtensions;

namespace Coinpedia.Core.Domain;

[JsonConverter(typeof(JsonConverter))]
public record CryptocurrencySymbol(string Value)
{
    public static Result<CryptocurrencySymbol, Error> TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new InvalidInput { Message = "Symbol cannot be null or white space", Context = value };
        }

        return new CryptocurrencySymbol(value.ToUpperInvariant());
    }

    public override string ToString() => Value;

    public class JsonConverter : JsonConverter<CryptocurrencySymbol>
    {
        public override CryptocurrencySymbol Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new CryptocurrencySymbol(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, CryptocurrencySymbol symbol, JsonSerializerOptions options) =>
            writer.WriteStringValue(symbol.Value);

        public override CryptocurrencySymbol ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new CryptocurrencySymbol(reader.GetString()!);

        public override void WriteAsPropertyName(Utf8JsonWriter writer, CryptocurrencySymbol value, JsonSerializerOptions options)
            => writer.WritePropertyName(value.ToString());
    }
}