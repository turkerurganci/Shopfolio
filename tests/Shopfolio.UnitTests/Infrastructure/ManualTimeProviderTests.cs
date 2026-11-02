using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

public sealed class ManualTimeProviderTests
{
    [Fact]
    public void GetUtcNow_AfterConstruction_ReturnsGivenInstant()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 3, 1, 21, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 3, 1, 21, 0, 0, TimeSpan.Zero), time.GetUtcNow());
    }

    [Fact]
    public void GetUtcNow_WithOffsetInput_ReturnsZeroOffsetInstant()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 3, 2, 0, 0, 0, TimeSpan.FromHours(3)));

        Assert.Equal(TimeSpan.Zero, time.GetUtcNow().Offset);
        Assert.Equal(new DateTimeOffset(2026, 3, 1, 21, 0, 0, TimeSpan.Zero), time.GetUtcNow());
    }

    [Fact]
    public void Advance_ByOneTick_MovesClockForwardByOneTick()
    {
        DateTimeOffset start = new(2026, 3, 1, 21, 0, 0, TimeSpan.Zero);
        ManualTimeProvider time = new(start);

        time.Advance(TimeSpan.FromTicks(1));

        Assert.Equal(start.AddTicks(1), time.GetUtcNow());
    }

    [Fact]
    public void SetUtcNow_ToEarlierInstant_MovesClockBack()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 3, 1, 21, 0, 0, TimeSpan.Zero));

        time.SetUtcNow(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.Equal(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), time.GetUtcNow());
    }
}
