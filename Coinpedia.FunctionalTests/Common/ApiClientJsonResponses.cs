namespace Coinpedia.FunctionalTests.Common;

public static class ApiClientJsonResponses
{
    public static class ExchangeRates
    {
        public static string Ok(string baseCurrency) => $$"""
            {
                "success": true,
                "timestamp": 1739142255,
                "base": "{{baseCurrency}}",
                "date": "2025-02-09",
                "rates": {
                    "USD": 1.030391,
                    "BRL": 5.982972,
                    "GBP": 0.831746,
                    "AUD": 1.648363,
                    "EUR": 1
                }
            }
            """;
    }

    public static class CoinMarketCap
    {
        public static string Ok(string symbol, string baseCurrency) => $$"""
            {
                "status": {
                    "timestamp": "2025-02-09T23:50:18.050Z",
                    "error_code": 0,
                    "error_message": null,
                    "elapsed": 23,
                    "credit_count": 1,
                    "notice": null
                },
                "data": {
                    "{{symbol}}": [
                        {
                            "id": 1,
                            "name": "{{symbol}}_name",
                            "symbol": "{{symbol}}",
                            "slug": "bitcoin",
                            "date_added": "2010-07-13T00:00:00.000Z",
                            "is_active": 1,
                            "infinite_supply": false,
                            "platform": null,
                            "is_fiat": 0,
                            "self_reported_circulating_supply": null,
                            "self_reported_market_cap": null,
                            "tvl_ratio": null,
                            "last_updated": "2025-02-09T23:48:00.000Z",
                            "quote": {
                                "{{baseCurrency}}": {
                                    "price": 93419.00775880407,
                                    "volume_24h": 26205547470.920208,
                                    "volume_change_24h": 15.766,
                                    "percent_change_1h": 0.30324863,
                                    "percent_change_24h": -0.25878976,
                                    "percent_change_7d": -1.24450144,
                                    "percent_change_30d": 1.68267769,
                                    "percent_change_60d": -4.88023128,
                                    "percent_change_90d": 8.5679016,
                                    "market_cap": 1851766145160.2246,
                                    "market_cap_dominance": 60.6765,
                                    "fully_diluted_market_cap": 1961799162934.8882,
                                    "tvl": null,
                                    "market_cap_by_total_supply": 1851766145160.2246,
                                    "last_updated": "2025-02-09T23:49:04.000Z"
                                }
                            }
                        },
                        {
                            "id": 34316,
                            "name": "HarryPotterTrumpSonic100Inu",
                            "symbol": "{{symbol}}",
                            "slug": "harrypottertrumpsonic100inu",
                            "date_added": "2024-11-29T10:04:10.000Z",
                            "platform": {
                                "id": 1027,
                                "name": "Ethereum",
                                "symbol": "ETH",
                                "slug": "ethereum",
                                "token_address": "0x7099aB9E42Fa7327a6b15E0a0c120c3e50d11BeC"
                            },
                            "is_active": 1,
                            "infinite_supply": false,
                            "is_fiat": 0,
                            "self_reported_circulating_supply": 1000420069,
                            "self_reported_market_cap": 166182.14855323487,
                            "tvl_ratio": null,
                            "last_updated": "2025-02-09T23:49:00.000Z",
                            "quote": {
                                "{{baseCurrency}}": {
                                    "price": 0.00016108414845936866,
                                    "volume_24h": 4056.8957418497776,
                                    "volume_change_24h": -43.3688,
                                    "percent_change_1h": 0.99277516,
                                    "percent_change_24h": -0.24395119,
                                    "percent_change_7d": -11.62921034,
                                    "percent_change_30d": -62.20050791,
                                    "percent_change_60d": -76.80451234,
                                    "percent_change_90d": 345.14703909,
                                    "market_cap": 0,
                                    "market_cap_dominance": 0,
                                    "fully_diluted_market_cap": 161151.81631949937,
                                    "tvl": null,
                                    "market_cap_by_total_supply": 161151.81491652783,
                                    "last_updated": "2025-02-09T23:49:04.000Z"
                                }
                            }
                        }
                    ]
                }
            }
            """;
    }
}