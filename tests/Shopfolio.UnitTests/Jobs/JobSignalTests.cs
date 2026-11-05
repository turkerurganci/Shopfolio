using Shopfolio.Infrastructure.Jobs;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The runner's wake-up (T220 criterion 1, KK1; 09 13.1 items 2, 3 and 11): a signal ends the wait at once, the five seconds end it on the
/// fake clock, repeated signals come down to one wake-up. No test waits for real time: the clock is moved.
/// </summary>
public sealed class JobSignalTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static ManualTimeProvider NewTime() => new(new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task WaitAsync_AfterNotify_EndsAtOnce_WithoutMovingTheClock()
    {
        ManualTimeProvider time = NewTime();
        JobSignal signal = new();
        signal.Notify();

        await signal.WaitAsync(JobTimings.PollInterval, time, Ct).WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Assert.Equal(0, time.ActiveTimerCount);
    }

    [Fact]
    public async Task WaitAsync_WhileWaiting_NotifyWakesItBeforeThePollInterval()
    {
        ManualTimeProvider time = NewTime();
        JobSignal signal = new();
        Task wait = signal.WaitAsync(JobTimings.PollInterval, time, Ct);
        await WaitForTimerAsync(time);
        Assert.False(wait.IsCompleted);

        signal.Notify();

        await wait.WaitAsync(TimeSpan.FromSeconds(30), Ct);
    }

    [Fact]
    public async Task WaitAsync_WithoutASignal_EndsWhenTheClockPassesThePollInterval_NotBefore()
    {
        ManualTimeProvider time = NewTime();
        JobSignal signal = new();
        Task wait = signal.WaitAsync(JobTimings.PollInterval, time, Ct);
        await WaitForTimerAsync(time);

        time.Advance(JobTimings.PollInterval - TimeSpan.FromMilliseconds(1));
        await Task.Yield();
        Assert.False(wait.IsCompleted);
        time.Advance(TimeSpan.FromMilliseconds(1));

        await wait.WaitAsync(TimeSpan.FromSeconds(30), Ct);
    }

    [Fact]
    public async Task Notify_CalledManyTimes_GivesOneWakeUp()
    {
        ManualTimeProvider time = NewTime();
        JobSignal signal = new();
        for (int i = 0; i < 5; i++)
        {
            signal.Notify();
        }

        await signal.WaitAsync(JobTimings.PollInterval, time, Ct).WaitAsync(TimeSpan.FromSeconds(30), Ct);
        Task second = signal.WaitAsync(JobTimings.PollInterval, time, Ct);
        await WaitForTimerAsync(time);

        Assert.False(second.IsCompleted);
        time.Advance(JobTimings.PollInterval);
        await second.WaitAsync(TimeSpan.FromSeconds(30), Ct);
    }

    [Fact]
    public async Task WaitAsync_Cancelled_Throws()
    {
        ManualTimeProvider time = NewTime();
        JobSignal signal = new();
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(Ct);
        Task wait = signal.WaitAsync(JobTimings.PollInterval, time, stop.Token);
        await WaitForTimerAsync(time);

        await stop.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
    }

    private static async Task WaitForTimerAsync(ManualTimeProvider time)
    {
        // The wait registers its timer on its first await; the test moves the clock only after that.
        DateTime until = DateTime.UtcNow.AddSeconds(30);
        while (time.ActiveTimerCount == 0 && DateTime.UtcNow < until)
        {
            await Task.Delay(5, Ct);
        }

        Assert.True(time.ActiveTimerCount > 0, "The wait did not register its timer.");
    }
}
