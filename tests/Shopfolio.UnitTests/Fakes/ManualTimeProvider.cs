namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written stateful fake clock (09 7.1 item 4, 19.1 item 3): <see cref="GetUtcNow"/> returns the set instant,
/// <see cref="SetUtcNow"/> and <see cref="Advance"/> move it. Tests never wait; they move this clock.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public ManualTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public void Advance(TimeSpan by)
    {
        _utcNow = _utcNow.Add(by);
    }
}
