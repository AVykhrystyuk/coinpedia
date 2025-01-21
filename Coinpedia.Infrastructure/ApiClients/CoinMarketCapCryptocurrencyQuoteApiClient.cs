using System.Net;
using System.Text.Json.Serialization;

using Coinpedia.Core;
using Coinpedia.Core.ApiClients;
using Coinpedia.Core.Common;
using Coinpedia.Core.Domain;
using Coinpedia.Core.Errors;

using Microsoft.Extensions.Logging;

namespace Coinpedia.Infrastructure.ApiClients;

public class CoinMarketCapCryptocurrencyQuoteApiClient(
    HttpClient httpClient,
    ILogger<CoinMarketCapCryptocurrencyQuoteApiClient> logger
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

        using var _ = logger.BeginAttributesScope(url, searchQuery);

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
                .Bind(ToCryptocurrencyQuote);

            Result<CryptocurrencyQuote, Error> ToCryptocurrencyQuote(ResponseContent responseContent)
            {
                var cryptocurrenciesPerSymbol = responseContent.CryptocurrencyDataPerSymbol!;

                using var _1 = logger.BeginAttributesScope(cryptocurrenciesPerSymbol.Count);

                if (cryptocurrenciesPerSymbol.Count < 1)
                {
                    logger.LogError("[CMC]: Unexpected response - 'data' collection is empty");
                    return new None { Message = "[CryptocurrencyApi]: Unexpected response - data collection is empty", Context = Context() };
                }

                if (cryptocurrenciesPerSymbol.Count > 1)
                {
                    logger.LogWarning("[CMC]: Data for more than one symbol is returned (while we only asked for one). Using the first one that matches the symbol");
                }

                if (!cryptocurrenciesPerSymbol.TryGetValue(searchQuery.Cryptocurrency.Value, out var cryptocurrencies))
                {
                    logger.LogError("[CMC]: Unexpected response - cannot find cryptocurrencyData for requested symbol");
                    return new None { Message = "[CryptocurrencyApi]: Unexpected response - cannot find cryptocurrencyData for requested symbol", Context = Context() };
                }

                using var _2 = logger.BeginAttributesScope(cryptocurrencies.Count);

                if (cryptocurrencies.Count < 1)
                {
                    logger.LogError("[CMC]: Unexpected response - cryptocurrencies are empty for the symbol");
                    return new NotFound { Message = "[CryptocurrencyApi]: Unexpected response - cryptocurrencies are empty for the symbol", Context = Context() };
                }

                var highestMarketCapCryptocurrency = cryptocurrencies.First();
                using var _3 = logger.BeginAttributesScope(highestMarketCapCryptocurrency.Id, highestMarketCapCryptocurrency.Name);

                if (cryptocurrencies.Count > 1)
                {
                    var cryptocurrencyNames = cryptocurrencies.Select(c => c.Name).ToArray();
                    logger.LogWarning("[CMC]: More than one cryptocurrencies are found for the symbol. Going to use the cryptocurrency with the highest market cap.");
                }

                var quotePerCurrency = highestMarketCapCryptocurrency.QuotePerCurrency;

                using var _4 = logger.BeginAttributesScope(quotePerCurrency.Count);

                if (quotePerCurrency.Count < 1)
                {
                    logger.LogError("[CMC]: Unexpected response - 'quote' collection is empty");
                    return new None { Message = "[CryptocurrencyApi]: Unexpected response - 'quote' collection is empty", Context = Context() };
                }

                if (quotePerCurrency.Count > 1)
                {
                    logger.LogWarning("[CMC]: 'quote' has data for more than one currency (while we only asked for one). Using the one that matches base currency");
                }

                if (!quotePerCurrency.TryGetValue(searchQuery.BaseCurrency.Value, out var quote))
                {
                    logger.LogError("[CMC]: Unexpected response - cannot find quoteData for base currency");
                    return new None { Message = "[CryptocurrencyApi]: Unexpected response - cannot find quoteData for base currency", Context = Context() };
                }

                return new CryptocurrencyQuote(searchQuery.Cryptocurrency, quote.LastUpdated, quote.Price ?? 0.0M, searchQuery.BaseCurrency);
            }
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
                    return new InternalError { Message = "[CryptocurrencyApi]: Unexpected response", Context = Context() };
                case var c:
                    return new InternalError { Message = "[CryptocurrencyApi]: Unexpected response", Context = NonSuccessfulContext(response.StatusCode) };
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
        public required DateTime LastUpdated { get; init; }

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
        public required DateTime LastUpdated { get; init; }
    }
}
