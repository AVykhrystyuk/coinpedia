using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

using Coinpedia.Core.Errors;

namespace Coinpedia.Core.Common;

public static class JsonSerializerEx
{
    public static Result<TValue, Error> Deserialize<TValue>(
        [StringSyntax(StringSyntaxAttribute.Json)] string json, 
        JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new InternalError {
                Message = "Json cannot be null or white space", 
                Context = new { json, Type = typeof(TValue).FullName }
            };
        }

        try
        {
            var value = JsonSerializer.Deserialize<TValue>(json, options);
            if (value is null)
            {
                return new InternalError
                {
                    Message = "Json is deserialized to null",
                    Context = new { json, Type = typeof(TValue).FullName }
                };
            }

            return value;
        }
        catch (JsonException ex)
        {
            return new InternalError
            {
                Message = $"Failed to deserialize json: {ex.Message}",
                Context = new { json, Type = typeof(TValue).FullName, ex.Path, ex.Message, ex.LineNumber },
                Exception = ex,
            };
        }
        catch (Exception ex)
        {
            return new InternalError
            {
                Message = $"Failed to deserialize json: {ex.Message}",
                Context = new { json, Type = typeof(TValue).FullName },
                Exception = ex,
            };
        }
    }
}
