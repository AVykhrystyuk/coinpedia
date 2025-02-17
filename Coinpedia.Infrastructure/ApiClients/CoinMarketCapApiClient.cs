using System.Net;
using System.Text.Json.Serialization;

using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Common;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;

namespace Coinpedia.Infrastructure.ApiClients;

public class CoinMarketCapApiClient(
    HttpClient httpClient,
    ILogger<CoinMarketCapApiClient> logger
) : ICryptocurrencyQuoteApiClient
{
    /// <returns>
    /// CryptocurrencyQuote | RequestCancelled | None | NotFound | TooManyRequests | InternalError
    /// </returns>
    public async Task<Result<CryptocurrencyQuote, Error>> GetCryptocurrencyQuote(
        CryptocurrencyQuoteSearchQuery searchQuery,
        CancellationToken cancellationToken = default)
    {
        var url = $"/v2/cryptocurrency/quotes/latest" +
            $"?convert={searchQuery.BaseCurrency}" +
            $"&symbol={searchQuery.Cryptocurrency}" +
            $"&aux=date_added,platform,is_active,is_fiat";

        using var _ = logger.BeginAttributedScope(url, searchQuery);

        HttpResponseMessage response;
        string responseContentAsText;
        try
        {
            response = await httpClient.GetAsync(url, cancellationToken);
            responseContentAsText = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested)
        {
            return new RequestCancelled { Message = "[CryptocurrencyApi]: Request cancelled", Context = Context(), Exception = ex };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[CMC]: Unexpected exception while getting latest cryptocurrency quotes");

            return new InternalError
            {
                Message = $"[CryptocurrencyApi]: Unexpected exception: {ex.Message}",
                Context = Context(),
                Exception = ex
            };
        }

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrWhiteSpace(responseContentAsText))
            {
                logger.LogError("[CMC]: Unexpected response - empty.");

                return new None { Message = $"[CryptocurrencyApi]: Unexpected response: empty", Context = Context() };
            }

            return DeserializeResponseContent(responseContentAsText, logger)
                .Bind(responseContent => responseContent.ToCryptocurrencyQuote(searchQuery, logger));
        }
        else // StatusCode is not successful
        {
            var (_, _, responseContent, error) = DeserializeResponseContent(responseContentAsText, logger);

            logger.LogError("[CMC]: non-successful response for latest cryptocurrency quotese, {@ResponseContentStatus}", responseContent?.Status);

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    // NOTE: never saw NotFound being returned, CoinMarketCap tends to return and empty object instead
                    return new NotFound { Message = "[CryptocurrencyApi]: Cryptocurrency is not found", Context = Context() };
                case HttpStatusCode.TooManyRequests:
                    return new TooManyRequests { Message = "[CryptocurrencyApi]: The API rate limit was exceeded; consider slowing down your API Request frequency", Context = Context() };
                case HttpStatusCode.PaymentRequired:
                    logger.Log(LogLevel.Critical, "[CMC]: API request was rejected due to it being a paid subscription plan with an overdue balance");
                    return new InternalError { Message = "[CryptocurrencyApi]: Unexpected response. For more info contact the support", Context = Context() };
                case var c:
                    return new FailedDependency { Message = "[CryptocurrencyApi]: Unexpected response. For more info contact the support", Context = NonSuccessfulContext(response.StatusCode) };
            }
        }

        object Context() => new { searchQuery };
        object NonSuccessfulContext(HttpStatusCode statusCode) => new { statusCode, searchQuery };

        static Result<ResponseContent, Error> DeserializeResponseContent(string json, ILogger logger) =>
            JsonSerializerEx.Deserialize<ResponseContent>(json)
                .TapError(err => logger.Log(
                    LogLevel.Critical,
                    err.Exception,
                    "[CMC]: response deserialization failed. {errorMessage}, errorContext: {@context}", err.Message, err.Context)
                );
    }

    public class ResponseContent
    {
        [JsonPropertyName("status")]
        public required Status Status { get; init; }

        [JsonPropertyName("data")]
        public IReadOnlyDictionary<string, IReadOnlyList<CryptocurrencyData>>? CryptocurrencyDataPerSymbol { get; init; }
    }

    public class Status
    {
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; init; }

        [JsonPropertyName("error_code")]
        public int ErrorCode { get; init; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; init; }

        [JsonPropertyName("elapsed")]
        public long Elapsed { get; init; }

        [JsonPropertyName("credit_count")]
        public long CreditCount { get; init; }
    }

    public class CryptocurrencyData
    {
        [JsonPropertyName("id")]
        public required long Id { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("symbol")]
        public required string Symbol { get; init; }

        [JsonPropertyName("date_added")]
        public DateTime? DateAdded { get; init; }

        [JsonPropertyName("is_active")]
        public int IsActive { get; init; }

        [JsonPropertyName("infinite_supply")]
        public bool InfiniteSupply { get; init; }

        [JsonPropertyName("platform")]
        public Platform? Platform { get; init; }

        [JsonPropertyName("is_fiat")]
        public int? IsFiat { get; init; }

        [JsonPropertyName("self_reported_circulating_supply")]
        public decimal? SelfReportedCirculatingSupply { get; init; }

        [JsonPropertyName("self_reported_market_cap")]
        public decimal? SelfReportedMarketCap { get; init; }

        [JsonPropertyName("last_updated")]
        public required DateTimeOffset LastUpdated { get; init; }

        [JsonPropertyName("quote")]
        public required IReadOnlyDictionary<string, Quote> QuotePerCurrency { get; init; }
    }

    public class Platform
    {
        [JsonPropertyName("id")]
        public required long Id { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("symbol")]
        public required string Symbol { get; init; }
    }

    public class Quote
    {
        [JsonPropertyName("price")]
        public decimal? Price { get; init; }

        [JsonPropertyName("volume_24h")]
        public decimal? Volume24H { get; init; }

        [JsonPropertyName("market_cap")]
        public decimal? MarketCap { get; init; }

        [JsonPropertyName("market_cap_dominance")]
        public decimal? MarketCapDominance { get; init; }

        [JsonPropertyName("fully_diluted_market_cap")]
        public decimal? FullyDilutedMarketCap { get; init; }

        [JsonPropertyName("last_updated")]
        public required DateTimeOffset LastUpdated { get; init; }
    }
}
