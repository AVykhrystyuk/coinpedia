namespace Coinpedia.FunctionalTests.Common;

public class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFunc) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(responseFunc(request));
    }
}