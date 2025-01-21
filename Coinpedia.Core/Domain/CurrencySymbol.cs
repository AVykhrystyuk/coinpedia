using System.Text.Json;
using System.Text.Json.Serialization;

using Coinpedia.Core.Errors;

namespace Coinpedia.Core.Domain;

[JsonConverter(typeof(JsonConverter))]
public record CurrencySymbol(string Value)
{
    public static Result<CurrencySymbol, Error> TryCreate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new InvalidInput { Message = "Symbol cannot be null or white space", Context = value };
        }

        return new CurrencySymbol(value.ToUpperInvariant());
    }

    public override string ToString() => Value;

    public class JsonConverter : JsonConverter<CurrencySymbol>
    {
        public override CurrencySymbol Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new CurrencySymbol(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, CurrencySymbol symbol, JsonSerializerOptions options) =>
            writer.WriteStringValue(symbol.Value);

        public override CurrencySymbol ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new CurrencySymbol(reader.GetString()!);

        public override void WriteAsPropertyName(Utf8JsonWriter writer, CurrencySymbol value, JsonSerializerOptions options)
            => writer.WritePropertyName(value.ToString());
    }
}