using System.Threading.RateLimiting;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The sliding window of the general limit moves with the fake clock, segment by segment, without waiting (T215 KK3; 07 2.8.2; 09 15.4 item 1;
/// 09 19.2 item 2). The window is sixty seconds in six segments of ten.
/// </summary>
public sealed class TimeDrivenSlidingWindowLimiterTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Segment = RateLimitDefaults.Window / RateLimitDefaults.Segments;

    private static bool Take(RateLimiter limiter)
    {
        using RateLimitLease lease = limiter.AttemptAcquire();
        return lease.IsAcquired;
    }

    private static TimeDrivenSlidingWindowLimiter Make(int permits, ManualTimeProvider time)
        => new(permits, RateLimitDefaults.Window, RateLimitDefaults.Segments, time);

    [Fact]
    public void WindowIsSixtySecondsInSixSegments()
    {
        Assert.Equal(TimeSpan.FromSeconds(60), RateLimitDefaults.Window);
        Assert.Equal(6, RateLimitDefaults.Segments);
        Assert.Equal(TimeSpan.FromSeconds(10), Segment);
    }

    [Fact]
    public void RequestsUpToThePermitLimit_AreLet_AndTheNextIsRefused()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(3, time);

        Assert.True(Take(limiter));
        Assert.True(Take(limiter));
        Assert.True(Take(limiter));
        Assert.False(Take(limiter));
    }

    [Fact]
    public void WithinASegment_NothingIsReleased_AndTheClockNeverMovesWithoutTheTest()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(2, time);
        Take(limiter);
        Take(limiter);

        time.Advance(Segment - TimeSpan.FromMilliseconds(1));

        Assert.False(Take(limiter));
    }

    [Fact]
    public void ASegmentsPermitsReturn_WhenThatSegmentLeavesTheWindow_NotBefore()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(3, time);
        Assert.True(Take(limiter));                 // segment 0: one permit
        time.Advance(Segment * 2);
        Assert.True(Take(limiter));                 // segment 2: two permits
        Assert.True(Take(limiter));
        Assert.False(Take(limiter));

        // Segment 0 leaves the window six segments after it began: before that the window is still full, after that one permit is back.
        time.Advance(Segment * 3);                  // now at segment 5
        Assert.False(Take(limiter));
        time.Advance(Segment);                      // now at segment 6: segment 0 is out
        Assert.True(Take(limiter));
        Assert.False(Take(limiter));
    }

    [Fact]
    public void AfterAWholeWindowWithoutRequests_EveryPermitIsBack()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(3, time);
        Take(limiter);
        Take(limiter);
        Take(limiter);
        Assert.False(Take(limiter));

        time.Advance(RateLimitDefaults.Window);

        Assert.True(Take(limiter));
        Assert.True(Take(limiter));
        Assert.True(Take(limiter));
        Assert.False(Take(limiter));
    }

    [Fact]
    public void ALongPause_DoesNotReleaseMoreThanTheLimit()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(2, time);
        Take(limiter);

        time.Advance(TimeSpan.FromHours(5));

        Assert.True(Take(limiter));
        Assert.True(Take(limiter));
        Assert.False(Take(limiter));
    }

    [Fact]
    public void IdleDuration_IsNull_UntilAWholeWindowPassedWithoutUse_ThenTheTimeSinceTheLastUse()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(2, time);
        Take(limiter);

        time.Advance(RateLimitDefaults.Window - TimeSpan.FromMilliseconds(1));
        Assert.Null(limiter.IdleDuration);

        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.Equal(RateLimitDefaults.Window, limiter.IdleDuration);

        time.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(RateLimitDefaults.Window + TimeSpan.FromSeconds(5), limiter.IdleDuration);
    }

    [Fact]
    public void AUse_RestartsTheIdleTime()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(2, time);
        time.Advance(RateLimitDefaults.Window);
        Assert.NotNull(limiter.IdleDuration);

        Take(limiter);

        Assert.Null(limiter.IdleDuration);
    }

    [Fact]
    public async Task AcquireAsync_AnswersAtOnce_WithoutQueueing()
    {
        ManualTimeProvider time = new(Start);
        using TimeDrivenSlidingWindowLimiter limiter = Make(1, time);

        using RateLimitLease first = await limiter.AcquireAsync(1, TestContext.Current.CancellationToken);
        using RateLimitLease second = await limiter.AcquireAsync(1, TestContext.Current.CancellationToken);

        Assert.True(first.IsAcquired);
        Assert.False(second.IsAcquired);
    }
}
