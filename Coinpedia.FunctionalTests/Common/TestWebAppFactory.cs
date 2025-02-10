using Coinpedia.WebApi.Config;
using Coinpedia.WebApi.Logging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

            services.RemoveOptions<Settings>();
            services.AddSingleton(_ => Options.Create(new Settings
            {
                BaseCurrency = BaseCurrency,
                RequiredCurrencies = $"USD,EUR,BRL,GBP,AUD{(ExtraCurrency is { } cur ? $",{cur}" : "")}",
            }));

            services.MockExchangeRatesApiClient((request) => ExchangeRatesApiClientResponseHandler(request));
            services.MockCryptocurrencyQuoteApiClient((request) => CryptocurrencyQuoteApiClientResponseHandler(request));
        });
    }

    public string BaseCurrency { get; set; } = "EUR";
    public string? ExtraCurrency { get; set; }

    public Func<HttpRequestMessage, HttpResponseMessage> ExchangeRatesApiClientResponseHandler { get; set; } = HttpResponseMessages.NotImplementedFunc;
    public Func<HttpRequestMessage, HttpResponseMessage> CryptocurrencyQuoteApiClientResponseHandler { get; set; } = HttpResponseMessages.NotImplementedFunc;
}
