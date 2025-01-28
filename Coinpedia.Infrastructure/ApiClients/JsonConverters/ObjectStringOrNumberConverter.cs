using System.Text.Json;
using System.Text.Json.Serialization;

namespace Coinpedia.Infrastructure.ApiClients.JsonConverters;

public class ObjectStringOrNumberConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int i))
            {
                return i;
            }

            if (reader.TryGetInt64(out long l))
            {
                return l;
            }

            //if (reader.TryGetDecimal(out decimal d))
            //{
            //    return d;
            //}

            return reader.GetDouble();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString()!;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unsupported JSON token: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;

            case long l:
                writer.WriteNumberValue(l);
                break;

            case int i:
                writer.WriteNumberValue(i);
                break;

            case double d:
                writer.WriteNumberValue(d);
                break;

            case decimal dec:
                writer.WriteNumberValue(dec);
                break;

            case string s:
                writer.WriteStringValue(s);
                break;

            default:
                throw new JsonException($"Unsupported value type: {value.GetType()}");
        }
    }
}