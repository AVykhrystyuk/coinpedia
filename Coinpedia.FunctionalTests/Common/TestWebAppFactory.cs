using Coinpedia.WebApi.Logging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using Serilog;

namespace Coinpedia.FunctionalTests.Common;

public class TestWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            // overrides whatever is configured in the app
            services.AddSerilog(configureLogger: (sp, loggerConfig) => loggerConfig
                .Configure(sp)
                .WriteTo.Console());

            services.TurnOffFusionCache();

            services.MockExchangeRatesApiClient((request) => ExchangeRatesApiClientResponseHandler(request));
            services.MockCryptocurrencyQuoteApiClient((request) => CryptocurrencyQuoteApiClientResponseHandler(request));
        });
    }

    public Func<HttpRequestMessage, HttpResponseMessage> ExchangeRatesApiClientResponseHandler { get; set; } = HttpResponseMessages.NotImplementedFunc;
    public Func<HttpRequestMessage, HttpResponseMessage> CryptocurrencyQuoteApiClientResponseHandler { get; set; } = HttpResponseMessages.NotImplementedFunc;
}
