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

        builder.ConfigureTestServices(OverrideWebHostConfiguration);
    }

    public TestSetup? TestSetup { get; set; }

    private void OverrideWebHostConfiguration(IServiceCollection services)
    {
        services.AddSerilog(configureLogger: (sp, loggerConfig) => loggerConfig
            .Configure(sp)
            .WriteTo.Console());

        services.TurnOffFusionCache();

        FakeSettings(services);

        services.MockExchangeRatesApiClient(ExchangeRatesApiClientResponseHandler);
        services.MockCryptocurrencyQuoteApiClient(CryptocurrencyQuoteApiClientResponseHandler);
    }

    private void FakeSettings(IServiceCollection services)
    {
        services.RemoveOptions<Settings>();
        services.AddSingleton(_ => Options.Create(new Settings
        {
            BaseCurrency = TestSetup?.BaseCurrency ?? "EUR",
            RequiredCurrencies = $"USD,EUR,BRL,GBP,AUD{(TestSetup?.ExtraCurrency is { } cur ? $",{cur}" : "")}",
        })); 
    }

    private HttpResponseMessage ExchangeRatesApiClientResponseHandler(HttpRequestMessage request)
    {
        var handler = TestSetup?.ExchangeRatesApiClientResponseHandler ?? HttpResponseMessages.NotImplementedFunc;
        return handler(request);
    }

    private HttpResponseMessage CryptocurrencyQuoteApiClientResponseHandler(HttpRequestMessage request)
    {
        var handler = TestSetup?.CryptocurrencyQuoteApiClientResponseHandler ?? HttpResponseMessages.NotImplementedFunc;
        return handler(request);
    }
}

public class TestSetup
{
    public string? BaseCurrency { get; set; }
    public string? ExtraCurrency { get; set; }

    public Func<HttpRequestMessage, HttpResponseMessage>? ExchangeRatesApiClientResponseHandler { get; set; }
    public Func<HttpRequestMessage, HttpResponseMessage>? CryptocurrencyQuoteApiClientResponseHandler { get; set; }
}
