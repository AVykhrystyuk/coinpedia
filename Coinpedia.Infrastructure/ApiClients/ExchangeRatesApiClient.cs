﻿using System.Net;
using System.Text.Json.Serialization;

using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Common;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;
using Coinpedia.Infrastructure.ApiClients.JsonConverters;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Coinpedia.Infrastructure.ApiClients;

public class ExchangeRatesApiClient(
    HttpClient httpClient,
    ILogger<ExchangeRatesApiClient> logger,
    IOptions<IExchangeRatesSettings> settings
) : ICurrencyRatesApiClient
{
    /// <returns>
    /// CurrencyRates | RequestCancelled | None | TooManyRequests | InternalError
    /// </returns>
    public async Task<Result<CurrencyRates, Error>> GetCurrencyRates(
        GetCurrencyRatesQuery ratesQuery,
        CancellationToken cancellationToken = default
    )
    {
        var apiKey = settings.Value.ApiKey;

        var uniqueCurrencies = ratesQuery.ForCurrencies
            .DistinctBy(c => c.Value)
            .ToList();

        if (uniqueCurrencies.Count != ratesQuery.ForCurrencies.Count)
        {
            ratesQuery = ratesQuery with { ForCurrencies = uniqueCurrencies };
        }

        var url = $"/v1/latest" +
            $"?base={ratesQuery.BaseCurrency}" +
            $"&symbols={string.Join(",", ratesQuery.ForCurrencies)}";

        using var _ = logger.BeginAttributedScope(url, ratesQuery);

        url += $"&access_key={apiKey}"; // to avoid logging access_key

        HttpResponseMessage response;
        string responseContentAsText;
        try
        {
            response = await httpClient.GetAsync(url, cancellationToken);
            responseContentAsText = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            return new RequestCancelled { Message = "[ExchangeRatesApi]: Request cancelled", Context = Context(), Exception = ex };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ER]: Unexpected exception while getting latest exchange rates");

            return new InternalError
            {
                Message = $"[ExchangeRatesApi]: Unexpected exception: {ex.Message}",
                Context = Context(),
                Exception = ex
            };
        }

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(responseContentAsText))
            {
                logger.LogError("[ER]: Unexpected response - empty.");

                return new None { Message = $"[ExchangeRatesApi]: Unexpected response: empty", Context = Context() };
            }

            return DeserializeResponseContent(responseContentAsText, logger)
                .Bind(responseContent => responseContent.ToCurrencyRates(ratesQuery, logger));
        }
        else // StatusCode is not successful
        {
            var (_, _, responseContent, error) = DeserializeErrorResponseContent(responseContentAsText, logger);

            logger.LogError("[ER]: non-successful response for latest cryptocurrency quotes, {@ResponseContent}", responseContent);

            var errorFromContent = responseContent?.Error;
            if (errorFromContent is not null)
            {
                switch (errorFromContent.Code)
                {
                    case ApiErrorCodes.MaximumMonthlyRequestsReached:
                        logger.Log(LogLevel.Critical, "[ER]: The maximum allowed API amount of monthly API requests has been reached.");
                        return new InternalError { Message = "[ExchangeRatesApi]: The maximum allowed API amount of monthly API requests has been reached.", Context = Context() };
                }
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.TooManyRequests:
                    return new TooManyRequests { Message = "[ExchangeRatesApi]: The API rate limit was exceeded; consider slowing down your API Request frequency", Context = Context() };
                case var c:
                    return new FailedDependency { Message = "[ExchangeRatesApi]: Unexpected response. For more info contact the support", Context = NonSuccessfulContext(response.StatusCode) };
            }
        }

        object Context() => new { ratesQuery };
        object NonSuccessfulContext(HttpStatusCode statusCode) => new { statusCode, ratesQuery };

        static Result<ResponseContent, Error> DeserializeResponseContent(string json, ILogger logger) =>
            JsonSerializerEx.Deserialize<ResponseContent>(json)
                .TapError(err => logger.Log(
                    LogLevel.Critical,
                    err.Exception,
                    "[ER]: response deserialization failed. {errorMessage}, errorContext: {@context}, {json}", err.Message, err.Context, json)
                );

        static Result<ErrorResponseContent, Error> DeserializeErrorResponseContent(string json, ILogger logger) =>
            JsonSerializerEx.Deserialize<ErrorResponseContent>(json)
                .TapError(err => logger.Log(
                    LogLevel.Critical,
                    err.Exception,
                    "[ER]: error-response deserialization failed. {errorMessage}, errorContext: {@context}, {json}", err.Message, err.Context, json)
                );

    }

    private class ApiErrorCodes
    {
        /// <summary>
        /// The maximum allowed API amount of monthly API requests has been reached
        /// </summary>
        public const int MaximumMonthlyRequestsReached = 104;
    }

    public class ResponseContent
    {
        [JsonPropertyName("success")]
        public required bool Success { get; init; }

        [JsonPropertyName("timestamp")]
        public required long UnixTimestamp { get; init; }

        [JsonPropertyName("base")]
        public required string Base { get; init; }

        [JsonPropertyName("rates")]
        public required IReadOnlyDictionary<string, decimal> Rates { get; init; }
    }

    public class ErrorResponseContent
    {
        [JsonPropertyName("success")]
        public bool? Success { get; init; }

        [JsonPropertyName("error")]
        public ErrorDetails? Error { get; init; }

        public class ErrorDetails
        {
            // it looks like the type could be `int` or `string`
            /*
                "error": {
                    "code": "invalid_base_currency",
                    "message": "An unexpected error ocurred. [Technical Support: support@apilayer.com]"
                }
                or
                {
                  "success": false,
                  "error": {
                    "code": 104,
                    "info": "Your monthly API request volume has been reached. Please upgrade your plan."
                  }
                }
             */
            [JsonConverter(typeof(ObjectStringOrNumberConverter))]
            [JsonPropertyName("code")]
            public required object Code { get; init; }

            [JsonPropertyName("message")]
            public string? Message { get; init; }

            [JsonPropertyName("info")]
            public string? Info { get; init; }
        }
    }
}

public interface IExchangeRatesSettings
{
    string ApiKey { get; }
}
