using Polly;
using Polly.Extensions.Http;

namespace Coinpedia.WebApi.Config;

public static class HttpRetryPolicies
{
    public static IAsyncPolicy<HttpResponseMessage> Default()
    {
        // - Network failures (as System.Net.Http.HttpRequestException)
        // - HTTP 5XX status codes (server errors)
        // - HTTP 408 status code (request timeout)
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            //.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(retryCount: 3, GetSleepDuration);

        static TimeSpan GetSleepDuration(int retryAttempt)
        {
            // retryAttempt:
            // - 1 for a first retry
            // - 2 for a second retry
            // ...            
            // sleepDurations:
            // - 2^0 = 1
            // - 2^1 = 2
            // - 2^2 = 4
            return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1));
        }
    }
}