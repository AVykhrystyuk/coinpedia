using System.Net;
using System.Text;

namespace Coinpedia.FunctionalTests.Common;

public static class HttpResponseMessages
{
    private static readonly string MediaType = "application/json";
    private static readonly Encoding Encoding = Encoding.UTF8;

    public static readonly Func<HttpRequestMessage, HttpResponseMessage> NotImplementedFunc = (request) => NotImplemented();

    public static HttpResponseMessage NotImplemented() => new(HttpStatusCode.NotImplemented)
    {
        Content = new StringContent("""{ "error": "NotImplemented" }""", Encoding, MediaType)
    };

    public static HttpResponseMessage OK(string content) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, Encoding, MediaType)
    };

    public static HttpResponseMessage InternalServerError() => new(HttpStatusCode.InternalServerError)
    {
        // Content = new StringContent(content, Encoding, MediaType)
    };
}