namespace Coinpedia.FunctionalTests.Common;

public class BaseFuncionalTest(TestWebAppFactory app) : IClassFixture<TestWebAppFactory>
{
    protected TestWebAppFactory App { get; } = app;
}
