namespace Coinpedia.FunctionalTests.Common;

public class BaseFuncionalTest(TestWebAppFactory app) : IClassFixture<TestWebAppFactory>
{
    protected HttpClient HttpClient { get; } = app.CreateClient();

    protected TestWebAppFactory App => app;
}
