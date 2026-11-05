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

    [Fact]
    public async Task Delay_OnTheFakeClock_EndsOnlyWhenTheClockReachesIt()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero));
        Task delay = Task.Delay(TimeSpan.FromSeconds(5), time, TestContext.Current.CancellationToken);
        Assert.Equal(1, time.ActiveTimerCount);

        time.Advance(TimeSpan.FromSeconds(4));
        Assert.False(delay.IsCompleted);
        Assert.Equal(1, time.ActiveTimerCount);
        time.Advance(TimeSpan.FromSeconds(1));

        await delay.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        Assert.Equal(0, time.ActiveTimerCount);
    }

    [Fact]
    public void Timer_Disposed_NeverFires_AndIsNoLongerActive()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero));
        int fired = 0;
        ITimer timer = time.CreateTimer(_ => fired++, null, TimeSpan.FromSeconds(5), Timeout.InfiniteTimeSpan);
        Assert.Equal(1, time.ActiveTimerCount);

        timer.Dispose();
        time.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(0, fired);
        Assert.Equal(0, time.ActiveTimerCount);
    }

    [Fact]
    public void Timer_WithAPeriod_FiresOncePerPeriodReached_AndStaysActive()
    {
        ManualTimeProvider time = new(new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero));
        int fired = 0;
        using ITimer timer = time.CreateTimer(_ => fired++, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        time.Advance(TimeSpan.FromSeconds(5));
        time.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(2, fired);
        Assert.Equal(1, time.ActiveTimerCount);
    }
}
