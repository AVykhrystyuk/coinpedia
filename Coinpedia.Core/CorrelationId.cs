namespace Coinpedia.Core;

public static class CorrelationId
{
    private static readonly AsyncLocal<string> _correlationId = new();

    public static string Value
    {
        get => _correlationId.Value ??= New();
        set => _correlationId.Value = value ?? New();
    }

    private static string New() => Guid.NewGuid().ToString();
}
